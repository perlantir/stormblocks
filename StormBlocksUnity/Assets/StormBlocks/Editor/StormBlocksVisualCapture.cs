using System;
using System.IO;
using StormBlocks.Core;
using StormBlocks.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StormBlocks.Editor
{
    public static class StormBlocksVisualCapture
    {
        private const string OutputPath = "Builds/VisualChecks/stormblocks-gameplay.png";
        private const string AppStoreOutputFolder = "Builds/AppStoreScreens";

        public static void CapturePortraitGameplay()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Storm Blocks Visual Capture Root");
            var view = root.AddComponent<StormBlocksGameView>();
            view.StartEndlessForTest(20260505UL);
            ConfigureSavedPushbackMoment(view);

            var camera = Camera.main;
            if (camera == null)
            {
                throw new MissingReferenceException("Storm Blocks visual capture could not find Main Camera.");
            }

            CaptureCamera(camera, OutputPath, true);
            AssetDatabase.Refresh();
        }

        public static void CaptureAppStoreScreenshots()
        {
            StormBlocksMarketingAssets.GenerateLaunchAssets();
            CaptureScenario("01_place_blocks_save_camp.png", ConfigureSavedPushbackMoment, true);
            CaptureScenario("02_beat_daily_storm.png", delegate(StormBlocksGameView view) { view.StartDaily(true); }, true);
            CaptureScenario("03_storm_trail_progression.png", delegate(StormBlocksGameView view)
            {
                ClickButton("MENU Mode Button");
                ClickButton("Storm Trail Button");
            }, true);
            CaptureScenario("04_tempest_trials_weekly.png", delegate(StormBlocksGameView view)
            {
                ClickButton("MENU Mode Button");
                ClickButton("Tempest Trials Button");
            }, true);
            CaptureScenario("05_cosmetic_profile.png", delegate(StormBlocksGameView view)
            {
                ClickButton("MENU Mode Button");
                ClickButton("Cosmetics Button");
            }, true);
            AssetDatabase.Refresh();
        }

        private static void CaptureScenario(string fileName, Action<StormBlocksGameView> setup, bool includeCanvas)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Storm Blocks App Store Capture Root");
            var view = root.AddComponent<StormBlocksGameView>();
            view.StartEndlessForTest(20260505UL);
            setup(view);

            var camera = Camera.main;
            if (camera == null)
            {
                throw new MissingReferenceException("Storm Blocks app-store capture could not find Main Camera.");
            }

            CaptureCamera(camera, Path.Combine(AppStoreOutputFolder, fileName), includeCanvas);
        }

        private static void ConfigureSavedPushbackMoment(StormBlocksGameView view)
        {
            const int row = 2;
            view.State.Queue.Clear();
            view.State.Queue.Add(new PieceDefinition("single", new[] { new GridPosition(0, 0) }));
            for (int x = 0; x < view.State.Board.Size; x++)
            {
                view.State.Board.ClearCell(new GridPosition(x, row));
            }

            view.State.Board.SetOccupant(new GridPosition(0, row), CellOccupant.Storm, string.Empty);
            for (int x = 1; x < 7; x++)
            {
                view.State.Board.SetOccupant(new GridPosition(x, row), CellOccupant.Block, "capture");
            }

            view.State.Board.SetSurvivor(new GridPosition(3, row), true);

            var result = view.TryPlaceForTest(0, new GridPosition(7, row));
            if (!result.Success || !result.Clear.AutomaticPushbackTriggered || result.Clear.SurvivorsRescuedAt.Count == 0)
            {
                throw new InvalidOperationException("Storm Blocks capture could not stage the saved pushback moment.");
            }
        }

        private static void CaptureCamera(Camera camera, string outputPath, bool includeCanvas)
        {
            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            CanvasState[] canvasStates = includeCanvas ? PrepareCanvases(camera) : new CanvasState[0];

            var texture = new Texture2D(1170, 2532, TextureFormat.RGB24, false);
            var renderTexture = new RenderTexture(1170, 2532, 24)
            {
                antiAliasing = 4
            };

            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            texture.ReadPixels(new Rect(0f, 0f, 1170f, 2532f), 0, 0);
            texture.Apply();
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RestoreCanvases(canvasStates);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static CanvasState[] PrepareCanvases(Camera camera)
        {
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>();
            var states = new CanvasState[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                states[i] = new CanvasState
                {
                    Canvas = canvases[i],
                    RenderMode = canvases[i].renderMode,
                    WorldCamera = canvases[i].worldCamera,
                    PlaneDistance = canvases[i].planeDistance
                };
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = camera;
                canvases[i].planeDistance = 2f;
            }

            return states;
        }

        private static void RestoreCanvases(CanvasState[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].Canvas == null)
                {
                    continue;
                }

                states[i].Canvas.renderMode = states[i].RenderMode;
                states[i].Canvas.worldCamera = states[i].WorldCamera;
                states[i].Canvas.planeDistance = states[i].PlaneDistance;
            }
        }

        private static void ClickButton(string objectName)
        {
            var buttonObject = GameObject.Find(objectName);
            if (buttonObject == null)
            {
                throw new MissingReferenceException("Storm Blocks capture could not find button: " + objectName);
            }

            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                throw new MissingComponentException("Storm Blocks capture object has no Button: " + objectName);
            }

            button.onClick.Invoke();
        }

        private struct CanvasState
        {
            public Canvas Canvas;
            public RenderMode RenderMode;
            public Camera WorldCamera;
            public float PlaneDistance;
        }
    }
}
