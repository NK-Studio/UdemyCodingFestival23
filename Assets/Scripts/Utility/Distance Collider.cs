using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class DistanceCollider : MonoBehaviour
{
    public bool SelfTransform = true;

    [ConditionalField(nameof(SelfTransform), false)]
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
                Debug.Log("PPAP : " + distance);

            if (distance < DistanceThreshold)
            {
                if (!_isTouching)
                {
                    _isTouching = true;
                    OnEnter?.Invoke();
                }

                OnStay?.Invoke();
            }
            else
            {
                if (_isTouching)
                {
                    _isTouching = false;
                    OnExit?.Invoke();
                }
            }
        }
    }
}