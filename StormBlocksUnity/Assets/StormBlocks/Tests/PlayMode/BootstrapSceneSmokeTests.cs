using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using StormBlocks.Core;
using StormBlocks.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StormBlocks.Tests.PlayMode
{
    public sealed class BootstrapSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator BootstrapViewBuildsBoardCameraAndHud()
        {
            var scene = SceneManager.CreateScene("BootstrapSceneSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Test Root");
            var view = root.AddComponent<StormBlocksBootstrapView>();

            view.Build();
            yield return null;

            Assert.IsNotNull(Camera.main);
            Assert.IsNotNull(GameObject.Find("Readable 8x8 Storm Board"));
            Assert.IsNotNull(GameObject.Find("Warm Central Rescue Camp"));
            Assert.IsNotNull(GameObject.Find("Storm Blocks Portrait HUD"));
        }

        [UnityTest]
        public IEnumerator PlayableViewStartsRunAndAcceptsPlacement()
        {
            var scene = SceneManager.CreateScene("PlayableSceneSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Playable Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(12345UL);
            yield return null;

            Assert.IsNotNull(view.State);
            Assert.IsNotNull(Camera.main);
            Assert.IsNotNull(GameObject.Find("Bottom Three Piece Tray"));
            Assert.IsNotNull(GameObject.Find("Storm Blocks Portrait HUD"));
            Assert.IsNotNull(GameObject.Find("Storm Blocks Safe Area"));
            Assert.IsNotNull(root.GetComponent<UnityShareService>());

            var result = view.TryPlaceForTest(0, new GridPosition(1, 1));
            yield return null;

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.GreaterOrEqual(view.State.Placements, 1);
        }

        [UnityTest]
        public IEnumerator PlayableViewExposesLaunchScreensProgressionAndSettings()
        {
            var scene = SceneManager.CreateScene("LaunchScreensSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Screens Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(67890UL);
            yield return null;

            Click("MENU Mode Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Endless Storm Button"));
            Assert.IsNotNull(GameObject.Find("Storm Trail Button"));
            Assert.IsNotNull(GameObject.Find("Tempest Trials Button"));
            Assert.IsNotNull(GameObject.Find("Settings Button"));
            Assert.IsNotNull(GameObject.Find("Accessibility Button"));
            Assert.IsNotNull(GameObject.Find("Credits Button"));

            Click("Storm Trail Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Next Level Button"));
            Assert.IsNotNull(GameObject.Find("Modes Button"));

            Click("Modes Button");
            yield return null;
            Click("Tempest Trials Button");
            yield return null;

            AssertButtonStartingWith("Run 1");
            Assert.IsNotNull(GameObject.Find("Next Run Button"));

            Click("Modes Button");
            yield return null;
            Click("Settings Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Music On Button"));
            Assert.IsNotNull(GameObject.Find("High Contrast Off Button"));
            Assert.IsNotNull(GameObject.Find("Large Text Off Button"));
            Assert.IsNotNull(GameObject.Find("Accessibility Button"));

            Click("Accessibility Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Reduced Motion Off Button"));
            Assert.IsNotNull(GameObject.Find("Color Safe Off Button"));
            Assert.IsNotNull(GameObject.Find("Low Detail Off Button"));
            Assert.IsNotNull(GameObject.Find("Settings Button"));

            Click("Modes Button");
            yield return null;
            Click("Credits Button");
            yield return null;

            AssertTextContains("Original puzzle game");

            Click("Modes Button");
            yield return null;
            Click("Profile Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Leaderboards Button"));

            Click("Achievements Button");
            yield return null;

            Assert.IsNotNull(GameObject.Find("Game Center Button"));
            Assert.IsNotNull(view.State);
        }

        [UnityTest]
        public IEnumerator PlayableViewShowsResultsAndRetriesAfterGameOver()
        {
            var scene = SceneManager.CreateScene("ResultsRetrySmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Retry Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(112233UL);
            yield return null;

            view.State.Board.SetOccupant(new GridPosition(3, 3), CellOccupant.Storm, string.Empty);
            var origin = FindValidOrigin(view.State, 0);
            var result = view.TryPlaceForTest(0, origin);
            yield return null;

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsTrue(result.GameOver);
            Assert.IsNotNull(GameObject.Find("Storm Blocks Screen Layer"));
            Assert.IsNotNull(GameObject.Find("Retry Button"));

            Click("Retry Button");
            yield return null;

            Assert.IsNotNull(view.State);
            Assert.IsFalse(view.State.IsGameOver);
            Assert.IsNull(GameObject.Find("Retry Button"));
        }

        [UnityTest]
        public IEnumerator NormalFlowDoesNotEmitGameErrors()
        {
            var scene = SceneManager.CreateScene("NoConsoleErrorsSmoke");
            SceneManager.SetActiveScene(scene);
            var capturedErrors = new List<string>();
            Application.LogCallback handler = delegate(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    capturedErrors.Add(type + ": " + condition + "\n" + stackTrace);
                }
            };

            Application.logMessageReceived += handler;
            try
            {
                var root = new GameObject("Storm Blocks Normal Flow Root");
                var view = root.AddComponent<StormBlocksGameView>();

                view.StartEndlessForTest(515151UL);
                yield return null;

                var result = view.TryPlaceForTest(0, FindValidOrigin(view.State, 0));
                yield return null;
                Assert.IsTrue(result.Success, result.FailureReason);

                Click("MENU Mode Button");
                yield return null;
                Click("Storm Trail Button");
                yield return null;
                Click("Modes Button");
                yield return null;
                Click("Tempest Trials Button");
                yield return null;
                Click("Modes Button");
                yield return null;
                Click("Settings Button");
                yield return null;
                Click("Modes Button");
                yield return null;
                Click("Profile Button");
                yield return null;
                Click("Achievements Button");
                yield return null;

                Assert.IsEmpty(capturedErrors, string.Join("\n", capturedErrors));
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }
        }

        [UnityTest]
        public IEnumerator ActiveTouchControlsStayInsideSafeAreaWithReleaseSizedTargets()
        {
            var scene = SceneManager.CreateScene("SafeAreaTouchSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Safe Area Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(616161UL);
            yield return null;

            var safeArea = GameObject.Find("Storm Blocks Safe Area");
            Assert.IsNotNull(safeArea);
            var safeTransform = safeArea.transform;
            AssertActiveButtonsAreSafe(safeTransform);

            Click("MENU Mode Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click("Storm Trail Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click("Modes Button");
            yield return null;
            Click("Settings Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click("Modes Button");
            yield return null;
            Click("Profile Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);
        }

        [UnityTest]
        public IEnumerator PlayableViewStaysWithinMobileSceneBudgets()
        {
            var scene = SceneManager.CreateScene("MobileBudgetSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Budget Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(24680UL);
            yield return null;

            int rendererCount = root.GetComponentsInChildren<Renderer>(false).Length;
            int triangleCount = CountTriangles(root);
            int audioListeners = root.GetComponentsInChildren<AudioListener>(false).Length;
            int canvases = root.GetComponentsInChildren<Canvas>(false).Length;

            Debug.Log("Storm Blocks mobile budget renderers=" + rendererCount + " triangles=" + triangleCount + " audioListeners=" + audioListeners + " canvases=" + canvases);
            Assert.LessOrEqual(rendererCount, 340);
            Assert.LessOrEqual(triangleCount, 250000);
            Assert.AreEqual(1, audioListeners);
            Assert.LessOrEqual(canvases, 1);
        }

        private static void Click(string objectName)
        {
            var buttonObject = GameObject.Find(objectName);
            Assert.IsNotNull(buttonObject, objectName);
            var button = buttonObject.GetComponent<Button>();
            Assert.IsNotNull(button, objectName);
            button.onClick.Invoke();
        }

        private static void AssertButtonStartingWith(string prefix)
        {
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].gameObject.name.StartsWith(prefix))
                {
                    return;
                }
            }

            Assert.Fail("Missing button starting with " + prefix);
        }

        private static void AssertTextContains(string fragment)
        {
            var labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);
            for (int i = 0; i < labels.Length; i++)
            {
                if (!string.IsNullOrEmpty(labels[i].text) && labels[i].text.Contains(fragment))
                {
                    return;
                }
            }

            Assert.Fail("Missing text containing " + fragment);
        }

        private static void AssertActiveButtonsAreSafe(Transform safeTransform)
        {
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude);
            Assert.Greater(buttons.Length, 0);

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                Assert.IsTrue(button.transform.IsChildOf(safeTransform), button.gameObject.name + " is outside the safe-area root.");

                var rect = button.GetComponent<RectTransform>();
                Assert.IsNotNull(rect, button.gameObject.name);
                Assert.GreaterOrEqual(rect.rect.width, 120f, button.gameObject.name + " width is below release touch target.");
                Assert.GreaterOrEqual(rect.rect.height, 68f, button.gameObject.name + " height is below release touch target.");

                var label = button.GetComponentInChildren<Text>();
                Assert.IsNotNull(label, button.gameObject.name + " is missing a visible label.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(label.text), button.gameObject.name + " has an empty label.");
            }
        }

        private static GridPosition FindValidOrigin(StormRunState state, int queueIndex)
        {
            var piece = state.Queue[queueIndex];
            for (int y = 0; y < state.Board.Size; y++)
            {
                for (int x = 0; x < state.Board.Size; x++)
                {
                    var origin = new GridPosition(x, y);
                    if (PlacementRules.CanPlace(state.Board, piece, origin))
                    {
                        return origin;
                    }
                }
            }

            Assert.Fail("No valid origin for queued piece " + piece.Id);
            return new GridPosition(0, 0);
        }

        private static int CountTriangles(GameObject root)
        {
            int triangles = 0;
            var filters = root.GetComponentsInChildren<MeshFilter>(false);
            for (int i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh != null)
                {
                    triangles += mesh.triangles.Length / 3;
                }
            }

            return triangles;
        }
    }
}
