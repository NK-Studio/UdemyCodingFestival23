using System;
using System.Collections;
using Data;
using DG.Tweening;
using NKStudio;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneSystem;

public class FruitManager : MonoBehaviour
{
    [Tooltip("과일을 생성할 캔버스")] public Canvas TargetCanvas;

    [Header("Rect Transform")] public RectTransform ItemBoxGroup;
    public RectTransform TriggerStickIn;

    [Header("스틱 애니메이터")] public Animator Stick;

    [Header("별 이펙트")] public GameObject StarFX;

    [Header("과일"), ArrayElementTitle("FruitType")]
    public FruitElement[] fruits;

    private InputController _inputController;

    private bool _hasControlFruit;

    public bool HasControlFruit
    {
        get => _hasControlFruit;
        set
        {
            if (_hasControlFruit != value)
            {
                // 컨트롤할 수 있는 과일이 있으면 스틱 힌트 애니메이션 활성화
                _hasControlFruit = value;
                TriggerStickIn.gameObject.SetActive(value);
            }
        }
    }

    private bool _finish;

    public bool Finish
    {
        get => _finish;
        set => _finish = value;
    }

    // Constants
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    private const float kItemBoxGroupX = -724f;
    private const float kItemBoxDuration = 1f;

    private void Start()
    {
        _inputController = FindAnyObjectByType<InputController>();
        
        StartCoroutine(FinishTask());
    }

    /// <summary>
    /// 과일을 모두 스틱에 넣었을 때 처리
    /// </summary>
    /// <returns></returns>
    private IEnumerator FinishTask()
    {
        yield return new WaitUntil(() => _finish);

        // 아이템 박스를 왼쪽으로 이동하도록 처리
        ItemBoxGroup.DOMoveX(kItemBoxGroupX, kItemBoxDuration).SetEase(Ease.OutSine).Play().SetLink(gameObject)
            .SetAutoKill();

        yield return new WaitForSeconds(0.5f);

        // 스틱 애니메이션 재생 및 별 FX 활성화
        Stick.SetTrigger(Trigger);
        StarFX.SetActive(true);

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
        Generate((EFruitType)fruitType);
    }

    /// <summary>
    /// 지정된 유형의 과일을 생성합니다.
    /// </summary>
    /// <param name="fruitType">생성할 과일의 유형입니다.</param>
    public void Generate(EFruitType fruitType)
    {
        // 과일 리스트에서 인자로 들어온 과일의 유형을 찾습니다.
        int targetIndex = -1;
        for (int i = 0; i < fruits.Length; i++)
        {
            if (fruits[i].FruitType == fruitType)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
            Assert.IsTrue(false, "해당 과일이 없습니다.");

        // 과일을 생성합니다.
        var fruit = Instantiate(fruits[targetIndex].Prefab, _inputController.TouchPosition, Quaternion.identity);
        fruit.transform.SetParent(TargetCanvas.transform);
        fruit.transform.localScale = Vector3.one;

        // 과일의 위치를 초기 재설정하고, 초기화를 처리합니다.
        if (fruit.TryGetComponent(out FruitItem item))
        {
            fruit.transform.position = _inputController.TouchPosition + (Vector2)item.Offset;
            item.Init(_inputController, this);
        }

        // 과일의 거리 콜라이더를 설정합니다.
        if (fruit.TryGetComponent(out DistanceCollider distanceCollider))
        {
            distanceCollider.Target = TriggerStickIn;
        }
    }
}