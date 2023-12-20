package com.unity3d.player

import android.app.AlertDialog
import android.os.VibrationEffect
import android.os.Vibrator

import com.unity3d.player.UnityPlayer
import com.unity3d.player.UnityPlayerActivity

class UPlugin : UnityPlayerActivity()
{
    private fun ShowDialogConfirm(title : String, message : String, yes : String, no: String, cancelable : Boolean)
    {
        AlertDialog.Builder(UnityPlayer.currentActivity)
            .setTitle(title)
            .setMessage(message)
            .setPositiveButton(yes) { _, _ ->
                UnityPlayer.UnitySendMessage("MobileDialogConfirm", "OnYesCallBack", "0");
            }
            .setNegativeButton(no) { _, _ ->
                UnityPlayer.UnitySendMessage("MobileDialogConfirm", "OnNoCallBack", "1");
            }
            .setCancelable(cancelable)
            .create()
            .show()
    }

    // 진동 호출
    private fun Vibrate(milliseconds : Long, amplitude : Int)
    {
        val vibrator = UnityPlayer.currentActivity.getSystemService(VIBRATOR_SERVICE) as Vibrator
        vibrator.vibrate(VibrationEffect.createOneShot(milliseconds, amplitude));
    }
}