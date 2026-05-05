using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace StormBlocks.Editor
{
    public static class StormBlocksBuildPipeline
    {
        private const string ScenePath = "Assets/StormBlocks/Scenes/StormBlocksMain.unity";
        private const string IOSOutputPath = "Builds/iOS/StormBlocks";
        private const string IOSSimulatorOutputPath = "Builds/iOSSimulator/StormBlocks";

        public static void BuildIOSDevelopment()
        {
            BuildIOS(IOSOutputPath, iOSSdkVersion.DeviceSDK);
        }

        public static void BuildIOSSimulatorDevelopment()
        {
            try
            {
                BuildIOS(IOSSimulatorOutputPath, iOSSdkVersion.SimulatorSDK);
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                AssetDatabase.SaveAssets();
            }
        }

#if UNITY_IOS
        [PostProcessBuild(1000)]
        public static void PostProcessIOS(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            string mainTarget = project.GetUnityMainTargetGuid();
            string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(unityFrameworkTarget, "GameKit.framework", false);
            project.WriteToFile(projectPath);

            var capabilities = new ProjectCapabilityManager(projectPath, "StormBlocks.entitlements", null, mainTarget);
            capabilities.AddGameCenter();
            capabilities.WriteToFile();
        }
#endif

        private static void ConfigureIOSSigningPlaceholders()
        {
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.iOS.appleDeveloperTeamID = "7JL22TDB44";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        }

        private static void BuildIOS(string relativeOutputPath, iOSSdkVersion sdkVersion)
        {
            StormBlocksProjectBootstrap.ConfigureProject();
            ConfigureIOSSigningPlaceholders();
            PlayerSettings.iOS.sdkVersion = sdkVersion;
            CleanGeneratedTestResources();

            string fullOutputPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativeOutputPath));
            if (Directory.Exists(fullOutputPath))
            {
                DeleteOutputDirectory(fullOutputPath);
            }

            Directory.CreateDirectory(fullOutputPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = fullOutputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("iOS export failed with result " + report.summary.result);
            }

            CleanGeneratedTestResources();
            AssetDatabase.SaveAssets();
        }

        private static void DeleteOutputDirectory(string fullOutputPath)
        {
            FileUtil.DeleteFileOrDirectory(fullOutputPath);
            if (!Directory.Exists(fullOutputPath))
            {
                return;
            }

            string retiredPath = fullOutputPath + ".old-" + DateTime.UtcNow.Ticks;
            Directory.Move(fullOutputPath, retiredPath);
            FileUtil.DeleteFileOrDirectory(retiredPath);
            if (Directory.Exists(retiredPath))
            {
                Directory.Delete(retiredPath, true);
            }
        }

        internal static void CleanGeneratedTestResources()
        {
            DeleteAssetIfPresent("Assets/Resources/PerformanceTestRunInfo.json");
            DeleteAssetIfPresent("Assets/Resources/PerformanceTestRunSettings.json");

            string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Resources");
            if (!Directory.Exists(resourcesPath))
            {
                return;
            }

            bool hasFiles = Directory.GetFiles(resourcesPath, "*", SearchOption.AllDirectories).Length > 0;
            bool hasDirectories = Directory.GetDirectories(resourcesPath, "*", SearchOption.AllDirectories).Length > 0;
            if (!hasFiles && !hasDirectories)
            {
                AssetDatabase.DeleteAsset("Assets/Resources");
            }
        }

        private static void DeleteAssetIfPresent(string assetPath)
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), assetPath)))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
    }

    internal sealed class StormBlocksBuildResourceSanitizer : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 10000; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            StormBlocksBuildPipeline.CleanGeneratedTestResources();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            StormBlocksBuildPipeline.CleanGeneratedTestResources();
        }
    }
}
