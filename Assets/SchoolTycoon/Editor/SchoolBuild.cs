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
        [MenuItem("School Simulation/Open Campus")]
        public static void Open() { EditorSceneManager.OpenScene(Scene); }

        [MenuItem("School Simulation/Build/Windows")]
        public static void Windows() { Build(BuildTarget.StandaloneWindows64, "Builds/Windows/TheSchoolSimulation.exe"); }
        [MenuItem("School Simulation/Build/Linux")]
        public static void Linux() { Build(BuildTarget.StandaloneLinux64, "Builds/Linux/TheSchoolSimulation.x86_64"); }
        [MenuItem("School Simulation/Build/Android APK")]
        public static void Android()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            Build(BuildTarget.Android, "Builds/Android/TheSchoolSimulation-0.2.0.apk");
        }
        [MenuItem("School Simulation/Build/WebGL")]
        public static void WebGL() { Build(BuildTarget.WebGL, "Builds/WebGL"); }

        private static void Build(BuildTarget target, string path)
        {
            PlayerSettings.companyName = "KoSch";
            PlayerSettings.productName = "The School Simulation";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target)), "cloud.kosch.schoolsimulation");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install the Unity build support module for " + target + " in Unity Hub.");
            string parent = Path.GetDirectoryName(path); if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { Scene }, locationPathName = path, target = target, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("School Simulation build failed: " + report.summary.result);
            Debug.Log("School Simulation built: " + path);
        }
    }
}
