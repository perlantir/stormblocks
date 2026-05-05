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
        public IEnumerator FirstMoveCoachTeachesWithNoTextAndDismissesAfterPlacement()
        {
            var scene = SceneManager.CreateScene("FirstMoveCoachSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Coach Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(24681UL);
            yield return null;

            var coach = FindChild(root, "Text-free First Move Coach");
            Assert.IsNotNull(coach);
            Assert.IsNotNull(FindChild(root, "Coach moving fingertip"));
            Assert.IsNotNull(FindChild(root, "Coach valid target glow"));
            Assert.AreEqual(0, coach.GetComponentsInChildren<Text>(false).Length);

            var result = view.TryPlaceForTest(0, FindValidOrigin(view.State, 0));
            yield return null;

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsNull(FindChild(root, "Coach moving fingertip"));
            Assert.AreEqual(0, coach.transform.childCount);
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

            Click(root, "MENU Mode Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Endless Storm Button"));
            Assert.IsNotNull(FindChild(root, "Storm Trail Button"));
            Assert.IsNotNull(FindChild(root, "Tempest Trials Button"));
            Assert.IsNotNull(FindChild(root, "Settings Button"));
            Assert.IsNotNull(FindChild(root, "Accessibility Button"));
            Assert.IsNotNull(FindChild(root, "Credits Button"));

            Click(root, "Storm Trail Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Next Level Button"));
            Assert.IsNotNull(FindChild(root, "Modes Button"));

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Tempest Trials Button");
            yield return null;

            AssertButtonStartingWith(root, "Run 1");
            Assert.IsNotNull(FindChild(root, "Next Run Button"));

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Settings Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Music On Button"));
            Assert.IsNotNull(FindChild(root, "High Contrast Off Button"));
            Assert.IsNotNull(FindChild(root, "Large Text Off Button"));
            Assert.IsNotNull(FindChild(root, "Accessibility Button"));

            Click(root, "Accessibility Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Reduced Motion Off Button"));
            Assert.IsNotNull(FindChild(root, "Color Safe Off Button"));
            Assert.IsNotNull(FindChild(root, "Low Detail Off Button"));
            Assert.IsNotNull(FindChild(root, "Settings Button"));

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Credits Button");
            yield return null;

            AssertTextContains(root, "Original puzzle game");

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Profile Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Leaderboards Button"));

            Click(root, "Achievements Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Game Center Button"));
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
            Assert.IsNotNull(FindChild(root, "Storm Blocks Screen Layer"));
            Assert.IsNotNull(FindChild(root, "Retry Button"));

            Click(root, "Retry Button");
            yield return null;

            Assert.IsNotNull(view.State);
            Assert.IsFalse(view.State.IsGameOver);
            Assert.IsNull(FindChild(root, "Retry Button"));
        }

        [UnityTest]
        public IEnumerator AutomaticPushbackSpawnsSignaturePerimeterRecoil()
        {
            var scene = SceneManager.CreateScene("PushbackSignatureSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Pushback Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(919191UL);
            yield return null;

            var result = StageSavedPushbackRow(view, 0);
            yield return null;

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsTrue(result.Clear.AutomaticPushbackTriggered, "Expected automatic pushback when clearing a row through a storm tile.");
            Assert.AreEqual(1, result.Clear.SurvivorsRescuedAt.Count);
            AssertTextContains(root, "Saved!");
            Assert.IsNotNull(FindChild(root, "Gold pushback wave row"));
            Assert.IsNotNull(FindChild(root, "Storm shatter flare"));
            Assert.IsNotNull(FindChild(root, "Pushback storm wall recoil north"));
            Assert.IsNotNull(FindChild(root, "Pushback storm wall recoil south"));
            Assert.IsNotNull(FindChild(root, "Pushback storm wall recoil west"));
            Assert.IsNotNull(FindChild(root, "Pushback storm wall recoil east"));
            Assert.IsNotNull(FindChild(root, "Saved rescue burst"));
            Assert.IsNotNull(FindChild(root, "Saved camp glow"));
        }

        [UnityTest]
        public IEnumerator AccessibilityReducedMotionAndLowDetailTrimSecondaryPushbackFx()
        {
            var scene = SceneManager.CreateScene("AccessibilityVfxSmoke");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Storm Blocks Accessibility VFX Root");
            var view = root.AddComponent<StormBlocksGameView>();

            view.StartEndlessForTest(717171UL);
            yield return null;

            Click(root, "MENU Mode Button");
            yield return null;
            Click(root, "Accessibility Button");
            yield return null;
            Click(root, "Reduced Motion Off Button");
            yield return null;
            Click(root, "Low Detail Off Button");
            yield return null;

            Assert.IsNotNull(FindChild(root, "Reduced Motion On Button"));
            Assert.IsNotNull(FindChild(root, "Low Detail On Button"));

            var result = StageSavedPushbackRow(view, 1);
            yield return null;

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsTrue(result.Clear.AutomaticPushbackTriggered);
            Assert.AreEqual(1, result.Clear.SurvivorsRescuedAt.Count);
            AssertTextContains(root, "Saved!");
            Assert.IsNotNull(FindChild(root, "Storm shatter flare"));
            Assert.IsNotNull(FindChild(root, "Pushback storm wall recoil north"));
            Assert.IsNotNull(FindChild(root, "Saved rescue burst"));
            Assert.IsNotNull(FindChild(root, "Saved camp glow"));
            Assert.IsNull(FindChild(root, "Gold pushback wave row"));
            Assert.IsNull(FindChild(root, "Cyan pushback wave row"));
            Assert.IsNull(FindChild(root, "Storm shatter lightning top"));
            Assert.IsNull(FindChild(root, "Pushback cyan recoil north"));
            Assert.IsNull(FindChild(root, "Block highlight dot"));
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

                Click(root, "MENU Mode Button");
                yield return null;
                Click(root, "Storm Trail Button");
                yield return null;
                Click(root, "Modes Button");
                yield return null;
                Click(root, "Tempest Trials Button");
                yield return null;
                Click(root, "Modes Button");
                yield return null;
                Click(root, "Settings Button");
                yield return null;
                Click(root, "Modes Button");
                yield return null;
                Click(root, "Profile Button");
                yield return null;
                Click(root, "Achievements Button");
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

            var safeArea = FindChild(root, "Storm Blocks Safe Area");
            Assert.IsNotNull(safeArea);
            var safeTransform = safeArea.transform;
            AssertActiveButtonsAreSafe(safeTransform);

            Click(root, "MENU Mode Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click(root, "Storm Trail Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Settings Button");
            yield return null;
            AssertActiveButtonsAreSafe(safeTransform);

            Click(root, "Modes Button");
            yield return null;
            Click(root, "Profile Button");
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
            Assert.LessOrEqual(rendererCount, 475);
            Assert.LessOrEqual(triangleCount, 250000);
            Assert.AreEqual(1, audioListeners);
            Assert.LessOrEqual(canvases, 1);
        }

        private static void Click(GameObject root, string objectName)
        {
            var buttonObject = FindChild(root, objectName);
            Assert.IsNotNull(buttonObject, objectName);
            var button = buttonObject.GetComponent<Button>();
            Assert.IsNotNull(button, objectName);
            button.onClick.Invoke();
        }

        private static GameObject FindChild(GameObject root, string objectName)
        {
            var transforms = root.GetComponentsInChildren<Transform>(false);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].gameObject.name == objectName)
                {
                    return transforms[i].gameObject;
                }
            }

            return null;
        }

        private static PlacementResult StageSavedPushbackRow(StormBlocksGameView view, int row)
        {
            view.State.Queue.Clear();
            view.State.Queue.Add(new PieceDefinition("single", new[] { new GridPosition(0, 0) }));
            for (int x = 0; x < view.State.Board.Size; x++)
            {
                view.State.Board.ClearCell(new GridPosition(x, row));
            }

            view.State.Board.SetOccupant(new GridPosition(0, row), CellOccupant.Storm, string.Empty);
            for (int x = 1; x < 7; x++)
            {
                view.State.Board.SetOccupant(new GridPosition(x, row), CellOccupant.Block, "setup");
            }

            view.State.Board.SetSurvivor(new GridPosition(1, row), true);
            return view.TryPlaceForTest(0, new GridPosition(7, row));
        }

        private static void AssertButtonStartingWith(GameObject root, string prefix)
        {
            var buttons = root.GetComponentsInChildren<Button>(false);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].gameObject.name.StartsWith(prefix))
                {
                    return;
                }
            }

            Assert.Fail("Missing button starting with " + prefix);
        }

        private static void AssertTextContains(GameObject root, string fragment)
        {
            var labels = root.GetComponentsInChildren<Text>(false);
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
            var buttons = safeTransform.GetComponentsInChildren<Button>(false);
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
