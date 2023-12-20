using System;
using Animation;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Lever : MonoBehaviour, IPointerDownHandler
{
    [field: SerializeField] public bool Interaction { get; set; } = true;

    [Header("Animation")]
    public Rotator OnRotator;
    public Rotator OffRotator;

    [Header("GFX")]
    public Image Target;

    [Header("Sprite")]
    public Sprite Normal;
    public Sprite Highlight;

    public bool ShowDebug;

    private AnimationSequencerController _boundSequence;
    private PotSystem _potSystem;
    private Subject<Unit> _onPointerDown = new();
    private IObservable<Unit> OnPointerDownAsObservable => _onPointerDown;

    private void Start()
    {
        // 할당
        _boundSequence = GetComponent<AnimationSequencerController>();
        _potSystem = FindAnyObjectByType<PotSystem>();
        
        // Interaction의 상태에 따른 활성화 처리
        this.UpdateAsObservable()
            .ObserveEveryValueChanged(_ => Interaction)
            .Subscribe(active => {
                if (active)
                    Enable();
                else
                    Disable();
            })
            .AddTo(this);

        // 레버를 누르면 설탕을 녹이는 애니메이션을 1회 재생합니다.
        OnPointerDownAsObservable
            .Where(_ => Interaction)
            .Take(1)
            .Subscribe(_ => _potSystem.MeltedSugarPlay())
            .AddTo(this);
    }

    protected virtual void Enable()
    {
        _boundSequence.Play();
        Target.sprite = Highlight;

        if (ShowDebug)
            Debug.Log("Enable");
    }

    /// <summary>
    /// 설탕을 비활성화합니다.
    /// </summary>
    protected virtual void Disable()
    {
        _boundSequence.Kill();
        Target.sprite = Normal;

        if (ShowDebug)
            Debug.Log("Disable");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _onPointerDown.OnNext(Unit.Default);
    }

    /// <summary>
    /// 점화하는 애니메이션을 실행합니다.
    /// </summary>
    /// <returns></returns>
    public TweenerCore<Quaternion, Vector3, QuaternionOptions> OnGas()
    {
        var sequence = OnRotator.Play(); 
        return sequence;
    }

    /// <summary>
    /// 소화하는 애니메이션을 실행합니다.
    /// </summary>
    /// <returns></returns>
    public TweenerCore<Quaternion, Vector3, QuaternionOptions> OffGas()
    {
        var sequence = OffRotator.Play(); 
        return sequence;
    }

    /// <summary>
    /// Bound 애니메이션을 중지합니다.
    /// </summary>
    public void BounceAnimationKill()
    {
        _boundSequence.Kill();
        _boundSequence.transform.localScale = Vector3.one;
    }
}
