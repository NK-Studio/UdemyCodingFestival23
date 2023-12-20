#if UNITY_IPHONE
using System;
using System.Runtime.InteropServices;
#endif

using UnityEngine;

namespace NKStudio
{
    public class MobileNative
    {

#if UNITY_IPHONE
        [DllImport ("__Internal")]
        private static extern void _Vibrate();
        
        [DllImport ("__Internal")]
         private static extern float _GetNativeScaleFactor();
        
#elif UNITY_ANDROID
        private static AndroidJavaObject _androidJavaObject;
#endif
        
        /// <summary>
        /// Android에서 기본 확인 대화 상자를 호출합니다. (Android Only)
        /// </summary>
        /// <param name="title">대화 제목 텍스트</param>
        /// <param name="message">대화 메시지 텍스트</param>
        /// <param name="yes">수락 버튼 텍스트</param>
        /// <param name="no">취소 버튼 텍스트</param>
        /// <param name="cancelable">안드로이드 전용. 대화 상자의 취소 가능한 속성을 설정할 수 있습니다.</param>
        public static void ShowDialogConfirm(string title, string message, string yes, string no,
            bool cancelable = true)
        {
#if UNITY_EDITOR

#else
    #if UNITY_IPHONE
                
    #elif UNITY_ANDROID
                if (_androidJavaObject == null) 
                    _androidJavaObject = new AndroidJavaObject("com.unity3d.player.UPlugin");

                _androidJavaObject.Call("ShowDialogConfirm",title, message, yes, no, cancelable);
    #endif
#endif
        }

        /// <summary>
        /// 진동을 발생합니다. (Both)
        /// </summary>
        /// <param name="milliseconds"></param>
        public static void Vibrate(long milliseconds , int amplitude)
        {
#if UNITY_EDITOR

#else
    #if UNITY_IPHONE
                _Vibrate();
    #elif UNITY_ANDROID
                if (_androidJavaObject == null) 
                    _androidJavaObject = new AndroidJavaObject("com.unity3d.player.UPlugin");

                _androidJavaObject.Call("Vibrate", milliseconds, amplitude);
    #endif
#endif
        }
        
        /// <summary>
        /// IOS의 네이티브 스케일 팩터를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public static float GetNativeScaleFactor()
        {
#if UNITY_EDITOR
        return 1f;
#else
#if UNITY_IPHONE
        return _GetNativeScaleFactor();
#else
        return 1f;
#endif
        
#endif

        }
    }
}