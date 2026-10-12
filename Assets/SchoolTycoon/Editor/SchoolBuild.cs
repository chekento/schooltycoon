using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KoSch.SchoolTycoon.Editor
{
    public static class SchoolBuild
    {
        public const string Scene = "Assets/Scenes/Campus.unity";
        [MenuItem("Rektor/Open Campus")]
        public static void Open() { EditorSceneManager.OpenScene(Scene); }

        [MenuItem("Rektor/Build/Windows")]
        public static void Windows() { Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Rektor.exe"); }
        [MenuItem("Rektor/Build/Linux")]
        public static void Linux() { Build(BuildTarget.StandaloneLinux64, "Builds/Linux/Rektor.x86_64"); }
        [MenuItem("Rektor/Build/Android APK")]
        public static void Android()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft=PlayerSettings.allowedAutorotateToLandscapeRight=true;
            PlayerSettings.allowedAutorotateToPortrait=PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.Android.bundleVersionCode=5;
            Build(BuildTarget.Android, "Builds/Android/Rektor-0.5.0.apk");
        }
        [MenuItem("Rektor/Build/WebGL")]
        public static void WebGL() { Build(BuildTarget.WebGL, "Builds/WebGL"); }

        public static void AndroidCI() { Android(); }

        private static void Build(BuildTarget target, string path)
        {
            PlayerSettings.companyName = "KoSch";
            PlayerSettings.productName = "Rektor";
            PlayerSettings.bundleVersion = "0.5.0";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target)), "cloud.kosch.rektor");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install the Unity build support module for " + target + " in Unity Hub.");
            string parent = Path.GetDirectoryName(path); if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { Scene }, locationPathName = path, target = target, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Rektor build failed: " + report.summary.result);
            Debug.Log("Rektor built: " + path);
        }
    }
}
