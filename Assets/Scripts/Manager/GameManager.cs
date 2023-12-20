using System.Collections.Generic;
using AutoSingleton;
using Data;
#if UNITY_ANDROID
using NKStudio;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

[ManagerDefaultPrefab("GameManager")]
public class GameManager : Singleton
{
    [SerializeField]
    private InputAction back;

    [ArrayElementTitle("FruitType")]
    public List<Fruit> Fruits = new();
    
    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        back.Enable();
#if UNITY_ANDROID
        back.performed += ShowExitDialog;
#endif
    }

    private void OnDisable()
    {
#if UNITY_ANDROID
        back.performed -= ShowExitDialog;
#endif
        back.Disable();
    }

#if UNITY_ANDROID
    private void ShowExitDialog(InputAction.CallbackContext ctx)
    {
        NativeDialog.OpenDialog(Application.productName, "정말로 게임을 종료 하시겠습니까?", "네", "아니요",
            Application.Quit,
            () => Debug.Log("취소"));
    }
#endif
    
    /// <summary>
    /// 마지막에 추가된 과일의 종류를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public EFruitType GetLastFruitType()
    {
        if (Fruits.Count == 0)
            return EFruitType.None;
        
        return Fruits[Fruits.Count - 1].FruitType;
    }
}
