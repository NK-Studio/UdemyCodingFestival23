using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using USingleton;


public class FruitStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("RectTransform")] public Transform Stick;

    [Header("애니메이션 커브")] public AnimationCurve UpDownCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("릴리즈")] [ArrayElementTitle("FruitType")]
    public FruitElement[] fruitElements;

    [Header("디버그")] [ArrayElementTitle("FruitType")]
    public List<Fruit> testFruit;

    // 설탕이 코팅되는 수준을 0~1로 처리
    private float _allCoatingAmount = 0f;
    
    private bool _isTouch; // 터치 여부
    private bool _isExecuted; // 탕후루를 한쪽 방향으로 이동 시켰을 때 여러번 실행되는 것을 막기 위한 녀석
    private float _horizontal; // 탕후루를 좌우로 움직이는 값 (-1 ~ 1)

    private Vector3 _initPosition; // 초기 위치 기록용
    private Vector3 _touchPosition; // 화면 터치시 좌표
    private Vector3 _touchStartPosition; // 터치 시작 위치
    private RectTransform _selfRectTransform; // 자신의 RectTransform
    private MainInputAction _mainInputAction; // 인풋 액션
    private List<MeltedFruit> _meltedFruits; // 꼬챙이에 꼿혀있는 과일들

    // Constants
    private const float CoatingAmount = 0.35f;
    private readonly Vector2 LimitAngle = new Vector2(-20, 20);

    private void Awake()
    {
        _mainInputAction = new MainInputAction();
    }

    private void Start()
    {
        // 할당
        _selfRectTransform = GetComponent<RectTransform>();
        _initPosition = _selfRectTransform.anchoredPosition;

        // Create Fruit
        var fruits = Singleton.Instance<GameManager>().Fruits;

#if UNITY_EDITOR
        // 디버그용
        if (fruits.Count == 0)
            fruits = testFruit;
#endif

        // 과일 순서를 뒤집는다.
        fruits.Reverse();

        // 설탕 코딩을 해야하는 과일의 개수만큼 공간을 확보한다.
        _meltedFruits = new List<MeltedFruit>(fruits.Count);

        if (Stick == null)
            Assert.IsNotNull(Stick, "Stick is null");

        // 과일을 생성한다.
        foreach (Fruit fruit in fruits.AsEnumerable().Reverse())
        {
            FruitElement fruitElement = fruitElements.First(element => element.FruitType == fruit.FruitType);

            GameObject fruitObject = Instantiate(fruitElement.Prefab, Stick);
            fruitObject.transform.rotation = Quaternion.identity;
            fruitObject.transform.localPosition = fruit.Position;
        }

        // Stick의 자식을 순서대로 넣는다.
        for (int i = 0; i < Stick.childCount; i++)
        {
            // 자식을 차례대로 0~n으로 가져온다.
            var child = Stick.GetChild(i);

            // 자식이 MeltedFruit 컴포넌트를 가지고 있다면 리스트에 추가한다.
            if (child.TryGetComponent(out MeltedFruit meltedFruit))
                _meltedFruits.Add(meltedFruit);
        }

        // 꼬챙이 앞부분 과일부터 설탕이 입혀져야하므로 리스트를 뒤집는다.
        _meltedFruits.Reverse();

        // 끝내는 애니메이션 실행
        StartCoroutine(FinishTask());
    }

    /// <summary>
    /// 끝내는 애니메이션 실행
    /// </summary>
    /// <returns></returns>
    private IEnumerator FinishTask()
    {
        yield return new WaitUntil(() => _allCoatingAmount >= 1f);

        PotSystem potSystem = FindAnyObjectByType<PotSystem>();
        potSystem.ShowFinish();
    }
    
    private void Update()
    {
        // 좌우 스와이프 값 (-1 ~ 1)
        if (_isTouch)
            _horizontal = GetSwipeValue();

        // 0~1로 처리
        float animationNormalize = (_horizontal + 1) / 2;

        // 스와이프 애니메이션
        SwipeAnimation(animationNormalize);

        // 코팅 시스템
        CoatingSystem();
    }

    private void CoatingSystem()
    {
        if (!_isExecuted)
        {
            switch (_horizontal)
            {
                case > 0.9f:
                    TriggerFruitCoating();
                    _isExecuted = true;
                    break;
                case < -0.9f:
                    TriggerFruitCoating();
                    _isExecuted = true;
                    break;
            }
        }
        else // if (_executedOnce)  
        {
            // 한번만 트리거 되도록 막는 녀석을 해제함
            if (_horizontal is < 0.1f and > -0.1f)
                _isExecuted = false;
        }
    }

    /// <summary>
    /// 과일 코팅을 시전합니다.
    /// </summary>
    private void TriggerFruitCoating()
    {
        // 1. 코팅할 과일을 선정한다. (조건은 meltedFruit.CoatingAmount가 1이 아니어야함. )

        MeltedFruit targetMeltedFruit = null;
        foreach (var meltedFruit in _meltedFruits)
            if (meltedFruit.CoatingAmount < 1f)
            {
                targetMeltedFruit = meltedFruit;
                break;
            }

        // 2. 스틱에 꼿혀져 있는 과일들이 전체적으로 얼마나 설탕물이 뭍었는지 체크하는 코드
        if (targetMeltedFruit)
            targetMeltedFruit.CoatingAmount += CoatingAmount;

        // 3. 전체적으로 얼마나 설탕물이 뭍었는지 체크하는 코드
        float totalCoatingAmount = 0f;

        // 3-1. 전체적으로 과일의 코팅량을 구한다.
        foreach (var meltedFruit in _meltedFruits)
            totalCoatingAmount += meltedFruit.CoatingAmount;

        totalCoatingAmount /= _meltedFruits.Count;

        // 3-2. 전체적으로 얼마나 설탕물이 뭍었는지 적용한다.
        _allCoatingAmount = totalCoatingAmount;
    }


    /// <summary>
    /// 좌우 스와이프할 때 스틱의 애니메이션을 적용합니다.
    /// </summary>
    /// <param name="value">0~1의 값</param>
    private void SwipeAnimation(float value)
    {
        // 스틱의 회전
        _selfRectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(LimitAngle.x, LimitAngle.y, value));

        // 스틱의 위치
        var upAmount = LimitAngle.y;
        _selfRectTransform.anchoredPosition =
            new Vector2(_initPosition.x, _initPosition.y + upAmount * UpDownCurve.Evaluate(value));
    }

    /// <summary>
    /// 스틱을 잡고 좌우로 움직였을 때 -1~1의 값을 리턴합니다.
    /// </summary>
    /// <returns></returns>
    private float GetSwipeValue()
    {
        //_touchPosition을 통해 왼쪽 또는 오른쪽 이동을 -1~ 1로 처리하는 코드
        //Horizontal = _touchPosition.x;
        var horizontal = (_touchPosition.x - _touchStartPosition.x) / 100;

        // Remap하여 -3 ~ 3을 -1 ~ 1로 처리
        horizontal = Mathf.Clamp(horizontal, -3, 3);
        horizontal /= 3;

        return horizontal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position; // 터치한 위치의 값을 바인딩
        _isTouch = true; // 터치 상태로 만들기
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 터치 해제
        _isTouch = false;

        // 원래대로 되돌리기
        _horizontal = 0f;
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

    private void OnTouchPosition(InputAction.CallbackContext ctx)
    {
        Vector2 touch = ctx.ReadValue<Vector2>();
        _touchPosition = touch;
    }
}