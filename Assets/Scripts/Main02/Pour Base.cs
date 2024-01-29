using System;
using Animation;
using BrunoMikoski.AnimationSequencer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NKStudio
{
    public abstract class PourBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool _interaction = true;
        
        [Tooltip("클릭을 제어합니다.")] 
        public bool Interaction = true;
        
        [SerializeField]
        protected GameObject PourObject;

        [SerializeField] 
        protected Rotator Rotate;
        
        [SerializeField]
        protected AnimationSequencerController Bounds;
        
        [SerializeField]
        protected bool ShowDebug;
        
        protected SelectableSpriteAnimation SelectableSpriteAnimation;
        protected PotSystem PotSystem;

        protected virtual void Start()
        {
            // 할당
            SelectableSpriteAnimation = GetComponent<SelectableSpriteAnimation>();
            PotSystem = FindAnyObjectByType<PotSystem>();

            // 설탕을 부을 때 연출 처리
            Rotate.OnComplete.AddListener(() => {
                PotSystem.Pouring = true;
                PourObject.SetActive(true);
            });
        }

        private void Update()
        {
            if (Interaction != _interaction)
            {
                _interaction = Interaction;
                
                if (_interaction)
                    Enable();
                else
                    Disable();
            }
        }

        /// <summary>
        /// 활성화합니다.
        /// </summary>
        protected virtual void Enable()
        {
            Bounds.Play(); // 바운스 애니메이션 재생
            SelectableSpriteAnimation.Interaction = true; // 스프라이트 스왑 애니메이션 재생 및 활성화 처리

            if (ShowDebug)
                Debug.Log("Enable");
        }

        /// <summary>
        /// 비활성화합니다.
        /// </summary>
        protected virtual void Disable()
        {
            // Transform 초기화
            Transform selfTransform = transform;
            selfTransform.rotation = Quaternion.identity; // 회전을 원래대로 돌립니다.
            selfTransform.localScale = Vector3.one; // 스케일을 원래대로 돌립니다.
            
            // Animation 초기화
            Bounds.Pause(); // 바운스 애니메이션 중지
            SelectableSpriteAnimation.Interaction = false; // 스프라이트 스왑 애니메이션 중지 및 비활성화 처리
            
            // 설탕 붓는 연출 초기화
            PourObject.SetActive(false); // 설탕 붓는 애니메이션 중지
            PotSystem.Pouring = false; // 설탕 붓는 연출 중지

            if (ShowDebug)
                Debug.Log("Disable");
        }
        
        protected virtual void OnTouchDown(PointerEventData eventData)
        {
            if (!Interaction)
                return;
            
            // 애니메이션을 재생합니다.
            Rotate.DoPlay();
            Bounds.Pause();
            
            // 스케일을 원래대로 돌립니다.
            transform.localScale = Vector3.one;

            if (ShowDebug)
                Debug.Log("Touch Down");
        }
        
        protected virtual void OnTouchUp(PointerEventData eventData)
        {
            if (!Interaction)
                return;

            // 애니메이션을 역재생합니다.
            Rotate.DoReverse(); // 애니메이션을 역재생합니다.
            Bounds.Play(); // 바운스 애니메이션 재생
            
            // 설탕 붓는 연출 초기화
            PourObject.SetActive(false); // 설탕 붓는 애니메이션 중지
            PotSystem.Pouring = false; // 설탕 붓는 연출 중지

            if (ShowDebug)
                Debug.Log("Touch Up");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnTouchDown(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            OnTouchUp(eventData);
        }
    }
}
