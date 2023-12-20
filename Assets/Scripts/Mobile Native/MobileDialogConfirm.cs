using UnityEngine;
using System;

namespace NKStudio
{
    public class MobileDialogConfirm : MonoBehaviour
    {
        #region PUBLIC_VARIABLES

        public Action yesAction;
        public Action noAction;
        public string title;
        public string message;
        public string yes;
        public string no;
        public string urlString;

        #endregion

        #region PUBLIC_FUNCTIONS

        // Constructor
        public static MobileDialogConfirm Create(string title, string message, string yes, string no, Action yesAction, Action noAction)
        {
            MobileDialogConfirm dialog = new GameObject("MobileDialogConfirm").AddComponent<MobileDialogConfirm>();
            dialog.title = title;
            dialog.message = message;
            dialog.yes = yes;
            dialog.no = no;
            dialog.yesAction = yesAction;
            dialog.noAction = noAction;
            dialog.Init();
            return dialog;
        }

        private void Init()
        {
            MobileNative.ShowDialogConfirm(title, message, yes, no);
        }

        #endregion
        
        public void OnYesCallBack(string message)
        {
            if (yesAction != null)
            {
                yesAction();
            }
            Destroy(gameObject);
        }
        public void OnNoCallBack(string message)
        {
            if (noAction != null)
            {
                noAction();
            }
            Destroy(gameObject);
        }

    }
}