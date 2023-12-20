using JetBrains.Annotations;
using NaughtyAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Animation
{
    /// <summary>
    /// 클릭 액션 여부에 따라 스프라이트를 변경하는 애니메이션을 처리합니다.
    /// </summary>
    public class SelectableSpriteAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
        IPointerExitHandler
    {
        private enum EState
        {
            Normal,
            Press,
            Highlight,
            Disable
        }

        [field: SerializeField] public bool Interaction { get; set; } = true;
        
        public bool SelfTarget = true;

        [ShowIf("ConditionTarget")]
        public Image Target;
        
        public Sprite Normal;
        public Sprite Press;
        public Sprite Highlight;
        public Sprite Disable;

        public UnityEvent OnDown;
        public UnityEvent OnUp;
        public UnityEvent EnableAction;
        public UnityEvent DisableAction;
        
        [UsedImplicitly] private bool ConditionTarget => SelfTarget == false;
        private EState _state = EState.Normal;

        private void Start()
        {
            // Interaction의 값이 변경됨의 여부에 따라 처리
            this.UpdateAsObservable()
                .ObserveEveryValueChanged(_ => Interaction)
                .Subscribe(active => {

                    switch (active)
                    {
                        case true:
                            EnableAction?.Invoke();
                            _state = EState.Normal;
                            break;
                        
                        case false :
                            DisableAction?.Invoke();
                            _state = EState.Disable;
                            break;
                    }
                })
                .AddTo(this);
        }

        private void Update()
        {
            if (Interaction)
            {
                switch (_state)
                {
                    case EState.Normal:
                        if (Normal)
                            Target.sprite = Normal;
                        break;
                    case EState.Highlight:
                        if (Highlight)
                            Target.sprite = Highlight;
                        break;
                    case EState.Press:
                        if (Press)
                            Target.sprite = Press;
                        break;
                }
            }
            else
            {
                if (Disable)
                    Target.sprite = Disable;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown?.Invoke();
            _state = EState.Press;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUp?.Invoke();
            _state = EState.Normal;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_state != EState.Press)
                _state = EState.Highlight;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_state != EState.Press)
                _state = EState.Normal;
        }
    }
}