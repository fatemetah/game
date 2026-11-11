using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Tools > Build Android APK
// Sets up the Android settings (name, icon, portrait, 64-bit) and builds SugarRush.apk
// into the "Builds" folder next to the project.
public static class AndroidBuilder
{
    [MenuItem("Tools/Build Android APK")]
    public static void Build()
    {
        // --- app identity
        PlayerSettings.companyName = "Fatemeh";
        PlayerSettings.productName = "Sugar Rush";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.fatemeh.sugarrush");
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.bundleVersionCode = 1;

        // --- phone settings
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;   // .apk, not .aab

        // --- icon
        const string iconPath = "Assets/UI/AppIcon.png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(iconPath);
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Default;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });
        }

        // --- scenes: menu first, then the level
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level_1.unity", true),
        };

        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds"));
        Directory.CreateDirectory(outDir);
        string apk = Path.Combine(outDir, "SugarRush.apk");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Menu.unity", "Assets/Scenes/Level_1.unity" },
            locationPathName = apk,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("APK READY: " + apk + "  (" + (report.summary.totalSize / (1024 * 1024)) + " MB)");
            EditorUtility.RevealInFinder(apk);
        }
        else
        {
            Debug.LogError("APK build failed: " + report.summary.result + " — see the errors above.");
        }
    }
}
