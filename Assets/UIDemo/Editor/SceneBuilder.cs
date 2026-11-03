using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using Slider = UnityEngine.UI.Slider;

namespace UIDemo.EditorTools
{
    /// <summary>
    /// Generator scen demonstracyjnych + polecenia w górnym menu "UI Demo".
    /// Sceny, grafiki (sprite'y), asset UiSkin i Panel Settings są tworzone kodem,
    /// więc zawsze można je odtworzyć (UI Demo > Generuj wszystkie sceny).
    /// </summary>
    public static partial class SceneBuilder
    {
        public const string Root = "Assets/UIDemo";
        public const string ScenesDir = Root + "/Scenes";
        const string SpritesDir = Root + "/Sprites";
        const string DataDir = Root + "/Data";
        const string MatDir = Root + "/Materials";
        const string UiDir = Root + "/UI";
        const string ActionsPath = "Assets/InputSystem_Actions.inputactions";
        const string TmpEssentials = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        public const string BuildPath = "Builds/Windows/UIDemo.exe";

        public struct SceneDef
        {
            public string file;
            public Action build;
            public SceneDef(string file, Action build) { this.file = file; this.build = build; }
        }

        /// <summary>Lista scen w kolejności numeracji (uGUI, UX, UI Toolkit).</summary>
        public static readonly SceneDef[] Scenes =
        {
            new SceneDef("01_Canvas_TrybyRenderowania", S01_CanvasModes),
            new SceneDef("02_RectTransform_Kotwice", S02_Anchors),
            new SceneDef("03_CanvasScaler_Rozdzielczosc", S03_Scaler),
            new SceneDef("04_Image_Sprite_9Slice", S04_Images),
            new SceneDef("05_TextMeshPro_Tekst", S05_Text),
            new SceneDef("06_Kontrolki_Zdarzenia", S06_Controls),
            new SceneDef("07_Nawigacja_Klawiatura_Pad", S07_Navigation),
            new SceneDef("08_Layout_ScrollView", S08_Layout),
            new SceneDef("09_HUD_Feedback", S09_Hud),
            new SceneDef("10_WorldSpace_Diegeza", S10_WorldSpace),
            new SceneDef("11_Menu_Pauza_Przejscia", S11_Menu),
            new SceneDef("12_Ustawienia_PlayerPrefs", S12_Settings),
            new SceneDef("13_DragDrop_Tooltip", S13_DragDrop),
            new SceneDef("14_Prawa_UX_Fitts_Hick", S14_UxLaws),
            new SceneDef("15_Dostepnosc", S15_Accessibility),
            new SceneDef("16_UIToolkit_UXML_USS", S16_ToolkitMenu),
            new SceneDef("17_UIToolkit_ListView_Binding", S17_ToolkitList),
            new SceneDef("18_UIToolkit_WorldSpace", S18_ToolkitWorld),
        };

        public static string ScenePath(string file) => $"{ScenesDir}/{file}.unity";

        // =====================================================================
        //  MENU
        // =====================================================================

        [MenuItem("UI Demo/Generuj wszystkie sceny", priority = 0)]
        public static void GenerateAllMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!GenerateAll(GenerateAllMenu))
                return; // trwa import TMP Essential Resources – generowanie ruszy po imporcie
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorUtility.DisplayDialog("UI Demo",
                $"Wygenerowano {Scenes.Length} scen w {ScenesDir} i dodano je do Build Settings.\n\n" +
                "Naciśnij Play. Klawisze 1/2 przełączają sceny.", "OK");
        }

        [MenuItem("UI Demo/Panel scen (otwieranie i generowanie)", priority = 1)]
        public static void OpenBrowser() => SceneBrowserWindow.ShowWindow();

        [MenuItem("UI Demo/Otwórz pierwszą scenę i uruchom Play", priority = 2)]
        public static void OpenFirstAndPlay()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!File.Exists(ScenePath(Scenes[0].file)) && !GenerateAll(OpenFirstAndPlay))
                return;
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorApplication.isPlaying = true;
        }

        [MenuItem("UI Demo/Zbuduj wersję Windows (x64)", priority = 20)]
        public static void BuildWindowsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var report = BuildWindows();
            bool ok = report != null && report.summary.result == BuildResult.Succeeded;
            if (ok)
            {
                EditorUtility.DisplayDialog("UI Demo", $"Build zakończony powodzeniem:\n{Path.GetFullPath(BuildPath)}", "OK");
                EditorUtility.RevealInFinder(BuildPath);
            }
            else
            {
                EditorUtility.DisplayDialog("UI Demo", "Build nie powiódł się – szczegóły w oknie Console.", "OK");
            }
        }

        [MenuItem("UI Demo/Napraw ustawienia projektu (Input System, TMP, Build Settings)", priority = 21)]
        public static void FixProjectSettingsMenu()
        {
            FixProjectSettings();
            EnsureTmp(null);
            UpdateBuildSettings();
            EditorUtility.DisplayDialog("UI Demo",
                "Ustawiono:\n• Active Input Handling = Input System Package (New)\n" +
                "• Project-wide Actions = Assets/InputSystem_Actions.inputactions\n" +
                "• TMP Essential Resources (import, jeśli brak)\n• sceny 01–18 w Build Settings", "OK");
        }

        [MenuItem("UI Demo/Otwórz instrukcję (README.md)", priority = 40)]
        public static void OpenReadme()
        {
            string p = Path.GetFullPath("README.md");
            if (File.Exists(p))
                EditorUtility.OpenWithDefaultApp(p);
            else
                Debug.LogWarning("Nie znaleziono README.md w katalogu projektu.");
        }

        // =====================================================================
        //  BATCH MODE (wywołania z linii poleceń: -executeMethod ...)
        // =====================================================================

        /// <summary>Unity.exe -batchmode -projectPath ... -executeMethod UIDemo.EditorTools.SceneBuilder.BatchGenerate</summary>
        public static void BatchGenerate()
        {
            int code = 0;
            try
            {
                GenerateAll(null, force: true);
                Debug.Log("[UI Demo] BatchGenerate: OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        /// <summary>Generuje sceny i buduje wersję Windows x64.</summary>
        public static void BatchGenerateAndBuildWindows()
        {
            int code = 0;
            try
            {
                GenerateAll(null, force: true);
                var report = BuildWindows();
                if (report == null || report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[UI Demo] Build Windows: NIEPOWODZENIE " + (report != null ? report.summary.result.ToString() : "brak raportu"));
                    code = 2;
                }
                else
                {
                    Debug.Log($"[UI Demo] Build Windows: OK – {report.summary.totalSize / (1024 * 1024)} MB, błędów: {report.summary.totalErrors}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        // =====================================================================
        //  TextMeshPro – zasoby podstawowe
        // =====================================================================

        static Action s_AfterTmpImport;

        static bool TmpReady() => Resources.Load<TMP_Settings>("TMP Settings") != null;

        /// <summary>
        /// Teksty uGUI w projekcie to TextMeshPro. Domyślna czcionka i ustawienia TMP pochodzą z pakietu
        /// "TMP Essential Resources" (w uGUI 2.0). Jeśli ich brak – importujemy pakiet; gdy import nie zakończy się
        /// od razu, generowanie zostanie wznowione po imporcie (continuation).
        /// </summary>
        static bool EnsureTmp(Action continuation)
        {
            if (TmpReady())
                return true;
            Debug.Log("[UI Demo] Import TMP Essential Resources…");
            try
            {
                AssetDatabase.ImportPackage(Path.GetFullPath(TmpEssentials), false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[UI Demo] Nie udało się zaimportować TMP Essential Resources: " + e.Message +
                                 "\nUżyj Window > TextMeshPro > Import TMP Essential Resources.");
                return true;
            }
            if (TmpReady() || continuation == null)
                return true;
            s_AfterTmpImport = continuation;
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            AssetDatabase.importPackageCompleted += OnTmpImported;
            return false;
        }

        static void OnTmpImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            var next = s_AfterTmpImport;
            s_AfterTmpImport = null;
            if (next != null)
                EditorApplication.delayCall += () => next();
        }

        // =====================================================================
        //  GENEROWANIE
        // =====================================================================

        /// <summary>Generuje wszystkie sceny. Zwraca false, gdy trzeba poczekać na import TMP (wtedy wywoła continuation).</summary>
        public static bool GenerateAll(Action continuation = null, bool force = false)
        {
            FixProjectSettings();
            if (!EnsureTmp(force ? null : continuation))
                return false;
            EnsureFolders();
            EnsureSharedAssets();
            try
            {
                for (int i = 0; i < Scenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("UI Demo", "Generowanie " + Scenes[i].file, (float)i / Scenes.Length);
                    GenerateScene(Scenes[i], false);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public static void GenerateScene(SceneDef def, bool prepare = true)
        {
            if (prepare)
            {
                FixProjectSettings();
                EnsureTmp(null);
                EnsureFolders();
                EnsureSharedAssets();
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s_Hud = null;
            s_Camera = null;
            UiFactory.Skin = s_Skin;
            def.build();
            string path = ScenePath(def.file);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("[UI Demo] Zapisano scenę: " + path);
        }

        public static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var d in Scenes)
            {
                string p = ScenePath(d.file);
                if (File.Exists(p))
                    list.Add(new EditorBuildSettingsScene(p, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }

        public static BuildReport BuildWindows()
        {
            UpdateBuildSettings();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                GenerateAll(null, force: true);
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath));
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            return BuildPipeline.BuildPlayer(opts);
        }

        public static void FixProjectSettings()
        {
            // 1) Active Input Handling = Input System Package (New)
            var psAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
            if (psAsset != null)
            {
                var so = new SerializedObject(psAsset);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue == 0)
                {
                    prop.intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.LogWarning("[UI Demo] Ustawiono Active Input Handling = Input System Package (New). Uruchom ponownie edytor.");
                }
            }

            // 2) Project-wide actions = domyślny plik InputSystem_Actions
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            if (actions != null && InputSystem.actions != actions)
            {
                InputSystem.actions = actions;
                Debug.Log("[UI Demo] Ustawiono Project-wide Actions: " + ActionsPath);
            }
            else if (actions == null)
            {
                Debug.LogWarning("[UI Demo] Nie znaleziono " + ActionsPath + " – sterowanie nie będzie działać.");
            }
        }

        static void EnsureFolders()
        {
            foreach (var f in new[] { Root, ScenesDir, SpritesDir, DataDir, MatDir, UiDir })
            {
                if (AssetDatabase.IsValidFolder(f))
                    continue;
                string parent = Path.GetDirectoryName(f).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, Path.GetFileName(f));
            }
        }

        // =====================================================================
        //  ZASOBY WSPÓLNE: sprite'y, UiSkin, Panel Settings
        // =====================================================================

        static UiSkin s_Skin;
        static Sprite s_White;   // biały sprite – do obrazów typu Filled (paski)
        static PanelSettings s_PanelScreen;
        static PanelSettings s_PanelWorld;

        static void EnsureSharedAssets()
        {
            const int S = 96;      // zaokrąglony prostokąt 96×96, promień 22, obramowanie 9-slice 28 px
            const int I = 128;     // ikony
            string d = SpritesDir;
            string skinPath = DataDir + "/UiSkin.asset";
            s_Skin = AssetDatabase.LoadAssetAtPath<UiSkin>(skinPath);
            if (s_Skin == null)
            {
                s_Skin = ScriptableObject.CreateInstance<UiSkin>();
                AssetDatabase.CreateAsset(s_Skin, skinPath);
            }
            var border = new Vector4(28, 28, 28, 28);
            s_Skin.rounded = SpriteGenerator.Get(d, "ZaokraglonyProstokat", S, S, SpriteGenerator.RoundedRectFill(S, 22f), border);
            s_Skin.roundedOutline = SpriteGenerator.Get(d, "Ramka", S, S, SpriteGenerator.RoundedRectOutline(S, 22f, 5f), border);
            s_Skin.circle = SpriteGenerator.Get(d, "Kolo", I, I, SpriteGenerator.Circle(I));
            s_Skin.tile = SpriteGenerator.Get(d, "Wzor", 64, 64, SpriteGenerator.TilePattern(64), default, true);
            s_Skin.vignette = SpriteGenerator.Get(d, "Winieta", 256, 256, SpriteGenerator.Vignette(256));
            s_Skin.iconHeart = SpriteGenerator.Get(d, "Ikona_Serce", I, I, SpriteGenerator.Heart(I));
            s_Skin.iconStar = SpriteGenerator.Get(d, "Ikona_Gwiazda", I, I, SpriteGenerator.Star(I));
            s_Skin.iconTriangle = SpriteGenerator.Get(d, "Ikona_Trojkat", I, I, SpriteGenerator.Polygon(I,
                new[] { new Vector2(0.5f, 0.92f), new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.12f) }));
            s_Skin.iconSquare = SpriteGenerator.Get(d, "Ikona_Kwadrat", I, I, SpriteGenerator.Polygon(I,
                new[] { new Vector2(0.12f, 0.12f), new Vector2(0.12f, 0.88f), new Vector2(0.88f, 0.88f), new Vector2(0.88f, 0.12f) }));
            s_Skin.iconDiamond = SpriteGenerator.Get(d, "Ikona_Romb", I, I, SpriteGenerator.Polygon(I,
                new[] { new Vector2(0.5f, 0.96f), new Vector2(0.04f, 0.5f), new Vector2(0.5f, 0.04f), new Vector2(0.96f, 0.5f) }));
            s_Skin.iconArrow = SpriteGenerator.Get(d, "Ikona_Strzalka", I, I, SpriteGenerator.Polygon(I,
                new[] { new Vector2(0.5f, 0.95f), new Vector2(0.12f, 0.35f), new Vector2(0.38f, 0.35f), new Vector2(0.38f, 0.05f),
                        new Vector2(0.62f, 0.05f), new Vector2(0.62f, 0.35f), new Vector2(0.88f, 0.35f) }));
            s_Skin.iconCheck = SpriteGenerator.Get(d, "Ikona_OK", I, I, SpriteGenerator.Stroke(I,
                new[] { new Vector2(0.16f, 0.52f), new Vector2(0.4f, 0.26f), new Vector2(0.86f, 0.76f) }, 0.07f));
            s_Skin.iconCross = SpriteGenerator.Get(d, "Ikona_Blad", I, I, SpriteGenerator.Cross(I, 0.07f));

            s_Skin.builtinBackground = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            s_Skin.builtinInputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            s_Skin.builtinKnob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            s_Skin.builtinCheckmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            s_Skin.builtinDropdownArrow = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
            s_Skin.builtinMask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            s_White = SpriteGenerator.Get(d, "Bialy", 8, 8, (x, y) => 1f);
            s_Skin.font = TMP_Settings.defaultFontAsset;
            EditorUtility.SetDirty(s_Skin);
            UiFactory.Skin = s_Skin;

            s_PanelScreen = PanelSettingsAsset("PanelSettings_Ekran", false);
            s_PanelWorld = PanelSettingsAsset("PanelSettings_Swiat", true);
            AssetDatabase.SaveAssets();
        }

        static ThemeStyleSheet RuntimeTheme()
        {
            string path = UiDir + "/UnityDefaultRuntimeTheme.tss";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "@import url(\"unity-theme://default\");\nVisualElement {}\n");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            return AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(path);
        }

        static PanelSettings PanelSettingsAsset(string name, bool world)
        {
            string path = $"{DataDir}/{name}.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(ps, path);
            }
            ps.themeStyleSheet = RuntimeTheme();
            ps.renderMode = world ? PanelRenderMode.WorldSpace : PanelRenderMode.ScreenSpaceOverlay;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.sortingOrder = 0;
            EditorUtility.SetDirty(ps);
            return ps;
        }

        static T LoadUi<T>(string file) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>($"{UiDir}/{file}");
            if (a == null)
                Debug.LogWarning($"[UI Demo] Nie znaleziono {UiDir}/{file}");
            return a;
        }

        // =====================================================================
        //  MATERIAŁY (sceny 3D)
        // =====================================================================

        static Shader LitShader
        {
            get
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                if (s == null) s = Shader.Find("Standard");
                return s;
            }
        }

        static Material Mat(string name, Color c)
        {
            string path = $"{MatDir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(LitShader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.3f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material M_Floor => Mat("Podloga", new Color(0.42f, 0.45f, 0.5f));
        static Material M_Red => Mat("Czerwony", new Color(0.86f, 0.27f, 0.23f));
        static Material M_Orange => Mat("Pomaranczowy", new Color(0.95f, 0.55f, 0.15f));
        static Material M_Green => Mat("Zielony", new Color(0.3f, 0.75f, 0.35f));
        static Material M_Blue => Mat("Niebieski", new Color(0.25f, 0.45f, 0.9f));
        static Material M_Purple => Mat("Fioletowy", new Color(0.6f, 0.35f, 0.85f));
        static Material M_Gray => Mat("Szary", new Color(0.55f, 0.57f, 0.6f));
        static Material M_Dark => Mat("Ciemny", new Color(0.16f, 0.17f, 0.2f));

        // =====================================================================
        //  SCENA, KAMERA, EVENT SYSTEM, HUD
        // =====================================================================

        static DemoHud s_Hud;
        static Camera s_Camera;

        static readonly Color ColTitle = new Color(1f, 0.82f, 0.4f);
        static readonly Color ColBackground = new Color(0.1f, 0.12f, 0.16f);

        /// <summary>
        /// Wspólny początek sceny: kamera, (światło i niebo dla scen 3D), Event System z Input System UI Input Module, HUD.
        /// </summary>
        static Camera Begin(string title, string description, string sceneHint, bool scene3D = false, bool eventSystem = true)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.46f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.21f, 0.2f);
            RenderSettings.skybox = scene3D ? AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat") : null;

            if (scene3D)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                light.color = new Color(1f, 0.97f, 0.92f);
                lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            }

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            s_Camera = camGo.AddComponent<Camera>();
            s_Camera.nearClipPlane = 0.1f;
            s_Camera.farClipPlane = 300f;
            s_Camera.fieldOfView = 60f;
            if (!scene3D)
            {
                s_Camera.clearFlags = CameraClearFlags.SolidColor;
                s_Camera.backgroundColor = ColBackground;
            }
            camGo.AddComponent<AudioListener>();

            if (eventSystem)
                CreateEventSystem();
            s_Hud = CreateHud(title, description, sceneHint);
            return s_Camera;
        }

        /// <summary>
        /// Event System + Input System UI Input Module z akcjami mapy "UI" z projektowego pliku akcji
        /// (Navigate, Submit, Cancel, Point, Click, RightClick, MiddleClick, ScrollWheel…).
        /// </summary>
        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var module = go.GetComponent<InputSystemUIInputModule>();
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            if (asset == null)
                return; // w trakcie gry moduł sam przypisze akcje domyślne (DefaultInputActions)
            var refs = AssetDatabase.LoadAllAssetsAtPath(ActionsPath).OfType<InputActionReference>().ToList();
            InputActionReference R(string map, string action) => refs.FirstOrDefault(r =>
                r != null && r.action != null && r.action.actionMap != null &&
                r.action.actionMap.name == map && r.action.name == action);
            var point = R("UI", "Point");
            if (point == null)
                return;
            module.actionsAsset = asset;
            module.point = point;
            module.leftClick = R("UI", "Click");
            module.rightClick = R("UI", "RightClick");
            module.middleClick = R("UI", "MiddleClick");
            module.scrollWheel = R("UI", "ScrollWheel");
            module.move = R("UI", "Navigate");
            module.submit = R("UI", "Submit");
            module.cancel = R("UI", "Cancel");
            module.trackedDevicePosition = R("UI", "TrackedDevicePosition");
            module.trackedDeviceOrientation = R("UI", "TrackedDeviceOrientation");
        }

        static DemoHud CreateHud(string title, string description, string hint)
        {
            var canvas = UiFactory.Canvas("HUD_Opis", 100, RenderMode.ScreenSpaceOverlay, null, raycaster: false);
            var root = canvas.transform;

            var panel = HudPanel("PanelOpisu", root, new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, -14), 520f);
            var titleText = UiFactory.Text("Tytul", panel, title, 26f, ColTitle, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            var descText = UiFactory.Text("Opis", panel, description, 18f, new Color(0.93f, 0.94f, 0.96f));
            descText.lineSpacing = 4f;

            var hintPanel = UiFactory.Rect("PasekPodpowiedzi", root);
            hintPanel.anchorMin = new Vector2(0, 0);
            hintPanel.anchorMax = new Vector2(1, 0);
            hintPanel.pivot = new Vector2(0.5f, 0);
            hintPanel.anchoredPosition = Vector2.zero;
            hintPanel.sizeDelta = new Vector2(0, 64);
            var hintBg = hintPanel.gameObject.AddComponent<Image>();
            hintBg.color = new Color(0.03f, 0.04f, 0.07f, 0.85f);
            hintBg.raycastTarget = false;
            UiFactory.VLayout(hintPanel.gameObject, 0f, 0, true, TextAnchor.MiddleCenter);
            ((VerticalLayoutGroup)hintPanel.GetComponent<VerticalLayoutGroup>()).padding = new RectOffset(16, 16, 8, 8);
            var hintText = UiFactory.Text("Podpowiedz", hintPanel, "", 18f, new Color(0.9f, 0.92f, 0.95f), TextAlignmentOptions.Center);

            var statusPanel = HudPanel("PanelStanu", root, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-14, -14), 480f);
            var statusText = UiFactory.Text("Stan", statusPanel, "", 17f, new Color(0.93f, 0.94f, 0.96f));
            statusPanel.gameObject.SetActive(false);

            var mgr = new GameObject("DemoManager");
            var hud = mgr.AddComponent<DemoHud>();
            hud.skin = s_Skin;
            hud.title = title;
            hud.description = description;
            hud.sceneHint = hint;
            hud.descriptionPanel = panel.gameObject;
            hud.titleText = titleText;
            hud.descriptionText = descText;
            hud.hintText = hintText;
            hud.statusPanel = statusPanel.gameObject;
            hud.statusText = statusText;
            hud.Refresh();
            return hud;
        }

        static RectTransform HudPanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, float width)
        {
            var img = UiFactory.Panel(name, parent, new Color(0.03f, 0.05f, 0.08f, 0.86f));
            var rt = img.rectTransform.At(anchor, pivot, pos, new Vector2(width, 100));
            UiFactory.VLayout(rt.gameObject, 6f, 16, true);
            return rt;
        }

        static T Manager<T>() where T : Component => s_Hud.gameObject.AddComponent<T>();

        // =====================================================================
        //  POMOCNICZE – UI
        // =====================================================================

        /// <summary>Canvas sceny demonstracyjnej (pod HUD-em).</summary>
        static RectTransform DemoCanvas(string name = "UI_Demo", int sortingOrder = 0)
            => (RectTransform)UiFactory.Canvas(name, sortingOrder).transform;

        /// <summary>Panel z nagłówkiem; zwraca prostokąt panelu (kotwica w środku ekranu).</summary>
        static RectTransform Card(string name, Transform parent, Vector2 pos, Vector2 size, string header = null,
            Color? color = null)
        {
            var img = UiFactory.Panel(name, parent, color ?? s_Skin.panel);
            var rt = img.rectTransform.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            if (!string.IsNullOrEmpty(header))
            {
                var h = UiFactory.Text("Naglowek", rt, header, 24f, ColTitle, TextAlignmentOptions.Top, FontStyles.Bold);
                h.rectTransform.Stretch(new Vector2(0, 1), new Vector2(1, 1), 12, -44, 12, 10);
            }
            return rt;
        }

        static TextMeshProUGUI Label(Transform parent, string text, Vector2 pos, Vector2 size, float fontSize = 22f,
            TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null)
        {
            var t = UiFactory.Text("Etykieta", parent, text, fontSize, color, align);
            t.rectTransform.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            return t;
        }

        static void Place(Component c, Vector2 pos, Vector2? size = null)
        {
            var rt = (RectTransform)c.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            if (size.HasValue) rt.sizeDelta = size.Value;
        }

        static T Feedback<T>(T selectable) where T : Selectable
        {
            selectable.gameObject.AddComponent<UiFeedback>();
            return selectable;
        }

        // =====================================================================
        //  POMOCNICZE – 3D
        // =====================================================================

        static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static GameObject Floor(float size = 60f)
            => Prim(PrimitiveType.Cube, "Podloga", new Vector3(0, -0.5f, 0), new Vector3(size, 1, size), M_Floor);

        /// <summary>Canvas World Space o rozmiarze w pikselach UI i skali (np. 0,01 → 1 px = 1 cm).</summary>
        static Canvas WorldCanvas(string name, Transform parent, Vector3 localPos, Vector2 sizePx, float scale)
        {
            var c = UiFactory.Canvas(name, 0, RenderMode.WorldSpace, s_Camera, raycaster: false);
            var rt = (RectTransform)c.transform;
            if (parent != null) rt.SetParent(parent, false);
            rt.localPosition = localPos;
            rt.sizeDelta = sizePx;
            rt.localScale = Vector3.one * scale;
            Object.DestroyImmediate(c.GetComponent<CanvasScaler>()); // Canvas Scaler nie dotyczy World Space
            return c;
        }
    }
}
