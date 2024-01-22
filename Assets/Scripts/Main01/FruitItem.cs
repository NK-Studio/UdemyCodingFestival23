using System;
using Animation;
using BrunoMikoski.AnimationSequencer;
using Data;
using NKStudio;
using UnityEngine;
using UnityEngine.InputSystem;
using USingleton;

public enum EFruitType
{
    None,
    Strawberry,
    ShineMuscat,
    Mandarine,
    Blueberry,
    BlackSapphire
}

public class FruitItem : MonoBehaviour
{
    [field: SerializeField]
    public bool Interection { get; set; } = true;

    [Header("과일 타입")] public EFruitType FruitType;

    [Header("떨어지는 속도")] public float FallSpeed = 1f;
    public float FallMultiplier = 100f;

    [Header("Audio"), SerializeField] private AudioSource fruitStickIn;

    [Header("드래그 시 과일 위치 오프셋")] public Vector3 Offset;

    // RectTransform
    private RectTransform _selfRectTransform; // 자신
    private RectTransform _stick; // 스틱
    private RectTransform _targetY; // 스틱 내부를 표현
    private RectTransform _stopPosition; // 멈춰야하는 위치

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
    private bool _activeDownCheck = true;

    // Constants
    private const int kDefaultVibrateTime = 100;
    private const int kDefaultVibrateAmplitude = 100;


    private bool _isTouchingStick;

    /// <summary>
    /// 스틱에 계속 닿고 있는지를 체크합니다.
    /// </summary>
    public bool IsTouchingStick;
    
    private void Awake()
    {
        _mainInputAction = new MainInputAction();
    }

    private void Start()
    {
        // 할당
        _selfRectTransform = GetComponent<RectTransform>();
        _animationSequencerController = GetComponent<AnimationSequencerController>();
        _rotator = GetComponent<Rotator>();
        _stopPosition = GameObject.Find("StopPosition").GetComponent<RectTransform>();
        _stick = GameObject.Find("Stick").GetComponent<RectTransform>();
        _targetY = GameObject.Find("In Line").GetComponent<RectTransform>();

        // 이벤트 등록
        if (gameObject.TryGetComponent(out _distanceCollider))
        {
            _distanceCollider.OnEnter.AddListener(OnEnter);
            _distanceCollider.OnExit.AddListener(OnExit);
        }
    }
    
    public void Init(InputController inputController, FruitManager fruitManager)
    {
        _fruitManager = fruitManager;
        _fruitManager.HasControlFruit = true;

        // 화면 터치에 손을 땟다면,
        inputController.OnPressAction += active =>
        {
            if (!active)
            {
                // 스톱 영역에 대한 허용 처리가 False라면, 더 이상 진행하지 않는다.
                if (!_activeDownCheck)
                    return;

                // 스틱에 꼿혀져 있는 상태라면, 더 이상 인터렉션을 멈추고 밑으로 떨어지는 애니메이션을 재생한다.
                if (IsTouchingStick)
                {
                    Interection = false;
                    _triggerStickFallAnimation = true;
                }
                else
                {
                    // 과일 매니저에 컨트롤 가능한 과일이 없음을 처리
                    _fruitManager.HasControlFruit = false;
                    Destroy(gameObject);
                }
            }
        };
    }

    private void OnEnable()
    {
        _mainInputAction.Enable();
        _mainInputAction.Player.TouchPosition.performed += OnTouchPosition;
    }

    private void OnDisable()
    {
        _mainInputAction.Player.TouchPosition.performed -= OnTouchPosition;
        _mainInputAction.Disable();
    }

    private void OnEnter()
    {
        // 과일 매니저가 끝났다면, 멈춰!
        if (_fruitManager.Finish)
            return;

        // 꼿아지는 것을 허락합니다.
        _allowFruitPiercing = true;
        _rotator.Play();
    }

    private void OnExit()
    {
        // 과일 매니저가 끝났다면, 멈춰!
        if (_fruitManager.Finish)
            return;

        // 스틱에 닿지 않았다면,
        if (!IsTouchingStick)
        {
            // 꼿아지는 것을 철회합니다.
            _allowFruitPiercing = false;
            _rotator.Reverse();
        }
    }

    private void Update()
    {
        TriggerTouchingStick();
        Move();
        UpdateStopSystem();
        ChangeValueByIsTouchingStick();
    }

    private void ChangeValueByIsTouchingStick()
    {
        if (IsTouchingStick != _isTouchingStick)
        {
            _isTouchingStick = IsTouchingStick;
            
            // 과일이 스틱 처음 부분에 닿았을 때 처리
            if (_isTouchingStick)
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
        }
    }

    /// <summary>
    /// 과일이 스틱의 시작부분에 닿았는지를 체크합니다.
    /// </summary>
    private void TriggerTouchingStick()
    {
        // Stop 영역에 대한 허용 처리가 True라면,
        if (_allowFruitPiercing)
        {
            // 과일의 바닥 Y값을 구합니다.
            float fruitBottomY = _selfRectTransform.position.y + _selfRectTransform.rect.yMin;

            // 과일이 스틱에 꼿혀지는 높이에 닿았다면(미만이라면),
            if (fruitBottomY < _targetY.position.y)
                IsTouchingStick = true;
            else
                IsTouchingStick = false;
        }
    }

    /// <summary>
    /// 이동 시스템을 처리합니다.
    /// </summary>
    private void Move()
    {
        // 스틱에서 떨어지는 애니메이션 재생 중이 아니라면,
        if (Interection)
        {
            // 스틱에 닿으면 스틱 중심에서 업-다운만 가능, 스틱 밖에 있으면 좌-우 이동도 가능
            if (IsTouchingStick)
            {
                Vector3 newPosition = new(_stick.position.x, _touchPosition.y, 0f);
                transform.position = newPosition + Offset;
            }
            else
            {
                Vector3 newPosition = new(_touchPosition.x, _touchPosition.y, 0f);
                transform.position = newPosition + Offset;
            }
        }
        else
        {
            // 떨어지는 애니메이션 재생
            if (_triggerStickFallAnimation)
                transform.Translate(Vector2.down * (FallSpeed * FallMultiplier * Time.deltaTime));
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

        if (!_activeDownCheck)
            return;

        // 자신의 높이의 절반을 구합니다.
        float halfSelfHeight = _selfRectTransform.sizeDelta.y / 2;

        if (_selfRectTransform.localPosition.y - halfSelfHeight < _stopPosition.localPosition.y)
        {
            // 진동 처리
            MobileNative.Vibrate(kDefaultVibrateTime, kDefaultVibrateAmplitude);

            // 떨어지는 애니메이션을 끄고, 스틱에 자식으로 넣습니다.
            _triggerStickFallAnimation = false;
            transform.SetParent(_stick);

            // 스틱에 닿은 과일의 위치를 재조정합니다.
            var nextPosition = Vector3.zero;
            nextPosition.x = _stick.position.x;

            var lastFruit = Singleton.Instance<GameManager>().GetLastFruitType();
            float addOffset = lastFruit switch
            {
                EFruitType.None => 0f,
                EFruitType.Strawberry => 0f,
                EFruitType.ShineMuscat => 0f,
                EFruitType.Mandarine => 20f,
                EFruitType.Blueberry => 10f,
                EFruitType.BlackSapphire => 40f,
                _ => throw new ArgumentOutOfRangeException()
            };

            nextPosition.y = _stopPosition.position.y + halfSelfHeight + addOffset;
            _selfRectTransform.position = nextPosition;

            // 더 이상 스톱 영역 높이 체크를 하지 않음
            _activeDownCheck = false;

            // 과일 매니저에 컨트롤 가능한 과일이 없음을 처리
            _fruitManager.HasControlFruit = false;

            // 과일 매니저에 과일을 추가합니다.
            Fruit item = new Fruit(FruitType, transform.localPosition);
            Singleton.Instance<GameManager>().Fruits.Add(item);

            // 혹시 모를 버그를 대비하여 과일의 GFX 사이즈를 초기화합니다.
            transform.GetChild(0).localScale = Vector3.one;

            // 애니메이션 재생
            _animationSequencerController.Play();

            // Stop 오브젝트의 위치를 다시 조정합니다. (30은 매직 넘버)
            const float kOffset = 30f;
            _stopPosition.localPosition = _selfRectTransform.localPosition + Vector3.up * (halfSelfHeight - kOffset);

            // 과일이 Finish 영역에 닿았을 때 처리를 합니다.
            GameObject physics = gameObject.transform.GetChild(0).GetChild(0).gameObject;
            if (physics.TryGetComponent(out BoxCollider2D boxCollider2D))
            {
                Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxCollider2D.size, 0);

                foreach (var coll in colliders)
                {
                    if (coll.gameObject.CompareTag("Finish"))
                    {
                        _fruitManager.Finish = true;
                        break;
                    }
                }
            }

            // 인터렉션 불가 처리
            Interection = false;
        }
    }

    private void OnTouchPosition(InputAction.CallbackContext ctx)
    {
        Vector2 touch = ctx.ReadValue<Vector2>();
        _touchPosition = touch;
    }
}