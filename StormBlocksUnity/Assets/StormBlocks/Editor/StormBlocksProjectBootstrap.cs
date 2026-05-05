using System.IO;
using System;
using StormBlocks.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace StormBlocks.Editor
{
    public static class StormBlocksProjectBootstrap
    {
        private const string ScenePath = "Assets/StormBlocks/Scenes/StormBlocksMain.unity";
        private const string RenderPipelineAssetPath = "Assets/StormBlocks/Art/Settings/StormBlocksMobileURP.asset";
        private const string UniversalRendererAssetPath = "Assets/StormBlocks/Art/Settings/StormBlocksMobileRenderer.asset";

        public static void ConfigureProject()
        {
            EnsureFolders();
            StormBlocksMarketingAssets.GenerateLaunchAssets();
            ConfigurePlayerSettings();
            ConfigureRenderPipeline();
            CreateMainScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/StormBlocks/Art/Settings");
            EnsureFolder("Assets/StormBlocks/Art/Generated");
            EnsureFolder("Assets/StormBlocks/Build");
            EnsureFolder("Assets/StormBlocks/Data/Configs");
            EnsureFolder("Assets/StormBlocks/Scenes");
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string name = Path.GetFileName(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Perlantir";
            PlayerSettings.productName = "Storm Blocks";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.perlantir.stormblocks");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, true);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.iOS, ApiCompatibilityLevel.NET_Standard);

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(StormBlocksMarketingAssets.IconAssetPath);
            if (icon != null)
            {
                foreach (IconKind kind in Enum.GetValues(typeof(IconKind)))
                {
                    int[] iconSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.iOS, kind);
                    if (iconSizes.Length > 0)
                    {
                        PlayerSettings.SetIcons(NamedBuildTarget.iOS, RepeatIcon(icon, iconSizes.Length), kind);
                    }
                }
            }
        }

        private static Texture2D[] RepeatIcon(Texture2D icon, int count)
        {
            var icons = new Texture2D[count];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = icon;
            }

            return icons;
        }

        private static void ConfigureRenderPipeline()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UniversalRendererAssetPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, UniversalRendererAssetPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(RenderPipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, RenderPipelineAssetPath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private static void CreateMainScene()
        {
            var sceneExists = File.Exists(ScenePath);
            var scene = sceneExists
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!sceneExists)
            {
                scene.name = "StormBlocksMain";
            }

            RenderSettings.ambientLight = new Color(0.16f, 0.17f, 0.30f);

            var root = FindLaunchRoot(scene);
            if (root == null)
            {
                root = new GameObject("Storm Blocks Launch Scene");
            }

            foreach (var candidate in scene.GetRootGameObjects())
            {
                if (candidate != root)
                {
                    UnityEngine.Object.DestroyImmediate(candidate);
                }
            }

            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }

            var views = root.GetComponents<StormBlocksGameView>();
            if (views.Length == 0)
            {
                root.AddComponent<StormBlocksGameView>();
            }
            else
            {
                for (int i = views.Length - 1; i > 0; i--)
                {
                    UnityEngine.Object.DestroyImmediate(views[i]);
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static GameObject FindLaunchRoot(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Storm Blocks Launch Scene")
                {
                    return root;
                }
            }

            return null;
        }
    }
}
