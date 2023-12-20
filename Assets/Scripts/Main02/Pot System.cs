using System.Collections;
using AutoSingleton;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using NKStudio;
using TMPro;
using UniRx;
using UnityEngine;

// 설탕이 부어지는 양과 물이 부어지는 양에 맞춰서 성공으로 처리할 것이다.
// 설탕이 60~70% 부어지고 나머지가 물이면 성공이다. 반대라면 실패하도록 처리
public class PotSystem : MonoBehaviour
{
    [Header("RectTransform")]
    public RectTransform Sugar;
    public RectTransform Water;
    
    [Header("GameObject")]
    public GameObject Fires;
    public GameObject FruitStick;
    
    [Header("애니메이션")]
    public AnimationSequencerController MeltingSugarSequence;
    [SerializeField] private Animator stickAnimator; // 스틱 애니메이터
    public float PourSpeed = 0.1f; // 채워지는 속도
    
    [Header("결과 배경")]
    [SerializeField] private GameObject[] resultBackgrounds;

    [Header("가스레인지 사운드"), SerializeField]
    private AudioSource gasRangeAudioSource;
    
    [Header("안내 텍스트")]
    [SerializeField] private TextMeshProUGUI InfoText;
    public string[] InfoTexts;
    
    public bool Pouring { get; set; }
    
    private FloatReactiveProperty _pourAmount;
    private readonly Vector2 PotFillRange = new Vector2(-630, 10); // 냄비 채우기 범위를 정의하는 상수 벡터
    
    private Sugar _sugar;
    private WaterPot _waterPot;
    private Lever _lever;
    private SceneLoader _sceneLoader;
    
    // Constants
    private const float BestSugarAmount = 0.6f;
    
    private void Awake()
    {
        _pourAmount = new FloatReactiveProperty(0);
    }
    
    private void Start()
    {
        // 계층 구조에서 설탕 클래스를 가진 녀석을 아무나 찾아서 가져온다.
        _sugar = FindAnyObjectByType<Sugar>();
        _waterPot = FindAnyObjectByType<WaterPot>();
        _lever = FindAnyObjectByType<Lever>();
        _sceneLoader = FindAnyObjectByType<SceneLoader>();
        
        // 설탕을 부어주세요
        InfoText.text = InfoTexts[0];

        // 설탕과 물을 스왑하는 처리
        _pourAmount
            .Where(value => value > BestSugarAmount)
            .Take(1) // 1회 처리해라
            .Subscribe(_ =>
            {
                // 물을 부어주세요.
                InfoText.text = InfoTexts[1];
                
                // 설탕을 비활성화하고 물을 활성화 합니다.
                _sugar.Interaction = false;
                _waterPot.Interaction = true;
            })
            .AddTo(this);

        // 모두 채워지면 Water Pot를 비활성화 처리 및 애니메이션 적용
        _pourAmount
            .Where(value => value >= 1.0f)
            .Take(1)
            .Subscribe(_ =>
            {
                // 물을 비활성화하고 레버를 활성화 합니다.
                _waterPot.Interaction = false;
                _lever.Interaction = true;

                // 설탕과 물을 안보이도록 처리
                _sugar.gameObject.SetActive(false);
                _waterPot.gameObject.SetActive(false);
            })
            .AddTo(this);
    }

    private void Update()
    {
        // 부어지는 양을 계산합니다.
        if (Pouring)
            _pourAmount.Value += PourSpeed * Time.deltaTime;

        // 0~1 사이의 값으로 제한
        _pourAmount.Value = Mathf.Clamp01(_pourAmount.Value);

        // 설탕은 BestSugarAmount(60%) 이상까지만 처리되고, 그 다음 물을 채워준다.
        if (_pourAmount.Value <= BestSugarAmount)
        {
            var sugarY = Mathf.Lerp(PotFillRange.x, PotFillRange.y, _pourAmount.Value);
            Sugar.anchoredPosition = new Vector2(0, sugarY);
        }
        else
        {
            var waterY = Mathf.Lerp(PotFillRange.x, PotFillRange.y, _pourAmount.Value);
            Water.anchoredPosition = new Vector2(0, waterY);
        }
    }
    
    /// <summary>
    /// 탕후루가 완성되어 연출을 처리함.
    /// </summary>
    public void ShowFinish()
    {
        stickAnimator.enabled = true;

        foreach (GameObject resultBackground in resultBackgrounds)
            resultBackground.SetActive(true);
    }

    /// <summary>
    /// 홈으로 이동합니다.
    /// </summary>
    public void GoHome()
    {
        Singleton.Instance<GameManager>().Fruits.Clear();
        _sceneLoader.ActivateLoadedScene();
    }
    
    /// <summary>
    /// 설탕 녹이는 연출 재생
    /// </summary>
    public void MeltedSugarPlay()
    {
        // 설탕이 녹는 연출을 재생합니다.
        StartCoroutine(MeltedSugarTask());
    }
    
    private IEnumerator MeltedSugarTask()
    {
        // 가스레인지 사운드 재생
        gasRangeAudioSource.Play();
        
        // 바운스를 끕니다.
        _lever.BounceAnimationKill();
        
        // 설탕을 녹이기 위한 레버를 켭니다.
        var gasOnSequence = _lever.OnGas();
        yield return gasOnSequence.WaitForCompletion();

        // 불을 점화합니다.
        Fires.SetActive(true);

        // 1초 정도 기다리기
        yield return new WaitForSeconds(1f);

        // 설탕 녹는 애니메이션 실행
        yield return MeltingSugarSequence.PlayEnumerator();

        // 1초 정도 기다리기
        yield return new WaitForSeconds(1f);

        // 설탕을 녹이기 위한 레버를 끕니다.
        var gasOffSequence = _lever.OffGas();
        yield return gasOffSequence.WaitForCompletion();

        // 레버 기능을 비활성화 합니다.
        _lever.Interaction = false;
        Fires.SetActive(false);

        // 과일 꼿아진 녀석 생성
        FruitStick.SetActive(true);
        
        // 탕후루를 좌우로 움직여주세요. (텍스트 변경)
        InfoText.text = InfoTexts[2];
    }
}