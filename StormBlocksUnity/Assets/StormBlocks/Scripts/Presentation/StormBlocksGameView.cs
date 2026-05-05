using System;
using System.Collections.Generic;
using StormBlocks.Core;
using StormBlocks.Gameplay;
using StormBlocks.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StormBlocks.Presentation
{
    public sealed class StormBlocksGameView : MonoBehaviour
    {
        private const int BoardSize = 8;
        private const float CellPitch = 0.82f;
        private const float BoardOrigin = -3.5f * CellPitch;

        [SerializeField] private bool autoStart = true;

        private readonly GameObject[,] _tileObjects = new GameObject[BoardSize, BoardSize];
        private readonly List<TrayPieceView> _trayPieces = new List<TrayPieceView>();
        private readonly List<Material> _runtimeMaterials = new List<Material>();
        private readonly List<UnityEngine.Object> _runtimeUiAssets = new List<UnityEngine.Object>();
        private readonly List<UnityEngine.Object> _runtimeMaterialAssets = new List<UnityEngine.Object>();
        private readonly Dictionary<PrimitiveType, Stack<GameObject>> _primitivePools = new Dictionary<PrimitiveType, Stack<GameObject>>();

        private Transform _boardRoot;
        private Transform _boardContentRoot;
        private Transform _trayRoot;
        private Transform _fxRoot;
        private Transform _poolRoot;
        private Transform _dragRoot;
        private Transform _ghostRoot;
        private Camera _camera;
        private Canvas _canvas;
        private Text _scoreLabel;
        private Text _rescuedLabel;
        private Text _bestLabel;
        private Text _modeLabel;
        private Text _phaseLabel;
        private Text _toastLabel;
        private RectTransform _screenLayer;
        private RectTransform _screenPanel;

        private Material _emptyTile;
        private Material _campTile;
        private Material _stormTile;
        private Material _warningTile;
        private Material _stormCloud;
        private Material _stormLightning;
        private Material _stormRain;
        private Material _goldGlow;
        private Material _campOrange;
        private Material _campLight;
        private Material _campCanvas;
        private Material _creamTile;
        private Material _boardRim;
        private Material _boardWarmRim;
        private Material _boardShadow;
        private Material _trayGlow;
        private Material _nearDeathGlow;
        private Material _survivorYellow;
        private Material _survivorBlue;
        private Material _survivorFace;
        private Material _survivorPink;
        private Material _softCloud;
        private Material _skyBlue;
        private Material _skyPurple;
        private Material _sunbeam;
        private Material _uiPanel;
        private Material _uiPanelWarm;
        private Material _ghostValid;
        private Material _ghostInvalid;
        private Material[] _blockMaterials;
        private Sprite _hudPanelSprite;
        private Sprite _hudWarmSprite;
        private Sprite _modalPanelSprite;
        private Sprite _buttonSprite;
        private Sprite _buttonWarmSprite;

        private StormRunSession _session;
        private GameModeDefinition _modeDefinition;
        private MockGameServices _services;
        private ILeaderboardService _leaderboards;
        private IAchievementService _achievements;
        private IShareService _shareService;
        private UnityGameCenterServices _gameCenter;
        private UnityLocalFeedbackService _feedback;
        private FileSaveService _saveService;
        private PlayerProfile _profile;
        private IReadOnlyList<PieceDefinition> _pieceLibrary;
        private StormTrailLevelDefinition _activeTrailLevel;
        private TempestWeekDefinition _activeTempestWeek;
        private TempestTrialRunDefinition _activeTempestRun;
        private RunSummary _lastSummary;
        private int _selectedTrailRegionIndex;
        private int _dragQueueIndex = -1;
        private GridPosition _currentDragOrigin;
        private bool _hasDragOrigin;
        private float _toastTimer;
        private float _fxTimer;
        private static Font _cachedUiFont;

        public StormRunState State
        {
            get { return _session != null ? _session.State : null; }
        }

        private void Start()
        {
            BuildScene();
            if (autoStart)
            {
                StartEndless(DateTime.UtcNow.TicksAsStableSeed());
            }
        }

        private void Update()
        {
            UpdateTimers();
            HandlePointerInput();
        }

        [ContextMenu("Build Playable Storm Blocks View")]
        public void BuildScene()
        {
            ClearChildren();
            _primitivePools.Clear();
            CreateServices();
            CreateMaterials();
            CreateUiSprites();
            SetupCameraAndLights();
            BuildBackground();
            BuildBoardShell();
            BuildTrayShell();
            BuildHud();
            EnsureEventSystem();
        }

        public void StartEndlessForTest(ulong seed)
        {
            BuildScene();
            StartEndless(seed);
        }

        public PlacementResult TryPlaceForTest(int queueIndex, GridPosition origin)
        {
            return PlaceQueuedPiece(queueIndex, origin);
        }

        public void StartEndless(ulong seed)
        {
            ClearModeContext();
            StartMode(ModeConfigFactory.CreateEndless(seed));
        }

        public void StartDaily(bool officialAttempt)
        {
            ClearModeContext();
            StartMode(ModeConfigFactory.CreateDaily(DateTime.UtcNow, officialAttempt).Mode);
        }

        public void StartPractice(ulong seed)
        {
            ClearModeContext();
            StartMode(ModeConfigFactory.CreatePractice(seed));
        }

        public void StartFirstStormTrailLevel()
        {
            StartStormTrailLevel(GetNextStormTrailLevel());
        }

        public void StartStormTrailLevel(StormTrailLevelDefinition level)
        {
            if (level == null)
            {
                return;
            }

            _activeTrailLevel = level;
            _activeTempestWeek = null;
            _activeTempestRun = null;
            StartMode(new GameModeDefinition
            {
                Id = level.Id,
                DisplayName = level.DisplayName,
                Description = level.TutorialBeat,
                Mode = GameModeId.StormTrail,
                Seed = level.Seed,
                Modifier = level.Modifier,
                Config = level.Config,
                IsLeaderboardEligible = false
            });
        }

        public void StartCurrentTempestTrial()
        {
            var week = ModeConfigFactory.CreateTempestWeek(DateTime.UtcNow);
            TempestTrialRunDefinition run = GetNextTempestRun(week);
            StartTempestRun(week, run);
        }

        public void StartTempestRun(TempestWeekDefinition week, TempestTrialRunDefinition run)
        {
            if (week == null || run == null)
            {
                return;
            }

            _activeTrailLevel = null;
            _activeTempestWeek = week;
            _activeTempestRun = run;
            StartMode(new GameModeDefinition
            {
                Id = run.Id,
                DisplayName = "Tempest Trial " + run.RunIndex,
                Description = run.Modifier.ToString(),
                Mode = GameModeId.TempestTrial,
                Seed = run.Seed,
                Modifier = run.Modifier,
                Config = run.Config,
                IsLeaderboardEligible = true
            });
        }

        private void StartMode(GameModeDefinition definition)
        {
            _modeDefinition = definition;
            _pieceLibrary = DefaultPieceLibrary.Create();
            _session = new StormRunSession(_pieceLibrary, _feedback, _feedback, _services);
            _session.Start(definition.Config, definition.Seed);
            SeedSurvivors(_session.State.Board, definition.Seed);
            _saveService.SaveRunSnapshot(StormRunSnapshot.FromState(_session.State));

            if (definition.Mode == GameModeId.DailyStorm)
            {
                _feedback.Play(AudioEventId.DailyStormStart);
            }

            HideOverlay();
            RefreshBoard();
            RefreshTray();
            RefreshHud();
        }

        private void CreateServices()
        {
            _services = new MockGameServices();
            _saveService = new FileSaveService(Application.persistentDataPath);
            _profile = _saveService.LoadProfile();
            CosmeticCatalog.EnsureDefaultCosmetics(_profile);
            _feedback = GetComponent<UnityLocalFeedbackService>();
            if (_feedback == null)
            {
                _feedback = gameObject.AddComponent<UnityLocalFeedbackService>();
            }

            _feedback.Configure(_profile.Settings);

            _gameCenter = GetComponent<UnityGameCenterServices>();
            if (_gameCenter == null)
            {
                _gameCenter = gameObject.AddComponent<UnityGameCenterServices>();
            }

            _gameCenter.Configure(_services, _services, _services);
            _leaderboards = _gameCenter;
            _achievements = _gameCenter;
            _shareService = GetComponent<UnityShareService>();
            if (_shareService == null)
            {
                _shareService = gameObject.AddComponent<UnityShareService>();
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyObject(transform.GetChild(i).gameObject);
            }
        }

        private void CreateMaterials()
        {
            bool highContrast = _profile != null && _profile.Settings.HighContrast;
            bool colorblind = _profile != null && _profile.Settings.ColorblindFriendly;
            for (int i = _runtimeMaterialAssets.Count - 1; i >= 0; i--)
            {
                DestroyUnityObject(_runtimeMaterialAssets[i]);
            }

            _runtimeMaterialAssets.Clear();
            _runtimeMaterials.Clear();
            _emptyTile = CreateToyMaterial("SB Empty Tile Gloss", highContrast ? new Color(0.13f, 0.22f, 0.50f) : new Color(0.42f, 0.48f, 0.72f), new Color(0.70f, 0.76f, 0.96f), new Color(0.20f, 0.24f, 0.46f), 0.72f, false);
            _campTile = CreateToyMaterial("SB Warm Camp Tile", highContrast ? new Color(1.0f, 0.60f, 0.14f) : new Color(0.76f, 0.50f, 0.32f), new Color(1.0f, 0.76f, 0.42f), new Color(0.46f, 0.24f, 0.18f), 0.66f, false);
            _creamTile = CreateToyMaterial("SB Cream Tile Highlight", new Color(0.80f, 0.64f, 0.42f), new Color(1.0f, 0.84f, 0.58f), new Color(0.48f, 0.30f, 0.20f), 0.7f, false);
            _stormTile = CreateToyMaterial("SB Storm Tile Cracked", highContrast ? new Color(0.02f, 0.03f, 0.12f) : new Color(0.13f, 0.17f, 0.34f), new Color(0.24f, 0.30f, 0.54f), new Color(0.04f, 0.05f, 0.14f), 0.34f, true);
            _warningTile = CreateToyMaterial("SB Storm Warning", highContrast ? new Color(1.0f, 0.88f, 0.10f) : new Color(0.12f, 0.80f, 1.0f), new Color(0.46f, 0.95f, 1.0f), new Color(0.08f, 0.34f, 0.70f), 0.5f, false);
            _stormCloud = CreateMaterial("SB Storm Cloud", highContrast ? new Color(0.03f, 0.02f, 0.18f) : new Color(0.30f, 0.27f, 0.58f), 0.22f);
            _softCloud = CreateMaterial("SB Soft Cloud", new Color(0.80f, 0.78f, 0.94f), 0.44f);
            _stormLightning = CreateMaterial("SB Storm Lightning", new Color(0.34f, 0.86f, 1.0f), 0.78f, 0.45f);
            _stormRain = CreateMaterial("SB Storm Rain", new Color(0.40f, 0.60f, 0.94f), 0.34f, 0.08f);
            _goldGlow = CreateMaterial("SB Pushback Gold Glow", new Color(0.95f, 0.50f, 0.10f), 0.82f, 0.22f);
            _campOrange = CreateMaterial("SB Camp Orange", new Color(1.0f, 0.34f, 0.08f), 0.48f);
            _campCanvas = CreateMaterial("SB Camp Canvas", new Color(1.0f, 0.67f, 0.28f), 0.58f);
            _campLight = CreateMaterial("SB Camp Lantern Glow", new Color(0.98f, 0.64f, 0.18f), 0.85f, 0.18f);
            _boardRim = CreateMaterial("SB Board Cyan Rim Glow", new Color(0.16f, 0.78f, 0.94f), 0.8f, 0.20f);
            _boardWarmRim = CreateMaterial("SB Board Warm Rounded Rim", new Color(0.33f, 0.20f, 0.25f), 0.55f);
            _boardShadow = CreateMaterial("SB Board Soft Shadow", new Color(0.06f, 0.06f, 0.18f), 0.28f);
            _trayGlow = CreateMaterial("SB Tray Pad Glow", new Color(0.95f, 0.56f, 0.16f), 0.82f, 0.18f);
            _nearDeathGlow = CreateMaterial("SB Near Death Warm Vignette", new Color(0.90f, 0.16f, 0.22f), 0.65f, 0.22f);
            _survivorYellow = CreateMaterial("SB Survivor Yellow", new Color(1.0f, 0.86f, 0.18f), 0.42f);
            _survivorBlue = CreateMaterial("SB Survivor Blue", new Color(0.08f, 0.74f, 1.0f), 0.46f);
            _survivorPink = CreateMaterial("SB Survivor Pink", new Color(1.0f, 0.45f, 0.68f), 0.48f);
            _survivorFace = CreateMaterial("SB Survivor Face", new Color(1.0f, 0.74f, 0.52f), 0.52f);
            _skyBlue = CreateMaterial("SB Sky Blue Haze", new Color(0.12f, 0.30f, 0.54f), 0.3f);
            _skyPurple = CreateMaterial("SB Sky Purple Haze", new Color(0.09f, 0.08f, 0.25f), 0.26f);
            _sunbeam = CreateMaterial("SB Warm Sunbeam", new Color(0.35f, 0.18f, 0.24f), 0.55f);
            _uiPanel = CreateMaterial("SB UI Purple", new Color(0.50f, 0.42f, 0.78f), 0.58f);
            _uiPanelWarm = CreateMaterial("SB UI Warm", new Color(0.88f, 0.50f, 0.26f), 0.58f);
            _ghostValid = CreateTransparentMaterial("SB Ghost Valid", new Color(1.0f, 0.82f, 0.28f, 0.55f));
            _ghostInvalid = CreateTransparentMaterial("SB Ghost Invalid", new Color(1.0f, 0.20f, 0.32f, 0.42f));
            _blockMaterials = colorblind
                ? new[]
                {
                    CreateToyMaterial("SB Block Sky", new Color(0.0f, 0.66f, 1.0f), new Color(0.45f, 0.95f, 1.0f), new Color(0.0f, 0.32f, 0.70f), 0.82f, false),
                    CreateToyMaterial("SB Block Amber", new Color(1.0f, 0.66f, 0.03f), new Color(1.0f, 0.88f, 0.28f), new Color(0.70f, 0.30f, 0.0f), 0.8f, false),
                    CreateToyMaterial("SB Block Mint", new Color(0.0f, 0.84f, 0.58f), new Color(0.48f, 1.0f, 0.74f), new Color(0.0f, 0.42f, 0.30f), 0.78f, false),
                    CreateToyMaterial("SB Block Rose", new Color(0.96f, 0.28f, 0.54f), new Color(1.0f, 0.62f, 0.78f), new Color(0.56f, 0.08f, 0.28f), 0.8f, false),
                    CreateToyMaterial("SB Block White Gold", new Color(1.0f, 0.86f, 0.24f), new Color(1.0f, 0.96f, 0.54f), new Color(0.68f, 0.46f, 0.0f), 0.82f, false)
                }
                : new[]
                {
                    CreateToyMaterial("SB Block Cyan Candy", new Color(0.02f, 0.78f, 1.0f), new Color(0.42f, 0.96f, 1.0f), new Color(0.0f, 0.36f, 0.70f), 0.86f, false),
                    CreateToyMaterial("SB Block Coral Candy", new Color(1.0f, 0.35f, 0.30f), new Color(1.0f, 0.62f, 0.54f), new Color(0.68f, 0.12f, 0.10f), 0.82f, false),
                    CreateToyMaterial("SB Block Lime Toy", new Color(0.46f, 0.92f, 0.10f), new Color(0.72f, 1.0f, 0.32f), new Color(0.20f, 0.52f, 0.0f), 0.82f, false),
                    CreateToyMaterial("SB Block Purple Toy", new Color(0.66f, 0.30f, 1.0f), new Color(0.86f, 0.62f, 1.0f), new Color(0.32f, 0.08f, 0.62f), 0.84f, false),
                    CreateToyMaterial("SB Block Honey Toy", new Color(1.0f, 0.72f, 0.16f), new Color(1.0f, 0.92f, 0.38f), new Color(0.70f, 0.34f, 0.0f), 0.86f, false)
                };
        }

        private Material CreateMaterial(string materialName, Color color, float smoothness)
        {
            return CreateMaterial(materialName, color, smoothness, 0f);
        }

        private Material CreateMaterial(string materialName, Color color, float smoothness, float emission)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (emission > 0f && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }

            _runtimeMaterials.Add(material);
            return material;
        }

        private Material CreateToyMaterial(string materialName, Color baseColor, Color highlightColor, Color edgeColor, float smoothness, bool stormCracks)
        {
            var material = CreateMaterial(materialName, baseColor, smoothness);
            var texture = new Texture2D(48, 48, TextureFormat.RGBA32, false)
            {
                name = materialName + " Toy Texture",
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color32[48 * 48];
            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    pixels[y * 48 + x] = ToyTexturePixel(x, y, 48, baseColor, highlightColor, edgeColor, stormCracks);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            _runtimeMaterialAssets.Add(texture);
            return material;
        }

        private static Color ToyTexturePixel(int x, int y, int size, Color baseColor, Color highlightColor, Color edgeColor, bool stormCracks)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            float dx = Mathf.Min(u, 1f - u);
            float dy = Mathf.Min(v, 1f - v);
            float edge = Mathf.Clamp01(Mathf.Min(dx, dy) * 12f);
            float highlight = Mathf.Clamp01((1f - Mathf.Abs(u - 0.32f) * 3.0f) * (1f - Mathf.Abs(v - 0.72f) * 3.2f));
            Color color = Color.Lerp(edgeColor, baseColor, edge);
            color = Color.Lerp(color, highlightColor, highlight * 0.35f);

            float corner = (u - 0.18f) * (u - 0.18f) + (v - 0.82f) * (v - 0.82f);
            if (corner < 0.012f)
            {
                color = Color.Lerp(color, Color.white, 0.38f);
            }

            if (stormCracks)
            {
                bool crackA = Mathf.Abs((u * 1.4f + v * 0.7f) - 0.74f) < 0.018f && u > 0.18f && u < 0.78f;
                bool crackB = Mathf.Abs((u * -0.8f + v * 1.2f) - 0.24f) < 0.015f && v > 0.24f && v < 0.86f;
                if (crackA || crackB)
                {
                    color = Color.Lerp(color, new Color(0.48f, 0.85f, 1.0f), 0.55f);
                }
            }

            return color;
        }

        private Material CreateTransparentMaterial(string materialName, Color color)
        {
            var material = CreateMaterial(materialName, color, 0.2f);
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.renderQueue = 3000;
            return material;
        }

        private void CreateUiSprites()
        {
            for (int i = _runtimeUiAssets.Count - 1; i >= 0; i--)
            {
                DestroyUnityObject(_runtimeUiAssets[i]);
            }

            _runtimeUiAssets.Clear();
            _hudPanelSprite = CreateRoundedPanelSprite("SB HUD Rounded Purple Sprite", 160, 80, 28f, 8f, new Color(0.50f, 0.43f, 0.78f, 0.94f), new Color(0.98f, 0.74f, 0.35f, 0.96f), new Color(0.90f, 0.86f, 1.0f, 0.36f));
            _hudWarmSprite = CreateRoundedPanelSprite("SB HUD Warm Badge Sprite", 160, 80, 28f, 8f, new Color(0.82f, 0.47f, 0.25f, 0.94f), new Color(1.0f, 0.78f, 0.34f, 0.96f), new Color(1.0f, 0.92f, 0.72f, 0.34f));
            _modalPanelSprite = CreateRoundedPanelSprite("SB Modal Rounded Panel Sprite", 180, 220, 30f, 7f, new Color(0.21f, 0.18f, 0.42f, 0.96f), new Color(0.78f, 0.70f, 1.0f, 0.65f), new Color(0.62f, 0.54f, 0.90f, 0.30f));
            _buttonSprite = CreateRoundedPanelSprite("SB Button Rounded Purple Sprite", 150, 72, 24f, 6f, new Color(0.40f, 0.34f, 0.66f, 0.98f), new Color(0.75f, 0.68f, 1.0f, 0.65f), new Color(0.72f, 0.64f, 1.0f, 0.32f));
            _buttonWarmSprite = CreateRoundedPanelSprite("SB Button Warm Rounded Sprite", 150, 72, 24f, 6f, new Color(0.82f, 0.47f, 0.24f, 0.98f), new Color(1.0f, 0.82f, 0.38f, 0.85f), new Color(1.0f, 0.88f, 0.65f, 0.38f));
        }

        private Sprite CreateRoundedPanelSprite(string spriteName, int width, int height, float radius, float border, Color fill, Color edge, Color highlight)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = spriteName + " Texture",
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color32[width * height];
            float maxRadius = Mathf.Min(radius, Mathf.Min(width, height) * 0.5f - 1f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = RoundedPanelPixel(x, y, width, height, maxRadius, border, fill, edge, highlight);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.name = spriteName;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _runtimeUiAssets.Add(texture);
            _runtimeUiAssets.Add(sprite);
            return sprite;
        }

        private static Color RoundedPanelPixel(int x, int y, int width, int height, float radius, float border, Color fill, Color edge, Color highlight)
        {
            float dx = Mathf.Min(x + 0.5f, width - x - 0.5f);
            float dy = Mathf.Min(y + 0.5f, height - y - 0.5f);
            bool inside = true;

            if (dx < radius && dy < radius)
            {
                float cornerX = dx - radius;
                float cornerY = dy - radius;
                inside = cornerX * cornerX + cornerY * cornerY <= radius * radius;
            }

            if (!inside)
            {
                return Color.clear;
            }

            bool edgePixel = dx < border || dy < border;
            if (dx < radius && dy < radius)
            {
                float cornerX = dx - radius;
                float cornerY = dy - radius;
                float innerRadius = Mathf.Max(0f, radius - border);
                edgePixel = edgePixel || cornerX * cornerX + cornerY * cornerY >= innerRadius * innerRadius;
            }

            float vertical = y / Mathf.Max(1f, height - 1f);
            Color color = edgePixel ? edge : fill;
            if (vertical > 0.62f)
            {
                color = Color.Lerp(color, highlight, (vertical - 0.62f) / 0.38f);
            }

            return color;
        }

        private void SetupCameraAndLights()
        {
            ClearExistingAudioListeners();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(transform);
            cameraObject.tag = "MainCamera";
            _camera = cameraObject.AddComponent<Camera>();
            _camera.transform.position = new Vector3(0f, 9.4f, -9.0f);
            _camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            _camera.orthographic = true;
            _camera.orthographicSize = 7.55f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.07f, 0.08f, 0.24f);
            cameraObject.AddComponent<AudioListener>();
            RenderSettings.ambientLight = new Color(0.24f, 0.25f, 0.42f);

            var key = new GameObject("Warm Camp Key Light");
            key.transform.SetParent(transform);
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1.0f, 0.92f, 0.78f);
            keyLight.intensity = 0.78f;
            key.transform.rotation = Quaternion.Euler(46f, -28f, 0f);

            var storm = new GameObject("Cool Storm Rim Light");
            storm.transform.SetParent(transform);
            storm.transform.position = new Vector3(-3.8f, 3.8f, 2.6f);
            var stormLight = storm.AddComponent<Light>();
            stormLight.type = LightType.Point;
            stormLight.color = new Color(0.35f, 0.90f, 1f);
            stormLight.intensity = 3.0f;
            stormLight.range = 7.8f;

            var camp = new GameObject("Warm Camp Glow Light");
            camp.transform.SetParent(transform);
            camp.transform.position = new Vector3(0f, 1.2f, -0.7f);
            var campLight = camp.AddComponent<Light>();
            campLight.type = LightType.Point;
            campLight.color = new Color(1.0f, 0.62f, 0.22f);
            campLight.intensity = 1.15f;
            campLight.range = 3.8f;
        }

        private static void ClearExistingAudioListeners()
        {
            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include);
            for (int i = 0; i < listeners.Length; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(listeners[i]);
                }
                else
                {
                    DestroyImmediate(listeners[i]);
                }
            }
        }

        private void BuildBackground()
        {
            CreateCube("Deep violet storm sky", transform, new Vector3(0f, -0.18f, 3.8f), new Vector3(13.0f, 0.08f, 9.2f), _skyPurple);
            CreateCube("Soft blue rescue sky", transform, new Vector3(0f, -0.17f, -2.0f), new Vector3(13.0f, 0.08f, 8.6f), _skyBlue);
            CreateCube("Warm sunset lower haze", transform, new Vector3(0f, -0.145f, -6.7f), new Vector3(13.0f, 0.07f, 3.2f), _sunbeam);
            CreateCube("Warm oval camp glow", transform, new Vector3(0f, -0.155f, -0.6f), new Vector3(5.6f, 0.035f, 1.45f), _sunbeam);
            CreateCube("Soft lower golden halo", transform, new Vector3(0f, -0.150f, -4.8f), new Vector3(5.0f, 0.035f, 0.85f), _sunbeam);
            CreateAtmosphereCloud("Left upper storm bank", new Vector3(-5.0f, 0.22f, 2.4f), 1.25f, _stormCloud);
            CreateAtmosphereCloud("Right upper storm bank", new Vector3(5.0f, 0.22f, 2.3f), 1.20f, _stormCloud);
            CreateLightningBolt("Backdrop left lightning", transform, new Vector3(-4.65f, 0.54f, 1.55f), 0.96f, -16f);
            CreateLightningBolt("Backdrop right lightning", transform, new Vector3(4.70f, 0.54f, 1.10f), 0.92f, 16f);
        }

        private void CreateAtmosphereCloud(string objectName, Vector3 center, float scale, Material material)
        {
            CreateSphere(objectName + " puff a", transform, center + new Vector3(-0.42f * scale, 0f, 0.02f * scale), new Vector3(1.05f * scale, 0.28f * scale, 0.62f * scale), material);
            CreateSphere(objectName + " puff b", transform, center + new Vector3(0.12f * scale, 0.06f * scale, 0.02f * scale), new Vector3(1.18f * scale, 0.34f * scale, 0.70f * scale), material);
            CreateSphere(objectName + " puff c", transform, center + new Vector3(0.58f * scale, -0.02f * scale, 0f), new Vector3(0.92f * scale, 0.24f * scale, 0.54f * scale), material);
        }

        private void BuildBoardShell()
        {
            _boardRoot = new GameObject("Readable 8x8 Storm Board").transform;
            _boardRoot.SetParent(transform);
            _boardContentRoot = new GameObject("Runtime Board Pieces Storm Survivors").transform;
            _boardContentRoot.SetParent(_boardRoot);
            _fxRoot = new GameObject("Juice VFX Root").transform;
            _fxRoot.SetParent(transform);
            _poolRoot = new GameObject("Storm Blocks Primitive Pool").transform;
            _poolRoot.SetParent(transform);
            _poolRoot.gameObject.SetActive(false);

            CreateCube("Soft board shadow plate", _boardRoot, new Vector3(0f, -0.08f, 0f), new Vector3(7.64f, 0.12f, 7.64f), _boardShadow);
            CreateCube("Warm rounded board underlay", _boardRoot, new Vector3(0f, -0.01f, 0f), new Vector3(7.34f, 0.12f, 7.34f), _boardWarmRim);
            CreateCube("Warm board rim north", _boardRoot, new Vector3(0f, 0.09f, 3.27f), new Vector3(7.42f, 0.16f, 0.20f), _boardWarmRim);
            CreateCube("Warm board rim south", _boardRoot, new Vector3(0f, 0.09f, -3.27f), new Vector3(7.42f, 0.16f, 0.20f), _boardWarmRim);
            CreateCube("Warm board rim west", _boardRoot, new Vector3(-3.27f, 0.09f, 0f), new Vector3(0.20f, 0.16f, 7.42f), _boardWarmRim);
            CreateCube("Warm board rim east", _boardRoot, new Vector3(3.27f, 0.09f, 0f), new Vector3(0.20f, 0.16f, 7.42f), _boardWarmRim);
            CreateCube("Cyan storm barrier north", _boardRoot, new Vector3(0f, 0.22f, 3.20f), new Vector3(7.15f, 0.06f, 0.06f), _boardRim);
            CreateCube("Cyan storm barrier south", _boardRoot, new Vector3(0f, 0.22f, -3.20f), new Vector3(7.15f, 0.06f, 0.06f), _boardRim);
            CreateCube("Cyan storm barrier west", _boardRoot, new Vector3(-3.20f, 0.22f, 0f), new Vector3(0.06f, 0.06f, 7.15f), _boardRim);
            CreateCube("Cyan storm barrier east", _boardRoot, new Vector3(3.20f, 0.22f, 0f), new Vector3(0.06f, 0.06f, 7.15f), _boardRim);
            CreateBoardStormCorner("North west storm curl", new Vector3(-3.15f, 0.40f, 3.05f), -20f);
            CreateBoardStormCorner("North east storm curl", new Vector3(3.15f, 0.40f, 3.05f), 20f);
            CreateBoardStormCorner("South west storm curl", new Vector3(-3.15f, 0.40f, -3.05f), 20f);
            CreateBoardStormCorner("South east storm curl", new Vector3(3.15f, 0.40f, -3.05f), -20f);

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    Vector3 center = CellCenter(x, y);
                    var tile = CreateCube("Tile " + x + "," + y, _boardRoot, center, new Vector3(0.74f, 0.16f, 0.74f), _emptyTile);
                    tile.transform.localPosition += Vector3.up * 0.02f;
                    _tileObjects[x, y] = tile;
                    CreateCube("Tile bevel top " + x + "," + y, tile.transform, new Vector3(0f, 0.085f, 0f), new Vector3(0.58f, 0.045f, 0.58f), _emptyTile);
                }
            }

            BuildCamp();
        }

        private void CreateBoardStormCorner(string objectName, Vector3 center, float rotation)
        {
            CreateSphere(objectName + " cloud large", _boardRoot, center, new Vector3(0.78f, 0.34f, 0.62f), _stormCloud);
            CreateSphere(objectName + " cloud side", _boardRoot, center + new Vector3(0.34f * Mathf.Sign(center.x), 0.02f, -0.08f * Mathf.Sign(center.z)), new Vector3(0.60f, 0.28f, 0.48f), _stormCloud);
            CreateSphere(objectName + " cyan core", _boardRoot, center + new Vector3(0.10f * Mathf.Sign(center.x), 0.03f, -0.10f * Mathf.Sign(center.z)), new Vector3(0.28f, 0.12f, 0.22f), _stormLightning);
            CreateLightningBolt(objectName + " lightning", _boardRoot, center + new Vector3(0.06f * Mathf.Sign(center.x), 0.22f, -0.10f * Mathf.Sign(center.z)), 0.46f, rotation);
        }

        private void BuildCamp()
        {
            var campRoot = new GameObject("Warm Central Rescue Camp");
            campRoot.transform.SetParent(_boardRoot);
            campRoot.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            CreateSphere("Camp warm halo", campRoot.transform, new Vector3(0f, -0.05f, 0f), new Vector3(1.72f, 0.10f, 1.72f), _campLight);
            CreateTentPrism("Camp tent canvas prism", campRoot.transform, new Vector3(0.08f, 0.22f, -0.02f), 1.08f, 0.76f, 0.86f, _campCanvas);
            CreateTrianglePanel("Camp tent dark doorway", campRoot.transform, new Vector3(0.08f, 0.23f, -0.48f), 0.38f, 0.48f, _boardShadow);
            CreateCube("Tent bright ridge", campRoot.transform, new Vector3(0.08f, 0.59f, -0.02f), new Vector3(0.08f, 0.06f, 0.94f), _goldGlow);
            CreateCube("Tent left canvas fold", campRoot.transform, new Vector3(-0.18f, 0.29f, -0.50f), new Vector3(0.08f, 0.58f, 0.055f), _campOrange).transform.rotation = Quaternion.Euler(0f, 0f, -20f);
            CreateCube("Tent right canvas fold", campRoot.transform, new Vector3(0.34f, 0.29f, -0.50f), new Vector3(0.08f, 0.58f, 0.055f), _campOrange).transform.rotation = Quaternion.Euler(0f, 0f, 20f);
            CreateCube("Tent warm front trim", campRoot.transform, new Vector3(0.08f, 0.16f, -0.51f), new Vector3(1.05f, 0.07f, 0.08f), _campOrange);
            CreateCube("Signal flag pole", campRoot.transform, new Vector3(0.63f, 0.72f, -0.31f), new Vector3(0.07f, 0.78f, 0.07f), _survivorYellow);
            CreateCube("Signal pennant", campRoot.transform, new Vector3(0.84f, 0.97f, -0.31f), new Vector3(0.36f, 0.18f, 0.06f), _campOrange);
            CreateCube("String light wire", campRoot.transform, new Vector3(0.08f, 0.61f, -0.46f), new Vector3(1.05f, 0.035f, 0.035f), _boardRim);
            for (int i = 0; i < 5; i++)
            {
                CreateSphere("Camp string bulb", campRoot.transform, new Vector3(-0.42f + i * 0.25f, 0.55f, -0.49f), new Vector3(0.08f, 0.08f, 0.08f), i % 2 == 0 ? _campLight : _goldGlow);
            }
            CreateCube("Camp fire log", campRoot.transform, new Vector3(-0.55f, 0.09f, -0.48f), new Vector3(0.34f, 0.08f, 0.10f), _boardShadow).transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            CreateCube("Camp fire cross log", campRoot.transform, new Vector3(-0.55f, 0.10f, -0.48f), new Vector3(0.32f, 0.07f, 0.09f), _boardShadow).transform.rotation = Quaternion.Euler(0f, -35f, 0f);
            CreateSphere("Camp fire glow", campRoot.transform, new Vector3(-0.55f, 0.18f, -0.48f), new Vector3(0.42f, 0.23f, 0.42f), _goldGlow);
            CreateSphere("Camp fire flame", campRoot.transform, new Vector3(-0.55f, 0.32f, -0.48f), new Vector3(0.20f, 0.34f, 0.20f), _campOrange);
            CreateCube("Base camp sign post", campRoot.transform, new Vector3(0.82f, 0.21f, 0.35f), new Vector3(0.06f, 0.30f, 0.06f), _boardShadow);
            CreateCube("Base camp sign board", campRoot.transform, new Vector3(0.82f, 0.39f, 0.35f), new Vector3(0.46f, 0.18f, 0.07f), _campCanvas);
        }

        private void BuildTrayShell()
        {
            _trayRoot = new GameObject("Bottom Three Piece Tray").transform;
            _trayRoot.SetParent(transform);
            _trayRoot.position = new Vector3(0f, 0.18f, -5.35f);
            CreateCube("Rounded purple tray base", _trayRoot, Vector3.zero, new Vector3(6.55f, 0.18f, 1.48f), _uiPanel);
            CreateSphere("Tray Static rounded left cap", _trayRoot, new Vector3(-3.24f, 0.02f, 0f), new Vector3(0.66f, 0.19f, 1.46f), _uiPanel);
            CreateSphere("Tray Static rounded right cap", _trayRoot, new Vector3(3.24f, 0.02f, 0f), new Vector3(0.66f, 0.19f, 1.46f), _uiPanel);
            CreateCube("Tray Static stitched top lip", _trayRoot, new Vector3(0f, 0.15f, 0.69f), new Vector3(5.95f, 0.06f, 0.06f), _uiPanelWarm);
            CreateCube("Tray Static stitched bottom lip", _trayRoot, new Vector3(0f, 0.15f, -0.69f), new Vector3(5.95f, 0.06f, 0.06f), _uiPanelWarm);
            CreateCube("Tray Static warm top rail", _trayRoot, new Vector3(0f, 0.25f, 0.83f), new Vector3(5.75f, 0.07f, 0.08f), _uiPanelWarm);
            for (int i = 0; i < 3; i++)
            {
                CreateSphere("Tray Static golden piece pad " + i, _trayRoot, new Vector3(-2.05f + i * 2.05f, 0.19f, -0.02f), new Vector3(1.10f, 0.10f, 0.62f), _trayGlow);
            }
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("Storm Blocks Portrait HUD");
            canvasObject.transform.SetParent(transform);
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform safeRoot = CreateSafeAreaRoot(canvasObject.transform);

            CreateDecorativePanel(safeRoot, "Connected Toy HUD Backplate", new Vector2(0f, 1080f), new Vector2(920f, 162f), new Color(0.54f, 0.47f, 0.80f, 0.82f), _hudPanelSprite, true);
            _scoreLabel = CreateHudPanel(safeRoot, "0\nSCORE", new Vector2(-320f, 1080f), new Vector2(268f, 120f), new Color(0.58f, 0.50f, 0.82f, 0.44f));
            _rescuedLabel = CreateHudPanel(safeRoot, "0\nRESCUED", new Vector2(0f, 1080f), new Vector2(250f, 138f), new Color(0.70f, 0.62f, 0.90f, 0.38f));
            _bestLabel = CreateHudPanel(safeRoot, "BEST 0", new Vector2(320f, 1080f), new Vector2(305f, 120f), new Color(0.58f, 0.50f, 0.82f, 0.44f));
            _modeLabel = CreateHudPanel(safeRoot, "ENDLESS STORM", new Vector2(0f, 950f), new Vector2(470f, 72f), new Color(0.86f, 0.48f, 0.25f, 0.96f));
            _phaseLabel = CreateHudPanel(safeRoot, "CALM", new Vector2(0f, 1005f), new Vector2(190f, 48f), new Color(0.38f, 0.32f, 0.62f, 0.70f));
            _toastLabel = CreateHudPanel(safeRoot, string.Empty, new Vector2(0f, 715f), new Vector2(470f, 90f), new Color(1.0f, 0.62f, 0.18f, 0.0f));
            _toastLabel.gameObject.transform.parent.gameObject.SetActive(false);

            CreateModeButton(safeRoot, "MENU", new Vector2(-500f, 1082f), ShowHomeScreen);
            BuildOverlayShell(safeRoot);
        }

        private static RectTransform CreateSafeAreaRoot(Transform parent)
        {
            var safeObject = new GameObject("Storm Blocks Safe Area");
            safeObject.transform.SetParent(parent, false);
            var rect = safeObject.AddComponent<RectTransform>();

            Rect safe = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0 || safe.width <= 0f || safe.height <= 0f)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
            }
            else
            {
                rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
                rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            }

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private Image CreateDecorativePanel(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color, Sprite sprite, bool shadow)
        {
            var panel = new GameObject(objectName);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            if (shadow)
            {
                AddGraphicShadow(image, new Color(0.03f, 0.02f, 0.09f, 0.40f), new Vector2(0f, -8f));
            }

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return image;
        }

        private Text CreateHudPanel(Transform parent, string text, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var panel = new GameObject(text.Replace("\n", " "));
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.sprite = color.r > color.b ? _hudWarmSprite : _hudPanelSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            AddGraphicShadow(image, new Color(0.03f, 0.02f, 0.09f, Mathf.Min(0.34f, color.a * 0.35f)), new Vector2(0f, -5f));
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var label = new GameObject("Label");
            label.transform.SetParent(panel.transform, false);
            var tmp = label.AddComponent<Text>();
            tmp.text = text;
            tmp.font = UiFont();
            tmp.fontSize = text.Contains("\n") ? 36 : 38;
            tmp.fontStyle = FontStyle.Bold;
            tmp.alignment = TextAnchor.MiddleCenter;
            tmp.color = Color.white;
            tmp.horizontalOverflow = HorizontalWrapMode.Overflow;
            tmp.verticalOverflow = VerticalWrapMode.Overflow;
            tmp.resizeTextForBestFit = true;
            tmp.resizeTextMinSize = 18;
            tmp.resizeTextMaxSize = tmp.fontSize;
            StyleUiText(tmp, new Color(0.12f, 0.09f, 0.23f, 0.86f), new Vector2(0f, -3f));
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return tmp;
        }

        private static void AddGraphicShadow(Graphic graphic, Color color, Vector2 distance)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void StyleUiText(Text text, Color outlineColor, Vector2 shadowDistance)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
            shadow.effectDistance = shadowDistance;
            shadow.useGraphicAlpha = true;
        }

        private void CreateModeButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(text + " Mode Button");
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.AddComponent<Image>();
            image.sprite = _buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.36f, 0.30f, 0.58f, 0.92f);
            AddGraphicShadow(image, new Color(0.03f, 0.02f, 0.09f, 0.32f), new Vector2(0f, -5f));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(delegate
            {
                _feedback.Play(AudioEventId.UiTap);
                action();
            });
            var colors = button.colors;
            colors.highlightedColor = new Color(0.54f, 0.46f, 0.80f, 1f);
            colors.pressedColor = new Color(0.94f, 0.62f, 0.28f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(142f, 82f);

            var label = new GameObject("Label");
            label.transform.SetParent(buttonObject.transform, false);
            var tmp = label.AddComponent<Text>();
            tmp.text = text;
            tmp.font = UiFont();
            tmp.fontSize = text.Length > 6 ? 24 : 28;
            tmp.fontStyle = FontStyle.Bold;
            tmp.alignment = TextAnchor.MiddleCenter;
            tmp.color = Color.white;
            tmp.horizontalOverflow = HorizontalWrapMode.Overflow;
            tmp.verticalOverflow = VerticalWrapMode.Overflow;
            tmp.resizeTextForBestFit = true;
            tmp.resizeTextMinSize = 14;
            tmp.resizeTextMaxSize = tmp.fontSize;
            StyleUiText(tmp, new Color(0.12f, 0.09f, 0.23f, 0.86f), new Vector2(0f, -3f));
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private void BuildOverlayShell(Transform parent)
        {
            var layer = new GameObject("Storm Blocks Screen Layer");
            layer.transform.SetParent(parent, false);
            _screenLayer = layer.AddComponent<RectTransform>();
            _screenLayer.anchorMin = Vector2.zero;
            _screenLayer.anchorMax = Vector2.one;
            _screenLayer.offsetMin = Vector2.zero;
            _screenLayer.offsetMax = Vector2.zero;

            var scrim = new GameObject("Storm Screen Scrim");
            scrim.transform.SetParent(layer.transform, false);
            var scrimImage = scrim.AddComponent<Image>();
            scrimImage.color = new Color(0.03f, 0.04f, 0.12f, 0.78f);
            var scrimRect = scrim.GetComponent<RectTransform>();
            scrimRect.anchorMin = Vector2.zero;
            scrimRect.anchorMax = Vector2.one;
            scrimRect.offsetMin = Vector2.zero;
            scrimRect.offsetMax = Vector2.zero;

            var panel = new GameObject("Storm Blocks Modal Panel");
            panel.transform.SetParent(layer.transform, false);
            var panelImage = panel.AddComponent<Image>();
            panelImage.sprite = _modalPanelSprite;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(0.22f, 0.18f, 0.42f, 0.96f);
            AddGraphicShadow(panelImage, new Color(0.02f, 0.01f, 0.06f, 0.46f), new Vector2(0f, -10f));
            _screenPanel = panel.GetComponent<RectTransform>();
            _screenPanel.anchorMin = new Vector2(0.5f, 0.5f);
            _screenPanel.anchorMax = new Vector2(0.5f, 0.5f);
            _screenPanel.anchoredPosition = new Vector2(0f, 20f);
            _screenPanel.sizeDelta = new Vector2(970f, 1360f);

            layer.SetActive(false);
        }

        private void ShowHomeScreen()
        {
            OpenScreen();
            CreateScreenText("Storm Blocks", new Vector2(0f, 570f), new Vector2(830f, 90f), 58f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Daily " + ModeConfigFactory.ToDateKey(DateTime.UtcNow) + "    Streak " + _profile.DailyStreak + "    Best " + _profile.BestEndlessScore, new Vector2(0f, 500f), new Vector2(850f, 60f), 30f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));

            CreateScreenButton("Endless Storm", new Vector2(-245f, 375f), new Vector2(390f, 90f), delegate { StartEndless(DateTime.UtcNow.TicksAsStableSeed()); }, new Color(0.50f, 0.42f, 0.78f, 1f));
            CreateScreenButton("Daily Storm", new Vector2(245f, 375f), new Vector2(390f, 90f), delegate { StartDaily(true); }, new Color(0.85f, 0.48f, 0.24f, 1f));
            CreateScreenButton("Storm Trail", new Vector2(-245f, 260f), new Vector2(390f, 90f), ShowStormTrailScreen, new Color(0.38f, 0.56f, 0.82f, 1f));
            CreateScreenButton("Tempest Trials", new Vector2(245f, 260f), new Vector2(390f, 90f), ShowTempestScreen, new Color(0.35f, 0.32f, 0.66f, 1f));
            CreateScreenButton("Practice", new Vector2(-245f, 145f), new Vector2(390f, 90f), delegate { StartPractice(DateTime.UtcNow.TicksAsStableSeed()); }, new Color(0.32f, 0.64f, 0.70f, 1f));
            CreateScreenButton("Profile", new Vector2(245f, 145f), new Vector2(390f, 90f), ShowProfileScreen, new Color(0.52f, 0.45f, 0.73f, 1f));
            CreateScreenButton("Cosmetics", new Vector2(-245f, 30f), new Vector2(390f, 90f), ShowCosmeticsScreen, new Color(0.70f, 0.43f, 0.68f, 1f));
            CreateScreenButton("Achievements", new Vector2(245f, 30f), new Vector2(390f, 90f), ShowAchievementsScreen, new Color(0.56f, 0.44f, 0.76f, 1f));
            CreateScreenButton("Settings", new Vector2(-245f, -85f), new Vector2(390f, 90f), ShowSettingsScreen, new Color(0.40f, 0.40f, 0.58f, 1f));
            CreateScreenButton("Accessibility", new Vector2(245f, -85f), new Vector2(390f, 90f), ShowAccessibilityScreen, new Color(0.36f, 0.50f, 0.62f, 1f));
            CreateScreenButton("Credits", new Vector2(-245f, -200f), new Vector2(390f, 90f), ShowCreditsScreen, new Color(0.34f, 0.30f, 0.52f, 1f));
            CreateScreenButton("Close", new Vector2(245f, -200f), new Vector2(390f, 90f), HideOverlay, new Color(0.28f, 0.24f, 0.42f, 1f));

            CreateScreenText("No ads. No paid power. Progression is cosmetic-only.", new Vector2(0f, -555f), new Vector2(850f, 58f), 26f, TextAnchor.MiddleCenter, new Color(0.84f, 0.82f, 1f));
        }

        private void ShowStormTrailScreen()
        {
            var catalog = ModeConfigFactory.CreateStormTrailCatalog();
            if (_selectedTrailRegionIndex < 0 || _selectedTrailRegionIndex >= catalog.Count)
            {
                _selectedTrailRegionIndex = 0;
            }

            var selected = catalog[_selectedTrailRegionIndex];
            OpenScreen();
            CreateScreenText("Storm Trail", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Stars " + CountStormTrailStars(catalog) + " / 360", new Vector2(0f, 505f), new Vector2(830f, 55f), 28f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));

            for (int i = 0; i < catalog.Count; i++)
            {
                int index = i;
                int row = i / 3;
                int col = i % 3;
                var region = catalog[i];
                int stars = CountRegionStars(region);
                Color color = i == _selectedTrailRegionIndex ? new Color(0.82f, 0.48f, 0.24f, 1f) : new Color(0.40f, 0.42f, 0.68f, 1f);
                CreateScreenButton(region.RegionIndex + ". " + region.DisplayName + "\n" + stars + "/30", new Vector2(-300f + col * 300f, 390f - row * 82f), new Vector2(270f, 68f), delegate
                {
                    _selectedTrailRegionIndex = index;
                    ShowStormTrailScreen();
                }, color);
            }

            CreateScreenText(selected.DisplayName, new Vector2(0f, 55f), new Vector2(830f, 55f), 34f, TextAnchor.MiddleCenter, Color.white);
            for (int i = 0; i < selected.Levels.Count; i++)
            {
                int index = i;
                int row = i / 5;
                int col = i % 5;
                var level = selected.Levels[i];
                int stars = GetTrailStars(level.Id);
                Color color = stars >= 3 ? new Color(0.70f, 0.50f, 0.18f, 1f) : stars > 0 ? new Color(0.40f, 0.60f, 0.72f, 1f) : new Color(0.34f, 0.30f, 0.55f, 1f);
                CreateScreenButton(level.LevelIndex + "\n" + StarsText(stars), new Vector2(-360f + col * 180f, -35f - row * 88f), new Vector2(145f, 72f), delegate { StartStormTrailLevel(selected.Levels[index]); }, color);
            }

            CreateScreenButton("Next Level", new Vector2(-245f, -305f), new Vector2(390f, 88f), StartFirstStormTrailLevel, new Color(0.80f, 0.48f, 0.24f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -305f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowTempestScreen()
        {
            var week = ModeConfigFactory.CreateTempestWeek(DateTime.UtcNow);
            OpenScreen();
            CreateScreenText("Tempest Trials", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Week " + week.WeekKey + "    Total " + ProfileProgression.CalculateTempestWeekScore(_profile, week), new Vector2(0f, 505f), new Vector2(830f, 55f), 28f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));

            for (int i = 0; i < week.Runs.Count; i++)
            {
                var run = week.Runs[i];
                string key = ProfileProgression.TempestRecordKey(week.WeekKey, run.Id);
                bool complete = _profile.TempestTrialHistory.TryGetValue(key, out var record) && record.Completed;
                string score = complete ? "Best " + record.Score : "Open";
                Color color = complete ? new Color(0.68f, 0.50f, 0.22f, 1f) : new Color(0.38f, 0.36f, 0.66f, 1f);
                CreateScreenButton("Run " + run.RunIndex + "  " + run.Modifier + "  " + score, new Vector2(0f, 385f - i * 105f), new Vector2(780f, 82f), delegate { StartTempestRun(week, run); }, color);
            }

            string completion = ProfileProgression.IsTempestWeekComplete(_profile, week) ? "Complete" : "In Progress";
            CreateScreenText(completion + "    Badge " + week.CosmeticBadgeRewardId, new Vector2(0f, -210f), new Vector2(820f, 58f), 26f, TextAnchor.MiddleCenter, new Color(0.84f, 0.82f, 1f));
            CreateScreenButton("Next Run", new Vector2(-245f, -330f), new Vector2(390f, 88f), StartCurrentTempestTrial, new Color(0.80f, 0.48f, 0.24f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -330f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowProfileScreen()
        {
            OpenScreen();
            CreateScreenText("Profile", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText(
                "Runs " + _profile.TotalRuns +
                "\nBest Endless " + _profile.BestEndlessScore +
                "\nBest Daily " + _profile.BestDailyScore +
                "\nBest Tempest " + _profile.BestTempestWeeklyScore +
                "\nDaily Streak " + _profile.DailyStreak +
                "\nSurvivors " + _profile.TotalSurvivorsRescued +
                "\nStorm Tiles Cleared " + _profile.TotalStormTilesDestroyed,
                new Vector2(0f, 240f),
                new Vector2(800f, 420f),
                34f,
                TextAnchor.MiddleCenter,
                new Color(0.92f, 0.92f, 1f));

            CreateScreenText("Cosmetics " + _profile.UnlockedCosmetics.Count + " / " + CosmeticCatalog.CreateLaunchCatalog().Count + "    Achievements " + _profile.CompletedAchievements.Count + " / " + Enum.GetValues(typeof(AchievementId)).Length, new Vector2(0f, -80f), new Vector2(840f, 64f), 28f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));
            CreateScreenText(GameCenterStatusText(), new Vector2(0f, -145f), new Vector2(820f, 54f), 24f, TextAnchor.MiddleCenter, new Color(0.80f, 0.86f, 1f));
            CreateScreenButton("Cosmetics", new Vector2(-245f, -260f), new Vector2(390f, 84f), ShowCosmeticsScreen, new Color(0.70f, 0.43f, 0.68f, 1f));
            CreateScreenButton("Achievements", new Vector2(245f, -260f), new Vector2(390f, 84f), ShowAchievementsScreen, new Color(0.56f, 0.44f, 0.76f, 1f));
            CreateScreenButton("Leaderboards", new Vector2(-245f, -375f), new Vector2(390f, 84f), ShowGameCenterLeaderboards, new Color(0.38f, 0.56f, 0.82f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -375f), new Vector2(390f, 84f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowCosmeticsScreen()
        {
            var catalog = CosmeticCatalog.CreateLaunchCatalog();
            OpenScreen();
            CreateScreenText("Cosmetics", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Cosmetic-only unlocks and equips", new Vector2(0f, 505f), new Vector2(830f, 55f), 28f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));

            for (int i = 0; i < catalog.Count; i++)
            {
                var cosmetic = catalog[i];
                int row = i / 2;
                int col = i % 2;
                bool unlocked = _profile.UnlockedCosmetics.Contains(cosmetic.Id);
                bool equipped = _profile.EquippedCosmetics.TryGetValue(cosmetic.Type, out string equippedId) && equippedId == cosmetic.Id;
                string state = equipped ? "Equipped" : unlocked ? "Unlocked" : "Locked";
                Color color = equipped ? new Color(0.84f, 0.52f, 0.22f, 1f) : unlocked ? new Color(0.45f, 0.56f, 0.74f, 1f) : new Color(0.24f, 0.22f, 0.38f, 1f);
                CreateScreenButton(cosmetic.DisplayName + "\n" + state, new Vector2(-235f + col * 470f, 395f - row * 88f), new Vector2(420f, 72f), delegate
                {
                    if (_profile.UnlockedCosmetics.Contains(cosmetic.Id))
                    {
                        ProfileProgression.EquipCosmetic(_profile, cosmetic);
                        _saveService.SaveProfile(_profile);
                        _feedback.Play(AudioEventId.CosmeticUnlock);
                        ShowCosmeticsScreen();
                    }
                }, color);
            }

            CreateScreenButton("Profile", new Vector2(-245f, -360f), new Vector2(390f, 88f), ShowProfileScreen, new Color(0.52f, 0.45f, 0.73f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -360f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowAchievementsScreen()
        {
            OpenScreen();
            CreateScreenText("Achievements", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            AchievementId[] achievements = (AchievementId[])Enum.GetValues(typeof(AchievementId));
            for (int i = 0; i < achievements.Length; i++)
            {
                var achievement = achievements[i];
                bool complete = _profile.CompletedAchievements.Contains(achievement);
                string status = complete ? "Done" : "Open";
                Color color = complete ? new Color(0.76f, 0.52f, 0.22f, 1f) : new Color(0.34f, 0.32f, 0.54f, 1f);
                CreateScreenText(AchievementDisplay(achievement) + "    " + status, new Vector2(0f, 390f - i * 82f), new Vector2(780f, 62f), 32f, TextAnchor.MiddleCenter, color);
            }

            CreateScreenButton("Game Center", new Vector2(0f, -275f), new Vector2(390f, 80f), ShowGameCenterAchievements, new Color(0.38f, 0.56f, 0.82f, 1f));
            CreateScreenButton("Profile", new Vector2(-245f, -380f), new Vector2(390f, 82f), ShowProfileScreen, new Color(0.52f, 0.45f, 0.73f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -380f), new Vector2(390f, 82f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowSettingsScreen()
        {
            OpenScreen();
            CreateScreenText("Settings", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Volume " + Mathf.RoundToInt(_profile.Settings.MasterVolume * 100f) + "%", new Vector2(0f, 475f), new Vector2(820f, 60f), 32f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));
            CreateScreenButton("-", new Vector2(-130f, 385f), new Vector2(150f, 80f), delegate { AdjustVolume(-0.1f); }, new Color(0.32f, 0.28f, 0.48f, 1f));
            CreateScreenButton("+", new Vector2(130f, 385f), new Vector2(150f, 80f), delegate { AdjustVolume(0.1f); }, new Color(0.80f, 0.48f, 0.24f, 1f));

            CreateToggleButton("Music", _profile.Settings.MusicEnabled, new Vector2(-245f, 255f), delegate { _profile.Settings.MusicEnabled = !_profile.Settings.MusicEnabled; SaveSettingsAndRefresh(); });
            CreateToggleButton("Effects", _profile.Settings.EffectsEnabled, new Vector2(245f, 255f), delegate { _profile.Settings.EffectsEnabled = !_profile.Settings.EffectsEnabled; SaveSettingsAndRefresh(); });
            CreateToggleButton("Haptics", _profile.Settings.HapticsEnabled, new Vector2(-245f, 140f), delegate { _profile.Settings.HapticsEnabled = !_profile.Settings.HapticsEnabled; SaveSettingsAndRefresh(); });
            CreateToggleButton("Reduced Motion", _profile.Settings.ReducedMotion, new Vector2(245f, 140f), delegate { _profile.Settings.ReducedMotion = !_profile.Settings.ReducedMotion; SaveSettingsAndRefresh(); });
            CreateToggleButton("High Contrast", _profile.Settings.HighContrast, new Vector2(-245f, 25f), delegate { _profile.Settings.HighContrast = !_profile.Settings.HighContrast; SaveSettingsRebuildAndRefresh(); });
            CreateToggleButton("Color Safe", _profile.Settings.ColorblindFriendly, new Vector2(245f, 25f), delegate { _profile.Settings.ColorblindFriendly = !_profile.Settings.ColorblindFriendly; SaveSettingsRebuildAndRefresh(); });
            CreateToggleButton("Left Hand", _profile.Settings.LeftHandedMode, new Vector2(-245f, -90f), delegate { _profile.Settings.LeftHandedMode = !_profile.Settings.LeftHandedMode; SaveSettingsAndRefresh(); });
            CreateToggleButton("Large Text", _profile.Settings.LargeText, new Vector2(245f, -90f), delegate { _profile.Settings.LargeText = !_profile.Settings.LargeText; SaveSettingsAndRefresh(); });
            CreateScreenButton("Accessibility", new Vector2(-245f, -330f), new Vector2(390f, 88f), ShowAccessibilityScreen, new Color(0.36f, 0.50f, 0.62f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -330f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowAccessibilityScreen()
        {
            OpenScreen();
            CreateScreenText("Accessibility", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText("Board, motion, controls", new Vector2(0f, 500f), new Vector2(840f, 56f), 28f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));

            CreateToggleButton("Reduced Motion", _profile.Settings.ReducedMotion, new Vector2(-245f, 345f), delegate
            {
                _profile.Settings.ReducedMotion = !_profile.Settings.ReducedMotion;
                SaveAccessibilityAndRefresh(false);
            });
            CreateToggleButton("High Contrast", _profile.Settings.HighContrast, new Vector2(245f, 345f), delegate
            {
                _profile.Settings.HighContrast = !_profile.Settings.HighContrast;
                SaveAccessibilityAndRefresh(true);
            });
            CreateToggleButton("Color Safe", _profile.Settings.ColorblindFriendly, new Vector2(-245f, 230f), delegate
            {
                _profile.Settings.ColorblindFriendly = !_profile.Settings.ColorblindFriendly;
                SaveAccessibilityAndRefresh(true);
            });
            CreateToggleButton("Left Hand", _profile.Settings.LeftHandedMode, new Vector2(245f, 230f), delegate
            {
                _profile.Settings.LeftHandedMode = !_profile.Settings.LeftHandedMode;
                SaveAccessibilityAndRefresh(false);
            });
            CreateToggleButton("Large Text", _profile.Settings.LargeText, new Vector2(-245f, 115f), delegate
            {
                _profile.Settings.LargeText = !_profile.Settings.LargeText;
                SaveAccessibilityAndRefresh(false);
            });
            CreateToggleButton("Low Detail", _profile.Settings.LowDetailMode, new Vector2(245f, 115f), delegate
            {
                _profile.Settings.LowDetailMode = !_profile.Settings.LowDetailMode;
                SaveAccessibilityAndRefresh(true);
            });

            CreateScreenText("Motion " + (_profile.Settings.ReducedMotion ? "Reduced" : "Full") +
                "    Contrast " + (_profile.Settings.HighContrast ? "High" : "Standard") +
                "\nPalette " + (_profile.Settings.ColorblindFriendly ? "Color Safe" : "Toy Storm") +
                "    Tray " + (_profile.Settings.LeftHandedMode ? "Left" : "Right") +
                "\nDetail " + (UseLowDetailVisuals() ? "Low" : "Full"),
                new Vector2(0f, -90f),
                new Vector2(820f, 170f),
                28f,
                TextAnchor.MiddleCenter,
                new Color(0.90f, 0.92f, 1f));

            CreateScreenButton("Settings", new Vector2(-245f, -330f), new Vector2(390f, 88f), ShowSettingsScreen, new Color(0.40f, 0.40f, 0.58f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -330f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void ShowCreditsScreen()
        {
            OpenScreen();
            CreateScreenText("Credits", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText(
                "Storm Blocks\n\nOriginal puzzle game by Perlantir.\n\nMade with Unity.\n\nFree-first casual play.\nNo forced ads. No paid power. No loot boxes.\n\nThank you for protecting the camp.",
                new Vector2(0f, 155f),
                new Vector2(820f, 700f),
                32f,
                TextAnchor.MiddleCenter,
                new Color(0.92f, 0.92f, 1f));

            CreateScreenButton("Modes", new Vector2(-245f, -360f), new Vector2(390f, 88f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
            CreateScreenButton("Close", new Vector2(245f, -360f), new Vector2(390f, 88f), HideOverlay, new Color(0.28f, 0.24f, 0.42f, 1f));
        }

        private void ShowResultsScreen(RunSummary summary, string extraLine)
        {
            OpenScreen();
            CreateScreenText("Results", new Vector2(0f, 570f), new Vector2(830f, 84f), 54f, TextAnchor.MiddleCenter, Color.white);
            CreateScreenText(_modeDefinition.DisplayName, new Vector2(0f, 505f), new Vector2(830f, 55f), 30f, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f));
            CreateScreenText("Score " + summary.Score + "\nSurvivors " + summary.SurvivorsRescued + "\nPushback " + summary.StormTilesDestroyed + "\nBest Combo " + summary.BestCombo + (string.IsNullOrEmpty(extraLine) ? string.Empty : "\n" + extraLine), new Vector2(0f, 250f), new Vector2(810f, 350f), 38f, TextAnchor.MiddleCenter, new Color(0.94f, 0.94f, 1f));
            CreateScreenButton("Retry", new Vector2(-245f, -125f), new Vector2(390f, 90f), RetryLastMode, new Color(0.80f, 0.48f, 0.24f, 1f));
            CreateScreenButton("Share", new Vector2(245f, -125f), new Vector2(390f, 90f), delegate
            {
                _shareService.ShareRun(summary);
                ShowToast("Share ready", 1.0f);
            }, new Color(0.50f, 0.42f, 0.78f, 1f));
            CreateScreenButton("Daily", new Vector2(-245f, -240f), new Vector2(390f, 90f), delegate { StartDaily(true); }, new Color(0.85f, 0.48f, 0.24f, 1f));
            CreateScreenButton("Modes", new Vector2(245f, -240f), new Vector2(390f, 90f), ShowHomeScreen, new Color(0.32f, 0.28f, 0.48f, 1f));
        }

        private void OpenScreen()
        {
            if (_screenLayer == null || _screenPanel == null)
            {
                return;
            }

            _screenLayer.gameObject.SetActive(true);
            ClearTransform(_screenPanel);
        }

        private void HideOverlay()
        {
            if (_screenLayer != null)
            {
                _screenLayer.gameObject.SetActive(false);
            }
        }

        private Text CreateScreenText(string text, Vector2 anchoredPosition, Vector2 size, float fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject("Screen Text");
            textObject.transform.SetParent(_screenPanel, false);
            var tmp = textObject.AddComponent<Text>();
            tmp.text = text;
            tmp.font = UiFont();
            tmp.fontSize = Mathf.RoundToInt(ScreenFont(fontSize));
            tmp.fontStyle = FontStyle.Bold;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.horizontalOverflow = HorizontalWrapMode.Wrap;
            tmp.verticalOverflow = VerticalWrapMode.Overflow;
            tmp.resizeTextForBestFit = true;
            tmp.resizeTextMinSize = 14;
            tmp.resizeTextMaxSize = tmp.fontSize;
            StyleUiText(tmp, new Color(0.10f, 0.08f, 0.22f, 0.72f), new Vector2(0f, -3f));
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return tmp;
        }

        private void CreateScreenButton(string text, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action, Color color)
        {
            var buttonObject = new GameObject(text.Replace("\n", " ") + " Button");
            buttonObject.transform.SetParent(_screenPanel, false);
            var image = buttonObject.AddComponent<Image>();
            image.sprite = color.r > color.b ? _buttonWarmSprite : _buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            AddGraphicShadow(image, new Color(0.02f, 0.01f, 0.06f, 0.40f), new Vector2(0f, -5f));
            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = color + new Color(0.10f, 0.10f, 0.10f, 0f);
            colors.pressedColor = new Color(0.95f, 0.62f, 0.28f, 1f);
            button.colors = colors;
            button.onClick.AddListener(delegate
            {
                _feedback.Play(AudioEventId.UiTap);
                action();
            });

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var label = new GameObject("Label");
            label.transform.SetParent(buttonObject.transform, false);
            var tmp = label.AddComponent<Text>();
            tmp.text = text;
            tmp.font = UiFont();
            tmp.fontSize = Mathf.RoundToInt(ScreenFont(text.Contains("\n") ? 24f : 30f));
            tmp.fontStyle = FontStyle.Bold;
            tmp.alignment = TextAnchor.MiddleCenter;
            tmp.color = Color.white;
            tmp.horizontalOverflow = HorizontalWrapMode.Wrap;
            tmp.verticalOverflow = VerticalWrapMode.Overflow;
            tmp.resizeTextForBestFit = true;
            tmp.resizeTextMinSize = 12;
            tmp.resizeTextMaxSize = tmp.fontSize;
            StyleUiText(tmp, new Color(0.10f, 0.08f, 0.22f, 0.86f), new Vector2(0f, -3f));
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);
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

        private void CreateToggleButton(string label, bool enabled, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            CreateScreenButton(label + "\n" + (enabled ? "On" : "Off"), anchoredPosition, new Vector2(390f, 88f), action, enabled ? new Color(0.52f, 0.58f, 0.72f, 1f) : new Color(0.28f, 0.25f, 0.42f, 1f));
        }

        private float ScreenFont(float baseSize)
        {
            return _profile != null && _profile.Settings.LargeText ? baseSize + 6f : baseSize;
        }

        private bool UseLowDetailVisuals()
        {
            if (_profile != null && _profile.Settings.LowDetailMode)
            {
                return true;
            }

#if UNITY_IOS && !UNITY_EDITOR
            return (SystemInfo.systemMemorySize > 0 && SystemInfo.systemMemorySize <= 4096)
                || (SystemInfo.graphicsMemorySize > 0 && SystemInfo.graphicsMemorySize <= 1536)
                || (SystemInfo.processorCount > 0 && SystemInfo.processorCount <= 4);
#else
            return false;
#endif
        }

        private void SaveSettingsAndRefresh()
        {
            _saveService.SaveProfile(_profile);
            _feedback.Configure(_profile.Settings);
            ShowSettingsScreen();
        }

        private void SaveSettingsRebuildAndRefresh()
        {
            _saveService.SaveProfile(_profile);
            _feedback.Configure(_profile.Settings);
            CreateMaterials();
            RefreshBoard();
            RefreshTray();
            ShowSettingsScreen();
        }

        private void SaveAccessibilityAndRefresh(bool rebuildPresentation)
        {
            _saveService.SaveProfile(_profile);
            _feedback.Configure(_profile.Settings);
            if (rebuildPresentation)
            {
                CreateMaterials();
                RefreshBoard();
                RefreshTray();
            }

            ShowAccessibilityScreen();
        }

        private void AdjustVolume(float delta)
        {
            _profile.Settings.MasterVolume = Mathf.Clamp01(_profile.Settings.MasterVolume + delta);
            SaveSettingsAndRefresh();
        }

        private string GameCenterStatusText()
        {
            if (_gameCenter != null && _gameCenter.IsAvailable)
            {
                return "Game Center connected";
            }

            return "Game Center ready on iOS";
        }

        private void ShowGameCenterLeaderboards()
        {
            if (_gameCenter != null)
            {
                _gameCenter.ShowLeaderboardUI();
            }

            ShowToast(GameCenterStatusText(), 1.0f);
        }

        private void ShowGameCenterAchievements()
        {
            if (_gameCenter != null)
            {
                _gameCenter.ShowAchievementsUI();
            }

            ShowToast(GameCenterStatusText(), 1.0f);
        }

        private void RetryLastMode()
        {
            if (_modeDefinition == null)
            {
                StartEndless(DateTime.UtcNow.TicksAsStableSeed());
                return;
            }

            if (_activeTrailLevel != null)
            {
                StartStormTrailLevel(_activeTrailLevel);
            }
            else if (_activeTempestWeek != null && _activeTempestRun != null)
            {
                StartTempestRun(_activeTempestWeek, _activeTempestRun);
            }
            else
            {
                StartMode(_modeDefinition);
            }
        }

        private void ClearModeContext()
        {
            _activeTrailLevel = null;
            _activeTempestWeek = null;
            _activeTempestRun = null;
        }

        private StormTrailLevelDefinition GetNextStormTrailLevel()
        {
            var catalog = ModeConfigFactory.CreateStormTrailCatalog();
            for (int i = 0; i < catalog.Count; i++)
            {
                for (int j = 0; j < catalog[i].Levels.Count; j++)
                {
                    var level = catalog[i].Levels[j];
                    if (GetTrailStars(level.Id) < 3)
                    {
                        _selectedTrailRegionIndex = i;
                        return level;
                    }
                }
            }

            _selectedTrailRegionIndex = catalog.Count - 1;
            return catalog[catalog.Count - 1].Levels[catalog[catalog.Count - 1].Levels.Count - 1];
        }

        private TempestTrialRunDefinition GetNextTempestRun(TempestWeekDefinition week)
        {
            for (int i = 0; i < week.Runs.Count; i++)
            {
                string key = ProfileProgression.TempestRecordKey(week.WeekKey, week.Runs[i].Id);
                if (!_profile.TempestTrialHistory.TryGetValue(key, out var record) || !record.Completed)
                {
                    return week.Runs[i];
                }
            }

            return week.Runs[0];
        }

        private int CountStormTrailStars(IReadOnlyList<StormTrailRegionDefinition> catalog)
        {
            int total = 0;
            for (int i = 0; i < catalog.Count; i++)
            {
                total += CountRegionStars(catalog[i]);
            }

            return total;
        }

        private int CountRegionStars(StormTrailRegionDefinition region)
        {
            int total = 0;
            for (int i = 0; i < region.Levels.Count; i++)
            {
                total += GetTrailStars(region.Levels[i].Id);
            }

            return total;
        }

        private int GetTrailStars(string levelId)
        {
            return _profile.StormTrailStars.TryGetValue(levelId, out int stars) ? stars : 0;
        }

        private static string StarsText(int stars)
        {
            if (stars >= 3)
            {
                return "***";
            }

            if (stars == 2)
            {
                return "**";
            }

            if (stars == 1)
            {
                return "*";
            }

            return "-";
        }

        private static string AchievementDisplay(AchievementId achievement)
        {
            switch (achievement)
            {
                case AchievementId.FirstRescue:
                    return "First Rescue";
                case AchievementId.FirstPushback:
                    return "First Pushback";
                case AchievementId.ClutchSave:
                    return "Clutch Save";
                case AchievementId.DailyStreak3:
                    return "Daily Streak 3";
                case AchievementId.DailyStreak7:
                    return "Daily Streak 7";
                case AchievementId.StormTrailRegionComplete:
                    return "Storm Trail Region";
                case AchievementId.TempestTrialComplete:
                    return "Tempest Week";
                case AchievementId.CosmeticCollector:
                    return "Cosmetic Collector";
                default:
                    return achievement.ToString();
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(transform);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void RefreshBoard()
        {
            ClearPooledTransform(_boardContentRoot);
            if (_session == null || _session.State == null)
            {
                return;
            }

            var board = _session.State.Board;
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    var position = new GridPosition(x, y);
                    var cell = board.GetCell(position);
                    Material tileMaterial = board.IsCampVisualCell(position) ? _campTile : cell.Occupant == CellOccupant.Storm ? _stormTile : cell.IsStormWarning ? _warningTile : _emptyTile;
                    SetTileMaterial(x, y, tileMaterial);

                    Vector3 center = CellCenter(x, y);
                    if (cell.Occupant == CellOccupant.Storm)
                    {
                        BuildStormCell(center, x, y);
                    }
                    else if (cell.Occupant == CellOccupant.Block)
                    {
                        CreateBlockCell(_boardContentRoot, center + Vector3.up * 0.18f, BlockMaterial(cell.PieceId), 0.64f);
                    }

                    if (cell.HasSurvivor)
                    {
                        BuildSurvivor(_boardContentRoot, center + new Vector3(0.14f, 0.35f, -0.08f), cell.Occupant == CellOccupant.Block ? _survivorBlue : _survivorYellow);
                    }
                }
            }

            if (StormResolver.IsNearDeath(board, _session.State.Config.StormRules))
            {
                BuildNearDeathPresentation();
            }
        }

        private void SetTileMaterial(int x, int y, Material material)
        {
            var renderers = _tileObjects[x, y].GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private void RefreshTray()
        {
            _trayPieces.Clear();
            for (int i = _trayRoot.childCount - 1; i >= 0; i--)
            {
                var child = _trayRoot.GetChild(i);
                if (child.name != "Rounded purple tray base" && !child.name.StartsWith("Tray Static", StringComparison.Ordinal))
                {
                    ReleasePooledOrDestroy(child.gameObject);
                }
            }

            if (_session == null || _session.State == null)
            {
                return;
            }

            float[] centers = { -2.05f, 0f, 2.05f };
            for (int i = 0; i < _session.State.Queue.Count; i++)
            {
                var piece = _session.State.Queue[i];
                var root = new GameObject("Tray Piece " + i + " " + piece.Id).transform;
                root.SetParent(_trayRoot, false);
                root.localPosition = new Vector3(centers[i], 0.32f, 0f);
                BuildPieceCells(root, piece, Vector3.zero, BlockMaterial(piece.Id), 0.30f, 0.24f, 0.34f);
                _trayPieces.Add(new TrayPieceView
                {
                    QueueIndex = i,
                    Piece = piece,
                    Root = root,
                    Center = _trayRoot.TransformPoint(root.localPosition),
                    HalfSize = new Vector2(0.88f, 0.68f)
                });
            }
        }

        private void RefreshHud()
        {
            if (_session == null || _session.State == null)
            {
                return;
            }

            var state = _session.State;
            _scoreLabel.text = state.Score + "\nSCORE";
            _rescuedLabel.text = state.SurvivorsRescued + "\nRESCUED";
            _bestLabel.text = "BEST " + GetBestForCurrentMode();
            _modeLabel.text = _modeDefinition.DisplayName.ToUpperInvariant();
            _phaseLabel.text = state.IsGameOver ? "STORM REACHED CAMP" : state.Phase.ToString().ToUpperInvariant();
            if (_feedback != null)
            {
                bool nearDeath = StormResolver.IsNearDeath(state.Board, state.Config.StormRules);
                float intensity = nearDeath ? 1f : state.Phase == StormPhase.Panic ? 0.55f : state.Phase == StormPhase.Strategic ? 0.25f : 0f;
                _feedback.SetNearDeathIntensity(intensity);
            }
        }

        private int GetBestForCurrentMode()
        {
            if (_modeDefinition.Mode == GameModeId.DailyStorm)
            {
                return _profile.BestDailyScore;
            }

            if (_modeDefinition.Mode == GameModeId.TempestTrial)
            {
                return _profile.BestTempestWeeklyScore;
            }

            return _profile.BestEndlessScore;
        }

        private void HandlePointerInput()
        {
            if (_session == null || _session.State == null || _session.State.IsGameOver)
            {
                return;
            }

            if (!ReadPointer(out Vector2 screen, out bool began, out bool held, out bool ended))
            {
                return;
            }

            if (began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Vector3 world = ScreenToBoardPlane(screen);
                BeginDrag(world);
            }

            if (_dragQueueIndex >= 0 && held)
            {
                Vector3 world = ScreenToBoardPlane(screen);
                UpdateDrag(world);
            }

            if (_dragQueueIndex >= 0 && ended)
            {
                EndDrag();
            }
        }

        private bool ReadPointer(out Vector2 screen, out bool began, out bool held, out bool ended)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                screen = touch.position;
                began = touch.phase == TouchPhase.Began;
                ended = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                held = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || began || ended;
                return true;
            }

            screen = Input.mousePosition;
            began = Input.GetMouseButtonDown(0);
            held = Input.GetMouseButton(0);
            ended = Input.GetMouseButtonUp(0);
            return began || held || ended;
        }

        private void BeginDrag(Vector3 world)
        {
            for (int i = 0; i < _trayPieces.Count; i++)
            {
                var tray = _trayPieces[i];
                if (Mathf.Abs(world.x - tray.Center.x) <= tray.HalfSize.x && Mathf.Abs(world.z - tray.Center.z) <= tray.HalfSize.y)
                {
                    _dragQueueIndex = tray.QueueIndex;
                    _dragRoot = new GameObject("Dragging Piece").transform;
                    _dragRoot.SetParent(transform);
                    BuildPieceCells(_dragRoot, tray.Piece, Vector3.zero, BlockMaterial(tray.Piece.Id), 0.64f, 0.28f, CellPitch);
                    _ghostRoot = new GameObject("Placement Ghost").transform;
                    _ghostRoot.SetParent(transform);
                    _feedback.Play(AudioEventId.PiecePickup);
                    _feedback.Play(HapticEventId.LightTap);
                    UpdateDrag(world);
                    return;
                }
            }
        }

        private void UpdateDrag(Vector3 world)
        {
            _dragRoot.position = new Vector3(world.x, 0.72f, world.z);
            _hasDragOrigin = TryWorldToGrid(world, out _currentDragOrigin);
            RefreshGhost();
        }

        private void RefreshGhost()
        {
            ClearPooledTransform(_ghostRoot);
            if (!_hasDragOrigin)
            {
                return;
            }

            var piece = _session.State.Queue[_dragQueueIndex];
            bool valid = PlacementRules.CanPlace(_session.State.Board, piece, _currentDragOrigin);
            BuildPieceCells(_ghostRoot, piece, CellCenter(_currentDragOrigin.X, _currentDragOrigin.Y) + Vector3.up * 0.06f, valid ? _ghostValid : _ghostInvalid, 0.66f, 0.06f, CellPitch);
        }

        private void EndDrag()
        {
            var result = _hasDragOrigin ? PlaceQueuedPiece(_dragQueueIndex, _currentDragOrigin) : PlacementResult.Failed("Piece was not released over the board.");
            if (!result.Success)
            {
                ShowToast("Try another spot", 0.75f);
            }

            ReleasePooledOrDestroy(_dragRoot.gameObject);
            ReleasePooledOrDestroy(_ghostRoot.gameObject);
            _dragRoot = null;
            _ghostRoot = null;
            _dragQueueIndex = -1;
            _hasDragOrigin = false;
        }

        private PlacementResult PlaceQueuedPiece(int queueIndex, GridPosition origin)
        {
            var result = _session.PlaceQueuedPiece(queueIndex, origin);
            if (!result.Success)
            {
                ShowToast("Blocked", 0.65f);
                return result;
            }

            ShowScoreFeedback(result);

            if (result.Clear.AutomaticPushbackTriggered)
            {
                SpawnPushbackFx(result.Clear);
            }

            _saveService.SaveRunSnapshot(StormRunSnapshot.FromState(_session.State));
            if (result.GameOver)
            {
                CompleteRun();
            }

            RefreshBoard();
            RefreshTray();
            RefreshHud();
            return result;
        }

        private void ShowScoreFeedback(PlacementResult result)
        {
            int points = result.Clear.Score.Total;
            if (points <= 0)
            {
                return;
            }

            if (result.Clear.ClutchSave)
            {
                ShowToast("Clutch Save +" + points, 1.1f);
            }
            else if (result.Clear.AutomaticPushbackTriggered)
            {
                ShowToast("Pushback +" + points, 1.0f);
            }
            else if (result.Clear.Lines.Count > 1)
            {
                ShowToast("Combo x" + result.Clear.Score.ComboMultiplier + " +" + points, 0.95f);
            }
            else if (result.Clear.Lines.Count > 0)
            {
                ShowToast("Clear +" + points, 0.85f);
            }
            else if (result.PerfectSet)
            {
                ShowToast("Perfect Set +" + points, 0.85f);
            }
        }

        private void CompleteRun()
        {
            var summary = RunSummary.FromRunState(_session.State, _modeDefinition.Mode == GameModeId.DailyStorm && _modeDefinition.IsOfficialAttempt);
            string resultLine = string.Empty;
            if (_modeDefinition.Mode == GameModeId.StormTrail && _activeTrailLevel != null)
            {
                int stars = ProfileProgression.ApplyStormTrailResult(_profile, _activeTrailLevel, summary, _achievements);
                resultLine = "Stars " + StarsText(stars);
            }
            else if (_modeDefinition.Mode == GameModeId.TempestTrial && _activeTempestWeek != null && _activeTempestRun != null)
            {
                int weeklyScore = ProfileProgression.ApplyTempestTrialResult(_profile, _activeTempestWeek, _activeTempestRun, summary, _leaderboards, _achievements);
                resultLine = "Weekly Total " + weeklyScore;
            }
            else
            {
                ProfileProgression.ApplyRunSummary(_profile, summary, DateTime.UtcNow, _leaderboards, _achievements);
            }

            _saveService.SaveProfile(_profile);
            _saveService.ClearRunSnapshot();
            _feedback.Play(_modeDefinition.Mode == GameModeId.DailyStorm ? AudioEventId.DailyStormEnd : AudioEventId.ResultsCelebration);
            ShowToast("Score " + summary.Score, 1.6f);
            _lastSummary = summary;
            ShowResultsScreen(summary, resultLine);
        }

        private void SpawnPushbackFx(ClearResolution clear)
        {
            ClearPooledTransform(_fxRoot);
            bool reducedMotion = _profile != null && _profile.Settings.ReducedMotion;
            bool lowDetail = UseLowDetailVisuals();
            if (!reducedMotion)
            {
                foreach (var line in clear.Lines)
                {
                    if (line.Kind == LineKind.Row)
                    {
                        for (int x = 0; x < BoardSize; x++)
                        {
                            CreatePooledCube("Gold pushback wave row", _fxRoot, CellCenter(x, line.Index) + Vector3.up * 0.46f, new Vector3(0.78f, 0.04f, 0.16f), _goldGlow);
                            if (!lowDetail)
                            {
                                CreatePooledCube("Cyan pushback wave row", _fxRoot, CellCenter(x, line.Index) + Vector3.up * 0.52f, new Vector3(0.36f, 0.035f, 0.09f), _stormLightning);
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < BoardSize; y++)
                        {
                            CreatePooledCube("Gold pushback wave column", _fxRoot, CellCenter(line.Index, y) + Vector3.up * 0.46f, new Vector3(0.16f, 0.04f, 0.78f), _goldGlow);
                            if (!lowDetail)
                            {
                                CreatePooledCube("Cyan pushback wave column", _fxRoot, CellCenter(line.Index, y) + Vector3.up * 0.52f, new Vector3(0.09f, 0.035f, 0.36f), _stormLightning);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < clear.StormTilesDestroyed.Count; i++)
            {
                Vector3 center = CellCenter(clear.StormTilesDestroyed[i].X, clear.StormTilesDestroyed[i].Y);
                CreatePooledSphere("Storm shatter flare", _fxRoot, center + Vector3.up * 0.60f, new Vector3(0.48f, 0.14f, 0.48f), _goldGlow);
                if (!lowDetail)
                {
                    CreateLightningBolt("Storm shatter lightning", _fxRoot, center + Vector3.up * 0.80f, 0.42f, 32f);
                }
            }

            _fxTimer = reducedMotion ? 0.35f : 0.8f;
        }

        private void UpdateTimers()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f && _toastLabel != null)
                {
                    _toastLabel.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }

            if (_fxTimer > 0f)
            {
                _fxTimer -= Time.deltaTime;
                if (_fxTimer <= 0f && _fxRoot != null)
                {
                    ClearPooledTransform(_fxRoot);
                }
            }
        }

        private void ShowToast(string text, float duration)
        {
            if (_toastLabel == null)
            {
                return;
            }

            _toastLabel.text = text;
            var panel = _toastLabel.gameObject.transform.parent.gameObject;
            panel.SetActive(true);
            panel.GetComponent<Image>().color = new Color(1.0f, 0.62f, 0.18f, 0.94f);
            _toastTimer = duration;
        }

        private void SeedSurvivors(BoardState board, ulong seed)
        {
            var rng = new DeterministicRandom(DailySeed.StableHash64(seed.ToString() + "|survivors"));
            int placed = 0;
            int attempts = 0;
            while (placed < 5 && attempts < 128)
            {
                attempts++;
                var position = new GridPosition(rng.NextInt(BoardSize), rng.NextInt(BoardSize));
                if (board.IsCampVisualCell(position) || board.GetCell(position).Occupant == CellOccupant.Storm || board.GetCell(position).HasSurvivor)
                {
                    continue;
                }

                board.SetSurvivor(position, true);
                placed++;
            }
        }

        private void BuildStormCell(Vector3 center, int x, int y)
        {
            bool lowDetail = UseLowDetailVisuals();
            if ((x + y) % 2 == 0)
            {
                CreatePooledSphere("Storm puff", _boardContentRoot, center + new Vector3(-0.17f, 0.30f, 0.04f), new Vector3(0.42f, 0.24f, 0.34f), _stormCloud);
                if (!lowDetail)
                {
                    CreatePooledSphere("Storm puff", _boardContentRoot, center + new Vector3(0.16f, 0.32f, 0.02f), new Vector3(0.48f, 0.28f, 0.38f), _stormCloud);
                }

                CreatePooledSphere("Storm puff glow", _boardContentRoot, center + new Vector3(0f, 0.38f, -0.10f), new Vector3(0.25f, 0.11f, 0.20f), _stormLightning);
            }

            if (!lowDetail && (x * 3 + y) % 5 == 0)
            {
                CreateLightningBolt("Storm cell lightning", _boardContentRoot, center + new Vector3(0.04f, 0.48f, -0.02f), 0.34f, -18f);
            }

            if (!lowDetail && (x + y) % 3 == 0)
            {
                var rain = CreatePooledCube("Storm rain streak", _boardContentRoot, center + new Vector3(-0.18f, 0.44f, -0.18f), new Vector3(0.035f, 0.025f, 0.34f), _stormRain);
                rain.transform.rotation = Quaternion.Euler(0f, 0f, -22f);
            }
        }

        private void BuildNearDeathPresentation()
        {
            CreatePooledCube("Near death pulse north", _boardContentRoot, new Vector3(0f, 0.56f, 3.36f), new Vector3(6.95f, 0.045f, 0.12f), _nearDeathGlow);
            CreatePooledCube("Near death pulse south", _boardContentRoot, new Vector3(0f, 0.56f, -3.36f), new Vector3(6.95f, 0.045f, 0.12f), _nearDeathGlow);
            CreatePooledCube("Near death pulse west", _boardContentRoot, new Vector3(-3.36f, 0.56f, 0f), new Vector3(0.12f, 0.045f, 6.95f), _nearDeathGlow);
            CreatePooledCube("Near death pulse east", _boardContentRoot, new Vector3(3.36f, 0.56f, 0f), new Vector3(0.12f, 0.045f, 6.95f), _nearDeathGlow);
            CreatePooledSphere("Near death camp pulse", _boardContentRoot, new Vector3(0f, 0.62f, 0f), new Vector3(1.80f, 0.10f, 1.80f), _campLight);
            CreateLightningBolt("Near death warning bolt west", _boardContentRoot, new Vector3(-3.18f, 0.78f, 1.9f), 0.58f, -18f);
            CreateLightningBolt("Near death warning bolt east", _boardContentRoot, new Vector3(3.18f, 0.78f, -1.9f), 0.58f, 18f);
        }

        private void BuildSurvivor(Transform parent, Vector3 center, Material outfit)
        {
            CreatePooledCube("Survivor raincoat body", parent, center + new Vector3(0f, -0.05f, 0.02f), new Vector3(0.24f, 0.30f, 0.16f), outfit);
            CreatePooledSphere("Survivor hood", parent, center + new Vector3(0f, 0.18f, 0.02f), new Vector3(0.27f, 0.27f, 0.27f), outfit);
            CreatePooledSphere("Survivor face", parent, center + new Vector3(0f, 0.18f, -0.14f), new Vector3(0.18f, 0.15f, 0.08f), _survivorFace);
            CreatePooledCube("Survivor left arm", parent, center + new Vector3(-0.16f, -0.02f, -0.03f), new Vector3(0.06f, 0.18f, 0.06f), outfit).transform.rotation = Quaternion.Euler(0f, 0f, -20f);
            CreatePooledCube("Survivor right arm", parent, center + new Vector3(0.16f, -0.02f, -0.03f), new Vector3(0.06f, 0.18f, 0.06f), outfit).transform.rotation = Quaternion.Euler(0f, 0f, 20f);
            CreatePooledCube("Survivor boots", parent, center + new Vector3(-0.065f, -0.24f, 0f), new Vector3(0.065f, 0.11f, 0.08f), _boardShadow);
            CreatePooledCube("Survivor boots", parent, center + new Vector3(0.065f, -0.24f, 0f), new Vector3(0.065f, 0.11f, 0.08f), _boardShadow);
        }

        private void BuildPieceCells(Transform parent, PieceDefinition piece, Vector3 origin, Material material, float footprint, float height, float pitch)
        {
            for (int i = 0; i < piece.Cells.Count; i++)
            {
                var cell = piece.Cells[i];
                Vector3 local = origin + new Vector3(cell.X * pitch, 0f, cell.Y * pitch);
                CreateBlockCell(parent, local, material, footprint, height);
            }
        }

        private void CreateBlockCell(Transform parent, Vector3 center, Material material, float footprint)
        {
            CreateBlockCell(parent, center, material, footprint, 0.30f);
        }

        private void CreateBlockCell(Transform parent, Vector3 center, Material material, float footprint, float height)
        {
            CreatePooledCube("Chunky toy block", parent, center, new Vector3(footprint, height, footprint), material);
            CreatePooledCube("Chunky toy block top bevel", parent, center + new Vector3(0f, height * 0.53f, 0f), new Vector3(footprint * 0.78f, height * 0.08f, footprint * 0.78f), material);
            if (!UseLowDetailVisuals())
            {
                CreatePooledSphere("Block highlight dot", parent, center + new Vector3(footprint * 0.24f, height * 0.55f, -footprint * 0.24f), new Vector3(footprint * 0.14f, height * 0.10f, footprint * 0.14f), _goldGlow);
            }
        }

        private void CreateLightningBolt(string objectName, Transform parent, Vector3 center, float height, float zRotation)
        {
            var top = CreatePooledCube(objectName + " top", parent, center + new Vector3(-0.045f, height * 0.15f, 0f), new Vector3(0.045f, height * 0.52f, 0.045f), _stormLightning);
            top.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
            var bottom = CreatePooledCube(objectName + " bottom", parent, center + new Vector3(0.055f, -height * 0.15f, 0f), new Vector3(0.045f, height * 0.52f, 0.045f), _stormLightning);
            bottom.transform.rotation = Quaternion.Euler(0f, 0f, zRotation + 42f);
        }

        private Material BlockMaterial(string pieceId)
        {
            int hash = 0;
            string value = pieceId ?? string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                hash = (hash * 31) + value[i];
            }

            return _blockMaterials[(int)((uint)hash % (uint)_blockMaterials.Length)];
        }

        private Vector3 ScreenToBoardPlane(Vector2 screen)
        {
            var ray = _camera.ScreenPointToRay(screen);
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        private static bool TryWorldToGrid(Vector3 world, out GridPosition position)
        {
            int x = Mathf.RoundToInt((world.x - BoardOrigin) / CellPitch);
            int y = Mathf.RoundToInt((world.z - BoardOrigin) / CellPitch);
            position = new GridPosition(x, y);
            return x >= 0 && y >= 0 && x < BoardSize && y < BoardSize;
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

        private GameObject CreateTentPrism(string objectName, Transform parent, Vector3 localPosition, float width, float height, float depth, Material material)
        {
            var vertices = new[]
            {
                new Vector3(-width * 0.5f, 0f, -depth * 0.5f),
                new Vector3(width * 0.5f, 0f, -depth * 0.5f),
                new Vector3(0f, height, -depth * 0.5f),
                new Vector3(-width * 0.5f, 0f, depth * 0.5f),
                new Vector3(width * 0.5f, 0f, depth * 0.5f),
                new Vector3(0f, height, depth * 0.5f)
            };

            var triangles = new[]
            {
                0, 2, 1,
                3, 4, 5,
                0, 3, 5,
                0, 5, 2,
                1, 2, 5,
                1, 5, 4,
                0, 1, 4,
                0, 4, 3
            };

            return CreateMeshObject(objectName, parent, localPosition, vertices, triangles, material);
        }

        private GameObject CreateTrianglePanel(string objectName, Transform parent, Vector3 localPosition, float width, float height, Material material)
        {
            var vertices = new[]
            {
                new Vector3(-width * 0.5f, 0f, 0f),
                new Vector3(width * 0.5f, 0f, 0f),
                new Vector3(0f, height, 0f)
            };
            var triangles = new[] { 0, 2, 1, 0, 1, 2 };
            return CreateMeshObject(objectName, parent, localPosition, vertices, triangles, material);
        }

        private GameObject CreateMeshObject(string objectName, Transform parent, Vector3 localPosition, Vector3[] vertices, int[] triangles, Material material)
        {
            var gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;

            var mesh = new Mesh
            {
                name = objectName + " Mesh",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return gameObject;
        }

        private GameObject CreatePooledCube(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreatePooledPrimitive(PrimitiveType.Cube, objectName, parent, localPosition, localScale, material);
        }

        private GameObject CreatePooledSphere(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreatePooledPrimitive(PrimitiveType.Sphere, objectName, parent, localPosition, localScale, material);
        }

        private GameObject CreatePooledPrimitive(PrimitiveType type, string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            if (!_primitivePools.TryGetValue(type, out var pool))
            {
                pool = new Stack<GameObject>();
                _primitivePools.Add(type, pool);
            }

            GameObject primitive = pool.Count > 0 ? pool.Pop() : CreatePoolablePrimitive(type);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            primitive.SetActive(true);
            return primitive;
        }

        private static GameObject CreatePoolablePrimitive(PrimitiveType type)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.AddComponent<PooledPrimitive>().Type = type;
            var collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return primitive;
        }

        private static Vector3 CellCenter(int x, int y)
        {
            return new Vector3(BoardOrigin + x * CellPitch, 0f, BoardOrigin + y * CellPitch);
        }

        private static void ClearTransform(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroyObject(root.GetChild(i).gameObject);
            }
        }

        private void ClearPooledTransform(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                ReleasePooledOrDestroy(root.GetChild(i).gameObject);
            }
        }

        private void ReleasePooledOrDestroy(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                ReleasePooledOrDestroy(gameObject.transform.GetChild(i).gameObject);
            }

            var pooled = gameObject.GetComponent<PooledPrimitive>();
            if (pooled == null)
            {
                DestroyObject(gameObject);
                return;
            }

            if (!_primitivePools.TryGetValue(pooled.Type, out var pool))
            {
                pool = new Stack<GameObject>();
                _primitivePools.Add(pooled.Type, pool);
            }

            gameObject.SetActive(false);
            gameObject.name = "Pooled " + pooled.Type;
            gameObject.transform.SetParent(_poolRoot != null ? _poolRoot : transform, false);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            gameObject.GetComponent<Renderer>().sharedMaterial = null;
            pool.Push(gameObject);
        }

        private static void DestroyObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private static void DestroyUnityObject(UnityEngine.Object unityObject)
        {
            if (unityObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(unityObject);
            }
            else
            {
                DestroyImmediate(unityObject);
            }
        }

        private struct TrayPieceView
        {
            public int QueueIndex;
            public PieceDefinition Piece;
            public Transform Root;
            public Vector3 Center;
            public Vector2 HalfSize;
        }

        private sealed class PooledPrimitive : MonoBehaviour
        {
            public PrimitiveType Type;
        }
    }

    internal static class StormBlocksDateSeedExtensions
    {
        public static ulong TicksAsStableSeed(this DateTime dateTime)
        {
            return DailySeed.StableHash64(dateTime.ToUniversalTime().Ticks.ToString());
        }

        public static ulong TicksAsStableSeed(this long ticks)
        {
            return DailySeed.StableHash64(ticks.ToString());
        }
    }
}
