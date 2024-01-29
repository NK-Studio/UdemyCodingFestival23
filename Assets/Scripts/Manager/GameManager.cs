using System.Collections.Generic;
using Data;
#if UNITY_ANDROID
using NKStudio;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using USingleton.AutoSingleton;

[Singleton(nameof(GameManager))]
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private InputAction back;

    [ArrayElementTitle("FruitType")]
    public List<FruitElement> Fruits = new();
    
    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        back.performed += ShowExitDialog;
        back.Enable();
    }

    private void OnDisable()
    {
        back.performed -= ShowExitDialog;
        back.Disable();
    }
    
    private void ShowExitDialog(InputAction.CallbackContext ctx)
    {
        Application.Quit();
    }

    /// <summary>
    /// 마지막에 추가된 과일의 종류를 반환합니다.
    /// </summary>
    /// <returns>마지막 과일 종류가 반환됩니다. 목록이 비어 있으면 EFruitType.None을 반환합니다.</returns>
    public EFruitType GetLastFruitType()
    {
        if (Fruits.Count == 0)
            return EFruitType.None;
        
        return Fruits[Fruits.Count - 1].FruitType;
    }
}
