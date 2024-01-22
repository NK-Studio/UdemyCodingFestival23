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

        private float _tick;
        
        private void Start()
        {
            // 할당
            _selfTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _tick += Time.deltaTime;

            if (_tick >= RotateDelay)
            {
                _selfTransform.Rotate(Vector3.forward, RotateAmount);
                _tick = 0;
            }
        }
    }
}