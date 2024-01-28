using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class DistanceCollider : MonoBehaviour
{
    public bool SelfTransform = true;

    [ConditionalField(nameof(SelfTransform), true)]
    public Transform TargetTransform;

    public RectTransform Target;

    public float DistanceThreshold = 0.1f;

    public UnityEvent OnEnter;
    public UnityEvent OnStay;
    public UnityEvent OnExit;

    public bool DebugMode;

    private bool _isTouching;

    private void Update()
    {
        if (Target != null)
        {
            float distance;

            if (SelfTransform)
                distance = Vector3.Distance(Target.position, transform.position);
            else
                distance = Vector3.Distance(Target.position, TargetTransform.position);

            if (DebugMode)
                Debug.Log("Unity Log : " + distance);

            // 영역 안에 들어왔을 때
            if (distance < DistanceThreshold)
            {
                // 연속적인 처리를 막기 위한 조건
                if (!_isTouching)
                {
                    _isTouching = true;
                    OnEnter?.Invoke();
                }

                // 연속적 트리거
                OnStay?.Invoke();
            }
            // 영역 밖에 나갔을 때
            else
            {
                // 닿았던 적이 있다면 Exit 동작
                if (_isTouching)
                {
                    _isTouching = false;
                    OnExit?.Invoke();
                }
            }
        }
    }
}