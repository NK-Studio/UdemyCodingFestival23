
using System;

namespace NKStudio
{
    public class NativeDialog
    {
        public NativeDialog() { }
        
        public static void OpenDialog(string title, string message, string yes, string no, Action yesAction = null, Action noAction = null)
        {
            MobileDialogConfirm.Create(title, message, yes, no, yesAction, noAction);
        }
    }
}