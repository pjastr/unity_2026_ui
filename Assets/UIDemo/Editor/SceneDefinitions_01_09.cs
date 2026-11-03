using System.Collections.Generic;
using TMPro;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo.EditorTools
{
    /// <summary>Sceny 01–09: podstawy uGUI (Canvas, RectTransform, Canvas Scaler, Image, TMP, kontrolki, nawigacja, układ, HUD).</summary>
    public static partial class SceneBuilder
    {
        const string HintKeys = "<b>Spacja</b> – {0}   <b>Shift</b> – {1}";

        static string Hint(string space, string shift) => string.Format(HintKeys, space, shift);

        // =====================================================================
        //  01 – Render Mode
        // =====================================================================
        static void S01_CanvasModes()
        {
            var cam = Begin("01 · Canvas – tryby renderowania",
                "<b>Canvas</b> to obszar, na którym rysowane są elementy uGUI. <b>Render Mode</b> określa, gdzie:\n" +
                "• <b>Screen Space – Overlay</b> – na wierzchu wszystkiego, bez kamery (HUD, menu),\n" +
                "• <b>Screen Space – Camera</b> – na płaszczyźnie przed kamerą (<i>Plane Distance</i>); obiekty 3D mogą zasłaniać UI, działa post-processing,\n" +
                "• <b>World Space</b> – Canvas jest obiektem sceny (pozycja, obrót, skala) – ekrany w świecie gry, VR.\n\n" +
                "Obserwuj sześcian: w trybie Camera przechodzi przez panel, w Overlay jest zawsze pod nim.\n" +
                "Ten panel opisu to osobny Canvas (Overlay, Sort Order = 100).",
                Hint("kolejny Render Mode", "kamera krąży / stoi"), scene3D: true);
            cam.transform.position = new Vector3(0f, 1.5f, 0f);
            cam.transform.rotation = Quaternion.identity;
            Floor();
            for (int i = 0; i < 5; i++)
                Prim(PrimitiveType.Cube, $"Slup_{i + 1}", new Vector3(-6f + i * 3f, 1.5f, 13f), new Vector3(1f, 3f, 1f), i % 2 == 0 ? M_Blue : M_Gray);
            var cube = Prim(PrimitiveType.Cube, "SzescianPrzelatujacy", new Vector3(1.6f, 1.3f, 6f), Vector3.one * 1.2f, M_Orange);

            var canvasRt = DemoCanvas();
            var card = Card("PanelUI", canvasRt, new Vector2(0f, -60f), new Vector2(820f, 440f), "Panel UI (Canvas demonstracyjny)");
            var info = Label(card, "Elementy uGUI: Image, TextMeshPro, Button, Slider.\nZmień tryb (Spacja) i porównaj zachowanie.",
                new Vector2(0f, 90f), new Vector2(760f, 90f), 24f);
            info.color = s_Skin.text;
            var b1 = Feedback(UiFactory.Button("PrzyciskA", card, "Przycisk A"));
            Place(b1, new Vector2(-170f, -10f), new Vector2(300f, 70f));
            var b2 = Feedback(UiFactory.Button("PrzyciskB", card, "Przycisk B"));
            Place(b2, new Vector2(170f, -10f), new Vector2(300f, 70f));
            var slider = UiFactory.Slider("Suwak", card, 0f, 1f, 0.6f);
            Place(slider, new Vector2(0f, -110f), new Vector2(640f, 34f));
            var icon = UiFactory.Icon("Ikona", card, s_Skin.iconStar, s_Skin.accent, 70f);
            Place(icon, new Vector2(330f, 160f));

            var worldPose = new GameObject("PozycjaWorldSpace").transform;
            worldPose.position = new Vector3(0f, 2.2f, 9f);
            worldPose.rotation = Quaternion.identity;

            var demo = Manager<CanvasModesDemo>();
            demo.demoCanvas = canvasRt.GetComponent<Canvas>();
            demo.cam = cam;
            demo.worldPose = worldPose;
            demo.movingCube = cube.transform;
        }

        // =====================================================================
        //  02 – RectTransform, kotwice
        // =====================================================================
        static void S02_Anchors()
        {
            Begin("02 · RectTransform – kotwice i pivot",
                "Ramka w środku udaje <b>ekran o zmiennym rozmiarze</b>. Elementy w środku mają różne ustawienia kotwic:\n" +
                "• <b>LewyGorny</b>, <b>LewyDolny</b>, <b>Srodek</b> – kotwice w jednym punkcie: stały rozmiar i stała odległość od punktu,\n" +
                "• <b>PasekGorny</b> – kotwice rozsunięte w poziomie: rozciąga się w szerz,\n" +
                "• <b>KolumnaPrawa</b> – kotwice rozsunięte w pionie,\n" +
                "• <b>Proporcjonalny</b> – kotwice (0,3; 0,15)–(0,6; 0,35) bez marginesów: rośnie proporcjonalnie.\n\n" +
                "Romby pokazują położenie anchorMin i anchorMax. W edytorze: zaznacz element i obejrzyj Rect Transform w Inspectorze " +
                "(menu kotwic + Alt / Shift).",
                Hint("kolejne proporcje rodzica", "płynna zmiana rozmiaru"));
            var canvasRt = DemoCanvas();

            var frameImg = UiFactory.Panel("Rodzic_Ramka", canvasRt, new Color(1f, 1f, 1f, 0.06f));
            var frame = frameImg.rectTransform.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(900f, 506f));
            var outline = UiFactory.Panel("Obramowanie", frame, s_Skin.accent);
            outline.sprite = s_Skin.roundedOutline;
            outline.rectTransform.Fill(-4f);
            var frameLabel = UiFactory.Text("OpisRamki", frame, "Rodzic", 22f, ColTitle, TextAlignmentOptions.Bottom);
            frameLabel.rectTransform.At(new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(600f, 34f));

            var items = new List<RectTransform>();
            var labels = new List<TMP_Text>();
            void Item(string name, Color c, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 offMin, Vector2 offMax)
            {
                var img = UiFactory.Panel(name, frame, c);
                var rt = img.rectTransform;
                rt.anchorMin = aMin;
                rt.anchorMax = aMax;
                rt.pivot = pivot;
                rt.offsetMin = offMin;
                rt.offsetMax = offMax;
                var t = UiFactory.Text("Opis", rt, name, 15f, Color.white, TextAlignmentOptions.Center);
                t.rectTransform.Fill(6f);
                t.enableAutoSizing = true;
                t.fontSizeMin = 8f;
                t.fontSizeMax = 15f;
                items.Add(rt);
                labels.Add(t);
            }
            // kotwica w punkcie: offsetMin/offsetMax = pozycja ± rozmiar względem kotwicy
            Item("LewyGorny", new Color(0.25f, 0.45f, 0.85f, 0.9f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -122), new Vector2(222, -12));
            Item("PasekGorny", new Color(0.2f, 0.6f, 0.62f, 0.9f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(236, -92), new Vector2(-206, -12));
            Item("KolumnaPrawa", new Color(0.55f, 0.35f, 0.8f, 0.9f), new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-194, 12), new Vector2(-12, -104));
            Item("Srodek", new Color(0.9f, 0.55f, 0.15f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-110, -50), new Vector2(110, 50));
            Item("LewyDolny", new Color(0.3f, 0.7f, 0.35f, 0.9f), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 12), new Vector2(222, 122));
            Item("Proporcjonalny", new Color(0.85f, 0.3f, 0.35f, 0.9f), new Vector2(0.3f, 0.15f), new Vector2(0.6f, 0.35f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var demo = Manager<AnchorsDemo>();
            demo.frame = frame;
            demo.frameLabel = frameLabel;
            demo.items = items.ToArray();
            demo.itemLabels = labels.ToArray();
            demo.markerSprite = s_Skin.iconDiamond;
        }

        // =====================================================================
        //  03 – Canvas Scaler
        // =====================================================================
        static void S03_Scaler()
        {
            Begin("03 · Canvas Scaler – rozdzielczość i skala",
                "Komponent <b>Canvas Scaler</b> (UI Scale Mode) decyduje, jak UI zmienia rozmiar przy innej rozdzielczości:\n" +
                "• <b>Constant Pixel Size</b> – stałe piksele: na 4K UI jest 2× mniejsze niż na Full HD,\n" +
                "• <b>Scale With Screen Size</b> – projektujemy dla 1920×1080 (Reference Resolution), a UI się skaluje; " +
                "<b>Match</b> 0 – dopasuj szerokość, 1 – wysokość, 0,5 – pośrednio (średnia w skali logarytmicznej),\n" +
                "• <b>Constant Physical Size</b> – stały rozmiar fizyczny (mm) wg DPI ekranu.\n\n" +
                "<b>Zmień rozmiar okna Game</b> (lista proporcji u góry okna: Free Aspect, 1280×720, 2560×1080…) i porównaj tryby. " +
                "Kwadraty mają 100×100 jednostek UI, narożne znaczniki są zakotwiczone w rogach Canvasa.",
                Hint("kolejny UI Scale Mode", "pokaż bezpieczny obszar (Screen.safeArea)"));
            var canvasRt = DemoCanvas();
            var canvas = canvasRt.GetComponent<Canvas>();

            var border = UiFactory.Panel("KrawedzCanvasa", canvasRt, new Color(0.4f, 0.75f, 1f, 0.8f));
            border.sprite = s_Skin.roundedOutline;
            border.rectTransform.Fill(4f);
            foreach (var corner in new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) })
            {
                var m = UiFactory.Panel($"Naroznik_{corner.x:0}{corner.y:0}", canvasRt, s_Skin.accent, false);
                m.rectTransform.At(corner, corner, Vector2.zero, new Vector2(40f, 40f));
            }

            var card = Card("Wzorce", canvasRt, new Vector2(0f, 40f), new Vector2(820f, 520f), "Elementy o stałym rozmiarze w jednostkach UI");
            for (int i = 0; i < 5; i++)
            {
                var sq = UiFactory.Panel($"Kwadrat_{i + 1}", card, i % 2 == 0 ? s_Skin.info : s_Skin.good);
                Place(sq, new Vector2(-300f + i * 150f, 90f), new Vector2(100f, 100f));
                var t = UiFactory.Text("Opis", sq.transform, "100 × 100", 18f, Color.white, TextAlignmentOptions.Center);
                t.rectTransform.Fill();
            }
            Label(card, "Tekst 26 px – minimum dla konsoli przy 1080p (Xbox Accessibility Guidelines)", new Vector2(0f, -40f), new Vector2(780f, 40f), 26f);
            Label(card, "Tekst 18 px – minimum dla PC przy 1080p", new Vector2(0f, -90f), new Vector2(780f, 30f), 18f);
            var btn = Feedback(UiFactory.Button("Przycisk", card, "Przycisk 300 × 64"));
            Place(btn, new Vector2(0f, -170f));

            var safe = UiFactory.Rect("BezpiecznyObszar", canvasRt);
            safe.Fill();
            safe.gameObject.AddComponent<SafeAreaFitter>();
            var safeImg = UiFactory.Panel("Obrys", safe, new Color(0.3f, 1f, 0.4f, 0.9f));
            safeImg.sprite = s_Skin.roundedOutline;
            safeImg.rectTransform.Fill(12f);
            var safeLabel = UiFactory.Text("Opis", safe, "Screen.safeArea", 24f, new Color(0.3f, 1f, 0.4f), TextAlignmentOptions.BottomRight);
            safeLabel.rectTransform.Fill(24f);
            safe.gameObject.SetActive(false);

            var demo = Manager<ScalerDemo>();
            demo.scaler = canvasRt.GetComponent<CanvasScaler>();
            demo.canvas = canvas;
            demo.safeAreaPanel = safe.gameObject;
        }

        // =====================================================================
        //  04 – Image
        // =====================================================================
        static void S04_Images()
        {
            Begin("04 · Image – Simple, Sliced, Tiled, Filled, maski",
                "Komponent <b>Image</b> wyświetla Sprite; <b>Image Type</b>:\n" +
                "• <b>Simple</b> – cały obraz rozciągnięty (zaokrąglone narożniki się zniekształcają),\n" +
                "• <b>Sliced</b> – <b>9-slice</b>: narożniki bez zmian, krawędzie i środek rozciągane; obramowanie (Border) ustawia się w Sprite Editorze – tu 28 px,\n" +
                "• <b>Tiled</b> – powtarzanie wzoru,\n" +
                "• <b>Filled</b> – częściowe wypełnienie (fillAmount): paski i wskaźniki odnowienia.\n\n" +
                "Na dole: <b>Rect Mask 2D</b> (prostokątne przycinanie, wydajne) i <b>Mask</b> (kształt z grafiki – koło).",
                Hint("animacja rozmiaru (włącz/wyłącz)", "kolejna metoda wypełnienia"));
            var canvasRt = DemoCanvas();
            var card = Card("Obrazy", canvasRt, new Vector2(0f, 120f), new Vector2(860f, 420f), "Ten sam sprite w różnych trybach");

            Image Box(string name, float x, string caption, Sprite sprite, Image.Type type, Color color)
            {
                var img = UiFactory.Panel(name, card, color, false);
                img.sprite = sprite;
                img.type = type;
                Place(img, new Vector2(x, -10f), new Vector2(190f, 190f));
                Label(card, caption, new Vector2(x, -150f), new Vector2(200f, 40f), 22f);
                return img;
            }
            var simple = Box("Simple", -315f, "Simple", s_Skin.rounded, Image.Type.Simple, s_Skin.info);
            var sliced = Box("Sliced", -105f, "Sliced (9-slice)", s_Skin.rounded, Image.Type.Sliced, new Color(0.2f, 0.7f, 0.7f));
            var tiled = Box("Tiled", 105f, "Tiled", s_Skin.tile, Image.Type.Tiled, new Color(0.9f, 0.9f, 1f));
            var bg = UiFactory.Icon("TloWypelnienia", card, s_Skin.circle, new Color(1f, 1f, 1f, 0.12f), 190f);
            Place(bg, new Vector2(315f, -10f));
            var filled = Box("Filled", 315f, "Filled", s_Skin.circle, Image.Type.Filled, s_Skin.accent);
            filled.fillMethod = Image.FillMethod.Radial360;

            // pasek wypełniany poziomo
            var barBg = UiFactory.Panel("PasekTlo", canvasRt, new Color(0f, 0f, 0f, 0.5f));
            Place(barBg, new Vector2(0f, -160f), new Vector2(600f, 36f));
            var bar = UiFactory.Panel("Pasek", barBg.transform, s_Skin.good, false);
            bar.sprite = s_White;
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.rectTransform.Fill(4f);
            Label(canvasRt, "Image Filled, Horizontal – pasek postępu", new Vector2(0f, -205f), new Vector2(600f, 32f), 20f);

            // Rect Mask 2D
            var maskCard = Card("RectMask2D", canvasRt, new Vector2(-220f, -340f), new Vector2(400f, 200f));
            var viewport = UiFactory.Rect("Okno", maskCard);
            viewport.Fill(14f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = UiFactory.Rect("Zawartosc", viewport);
            content.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 300f));
            var icons = new[] { s_Skin.iconStar, s_Skin.iconHeart, s_Skin.iconDiamond, s_Skin.iconTriangle, s_Skin.iconSquare };
            var colors = new[] { s_Skin.accent, s_Skin.bad, s_Skin.info, s_Skin.good, new Color(0.7f, 0.45f, 1f) };
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 6; c++)
            {
                int k = (r * 6 + c) % icons.Length;
                var ic = UiFactory.Icon($"Ikona_{r}_{c}", content, icons[k], colors[k], 70f);
                Place(ic, new Vector2(-265f + c * 106f, 100f - r * 100f));
            }
            Label(canvasRt, "Rect Mask 2D", new Vector2(-220f, -458f), new Vector2(300f, 30f), 20f);

            // Mask z kształtem koła
            var circleMask = UiFactory.Icon("MaskaKolo", canvasRt, s_Skin.circle, Color.white, 190f);
            Place(circleMask, new Vector2(220f, -340f));
            circleMask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var portrait = UiFactory.Rect("Portret", circleMask.transform);
            portrait.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 300f));
            var tileImg = portrait.gameObject.AddComponent<Image>();
            tileImg.sprite = s_Skin.tile;
            tileImg.type = Image.Type.Tiled;
            tileImg.color = new Color(0.35f, 0.55f, 0.95f);
            tileImg.raycastTarget = false;
            var face = UiFactory.Icon("Gwiazda", portrait, s_Skin.iconStar, s_Skin.accent, 150f);
            Place(face, Vector2.zero);
            Label(canvasRt, "Mask (sprite koła)", new Vector2(220f, -458f), new Vector2(300f, 30f), 20f);

            var demo = Manager<ImageTypesDemo>();
            demo.resizable = new[] { simple.rectTransform, sliced.rectTransform, tiled.rectTransform };
            demo.filled = filled;
            demo.filledBar = bar;
            demo.maskedContent = content;
            demo.portraitContent = portrait;
        }

        // =====================================================================
        //  05 – TextMeshPro
        // =====================================================================
        static void S05_Text()
        {
            Begin("05 · TextMeshPro – tekst w interfejsie",
                "<b>TextMeshPro</b> (w Unity 6 część pakietu uGUI 2.0) rysuje tekst metodą <b>SDF</b> – ostry przy każdej skali, " +
                "z obrysem liczonym w shaderze.\n" +
                "• u góry: znaczniki <b>rich text</b> (&lt;b&gt;, &lt;color&gt;, &lt;size&gt;, &lt;mark&gt;…),\n" +
                "• w środku: przyciski o <b>stałej szerokości</b> – ten sam napis po polsku, angielsku i niemiecku ma różną długość; " +
                "sprawdź tryby <b>Overflow</b>, <b>Ellipsis</b>, <b>Truncate</b>, <b>Auto Size</b> i zawijanie,\n" +
                "• na dole: obrys (Outline) i odstęp znaków zmieniane w trakcie gry.\n\n" +
                "Polskie znaki pochodzą z czcionki zastępczej (fallback) domyślnej czcionki TMP.",
                Hint("kolejny język (PL / EN / DE)", "kolejny tryb przepełnienia tekstu"));
            var canvasRt = DemoCanvas();

            var rich = Card("RichText", canvasRt, new Vector2(0f, 300f), new Vector2(840f, 300f), "Znaczniki rich text");
            var richText = UiFactory.Text("Przyklady", rich,
                "<b>pogrubienie</b>, <i>kursywa</i>, <u>podkreślenie</u>, <s>przekreślenie</s>\n" +
                "<color=#FF6B5B>czerwony</color>, <color=#5BC0FF>niebieski</color>, <color=#7CFC9A>zielony</color>\n" +
                "<size=70%>mniejszy</size>, normalny, <size=140%>większy</size>\n" +
                "<mark=#FFD24A55>wyróżnienie tła</mark>, <cspace=8>rozstrzelony</cspace>, X<sup>2</sup>, H<sub>2</sub>O\n" +
                "<font-weight=900>gruby</font-weight>  <alpha=#88>półprzezroczysty</alpha>  <smallcaps>Kapitaliki</smallcaps>",
                24f, null, TextAlignmentOptions.TopLeft);
            richText.rectTransform.Stretch(Vector2.zero, Vector2.one, 24, 14, 24, 54);

            var lang = Label(canvasRt, "Język", new Vector2(0f, 90f), new Vector2(840f, 40f), 24f);
            var boxes = new List<TMP_Text>();
            for (int i = 0; i < 4; i++)
            {
                var b = UiFactory.Panel($"PoleStale_{i + 1}", canvasRt, s_Skin.control);
                Place(b, new Vector2(i % 2 == 0 ? -170f : 170f, i < 2 ? 10f : -80f), new Vector2(300f, 70f));
                var t = UiFactory.Text("Tekst", b.transform, "", 30f, Color.white, TextAlignmentOptions.Center);
                t.rectTransform.Fill(10f);
                boxes.Add(t);
            }
            var outlineSample = Label(canvasRt, "Obrys SDF", new Vector2(0f, -250f), new Vector2(800f, 110f), 80f);
            outlineSample.fontStyle = FontStyles.Bold;
            outlineSample.color = s_Skin.accent;
            outlineSample.outlineColor = new Color32(30, 20, 10, 255);
            outlineSample.outlineWidth = 0.15f;
            Label(canvasRt, "<size=80%>outlineWidth i characterSpacing zmieniane w Update()</size>", new Vector2(0f, -330f), new Vector2(800f, 36f), 22f);

            var demo = Manager<TextDemo>();
            demo.fixedBoxes = boxes.ToArray();
            demo.languageLabel = lang;
            demo.outlineSample = outlineSample;
        }

        // =====================================================================
        //  06 – kontrolki
        // =====================================================================
        static void S06_Controls()
        {
            Begin("06 · Kontrolki i zdarzenia",
                "Kontrolki uGUI dziedziczą po <b>Selectable</b> (stany: normalny, najechany, zaznaczony, wciśnięty, nieaktywny):\n" +
                "• <b>Button</b> – onClick: pierwszy przycisk ma słuchacza dodanego w kodzie (AddListener), " +
                "drugi – zdarzenie zapisane w scenie (pole <b>On Click ()</b> w Inspectorze), trzeci jest nieaktywny,\n" +
                "• <b>Toggle</b>, <b>ToggleGroup</b> (wybór jednej opcji), <b>Slider</b>, <b>TMP_Dropdown</b>, <b>TMP_InputField</b> – onValueChanged, onEndEdit.\n\n" +
                "Zdarzenia trafiają do dziennika. Przyciski mają też dźwięk i powiększenie przy najechaniu (komponent UiFeedback).",
                Hint("kolejny typ przejścia (Transition)", "interactable włącz/wyłącz"));
            var canvasRt = DemoCanvas();
            var card = Card("Kontrolki", canvasRt, new Vector2(0f, 20f), new Vector2(860f, 860f), "Kontrolki uGUI");

            var demo = Manager<ControlsDemo>();

            var codeBtn = Feedback(UiFactory.Button("Przycisk_Kod", card, "Przycisk (kod)"));
            Place(codeBtn, new Vector2(-205f, 300f), new Vector2(360f, 64f));
            var inspBtn = Feedback(UiFactory.Button("Przycisk_Inspector", card, "Przycisk (Inspector)"));
            Place(inspBtn, new Vector2(-205f, 220f), new Vector2(360f, 64f));
            UnityEventTools.AddPersistentListener(inspBtn.onClick, demo.OnInspectorButton);
            var disBtn = Feedback(UiFactory.Button("Przycisk_Nieaktywny", card, "Nieaktywny"));
            Place(disBtn, new Vector2(-205f, 140f), new Vector2(360f, 64f));
            disBtn.interactable = false;

            var subs = Feedback(UiFactory.Toggle("Toggle_Napisy", card, "Napisy", true));
            Place(subs, new Vector2(-205f, 50f), new Vector2(360f, 44f));
            Label(card, "Poziom trudności (ToggleGroup):", new Vector2(-205f, -10f), new Vector2(360f, 34f), 22f, TextAlignmentOptions.Left);
            var groupGo = UiFactory.Rect("ToggleGroup", card);
            var group = groupGo.gameObject.AddComponent<ToggleGroup>();
            var diff = new List<Toggle>();
            string[] levels = { "Łatwy", "Normalny", "Trudny" };
            for (int i = 0; i < levels.Length; i++)
            {
                var t = Feedback(UiFactory.Toggle("Toggle_" + levels[i], card, levels[i], i == 1));
                Place(t, new Vector2(-205f, -60f - i * 52f), new Vector2(360f, 44f));
                t.group = group;
                diff.Add(t);
            }

            var volLabel = Label(card, "Głośność: 80%", new Vector2(205f, 310f), new Vector2(360f, 34f), 22f, TextAlignmentOptions.Left);
            var vol = Feedback(UiFactory.Slider("Slider_Glosnosc", card, 0f, 100f, 80f, true));
            Place(vol, new Vector2(205f, 265f), new Vector2(360f, 34f));
            Label(card, "Jakość grafiki (Dropdown):", new Vector2(205f, 200f), new Vector2(360f, 34f), 22f, TextAlignmentOptions.Left);
            var dd = Feedback(UiFactory.Dropdown("Dropdown_Jakosc", card, new List<string> { "Niska", "Średnia", "Wysoka", "Ultra" }));
            Place(dd, new Vector2(205f, 150f), new Vector2(360f, 52f));
            dd.value = 2;
            Label(card, "Imię gracza (InputField):", new Vector2(205f, 85f), new Vector2(360f, 34f), 22f, TextAlignmentOptions.Left);
            var input = Feedback(UiFactory.InputField("InputField_Imie", card, "Wpisz imię…"));
            Place(input, new Vector2(205f, 35f), new Vector2(360f, 52f));

            var result = Label(card, "Witaj, graczu!", new Vector2(0f, -300f), new Vector2(780f, 70f), 32f);
            result.color = s_Skin.accent;
            result.fontStyle = FontStyles.Bold;

            demo.codeButton = codeBtn;
            demo.inspectorButton = inspBtn;
            demo.disabledButton = disBtn;
            demo.subtitlesToggle = subs;
            demo.difficulty = diff.ToArray();
            demo.volumeSlider = vol;
            demo.volumeLabel = volLabel;
            demo.qualityDropdown = dd;
            demo.nameInput = input;
            demo.resultText = result;
            demo.highlightedSprite = s_Skin.roundedOutline;
            demo.controlsRoot = card;
        }

        // =====================================================================
        //  07 – nawigacja
        // =====================================================================
        static void S07_Navigation()
        {
            Begin("07 · Nawigacja klawiaturą i padem",
                "Bez myszy UI obsługuje się <b>zaznaczeniem</b> (EventSystem.currentSelectedGameObject):\n" +
                "• <b>strzałki / WASD / lewa gałka</b> – ruch zaznaczenia (akcja UI/Navigate),\n" +
                "• <b>Enter / przycisk A</b> – zatwierdzenie (UI/Submit), <b>Esc / przycisk B</b> – anulowanie (UI/Cancel).\n\n" +
                "Ustawienie <b>Navigation</b> każdego Selectable: <i>Automatic</i> (sąsiad wg położenia), <i>Horizontal</i>, <i>Vertical</i>, " +
                "<i>Explicit</i> (sąsiedzi wskazani ręcznie – tu z zawijaniem i przejściem do listy z prawej), <i>None</i>.\n" +
                "Po otwarciu menu trzeba zaznaczyć pierwszy element – inaczej pad „nie działa”. Shift usuwa zaznaczenie – sprawdź, co się stanie.",
                Hint("kolejny tryb Navigation siatki", "usuń zaznaczenie / automatyczne ponowne zaznaczanie"));
            var canvasRt = DemoCanvas();
            var card = Card("Menu", canvasRt, new Vector2(0f, 0f), new Vector2(880f, 520f), "Menu – siatka 3×3 i lista");

            string[] names = { "Nowa gra", "Wczytaj", "Opcje", "Sklep", "Mapa", "Dziennik", "Postać", "Ekwipunek", "Osiągnięcia" };
            var grid = new List<Selectable>();
            for (int i = 0; i < names.Length; i++)
            {
                int r = i / 3, c = i % 3;
                var b = Feedback(UiFactory.Button("Siatka_" + names[i], card, names[i], 24f, new Vector2(190f, 84f)));
                Place(b, new Vector2(-300f + c * 210f, 110f - r * 110f));
                grid.Add(b);
            }
            var side = new List<Selectable>();
            string[] sideNames = { "Pomoc", "Autorzy", "Wyjście" };
            for (int i = 0; i < sideNames.Length; i++)
            {
                var b = Feedback(UiFactory.Button("Lista_" + sideNames[i], card, sideNames[i], 24f, new Vector2(200f, 84f)));
                Place(b, new Vector2(320f, 110f - i * 110f));
                side.Add(b);
            }
            var frame = UiFactory.Panel("RamkaZaznaczenia", card, s_Skin.accent);
            frame.sprite = s_Skin.roundedOutline;
            Place(frame, Vector2.zero, new Vector2(200f, 100f));

            var demo = Manager<NavigationDemo>();
            demo.grid = grid.ToArray();
            demo.columns = 3;
            demo.sideList = side.ToArray();
            demo.firstSelected = grid[0].gameObject;
            demo.selectionFrame = frame.rectTransform;
        }

        // =====================================================================
        //  08 – układ automatyczny
        // =====================================================================
        static void S08_Layout()
        {
            Begin("08 · Układ automatyczny i Scroll View",
                "Grupy układu ustawiają elementy za nas:\n" +
                "• lista – <b>Vertical Layout Group</b> + <b>Content Size Fitter</b> w obiekcie Content przewijanym przez <b>ScrollRect</b>; " +
                "wiersz – <b>Horizontal Layout Group</b>, nazwa z <b>Layout Element</b> (Flexible Width = 1),\n" +
                "• siatka – <b>Grid Layout Group</b> (Cell Size, Spacing, Constraint),\n" +
                "• pasek narzędzi u góry – Horizontal Layout Group,\n" +
                "• dymek z prawej – Content Size Fitter dopasowuje wysokość tła do tekstu.\n\n" +
                "Widok przewijany = ScrollRect + Viewport z maską (Rect Mask 2D) + Content.",
                Hint("dodaj przedmiot", "ograniczenie siatki (Constraint)"));
            var canvasRt = DemoCanvas();

            // pasek narzędzi
            var toolbar = UiFactory.Panel("PasekNarzedzi", canvasRt, s_Skin.panel);
            Place(toolbar, new Vector2(0f, 440f), new Vector2(520f, 84f));
            var h = UiFactory.HLayout(toolbar.gameObject, 12f, 10, TextAnchor.MiddleCenter);
            var tools = new[] { s_Skin.iconHeart, s_Skin.iconStar, s_Skin.iconDiamond, s_Skin.iconTriangle, s_Skin.iconSquare };
            foreach (var t in tools)
            {
                var b = Feedback(UiFactory.Button("Narzedzie_" + t.name, toolbar.transform, "", 1f, new Vector2(80f, 64f)));
                var ic = UiFactory.Icon("Ikona", b.transform, t, s_Skin.accent, 44f);
                Place(ic, Vector2.zero);
            }

            // lista
            var listScroll = UiFactory.ScrollView("Lista", canvasRt, out var listContent);
            Place(listScroll, new Vector2(-235f, -20f), new Vector2(400f, 640f));
            UiFactory.VLayout(listContent.gameObject, 6f, 4, true);
            Label(canvasRt, "Lista (Vertical Layout Group)", new Vector2(-235f, 330f), new Vector2(400f, 34f), 22f);

            // siatka
            var gridScroll = UiFactory.ScrollView("Siatka", canvasRt, out var gridContent);
            Place(gridScroll, new Vector2(225f, -20f), new Vector2(440f, 640f));
            var grid = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(96f, 96f);
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(6, 6, 6, 6);
            grid.childAlignment = TextAnchor.UpperCenter;
            gridContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Label(canvasRt, "Siatka (Grid Layout Group)", new Vector2(225f, 330f), new Vector2(440f, 34f), 22f);

            // dymek z Content Size Fitter (prawy dolny obszar)
            var bubble = UiFactory.Panel("Dymek", canvasRt, new Color(0.95f, 0.95f, 0.98f, 0.97f));
            var brt = bubble.rectTransform.At(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 110f), new Vector2(420f, 80f));
            UiFactory.VLayout(brt.gameObject, 0f, 18, true);
            var bubbleText = UiFactory.Text("Tekst", brt, "Naciśnij Spację, aby dodać przedmiot.", 22f, new Color(0.1f, 0.1f, 0.14f));

            var demo = Manager<LayoutDemo>();
            demo.listContent = listContent;
            demo.gridContent = gridContent;
            demo.grid = grid;
            demo.listScroll = listScroll;
            demo.gridScroll = gridScroll;
            demo.bubbleText = bubbleText;
            demo.icons = tools;
        }

        // =====================================================================
        //  09 – HUD
        // =====================================================================
        static void S09_Hud()
        {
            Begin("09 · HUD i informacja zwrotna",
                "Elementy HUD i ich „juice”:\n" +
                "• <b>pasek zdrowia z opóźnionym śladem</b> – jasny ślad pokazuje, ile właśnie stracono; drżenie i pulsowanie przy niskim zdrowiu,\n" +
                "• <b>HUD adaptacyjny</b> – pełny pasek po chwili staje się półprzezroczysty,\n" +
                "• <b>liczby obrażeń</b> unoszące się nad celem (trafienia krytyczne – większe, żółte),\n" +
                "• <b>winieta</b> – czerwony błysk krawędzi ekranu,\n" +
                "• <b>odnowienie umiejętności</b> – Image Filled (Radial 360) i licznik sekund,\n" +
                "• <b>powiadomienia</b> (toasty) w kolejce u góry ekranu.",
                Hint("otrzymaj obrażenia", "leczenie (umiejętność z odnowieniem)"));
            var canvasRt = DemoCanvas();

            var vignette = UiFactory.Icon("Winieta", canvasRt, s_Skin.vignette, new Color(0.85f, 0.05f, 0.05f, 0f), 10f);
            vignette.preserveAspect = false;
            vignette.rectTransform.Fill();

            // "przeciwnik"
            var target = UiFactory.Icon("Przeciwnik", canvasRt, s_Skin.circle, new Color(0.35f, 0.4f, 0.55f), 200f);
            Place(target, new Vector2(0f, 40f));
            var face = UiFactory.Icon("Symbol", target.transform, s_Skin.iconStar, s_Skin.accent, 120f);
            Place(face, Vector2.zero);
            Label(canvasRt, "Cel", new Vector2(0f, -85f), new Vector2(200f, 34f), 24f);
            var floatRoot = UiFactory.Rect("LiczbyObrazen", canvasRt); // nad celem
            floatRoot.Fill();

            // pasek zdrowia – lewy dolny róg (nad paskiem podpowiedzi)
            var barRoot = UiFactory.Rect("PasekZdrowia", canvasRt);
            barRoot.At(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 100f), new Vector2(560f, 80f));
            var group = barRoot.gameObject.AddComponent<CanvasGroup>();
            var heart = UiFactory.Icon("Serce", barRoot, s_Skin.iconHeart, s_Skin.bad, 64f);
            heart.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(64f, 64f));
            var bar = UiFactory.Panel("Pasek", barRoot, new Color(0f, 0f, 0f, 0.65f));
            bar.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(480f, 46f));
            Image Fill(string name, Color c)
            {
                var img = UiFactory.Panel(name, bar.transform, c, false);
                img.sprite = s_White;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.rectTransform.Fill(5f);
                return img;
            }
            var ghost = Fill("Slad", new Color(1f, 0.92f, 0.75f, 0.9f));
            var front = Fill("Zdrowie", new Color(0.3f, 0.85f, 0.4f));
            var hpText = UiFactory.Text("Wartosc", bar.transform, "100 / 100", 24f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            hpText.rectTransform.Fill();
            hpText.outlineWidth = 0.2f;
            hpText.outlineColor = new Color32(0, 0, 0, 200);

            // umiejętność – dół, środek
            var ability = UiFactory.Panel("Umiejetnosc", canvasRt, s_Skin.panel);
            Place(ability, new Vector2(0f, -360f), new Vector2(120f, 120f));
            var abIcon = UiFactory.Icon("Ikona", ability.transform, s_Skin.iconHeart, Color.white, 80f);
            Place(abIcon, Vector2.zero);
            abIcon.color = new Color(0.4f, 1f, 0.5f);
            var cd = UiFactory.Icon("Odnowienie", ability.transform, s_Skin.circle, new Color(0f, 0f, 0f, 0.7f), 112f);
            Place(cd, Vector2.zero);
            cd.type = Image.Type.Filled;
            cd.fillMethod = Image.FillMethod.Radial360;
            cd.fillOrigin = (int)Image.Origin360.Top;
            cd.fillClockwise = false;
            cd.fillAmount = 0f;
            var cdText = UiFactory.Text("Licznik", ability.transform, "Shift", 26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            cdText.rectTransform.Fill();
            Label(canvasRt, "Leczenie", new Vector2(0f, -440f), new Vector2(200f, 30f), 20f);

            // powiadomienia – góra, środek
            var toasts = UiFactory.Rect("Powiadomienia", canvasRt);
            toasts.At(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(640f, 220f));
            var v = UiFactory.VLayout(toasts.gameObject, 8f, 0, false, TextAnchor.UpperCenter, controlWidth: true);
            v.childForceExpandWidth = false;

            var demo = Manager<HudDemo>();
            demo.healthFront = front;
            demo.healthGhost = ghost;
            demo.healthBar = bar.rectTransform;
            demo.healthText = hpText;
            demo.healthGroup = group;
            demo.abilityIcon = abIcon;
            demo.abilityCooldown = cd;
            demo.abilityText = cdText;
            demo.vignette = vignette;
            demo.target = target.rectTransform;
            demo.floatingRoot = floatRoot;
            demo.toastRoot = toasts;
        }
    }
}
