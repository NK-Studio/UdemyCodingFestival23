using BrunoMikoski.AnimationSequencer;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class MeltedFruit : MonoBehaviour
{
    public bool SelfTarget = true;

    [ShowIf("ConditionTarget")]
    public Image Target;

    public Sprite Normal;
    public Sprite CoatedFruit;
    
    [Header("Audio"), SerializeField]
    private AudioSource metedSoundSource;
    
    private float _coatingAmount;
    public float CoatingAmount
    {
        get => _coatingAmount;
        set
        {
            var newAmount = _coatingAmount + value;
            _coatingAmount = Mathf.Clamp01(newAmount);

            if (newAmount >= 1.0f)
                SetCoating(true);
        }
    }

    [UsedImplicitly] private bool ConditionTarget => SelfTarget == false;
    private AnimationSequencerController _animationSequencerController;

    private void Start()
    {
        // 할당
        _animationSequencerController = GetComponent<AnimationSequencerController>();
        
        if (SelfTarget)
            Target = GetComponent<Image>();
    }

    /// <summary>
    /// 설탕 코팅 적용 여부를 설정합니다.
    /// </summary>
    /// <param name="isCoating">true시 설탕 코팅을 적용합니다.</param>
    public void SetCoating(bool isCoating)
    {
        switch (isCoating)
        {
            case false :
                Target.sprite = Normal;
                break;
            case true :
                metedSoundSource.Play();
                Target.sprite = CoatedFruit;
                break;
        }

        Target.SetNativeSize();
        _animationSequencerController.Play();
    }
}
