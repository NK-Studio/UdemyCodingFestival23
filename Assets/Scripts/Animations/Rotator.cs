using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MyBox;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Animation
{
    /// <summary>
    /// 회전에 대한 애니메이션을 처리합니다.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [field: SerializeField] public bool Interaction { get; set; } = true;
        public bool SelfTarget = true;

        [ConditionalField(nameof(SelfTarget), true)]
        public Transform Target;

        public Vector3 TargetAngle;
        public float Duration = 1;
        public RotateMode RotateType;
        
        [SearchableEnum]
        public Ease EaseType = Ease.Linear;
        public UnityEvent OnComplete;

        private Vector3 _baseAngle;
        private RectTransform _selfTransform;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _sequence;

        private void Start()
        {
            // 초기 값을 저장하는 처리
            if (SelfTarget)
            {
                if (TryGetComponent(out _selfTransform))
                    _baseAngle = _selfTransform.localEulerAngles;
            }
            else
            {
                if (Target)
                    _baseAngle = Target.localEulerAngles;
                else
                    Assert.IsNotNull(Target, "Target is null");
            }
        }

        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        public TweenerCore<Quaternion, Vector3, QuaternionOptions> DoPlay()
        {
            if (!Interaction)
                return null;

            _sequence?.Kill();

            if (SelfTarget)
                _sequence = _selfTransform.DORotate(TargetAngle, Duration, RotateType).SetEase(EaseType).Play().OnComplete(() => OnComplete?.Invoke()).SetLink(gameObject).SetAutoKill();
            else
                _sequence = Target.DORotate(TargetAngle, Duration, RotateType).SetEase(EaseType).Play().OnComplete(() => OnComplete?.Invoke()).SetLink(gameObject).SetAutoKill();

            return _sequence;
        }
        
        /// <summary>
        /// 애니메이션을 역재생합니다.
        /// </summary>
        public TweenerCore<Quaternion, Vector3, QuaternionOptions> DoReverse()
        {
            if (!Interaction)
                return null;

            _sequence?.Kill();

            if (SelfTarget)
                _sequence = _selfTransform.DORotate(_baseAngle, Duration, RotateType).SetEase(EaseType).Play().SetLink(gameObject).SetAutoKill();
            else
                _sequence = Target.DORotate(_baseAngle, Duration, RotateType).SetEase(EaseType).Play().SetLink(gameObject).SetAutoKill();

            return _sequence;
        }

        /// <summary>
        /// 시퀀스를 종료합니다.
        /// </summary>
        public void Kill()
        {
            _sequence?.Kill();
        }
        
        #region Other
        /// <summary>
        /// 테스트 재생을 처리합니다.
        /// </summary>
        public void Play()
        {
            DoPlay();
        }
        
        /// <summary>
        /// 테스트 역재생을 처리합니다.
        /// </summary>
        public void Reverse()
        {
            DoReverse();
        }
        #endregion
    }
}
