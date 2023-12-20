using System;
using UniRx;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// 딜레이에 맞춰 정해진 양에 맞춰 회전하는 애니메이션을 처리합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Rotator02 : MonoBehaviour
    {
        public float RotateAmount = 10f;
        public float RotateDelay = 0.2f;

        private RectTransform _selfTransform;

        private void Start()
        {
            // 할당
            _selfTransform = GetComponent<RectTransform>();

            // UniRX Interval
            Observable.Interval(TimeSpan.FromSeconds(RotateDelay))
                .Subscribe(_ => _selfTransform.Rotate(Vector3.forward, RotateAmount))
                .AddTo(this);
        }
    }
}