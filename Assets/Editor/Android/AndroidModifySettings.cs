using UnityEditor.Android;
using Unity.Android.Gradle.Manifest;

public class AndroidModifySettings : AndroidProjectFilesModifier
{
    public override void OnModifyAndroidProjectFiles(AndroidProjectFiles projectFiles)
    {
        var usesPermission = new UsesPermission();
        projectFiles.UnityLibraryManifest.Manifest.UsesPermissionList.AddElement(usesPermission);
        usesPermission.Attributes.Name.Set("android.permission.VIBRATE");
    }
}