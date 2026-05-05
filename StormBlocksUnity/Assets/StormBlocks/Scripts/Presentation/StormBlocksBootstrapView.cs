using System.Collections.Generic;
using StormBlocks.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StormBlocks.Presentation
{
    public sealed class StormBlocksBootstrapView : MonoBehaviour
    {
        private const int BoardSize = 8;
        private const float CellPitch = 0.94f;
        private const float BoardOrigin = -3.5f * CellPitch;
        private static Font _cachedUiFont;

        [SerializeField] private bool rebuildOnStart = true;

        private readonly List<Material> _runtimeMaterials = new List<Material>();

        private Material _emptyTile;
        private Material _campTile;
        private Material _stormTile;
        private Material _stormCloud;
        private Material _warningTile;
        private Material _goldGlow;
        private Material _campOrange;
        private Material _survivorYellow;
        private Material _survivorBlue;
        private Material _uiPanel;
        private Material _blockCyan;
        private Material _blockCoral;
        private Material _blockLime;
        private Material _blockPurple;

        private void Start()
        {
            if (rebuildOnStart)
            {
                Build();
            }
        }

        [ContextMenu("Build Storm Blocks Bootstrap View")]
        public void Build()
        {
            ClearChildren();
            CreateMaterials();
            SetupCameraAndLights();
            BuildBackground();
            BuildBoard();
            BuildCamp();
            BuildSampleBlocks();
            BuildSurvivors();
            BuildPieceTray();
            BuildHud();
            BuildPushbackMoment();
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void CreateMaterials()
        {
            _runtimeMaterials.Clear();
            _emptyTile = CreateMaterial("SB Empty Tile", new Color(0.64f, 0.64f, 0.82f), 0.25f);
            _campTile = CreateMaterial("SB Warm Camp Tile", new Color(0.86f, 0.58f, 0.31f), 0.35f);
            _stormTile = CreateMaterial("SB Storm Tile", new Color(0.18f, 0.22f, 0.42f), 0.1f);
            _stormCloud = CreateMaterial("SB Storm Cloud", new Color(0.24f, 0.19f, 0.48f), 0.05f);
            _warningTile = CreateMaterial("SB Storm Warning", new Color(0.42f, 0.82f, 1.0f), 0.35f);
            _goldGlow = CreateMaterial("SB Pushback Gold", new Color(1.0f, 0.72f, 0.22f), 0.55f);
            _campOrange = CreateMaterial("SB Camp Orange", new Color(1.0f, 0.42f, 0.14f), 0.25f);
            _survivorYellow = CreateMaterial("SB Survivor Yellow", new Color(1.0f, 0.86f, 0.23f), 0.25f);
            _survivorBlue = CreateMaterial("SB Survivor Blue", new Color(0.12f, 0.72f, 1.0f), 0.25f);
            _uiPanel = CreateMaterial("SB UI Panel Purple", new Color(0.50f, 0.42f, 0.78f), 0.45f);
            _blockCyan = CreateMaterial("SB Block Cyan", new Color(0.04f, 0.74f, 0.95f), 0.55f);
            _blockCoral = CreateMaterial("SB Block Coral", new Color(1.0f, 0.34f, 0.30f), 0.5f);
            _blockLime = CreateMaterial("SB Block Lime", new Color(0.43f, 0.88f, 0.11f), 0.5f);
            _blockPurple = CreateMaterial("SB Block Purple", new Color(0.62f, 0.25f, 0.95f), 0.5f);
        }

        private Material CreateMaterial(string materialName, Color color, float smoothness)
        {
            var material = StormBlocksRuntimeMaterialFactory.Create(
                materialName,
                color,
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Sprites/Default",
                "Standard");

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            _runtimeMaterials.Add(material);
            return material;
        }

        private void SetupCameraAndLights()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.SetParent(transform);
            camera.transform.position = new Vector3(0f, 8.6f, -9.4f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 6.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.18f, 0.16f, 0.36f);
            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            var key = new GameObject("Warm Camp Key Light");
            key.transform.SetParent(transform);
            key.transform.position = new Vector3(0f, 4.6f, -2.8f);
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1f, 0.82f, 0.58f);
            keyLight.intensity = 1.45f;
            key.transform.rotation = Quaternion.Euler(52f, -22f, 0f);

            var storm = new GameObject("Cool Storm Rim Light");
            storm.transform.SetParent(transform);
            storm.transform.position = new Vector3(-3.8f, 3.0f, 1.2f);
            var stormLight = storm.AddComponent<Light>();
            stormLight.type = LightType.Point;
            stormLight.color = new Color(0.42f, 0.85f, 1f);
            stormLight.intensity = 3.0f;
            stormLight.range = 7.0f;
        }

        private void BuildBackground()
        {
            CreateCube("Soft dusk rescue backdrop", transform, new Vector3(0f, -0.08f, 0.15f), new Vector3(10.8f, 0.08f, 12.8f), CreateMaterial("SB Background Warm Dusk", new Color(0.44f, 0.35f, 0.58f), 0.2f));
            CreateCube("Warm oval camp glow", transform, new Vector3(0f, 0.02f, -0.2f), new Vector3(8.3f, 0.035f, 3.0f), CreateMaterial("SB Camp Ground Glow", new Color(1.0f, 0.62f, 0.22f), 0.6f));
        }

        private void BuildBoard()
        {
            var boardRoot = new GameObject("Readable 8x8 Storm Board");
            boardRoot.transform.SetParent(transform);

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    bool camp = (x == 3 || x == 4) && (y == 3 || y == 4);
                    bool storm = x == 0 || y == 0 || x == BoardSize - 1 || y == BoardSize - 1;
                    bool warning = (x == 1 && y == 0) || (x == 6 && y == 0) || (x == 0 && y == 6);
                    Material tileMaterial = camp ? _campTile : storm ? _stormTile : warning ? _warningTile : _emptyTile;
                    Vector3 center = CellCenter(x, y);

                    var baseTile = CreateCube("Tile " + x + "," + y, boardRoot.transform, center, new Vector3(0.82f, 0.14f, 0.82f), tileMaterial);
                    baseTile.transform.localPosition += Vector3.up * 0.02f;
                    CreateCube("Tile bevel top " + x + "," + y, baseTile.transform, new Vector3(0f, 0.085f, 0f), new Vector3(0.70f, 0.045f, 0.70f), tileMaterial);

                    if (storm)
                    {
                        BuildStormPuffs(boardRoot.transform, center, x, y);
                    }
                }
            }
        }

        private void BuildStormPuffs(Transform parent, Vector3 center, int x, int y)
        {
            if ((x + y) % 2 != 0)
            {
                return;
            }

            CreateSphere("Storm puff", parent, center + new Vector3(-0.17f, 0.28f, 0.04f), new Vector3(0.36f, 0.22f, 0.30f), _stormCloud);
            CreateSphere("Storm puff", parent, center + new Vector3(0.16f, 0.30f, 0.02f), new Vector3(0.42f, 0.25f, 0.34f), _stormCloud);
            CreateCube("Storm lightning", parent, center + new Vector3(0.05f, 0.32f, -0.22f), new Vector3(0.045f, 0.035f, 0.46f), _warningTile).transform.rotation = Quaternion.Euler(0f, 0f, 22f);
        }

        private void BuildCamp()
        {
            var campRoot = new GameObject("Warm Central Rescue Camp");
            campRoot.transform.SetParent(transform);
            campRoot.transform.position = new Vector3(0f, 0.24f, 0f);

            CreateCube("Tent body left fold", campRoot.transform, new Vector3(-0.17f, 0.24f, 0f), new Vector3(0.52f, 0.46f, 0.76f), _campOrange).transform.rotation = Quaternion.Euler(0f, 0f, -18f);
            CreateCube("Tent body right fold", campRoot.transform, new Vector3(0.17f, 0.24f, 0f), new Vector3(0.52f, 0.46f, 0.76f), _campOrange).transform.rotation = Quaternion.Euler(0f, 0f, 18f);
            CreateCube("Signal flag", campRoot.transform, new Vector3(0.48f, 0.78f, -0.28f), new Vector3(0.08f, 0.72f, 0.08f), _survivorYellow);
            CreateCube("Signal pennant", campRoot.transform, new Vector3(0.68f, 0.98f, -0.28f), new Vector3(0.34f, 0.16f, 0.06f), _campOrange);
            CreateSphere("Camp fire glow", campRoot.transform, new Vector3(-0.48f, 0.16f, -0.42f), new Vector3(0.35f, 0.18f, 0.35f), _goldGlow);
        }

        private void BuildSampleBlocks()
        {
            var blocksRoot = new GameObject("Toy Block Placement Samples");
            blocksRoot.transform.SetParent(transform);
            BuildBlock(blocksRoot.transform, _blockCyan, new[] { new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(1, 2) });
            BuildBlock(blocksRoot.transform, _blockLime, new[] { new GridPosition(5, 1), new GridPosition(5, 2), new GridPosition(6, 2) });
            BuildBlock(blocksRoot.transform, _blockPurple, new[] { new GridPosition(5, 5), new GridPosition(6, 5), new GridPosition(6, 6) });
            BuildBlock(blocksRoot.transform, _blockCoral, new[] { new GridPosition(2, 5), new GridPosition(2, 6) });
        }

        private void BuildBlock(Transform parent, Material material, IEnumerable<GridPosition> cells)
        {
            foreach (var cell in cells)
            {
                Vector3 center = CellCenter(cell.X, cell.Y) + Vector3.up * 0.18f;
                CreateCube("Chunky toy block", parent, center, new Vector3(0.72f, 0.30f, 0.72f), material);
                CreateSphere("Block highlight dot", parent, center + new Vector3(0.18f, 0.18f, -0.18f), new Vector3(0.10f, 0.035f, 0.10f), _goldGlow);
            }
        }

        private void BuildSurvivors()
        {
            var survivors = new GameObject("Tiny Survivor Characters");
            survivors.transform.SetParent(transform);
            BuildSurvivor(survivors.transform, CellCenter(2, 2) + new Vector3(-0.12f, 0.32f, 0.05f), _survivorYellow);
            BuildSurvivor(survivors.transform, CellCenter(5, 4) + new Vector3(0.15f, 0.32f, -0.08f), _survivorBlue);
            BuildSurvivor(survivors.transform, CellCenter(1, 5) + new Vector3(0.08f, 0.32f, 0.04f), _survivorYellow);
        }

        private void BuildSurvivor(Transform parent, Vector3 center, Material outfit)
        {
            CreateSphere("Survivor hood", parent, center + Vector3.up * 0.18f, new Vector3(0.22f, 0.22f, 0.22f), outfit);
            CreateSphere("Survivor face", parent, center + new Vector3(0f, 0.18f, -0.04f), new Vector3(0.14f, 0.13f, 0.10f), CreateMaterial("SB Survivor Face", new Color(1.0f, 0.72f, 0.46f), 0.4f));
            CreateCube("Survivor body", parent, center + new Vector3(0f, -0.02f, 0f), new Vector3(0.20f, 0.26f, 0.14f), outfit);
        }

        private void BuildPieceTray()
        {
            var tray = new GameObject("Bottom Three Piece Tray");
            tray.transform.SetParent(transform);
            tray.transform.position = new Vector3(0f, 0.18f, -5.0f);
            CreateCube("Rounded purple tray base", tray.transform, Vector3.zero, new Vector3(5.9f, 0.18f, 1.35f), _uiPanel);

            BuildTrayPiece(tray.transform, new Vector3(-2.0f, 0.22f, 0f), _blockCyan, new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(0, 1) });
            BuildTrayPiece(tray.transform, new Vector3(0f, 0.22f, 0f), _blockLime, new[] { new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(1, 2) });
            BuildTrayPiece(tray.transform, new Vector3(2.0f, 0.22f, 0f), _blockCoral, new[] { new GridPosition(0, 0), new GridPosition(1, 0) });
        }

        private void BuildTrayPiece(Transform parent, Vector3 origin, Material material, IEnumerable<GridPosition> cells)
        {
            foreach (var cell in cells)
            {
                CreateCube("Tray block cell", parent, origin + new Vector3((cell.X - 1) * 0.26f, cell.Y * 0.03f, (cell.Y - 1) * 0.26f), new Vector3(0.24f, 0.20f, 0.24f), material);
            }
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("Storm Blocks Portrait HUD");
            canvasObject.transform.SetParent(transform);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1170f, 2532f);
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateHudPanel(canvasObject.transform, "2480\nSCORE", new Vector2(-310f, 1080f), new Vector2(270f, 128f), new Color(0.48f, 0.40f, 0.76f, 0.92f));
            CreateHudPanel(canvasObject.transform, "12\nRESCUED", new Vector2(0f, 1080f), new Vector2(250f, 150f), new Color(0.58f, 0.48f, 0.82f, 0.94f));
            CreateHudPanel(canvasObject.transform, "BEST 8740\nDAILY 3120", new Vector2(320f, 1080f), new Vector2(310f, 128f), new Color(0.48f, 0.40f, 0.76f, 0.92f));
            CreateHudPanel(canvasObject.transform, "DRAG BLOCKS", new Vector2(0f, -925f), new Vector2(330f, 82f), new Color(0.42f, 0.35f, 0.67f, 0.90f));
        }

        private void CreateHudPanel(Transform parent, string text, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var panel = new GameObject(text.Replace("\n", " "));
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var label = new GameObject("Label");
            label.transform.SetParent(panel.transform, false);
            var uiText = label.AddComponent<Text>();
            uiText.text = text;
            uiText.font = UiFont();
            uiText.fontSize = text.Contains("\n") ? 36 : 38;
            uiText.fontStyle = FontStyle.Bold;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = Color.white;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.resizeTextForBestFit = true;
            uiText.resizeTextMinSize = 18;
            uiText.resizeTextMaxSize = uiText.fontSize;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private static Font UiFont()
        {
            if (_cachedUiFont != null)
            {
                return _cachedUiFont;
            }

            _cachedUiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cachedUiFont == null)
            {
                _cachedUiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return _cachedUiFont;
        }

        private void BuildPushbackMoment()
        {
            var pushback = new GameObject("Automatic Storm Pushback Signature Moment");
            pushback.transform.SetParent(transform);
            for (int x = 0; x < BoardSize; x++)
            {
                CreateCube("Gold pushback wave row", pushback.transform, CellCenter(x, 3) + Vector3.up * 0.42f, new Vector3(0.78f, 0.035f, 0.16f), _goldGlow);
            }

            CreateSphere("Clutch flare", pushback.transform, CellCenter(0, 3) + Vector3.up * 0.55f, new Vector3(0.40f, 0.10f, 0.40f), _goldGlow);
            CreateSphere("Clutch flare", pushback.transform, CellCenter(7, 3) + Vector3.up * 0.55f, new Vector3(0.40f, 0.10f, 0.40f), _goldGlow);
        }

        private GameObject CreateCube(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private GameObject CreateSphere(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = objectName;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = localScale;
            sphere.GetComponent<Renderer>().sharedMaterial = material;
            return sphere;
        }

        private static Vector3 CellCenter(int x, int y)
        {
            return new Vector3(BoardOrigin + x * CellPitch, 0f, BoardOrigin + y * CellPitch);
        }
    }
}
