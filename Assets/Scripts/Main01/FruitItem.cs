using Data;
using System;
using Animation;
using NKStudio;
using USingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using BrunoMikoski.AnimationSequencer;
using JetBrains.Annotations;
using MyBox;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FruitItem : MonoBehaviour
{
    [Header("과일 타입"), SerializeField] private EFruitType fruitType;

    [Header("떨어지는 속도"), SerializeField] private float fallSpeed = 1f;
    [SerializeField] private float fallMultiplier = 100f;

    [Header("드래그 시 과일 위치 오프셋")] public Vector3 Offset;

    [Header("Audio"), SerializeField] private AudioSource fruitStickIn;

    // RectTransform
    private RectTransform _selfRectTransform; // 자신
    private RectTransform _stick; // 스틱
    private RectTransform _startPoint; // 스틱 내부를 표현
    private RectTransform _stopPoint; // 멈춰야하는 위치

    // 입력 객체
    private MainInputAction _mainInputAction;

    // 과일 매니저
    private FruitManager _fruitManager;

    // 거리 콜라이더
    private DistanceCollider _distanceCollider;

    // 애니메이션
    private Rotator _rotator;
    private AnimationSequencerController _animationSequencerController;

    // 터치 위치
    private Vector3 _touchPosition;

    // 과일이 꼿아지고 있는 것을 허용하는 조건
    private bool _allowFruitPiercing;

    // 떨어지는 애니메이션을 트리거하는 조건
    private bool _triggerStickFallAnimation;

    // 스톱 영역에 닿았을 때 처리를 허용하는 트리거
    private bool _isEnterStopPoint;

    // 이동 가능 여부
    private bool _moveEnable = true;

    // Constants
    private const int kDefaultVibrateTime = 100;
    private const int kDefaultVibrateAmplitude = 100;

    // 스틱의 시작 포인트에 닿고 있는지를 체크합니다.
    private bool _isTouchingStartPoint
    {
        get => _prevIsTouchingStartPoint;
        set
        {
            if (_prevIsTouchingStartPoint == value)
                return;

            _prevIsTouchingStartPoint = value;

            // 스틱에 닿았다면, 과일 매니저에 컨트롤 가능한 과일이 없음을 처리
            PlayEffect();
        }
    }

    private bool _prevIsTouchingStartPoint;

    private void Awake()
    {
        _mainInputAction = new MainInputAction();
    }

    private void Start()
    {
        // 할당
        _rotator = GetComponent<Rotator>();
        _selfRectTransform = GetComponent<RectTransform>();
        _animationSequencerController = GetComponent<AnimationSequencerController>();
        _startPoint = GameObject.Find("Start-Point").GetComponent<RectTransform>();
        _stopPoint = GameObject.Find("Stop-Point").GetComponent<RectTransform>();
        _stick = GameObject.Find("Stick").GetComponent<RectTransform>();

        // 이벤트 등록
        if (gameObject.TryGetComponent(out _distanceCollider))
        {
            _distanceCollider.OnEnter.AddListener(OnHintEnter);
            _distanceCollider.OnExit.AddListener(OnHintExit);
        }
    }

    public void Init(FruitManager fruitManager)
    {
        _fruitManager = fruitManager;
        _fruitManager.ShowHintUI(true);
    }

    private void OnEnable()
    {
        _mainInputAction.Enable();
        _mainInputAction.Player.TouchPosition.performed += OnTouchPosition;
        _mainInputAction.Player.TouchPress.canceled += OnTouchRelease;
    }

    private void OnDisable()
    {
        _mainInputAction.Player.TouchPosition.performed -= OnTouchPosition;
        _mainInputAction.Player.TouchPress.canceled -= OnTouchRelease;
        _mainInputAction.Disable();
    }

    private void OnHintEnter()
    {
        // 꼿아지는 것을 허락합니다.
        _allowFruitPiercing = true;
        _rotator.Play();
    }

    private void OnHintExit()
    {
        // 스틱의 시작 위치에 닿지 않았다면,
        // 힌트에서 멀어지면 리버스해도 괜찮음
        // 하지만 시작 위치에 이미 닿았다면, 리버스하면 안됨
        if (_isTouchingStartPoint) return;

        // 꼿아지는 것을 철회합니다.
        _allowFruitPiercing = false;
        _rotator.Reverse();
    }

    private void Update()
    {
        UpdateMove();
        UpdateIsTouchingStartPoint();
        UpdateStopSystem();
    }

    /// <summary>
    /// 진동 및 애니메이션 재생, 사운드 재생을 처리합니다.
    /// </summary>
    private void PlayEffect()
    {
        // 진동 처리
        MobileNative.Vibrate(kDefaultVibrateTime, kDefaultVibrateAmplitude);

        // 애니메이션 시퀀스로 인해 뒤틀린 사이즈를 복구 (보안)
        transform.GetChild(0).localScale = Vector3.one;

        // 애니메이션 재생
        _animationSequencerController.Play();

        // 사운드 재생
        fruitStickIn.Play();
    }

    /// <summary>
    /// 과일이 스틱의 시작부분에 닿았는지를 체크합니다.
    /// </summary>
    private void UpdateIsTouchingStartPoint()
    {
        // Stop 영역에 대한 허용 처리가 True라면,
        if (_allowFruitPiercing)
        {
            // 과일의 바닥 Y값을 구합니다.
            float halfHeight = _selfRectTransform.rect.height / 2f;
            float currentFruitBottomY = _selfRectTransform.position.y - halfHeight;

            // 과일이 스틱에 꼿혀지는 높이에 닿았다면(미만이라면),
            if (currentFruitBottomY < _startPoint.position.y)
                _isTouchingStartPoint = true;
            else
                _isTouchingStartPoint = false;
        }
    }

    /// <summary>
    /// 과일이 스틱의 끝 부분에 닿았는지 윗 부분으로 체크합니다.
    /// </summary>
    /// <returns>과일이 막대 꼭대기에 닿으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.</returns>
    private bool IsEnterFinishStick()
    {
        // 과일의 윗 Y값을 구합니다.
        float halfHeight = _selfRectTransform.rect.height / 2f;
        float currentFruitUpY = _selfRectTransform.anchoredPosition.y + halfHeight;
        return currentFruitUpY > _startPoint.anchoredPosition.y;
    }

    /// <summary>
    /// 이동 시스템을 처리합니다.
    /// </summary>
    private void UpdateMove()
    {
        // 인터렉션 가능 시, 터치 위치에 따라 이동합니다.
        if (_moveEnable)
        {
            // 스틱에 닿으면 스틱 중심에서 업-다운만 가능, 스틱 밖에 있으면 좌-우 이동도 가능
            if (_isTouchingStartPoint)
            {
                Vector3 newPosition = new(_stick.position.x, _touchPosition.y, 0f);
                _selfRectTransform.position = newPosition + Offset;
            }
            else
            {
                Vector3 newPosition = new(_touchPosition.x, _touchPosition.y, 0f);
                _selfRectTransform.position = newPosition + Offset;
            }
        }
        // 인터렉션 불가 시, 떨어지는 애니메이션을 재생합니다.
        else
        {
            // 떨어지는 애니메이션 재생
            if (_triggerStickFallAnimation)
                _selfRectTransform.Translate(Vector2.down * (fallSpeed * fallMultiplier * Time.deltaTime));
        }
    }

    /// <summary>
    /// Stop 오브젝트에 닿았을 때 멈추도록 처리합니다.
    /// </summary>
    private void UpdateStopSystem()
    {
        // 과일이 꼿아지고 있는 상태가 아니라면,
        if (!_allowFruitPiercing)
            return;

        // 스톱 포인트에 닿았다면, return 
        if (_isEnterStopPoint)
            return;

        // 자신의 높이의 절반을 구합니다.
        float halfHeight = _selfRectTransform.rect.height / 2f;
        float currentFruitBottomY = _selfRectTransform.anchoredPosition.y - halfHeight;

        if (currentFruitBottomY < _stopPoint.localPosition.y)
        {
            // 진동 처리
            MobileNative.Vibrate(kDefaultVibrateTime, kDefaultVibrateAmplitude);

            // 떨어지는 애니메이션을 끄고, 스틱에 자식으로 넣습니다.
            _triggerStickFallAnimation = false;
            transform.SetParent(_stick);

            // 스틱에 닿은 과일의 위치를 재조정합니다.
            var nextPosition = Vector3.zero;
            nextPosition.x = _stick.position.x;

            // 과일의 유형에 따라 멈출 위치를 조정합니다.
            var lastFruit = Singleton.Instance<GameManager>().GetLastFruitType();
            float addOffset = lastFruit switch
            {
                EFruitType.None => 0f,
                EFruitType.Strawberry => 5f,
                EFruitType.ShineMuscat => 0f,
                EFruitType.Mandarine => 100f,
                EFruitType.Blueberry => 10f,
                EFruitType.BlackSapphire => 40f,
                _ => throw new ArgumentOutOfRangeException()
            };

            nextPosition.y = _stopPoint.position.y + halfHeight + addOffset;
            _selfRectTransform.position = nextPosition;

            // 더 이상 스톱 영역 높이 체크를 하지 않음
            _isEnterStopPoint = true;

            // 과일 매니저에 컨트롤 가능한 과일이 없음을 처리
            _fruitManager.ShowHintUI(false);

            // 과일 매니저에 과일을 추가합니다.
            FruitElement item = new FruitElement(fruitType, transform.localPosition);
            Singleton.Instance<GameManager>().Fruits.Add(item);

            // 혹시 모를 버그를 대비하여 과일의 GFX 사이즈를 초기화합니다.
            transform.GetChild(0).localScale = Vector3.one;

            // 애니메이션 재생
            _animationSequencerController.Play();

            // Stop 오브젝트의 위치를 다시 조정합니다. (30은 매직 넘버)
            const float kOffset = 30f;
            _stopPoint.localPosition = _selfRectTransform.localPosition + Vector3.up * (halfHeight - kOffset);

            // 동작 불가 처리
            _moveEnable = false;

            // 등록 해제
            _distanceCollider.OnEnter.RemoveListener(OnHintEnter);
            _distanceCollider.OnExit.RemoveListener(OnHintExit);

            // 과일이 Finish 영역에 닿았을 때 처리를 합니다.
            if (IsEnterFinishStick())
                _fruitManager.Finish = true;
        }
    }

    private void OnTouchPosition(InputAction.CallbackContext ctx)
    {
        Vector2 touch = ctx.ReadValue<Vector2>();
        _touchPosition = touch;
    }

    private void OnTouchRelease(InputAction.CallbackContext obj)
    {
        // 스톱 포인트에 닿았다면 더 이상 인터렉션을 처리하지 않는다.
        if (_isEnterStopPoint)
            return;

        // 스틱에 꼿혀져 있는 상태라면, 더 이상 인터렉션을 멈추고 밑으로 떨어지는 애니메이션을 재생한다.
        if (_isTouchingStartPoint)
        {
            // 동작 불가 처리
            _moveEnable = false;

            // 떨어지는 애니메이션을 트리거합니다.
            _triggerStickFallAnimation = true;
        }
        else
        {
            // 힌트 UI를 숨깁니다.
            _fruitManager.ShowHintUI(false);

            // 자신 제거
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    [ButtonMethod, UsedImplicitly]
    private void SetChildSize()
    {
        RectTransform GFX = transform.GetChild(0).GetComponent<RectTransform>();

        if (TryGetComponent(out RectTransform self))
        {
            Undo.RecordObject(self, "Set Size");
            self.sizeDelta = GFX.sizeDelta;
            EditorUtility.SetDirty(self);
        }
    }
#endif
}