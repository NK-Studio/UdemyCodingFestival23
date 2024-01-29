using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneSystem;

public class FruitManager : MonoBehaviour
{
    [Header("타겟 캔버스"), Tooltip("과일을 생성할 캔버스"),SerializeField] private Canvas targetCanvas;

    [Header("Rect Transform"),SerializeField] private RectTransform itemBoxGroup;
    public RectTransform HintUI;

    [Header("스틱 애니메이터"),SerializeField] private Animator stick;

    [Header("별 이펙트"), SerializeField] private GameObject starGroup;

    [Header("과일"), ArrayElementTitle("FruitType")]
    public FruitPrefab[] fruits;

    /// <summary>
    /// 작업이 완료되었는지 여부를 나타내는 값을 가져오거나 설정합니다.
    /// </summary>
    /// <remarks>
    /// 이 속성은 작업이 완료되었는지 여부를 확인하는 데 사용됩니다.
    /// </remarks>
    /// <value>
    /// 작업이 완료되면 <c>true</c>입니다. 그렇지 않으면 <c>false</c>입니다.
    /// </value>
    public bool Finish { get; set; }

    // Constants
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    private const float kItemBoxGroupX = -724f;
    private const float kItemBoxDuration = 1f;

    private MainInputAction _mainInputAction;

    // 터치 위치
    private Vector2 _touchPosition;
    
    private void Awake()
    {
        _mainInputAction = new MainInputAction();
    }

    private void OnEnable()
    {
        _mainInputAction.Player.TouchPosition.performed +=OnTouchPosition;
        _mainInputAction.Enable();
    }

    private void OnTouchPosition(InputAction.CallbackContext obj)
    {
        _touchPosition = obj.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        _mainInputAction.Player.TouchPosition.performed -= OnTouchPosition;
        _mainInputAction.Disable();
    }

    private void Start()
    {
        StartCoroutine(FinishTask());
    }

    /// <summary>
    /// 과일을 모두 스틱에 넣었을 때 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator FinishTask()
    {
        yield return new WaitUntil(() => Finish);

        // 아이템 박스를 왼쪽으로 이동하도록 처리
        itemBoxGroup.DOMoveX(kItemBoxGroupX, kItemBoxDuration).SetEase(Ease.OutSine).Play().SetLink(gameObject)
            .SetAutoKill();

        yield return new WaitForSeconds(0.5f);

        // 스틱 애니메이션 재생 및 별 FX 활성화
        stick.SetTrigger(Trigger);
        starGroup.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        // 다음 씬으로 이동
        SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();
        sceneLoader.AllowCompletion();
    }

    /// <summary>
    /// 지정된 유형의 과일을 생성합니다.
    /// </summary>
    /// <param name="fruitType">생성할 과일의 유형입니다.</param>
    [VisibleEnum(typeof(EFruitType))]
    public void Generate(int fruitType)
    {
        // 과일 꼬치에 과일이 모두 꼿아졌다면 더 이상 과일을 생성하지 않습니다.
        if (Finish)
            return;
        
        // 과일 리스트에서 인자로 들어온 과일의 유형을 찾습니다.
        int targetIndex = -1;
        for (int i = 0; i < fruits.Length; i++)
        {
            if (fruits[i].FruitType != (EFruitType)fruitType) continue;
            
            targetIndex = i;
            break;
        }

        if (targetIndex == -1)
            Assert.IsTrue(false, "해당 과일이 없습니다.");
        
        // 과일을 생성합니다.
        GameObject fruit = Instantiate(fruits[targetIndex].Prefab, targetCanvas.transform);
        fruit.transform.localScale = Vector3.one;

        // 과일의 위치를 초기 재설정하고, 초기화를 처리합니다.
        if (fruit.TryGetComponent(out FruitItem item))
        {
            fruit.transform.position = _touchPosition + (Vector2)item.Offset;
            item.Init(this);
        }

        // 과일의 거리 콜라이더를 설정합니다.
        if (fruit.TryGetComponent(out DistanceCollider distanceCollider)) 
            distanceCollider.Target = HintUI;
    }
    
    /// <summary>
    /// 힌트 UI를 표시하거나 숨깁니다.
    /// </summary>
    /// <param name="value">힌트 UI를 표시할지 숨길지를 나타내는 부울 값입니다.</param>
    public void ShowHintUI(bool value)
    {
        HintUI.gameObject.SetActive(value);
    }
}