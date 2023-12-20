using UnityEngine;

public class Test01 : MonoBehaviour
{
    public RectTransform Point;
    private RectTransform _selfRectTransform;

    // Start is called before the first frame update
    void Start()
    {
        _selfRectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // // 자신의 높이의 절반을 구합니다.
        float halfSelfHeight = _selfRectTransform.sizeDelta.y/2;
        //
        // // 살짝 겹쳐 보이도록 처리
        // const float kOffset = 30f;
        //
        // // 위치 수정
        // Point.localPosition = _selfRectTransform.localPosition + Vector3.up * (halfSelfHeight - kOffset);

        if (_selfRectTransform.localPosition.y - halfSelfHeight < Point.localPosition.y)
        {
            Debug.Log("이하입니다.");
        }
    }
}
