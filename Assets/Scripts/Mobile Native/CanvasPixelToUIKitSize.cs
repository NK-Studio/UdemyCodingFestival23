using System;
using NKStudio;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPixelToUIKitSize : MonoBehaviour
{
    private CanvasScaler _canvasScaler;

    private void Start()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        Resize();
    }

    private void Resize()
    {
        if (Application.isEditor)
            return;

        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

#if UNITY_ANDROID
        _canvasScaler.scaleFactor = Screen.dpi / 160;
#elif UNITY_IPHONE
        _canvasScaler.scaleFactor = MobileNative.GetNativeScaleFactor();
#endif
    }
}
