using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using Slider = UnityEngine.UI.Slider;

namespace UIDemo.EditorTools
{
    /// <summary>Sceny 10–18: UI w świecie, menu i pauza, ustawienia, drag &amp; drop, prawa UX, dostępność, UI Toolkit.</summary>
    public static partial class SceneBuilder
    {
        // =====================================================================
        //  10 – World Space, rodzaje UI
        // =====================================================================
        static void S10_WorldSpace()
        {
            var cam = Begin("10 · UI w przestrzeni świata",
                "Rodzaje interfejsu (Fagerholt i Lorentzon):\n" +
                "• <b>przestrzenny</b> – paski zdrowia nad przeciwnikami: Canvas <b>World Space</b> (skala 0,008) + komponent Billboard, " +
                "pasek pojawia się tylko po trafieniu,\n" +
                "• <b>diegetyczny</b> – ekran terminala: Canvas World Space na obiekcie, element świata gry,\n" +
                "• <b>niediegetyczny</b> – znacznik celu na Canvasie Overlay: pozycja z <b>Camera.WorldToScreenPoint</b>; " +
                "gdy cel jest poza ekranem, znacznik zostaje przy krawędzi ze strzałką.\n\n" +
                "Pomarańczowa kolumna to cel. Kamera krąży wokół areny.",
                Hint("trafienie losowego przeciwnika", "kamera krąży / stoi"), scene3D: true);
            Floor(50f);

            var enemies = new List<Transform>();
            var bars = new List<WorldHealthBar>();
            var positions = new[] { new Vector3(-4f, 1f, 2f), new Vector3(0f, 1f, -3f), new Vector3(4f, 1f, 1.5f) };
            var mats = new[] { M_Red, M_Purple, M_Blue };
            for (int i = 0; i < positions.Length; i++)
            {
                var e = Prim(PrimitiveType.Capsule, $"Przeciwnik_{i + 1}", positions[i], Vector3.one, mats[i]);
                enemies.Add(e.transform);
                var c = WorldCanvas("PasekZdrowia", e.transform, new Vector3(0f, 1.5f, 0f), new Vector2(240f, 44f), 0.008f);
                var group = c.gameObject.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                c.gameObject.AddComponent<Billboard>().cam = cam;
                var bg = UiFactory.Panel("Tlo", c.transform, new Color(0f, 0f, 0f, 0.7f));
                bg.rectTransform.Fill();
                Image Fill(string name, Color col)
                {
                    var img = UiFactory.Panel(name, bg.transform, col, false);
                    img.sprite = s_White;
                    img.type = Image.Type.Filled;
                    img.fillMethod = Image.FillMethod.Horizontal;
                    img.rectTransform.Fill(5f);
                    return img;
                }
                var ghost = Fill("Slad", new Color(1f, 0.9f, 0.7f));
                var fill = Fill("Zdrowie", new Color(0.9f, 0.25f, 0.2f));
                var label = UiFactory.Text("Wartosc", bg.transform, "100", 26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                label.rectTransform.Fill();
                var bar = c.gameObject.AddComponent<WorldHealthBar>();
                bar.fill = fill;
                bar.ghost = ghost;
                bar.label = label;
                bar.group = group;
                bars.Add(bar);
            }

            // terminal – UI diegetyczne
            var terminal = Prim(PrimitiveType.Cube, "Terminal", new Vector3(-7f, 1.3f, -3f), new Vector3(2.6f, 2.6f, 0.4f), M_Dark);
            var screen = WorldCanvas("EkranTerminala", null, Vector3.zero, new Vector2(420f, 280f), 0.0055f);
            screen.transform.position = new Vector3(-7f, 1.45f, -2.78f);
            screen.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var screenBg = UiFactory.Panel("Tlo", screen.transform, new Color(0.02f, 0.08f, 0.04f, 1f));
            screenBg.rectTransform.Fill();
            var termText = UiFactory.Text("Tekst", screenBg.transform, "TERMINAL", 30f, new Color(0.5f, 1f, 0.6f));
            termText.rectTransform.Fill(18f);

            // cel
            var goal = Prim(PrimitiveType.Cylinder, "Cel", new Vector3(14f, 2f, 10f), new Vector3(1f, 2f, 1f), M_Orange);

            // znacznik celu na Canvasie Overlay
            var canvasRt = DemoCanvas();
            var marker = UiFactory.Rect("ZnacznikCelu", canvasRt);
            marker.anchorMin = marker.anchorMax = Vector2.zero; // pozycja ustawiana w pikselach ekranu (transform.position)
            marker.sizeDelta = new Vector2(120f, 120f);
            var diamond = UiFactory.Icon("Romb", marker, s_Skin.iconDiamond, s_Skin.accent, 46f);
            Place(diamond, new Vector2(0f, 10f));
            var dist = UiFactory.Text("Odleglosc", marker, "Cel", 22f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            dist.rectTransform.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(200f, 30f));
            dist.outlineWidth = 0.25f;
            dist.outlineColor = new Color32(0, 0, 0, 220);
            var arrowRoot = UiFactory.Rect("Strzalka", marker);
            arrowRoot.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(110f, 110f));
            var arrow = UiFactory.Icon("Grot", arrowRoot, s_Skin.iconArrow, s_Skin.accent, 40f);
            arrow.rectTransform.At(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(40f, 40f));

            var ind = Manager<OffscreenIndicator>();
            ind.target = goal.transform;
            ind.cam = cam;
            ind.marker = marker;
            ind.arrow = arrowRoot;
            ind.distanceText = dist;
            ind.worldOffset = new Vector3(0f, 2.6f, 0f);

            var demo = Manager<WorldSpaceDemo>();
            demo.cam = cam;
            demo.enemies = enemies.ToArray();
            demo.bars = bars.ToArray();
            demo.terminalText = termText;
            demo.indicator = ind;
        }

        // =====================================================================
        //  11 – menu, pauza
        // =====================================================================
        static void S11_Menu()
        {
            var cam = Begin("11 · Menu, pauza i przejścia",
                "• <b>Stos ekranów</b>: menu główne → ustawienia; <b>Esc</b> (UI/Cancel) zawsze cofa o jeden ekran, " +
                "a zaznaczenie wraca na przycisk, z którego przyszliśmy,\n" +
                "• <b>Graj</b> → rozgrywka; <b>Esc</b> lub <b>Spacja</b> – pauza: <b>Time.timeScale = 0</b>. " +
                "Lewy sześcian (Time.deltaTime) staje, prawy (Time.unscaledDeltaTime) obraca się dalej – tak samo animacje menu,\n" +
                "• przejścia: <b>CanvasGroup</b> (alpha, interactable, blocksRaycasts) animowane w korutynie,\n" +
                "• <b>Wyjdź</b> → okno potwierdzenia (modalne) – domyślnie zaznaczone bezpieczne „Nie”.",
                Hint("pauza / wznowienie (w rozgrywce)", "przejścia 0,25 s ↔ natychmiast"), scene3D: true);
            cam.transform.position = new Vector3(0f, 1.6f, -6f);
            cam.transform.LookAt(new Vector3(0f, 1f, 0f));
            Floor(30f);
            var spinA = Prim(PrimitiveType.Cube, "Szescian_deltaTime", new Vector3(-2.2f, 1.2f, 0f), Vector3.one * 1.4f, M_Blue);
            var spinB = Prim(PrimitiveType.Cube, "Szescian_unscaledDeltaTime", new Vector3(2.2f, 1.2f, 0f), Vector3.one * 1.4f, M_Green);

            var canvasRt = DemoCanvas();

            CanvasGroup Screen(string name, bool dim)
            {
                var root = UiFactory.Rect(name, canvasRt);
                root.Fill();
                var g = root.gameObject.AddComponent<CanvasGroup>();
                if (dim)
                {
                    var d = UiFactory.Panel("Przyciemnienie", root, new Color(0f, 0f, 0f, 0.6f), false);
                    d.rectTransform.Fill();
                    d.raycastTarget = true; // blokuje kliknięcia w to, co pod spodem
                }
                return g;
            }
            Button Btn(Transform parent, string name, string text, float y)
            {
                var b = Feedback(UiFactory.Button(name, parent, text, 28f, new Vector2(340f, 66f)));
                Place(b, new Vector2(0f, y));
                return b;
            }

            var hud = Screen("RozgrywkaHUD", false);
            var hudText = UiFactory.Text("Info", hud.transform, "<b>Rozgrywka</b>   Esc / Spacja – pauza", 26f, Color.white, TextAlignmentOptions.Center);
            hudText.rectTransform.At(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(700f, 44f));

            var main = Screen("MenuGlowne", false);
            var mainCard = Card("Karta", main.transform, new Vector2(0f, 0f), new Vector2(460f, 440f));
            Label(mainCard, "<b>Moja Gra</b>", new Vector2(0f, 150f), new Vector2(400f, 70f), 48f).color = s_Skin.accent;
            var play = Btn(mainCard, "Graj", "Graj", 50f);
            var settingsBtn = Btn(mainCard, "Ustawienia", "Ustawienia", -30f);
            var quit = Btn(mainCard, "Wyjdz", "Wyjdź", -110f);

            var settings = Screen("Ustawienia", false);
            var setCard = Card("Karta", settings.transform, Vector2.zero, new Vector2(560f, 420f), "Ustawienia");
            var t1 = Feedback(UiFactory.Toggle("Napisy", setCard, "Napisy", true));
            Place(t1, new Vector2(0f, 90f), new Vector2(380f, 44f));
            var t2 = Feedback(UiFactory.Toggle("Wibracje", setCard, "Wibracje pada", false));
            Place(t2, new Vector2(0f, 35f), new Vector2(380f, 44f));
            var s1 = Feedback(UiFactory.Slider("Glosnosc", setCard, 0f, 1f, 0.8f));
            Place(s1, new Vector2(0f, -30f), new Vector2(380f, 30f));
            var back = Btn(setCard, "Wroc", "Wróć (Esc)", -130f);

            var pause = Screen("Pauza", true);
            var pauseCard = Card("Karta", pause.transform, Vector2.zero, new Vector2(460f, 420f), "Pauza");
            var resume = Btn(pauseCard, "Wznow", "Wznów", 70f);
            var pauseSettings = Btn(pauseCard, "Ustawienia", "Ustawienia", -10f);
            var toMenu = Btn(pauseCard, "MenuGlowne", "Menu główne", -90f);

            var confirm = Screen("Potwierdzenie", true);
            var confCard = Card("Karta", confirm.transform, Vector2.zero, new Vector2(560f, 280f), "Wyjść z gry?");
            Label(confCard, "Niezapisany postęp zostanie utracony.", new Vector2(0f, 30f), new Vector2(500f, 40f), 24f);
            var yes = Feedback(UiFactory.Button("Tak", confCard, "Tak, wyjdź", 26f, new Vector2(220f, 64f)));
            Place(yes, new Vector2(-125f, -70f));
            var no = Feedback(UiFactory.Button("Nie", confCard, "Nie", 26f, new Vector2(220f, 64f)));
            Place(no, new Vector2(125f, -70f));

            var demo = Manager<MenuDemo>();
            demo.mainMenu = main;
            demo.settingsMenu = settings;
            demo.pauseMenu = pause;
            demo.confirmDialog = confirm;
            demo.gameplayHud = hud;
            demo.playButton = play;
            demo.settingsButton = settingsBtn;
            demo.quitButton = quit;
            demo.settingsBackButton = back;
            demo.resumeButton = resume;
            demo.pauseSettingsButton = pauseSettings;
            demo.pauseMenuButton = toMenu;
            demo.confirmYesButton = yes;
            demo.confirmNoButton = no;
            demo.scaledSpinner = spinA.transform;
            demo.unscaledSpinner = spinB.transform;
        }

        // =====================================================================
        //  12 – ustawienia, PlayerPrefs
        // =====================================================================
        static void S12_Settings()
        {
            Begin("12 · Menu ustawień i PlayerPrefs",
                "Ustawienia gracza zapisywane w <b>PlayerPrefs</b> (SetFloat/SetInt, GetFloat/GetInt z wartością domyślną):\n" +
                "• zmiany działają <b>od razu</b> (podgląd) – głośność, skala UI, rozmiar tekstu,\n" +
                "• <b>Skala UI</b> zmienia Reference Resolution Canvas Scalera,\n" +
                "• zmiana <b>rozdzielczości</b> wymaga potwierdzenia; bez reakcji – powrót po 10 s " +
                "(w edytorze Screen.SetResolution nie działa – sprawdź w buildzie),\n" +
                "• <b>Przywróć domyślne</b> usuwa klucze (DeleteKey).\n\n" +
                "Zrestartuj scenę (przytrzymaj E) – wartości zostaną wczytane z PlayerPrefs.",
                Hint("przywróć domyślne", "wczytaj ponownie z PlayerPrefs"));
            var canvasRt = DemoCanvas();
            var card = Card("Ustawienia", canvasRt, new Vector2(0f, 20f), new Vector2(860f, 880f), "Ustawienia");

            float y = 330f;
            void Row(string label) => Label(card, label, new Vector2(-250f, y), new Vector2(300f, 40f), 24f, TextAlignmentOptions.Left);

            Row("Głośność");
            var vol = Feedback(UiFactory.Slider("Glosnosc", card, 0f, 100f, 80f, true));
            Place(vol, new Vector2(80f, y), new Vector2(300f, 30f));
            var volVal = Label(card, "80%", new Vector2(270f, y), new Vector2(80f, 40f), 22f);
            var test = Feedback(UiFactory.Button("Test", card, "▶", 24f, new Vector2(56f, 48f)));
            Place(test, new Vector2(355f, y));
            y -= 80f;
            Row("Pełny ekran");
            var full = Feedback(UiFactory.Toggle("PelnyEkran", card, "włączony", true));
            Place(full, new Vector2(80f, y), new Vector2(300f, 44f));
            y -= 80f;
            Row("Rozdzielczość");
            var res = Feedback(UiFactory.Dropdown("Rozdzielczosc", card, new List<string> { "1920 × 1080" }));
            Place(res, new Vector2(140f, y), new Vector2(420f, 52f));
            y -= 80f;
            Row("Jakość grafiki");
            var q = Feedback(UiFactory.Dropdown("Jakosc", card, new List<string> { "PC" }));
            Place(q, new Vector2(140f, y), new Vector2(420f, 52f));
            y -= 80f;
            Row("Skala UI");
            var scale = Feedback(UiFactory.Slider("SkalaUI", card, 0.75f, 1.25f, 1f));
            Place(scale, new Vector2(80f, y), new Vector2(300f, 30f));
            var scaleVal = Label(card, "100%", new Vector2(285f, y), new Vector2(100f, 40f), 22f);
            y -= 80f;
            Row("Rozmiar tekstu");
            var ts = Feedback(UiFactory.Slider("RozmiarTekstu", card, 18f, 40f, 26f, true));
            Place(ts, new Vector2(80f, y), new Vector2(300f, 30f));
            y -= 80f;
            Row("Napisy");
            var subs = Feedback(UiFactory.Toggle("Napisy", card, "włączone", true));
            Place(subs, new Vector2(80f, y), new Vector2(300f, 44f));
            y -= 75f;
            var sample = Label(card, "Przykładowy napis: „Musimy dotrzeć do portu przed zmrokiem.”", new Vector2(0f, y), new Vector2(780f, 60f), 26f);
            var reset = Feedback(UiFactory.Button("Domyslne", card, "Przywróć domyślne", 24f, new Vector2(320f, 60f)));
            Place(reset, new Vector2(0f, -385f));

            // potwierdzenie zmiany rozdzielczości
            var confRoot = UiFactory.Rect("PotwierdzenieRozdzielczosci", canvasRt);
            confRoot.Fill();
            var confGroup = confRoot.gameObject.AddComponent<CanvasGroup>();
            var dim = UiFactory.Panel("Przyciemnienie", confRoot, new Color(0f, 0f, 0f, 0.6f), false);
            dim.rectTransform.Fill();
            dim.raycastTarget = true;
            var confCard = Card("Karta", confRoot, Vector2.zero, new Vector2(600f, 300f));
            var confText = Label(confCard, "Zachować?", new Vector2(0f, 50f), new Vector2(560f, 110f), 26f);
            var keep = Feedback(UiFactory.Button("Zachowaj", confCard, "Zachowaj", 26f, new Vector2(240f, 64f)));
            Place(keep, new Vector2(-135f, -80f));
            var revert = Feedback(UiFactory.Button("Przywroc", confCard, "Przywróć", 26f, new Vector2(240f, 64f)));
            Place(revert, new Vector2(135f, -80f));

            var demo = Manager<SettingsDemo>();
            demo.volume = vol;
            demo.volumeValue = volVal;
            demo.testSound = test;
            demo.fullscreen = full;
            demo.resolution = res;
            demo.quality = q;
            demo.uiScale = scale;
            demo.uiScaleValue = scaleVal;
            demo.settingsScaler = canvasRt.GetComponent<CanvasScaler>();
            demo.textSize = ts;
            demo.textSample = sample;
            demo.subtitles = subs;
            demo.resetButton = reset;
            demo.confirmPanel = confGroup;
            demo.confirmText = confText;
            demo.keepButton = keep;
            demo.revertButton = revert;
        }

        // =====================================================================
        //  13 – drag & drop, tooltip
        // =====================================================================
        static void S13_DragDrop()
        {
            Begin("13 · Przeciąganie i podpowiedzi",
                "Ekwipunek z <b>drag &amp; drop</b>:\n" +
                "• przedmiot – IBeginDragHandler, IDragHandler, IEndDragHandler; podczas przeciągania jest na wierzchu, " +
                "a CanvasGroup.blocksRaycasts = false, żeby slot pod kursorem dostał <b>OnDrop</b>,\n" +
                "• slot – IDropHandler: zajęty slot zamienia przedmioty miejscami, kosz usuwa przedmiot,\n" +
                "• upuszczenie poza slotem – powrót na miejsce.\n\n" +
                "<b>Podpowiedź</b> (tooltip) pojawia się po opóźnieniu, podąża za kursorem i nie wychodzi poza ekran.",
                Hint("sortuj automatycznie", "opóźnienie podpowiedzi 0 / 0,4 / 1 s"));
            var canvasRt = DemoCanvas();
            var card = Card("Ekwipunek", canvasRt, new Vector2(0f, 40f), new Vector2(820f, 600f), "Ekwipunek");

            var slots = new List<InventorySlot>();
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 4; c++)
            {
                var img = UiFactory.Panel($"Slot_{r * 4 + c + 1}", card, new Color(0.2f, 0.24f, 0.32f), true);
                img.raycastTarget = true;
                Place(img, new Vector2(-300f + c * 150f, 150f - r * 150f), new Vector2(130f, 130f));
                var slot = img.gameObject.AddComponent<InventorySlot>();
                slot.background = img;
                slots.Add(slot);
            }
            var trashImg = UiFactory.Panel("Kosz", canvasRt, new Color(0.35f, 0.18f, 0.18f), true);
            trashImg.raycastTarget = true;
            Place(trashImg, new Vector2(0f, -340f), new Vector2(300f, 90f));
            var trash = trashImg.gameObject.AddComponent<InventorySlot>();
            trash.isTrash = true;
            trash.background = trashImg;
            trash.normalColor = new Color(0.35f, 0.18f, 0.18f);
            var trashLabel = UiFactory.Text("Opis", trashImg.transform, "Kosz", 26f, Color.white, TextAlignmentOptions.Center);
            trashLabel.rectTransform.Fill();
            slots.Add(trash);

            var dragLayer = UiFactory.Rect("WarstwaPrzeciagania", canvasRt);
            dragLayer.Fill();

            var items = new (string name, string desc, Sprite icon, Color color)[]
            {
                ("Mikstura zdrowia", "Przywraca 50 punktów zdrowia.\n<color=#7CFC9A>Zwykły</color>", s_Skin.iconHeart, new Color(0.9f, 0.3f, 0.3f)),
                ("Gwiazda nocy", "Świeci w ciemności.\n<color=#5BC0FF>Rzadki</color>", s_Skin.iconStar, new Color(1f, 0.8f, 0.25f)),
                ("Kryształ many", "Przywraca 30 punktów many.\n<color=#C38BFF>Epicki</color>", s_Skin.iconDiamond, new Color(0.4f, 0.6f, 1f)),
                ("Amulet ochrony", "+10 do obrony.\n<color=#FFA028>Legendarny</color>", s_Skin.iconTriangle, new Color(0.4f, 0.85f, 0.5f)),
                ("Kamień runiczny", "Materiał do ulepszeń.\n<color=#7CFC9A>Zwykły</color>", s_Skin.iconSquare, new Color(0.7f, 0.5f, 1f)),
                ("Bandaż", "Zatrzymuje krwawienie.\n<color=#7CFC9A>Zwykły</color>", s_Skin.iconCross, new Color(0.9f, 0.9f, 0.9f)),
                ("Klucz", "Otwiera skrzynię w lochu.\n<color=#5BC0FF>Rzadki</color>", s_Skin.iconCheck, new Color(1f, 0.6f, 0.2f)),
            };
            int[] at = { 0, 1, 3, 4, 6, 9, 10 };
            for (int i = 0; i < items.Length; i++)
            {
                var slot = slots[at[i]];
                var img = UiFactory.Panel("Przedmiot_" + items[i].name, slot.transform, new Color(0.12f, 0.14f, 0.2f), true);
                img.raycastTarget = true;
                img.rectTransform.Fill(8f);
                img.gameObject.AddComponent<CanvasGroup>();
                var icon = UiFactory.Icon("Ikona", img.transform, items[i].icon, items[i].color, 72f);
                Place(icon, Vector2.zero);
                var it = img.gameObject.AddComponent<InventoryItem>();
                it.itemName = items[i].name;
                it.description = items[i].desc;
                it.dragLayer = dragLayer;
            }

            // podpowiedź
            var tip = UiFactory.Panel("Podpowiedz", canvasRt, new Color(0.04f, 0.05f, 0.08f, 0.96f));
            tip.rectTransform.At(Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(340f, 100f));
            UiFactory.VLayout(tip.gameObject, 4f, 14, true);
            var tipGroup = tip.gameObject.AddComponent<CanvasGroup>();
            var tipTitle = UiFactory.Text("Tytul", tip.transform, "Przedmiot", 24f, ColTitle, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            var tipBody = UiFactory.Text("Opis", tip.transform, "", 20f, new Color(0.88f, 0.9f, 0.94f));
            var tooltip = tip.gameObject.AddComponent<Tooltip>();
            tooltip.panel = tip.rectTransform;
            tooltip.group = tipGroup;
            tooltip.title = tipTitle;
            tooltip.body = tipBody;
            tooltip.canvas = canvasRt.GetComponent<Canvas>();

            var demo = Manager<DragDropDemo>();
            demo.slots = slots.ToArray();
            demo.tooltip = tooltip;
        }

        // =====================================================================
        //  14 – prawa UX
        // =====================================================================
        static void S14_UxLaws()
        {
            Begin("14 · Prawa Fittsa i Hicka – pomiar",
                "<b>Prawo Fittsa</b>: czas trafienia w cel T = a + b·log<sub>2</sub>(D/W + 1), gdzie D – odległość, W – szerokość celu. " +
                "Kliknij <b>Start</b>, a potem jak najszybciej pomarańczowe koło (3 rozmiary, 2 odległości).\n\n" +
                "<b>Prawo Hicka</b>: czas decyzji T = a + b·log<sub>2</sub>(n + 1), gdzie n – liczba opcji. " +
                "Kliknij <b>Start</b>, poczekaj na literę i kliknij przycisk z tą literą (n = 2, 4 lub 8).\n\n" +
                "Wyniki (średnie czasy i dopasowana prosta) są w panelu po prawej na dole. Kliknięcie obok celu liczy się jako błąd.",
                Hint("przełącz test Fitts ↔ Hick", "wyczyść wyniki"));
            var canvasRt = DemoCanvas();
            var arena = UiFactory.Panel("Arena", canvasRt, s_Skin.panel);
            Place(arena, new Vector2(0f, 20f), new Vector2(850f, 640f));
            arena.raycastTarget = true;
            var bg = arena.gameObject.AddComponent<Button>();
            bg.transition = Selectable.Transition.None;
            var nav = new Navigation { mode = Navigation.Mode.None };
            bg.navigation = nav;
            var title = UiFactory.Text("Tytul", arena.transform, "", 28f, ColTitle, TextAlignmentOptions.Top);
            title.rectTransform.Stretch(new Vector2(0, 1), new Vector2(1, 1), 20, -54, 20, 14);
            var prompt = UiFactory.Text("Polecenie", arena.transform, "", 30f, Color.white, TextAlignmentOptions.Top);
            prompt.rectTransform.Stretch(new Vector2(0, 1), new Vector2(1, 1), 20, -120, 20, 62);

            var results = UiFactory.Panel("Wyniki", canvasRt, new Color(0.03f, 0.05f, 0.08f, 0.86f));
            results.rectTransform.At(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 80f), new Vector2(440f, 100f));
            UiFactory.VLayout(results.gameObject, 4f, 14, true);
            var resText = UiFactory.Text("Tekst", results.transform, "", 18f, new Color(0.93f, 0.94f, 0.96f));
            resText.textWrappingMode = TextWrappingModes.NoWrap;

            var demo = Manager<UxLawsDemo>();
            demo.arena = arena.rectTransform;
            demo.arenaBackground = bg;
            demo.title = title;
            demo.prompt = prompt;
            demo.results = resText;
            demo.circle = s_Skin.circle;
        }

        // =====================================================================
        //  15 – dostępność
        // =====================================================================
        static void S15_Accessibility()
        {
            Begin("15 · Dostępność interfejsu",
                "Wybrane zalecenia (Game Accessibility Guidelines, Xbox Accessibility Guidelines):\n" +
                "• <b>skalowanie tekstu</b> do 200% bez utraty treści (wartość początkowa z AccessibilitySettings.fontScale),\n" +
                "• <b>wysoki kontrast</b> (≥ 7:1),\n" +
                "• informacja <b>nie tylko kolorem</b> – włącz „Kolor + kształt” i porównaj z symulacją daltonizmu (Spacja),\n" +
                "• <b>napisy</b> z tłem i nazwą mówiącego.\n\n" +
                "<b>Czytnik ekranu</b> (Unity 6.3: Narrator, VoiceOver, TalkBack): Shift tworzy <b>AccessibilityHierarchy</b> z węzłami " +
                "dla przycisków i suwaka. Włącz Narratora (Win + Ctrl + Enter) w buildzie i przejdź po elementach.",
                Hint("symulacja daltonizmu", "AccessibilityHierarchy dla czytnika ekranu"));
            var canvasRt = DemoCanvas();
            var scroll = UiFactory.ScrollView("Przewijanie", canvasRt, out var content);
            Place(scroll, new Vector2(0f, 20f), new Vector2(840f, 900f));
            var v = UiFactory.VLayout(content.gameObject, 12f, 20, true);
            v.childForceExpandWidth = true;

            TextMeshProUGUI Txt(string name, string text, float size, Color? c = null, FontStyles st = FontStyles.Normal)
            {
                var t = UiFactory.Text(name, content, text, size, c, TextAlignmentOptions.Left, st);
                return t;
            }
            Txt("Tytul", "Ustawienia dostępności", 32f, ColTitle, FontStyles.Bold);

            // skala tekstu
            var row = UiFactory.Rect("SkalaTekstu", content);
            UiFactory.Prefer(row.gameObject, -1f, 44f);
            var lbl = UiFactory.Text("Etykieta", row, "Skala tekstu", 24f, null, TextAlignmentOptions.MidlineLeft);
            lbl.rectTransform.Stretch(new Vector2(0, 0), new Vector2(0.35f, 1));
            var slider = Feedback(UiFactory.Slider("Suwak", row, 50f, 200f, 100f, true));
            ((RectTransform)slider.transform).Stretch(new Vector2(0.37f, 0.2f), new Vector2(0.82f, 0.8f));
            var val = UiFactory.Text("Wartosc", row, "100%", 24f, s_Skin.accent, TextAlignmentOptions.MidlineRight);
            val.rectTransform.Stretch(new Vector2(0.84f, 0), new Vector2(1, 1));

            Toggle Tg(string name, string text, bool on)
            {
                var t = Feedback(UiFactory.Toggle(name, content, text, on));
                UiFactory.Prefer(t.gameObject, -1f, 44f);
                return t;
            }
            var hc = Tg("WysokiKontrast", "Wysoki kontrast", false);
            var shape = Tg("KolorKsztalt", "Kolor + kształt (nie tylko kolor)", false);
            var subBox = Tg("TloNapisow", "Tło napisów", true);

            Txt("NaglowekMenu", "Menu", 26f, ColTitle, FontStyles.Bold);
            var buttons = new List<Button>();
            foreach (var n in new[] { "Nowa gra", "Wczytaj grę", "Ustawienia" })
            {
                var b = Feedback(UiFactory.Button("Przycisk_" + n, content, n, 26f));
                UiFactory.Prefer(b.gameObject, -1f, 60f);
                buttons.Add(b);
            }

            Txt("NaglowekStan", "Stan połączeń", 26f, ColTitle, FontStyles.Bold);
            var dots = new List<Image>();
            var icons = new List<Image>();
            var labels = new List<TMP_Text>();
            var names = new[] { "Serwer", "Zapis w chmurze", "Mikrofon" };
            var ok = new[] { true, false, true };
            for (int i = 0; i < names.Length; i++)
            {
                var r = UiFactory.Rect("Wskaznik_" + names[i], content);
                UiFactory.Prefer(r.gameObject, -1f, 40f);
                var dot = UiFactory.Icon("Kolor", r, s_Skin.circle, Color.white, 30f);
                dot.rectTransform.At(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(30f, 30f));
                var ic = UiFactory.Icon("Ksztalt", dot.transform, ok[i] ? s_Skin.iconCheck : s_Skin.iconCross, Color.white, 22f);
                Place(ic, Vector2.zero);
                var t = UiFactory.Text("Opis", r, names[i], 24f, null, TextAlignmentOptions.MidlineLeft);
                t.rectTransform.Stretch(Vector2.zero, Vector2.one, 44f, 0f, 0f, 0f);
                dots.Add(dot);
                icons.Add(ic);
                labels.Add(t);
            }

            Txt("NaglowekNapisy", "Napisy dialogów", 26f, ColTitle, FontStyles.Bold);
            var subBg = UiFactory.Panel("TloNapisu", content, new Color(0f, 0f, 0f, 0.75f));
            UiFactory.VLayout(subBg.gameObject, 0f, 14, false);
            var subText = UiFactory.Text("Napis", subBg.transform, "", 26f, Color.white, TextAlignmentOptions.Center);

            var demo = Manager<AccessibilityDemo>();
            demo.contentRoot = content;
            demo.panels = new[] { scroll.GetComponent<Image>() };
            demo.menuButtons = buttons.ToArray();
            demo.statusDots = dots.ToArray();
            demo.statusIcons = icons.ToArray();
            demo.statusLabels = labels.ToArray();
            demo.statusNames = names;
            demo.statusOk = ok;
            demo.subtitleBackground = subBg;
            demo.subtitleText = subText;
            demo.textScale = slider;
            demo.textScaleValue = val;
            demo.highContrast = hc;
            demo.shapeAndColor = shape;
            demo.subtitleBox = subBox;
            demo.iconOk = s_Skin.iconCheck;
            demo.iconError = s_Skin.iconCross;
        }

        // =====================================================================
        //  16–18 – UI Toolkit
        // =====================================================================
        static UIDocument Document(string name, PanelSettings ps, VisualTreeAsset uxml)
        {
            var go = new GameObject(name);
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = ps;
            doc.visualTreeAsset = uxml;
            return doc;
        }

        static void S16_ToolkitMenu()
        {
            Begin("16 · UI Toolkit – UXML, USS, C#",
                "<b>UI Toolkit</b>: struktura w pliku <b>UXML</b> (Assets/UIDemo/UI/MainMenu.uxml), wygląd w <b>USS</b> (MainMenu.uss), " +
                "logika w C# (ToolkitMenuDemo.cs). Komponent <b>UI Document</b> łączy UXML z <b>Panel Settings</b> " +
                "(skalowanie jak Canvas Scaler, motyw TSS).\n" +
                "• <b>:hover</b>, <b>:focus</b>, <b>:active</b> i przejścia <b>transition-*</b> – zdefiniowane w USS, bez kodu,\n" +
                "• C#: Q&lt;Button&gt;(\"play\").clicked, RegisterValueChangedCallback, RegisterCallback&lt;PointerEnterEvent&gt;,\n" +
                "• motyw jasny = drugi arkusz USS nadpisujący <b>zmienne</b> (--accent, --card…).\n\n" +
                "Otwórz pliki UXML/USS w <b>UI Builder</b> (dwuklik) i podejrzyj drzewo w <b>UI Toolkit Debugger</b>.",
                Hint("motyw jasny / ciemny", "klasa .compact (ToggleInClassList)"), eventSystem: false);
            var doc = Document("UIDocument_Menu", s_PanelScreen, LoadUi<VisualTreeAsset>("MainMenu.uxml"));
            var demo = doc.gameObject.AddComponent<ToolkitMenuDemo>();
            demo.lightTheme = LoadUi<StyleSheet>("MainMenuLight.uss");
        }

        static void S17_ToolkitList()
        {
            Begin("17 · UI Toolkit – ListView i wiązanie danych",
                "• <b>ListView</b> – 1000 przedmiotów, ale w drzewie UI istnieje tylko kilkanaście wierszy (wirtualizacja): " +
                "<b>makeItem</b> tworzy wiersz, <b>bindItem</b> wypełnia go danymi przy przewijaniu,\n" +
                "• <b>wiązanie danych</b> (runtime data binding, Unity 2023.2+): root.dataSource = obiekt PlayerStats; " +
                "Label.text, ProgressBar.value i title są aktualizowane automatycznie (<b>DataBinding</b>, tryb ToTarget),\n" +
                "• pole <b>Imię</b> – tryb <b>TwoWay</b>: wpisany tekst zmienia dane, a dane – nagłówek karty,\n" +
                "• PlayerStats: atrybut <b>[CreateProperty]</b> i interfejs <b>INotifyBindablePropertyChanged</b>.",
                Hint("obrażenia i złoto (zmiana danych)", "dodaj 100 przedmiotów"), eventSystem: false);
            var doc = Document("UIDocument_Ekwipunek", s_PanelScreen, LoadUi<VisualTreeAsset>("Inventory.uxml"));
            doc.gameObject.AddComponent<ToolkitListBindingDemo>();
        }

        static void S18_ToolkitWorld()
        {
            var cam = Begin("18 · UI Toolkit w przestrzeni świata",
                "Od Unity 6.2 panele UI Toolkit mogą być w przestrzeni 3D:\n" +
                "• asset <b>Panel Settings</b> (Assets/UIDemo/Data/PanelSettings_Swiat) – <b>Render Mode = World Space</b>,\n" +
                "• każdy panel to <b>UI Document</b> na obiekcie sceny: <b>World Space Size Mode = Fixed</b>, rozmiar 400×170 px " +
                "(100 px = 1 jednostka), skala obiektu 0,5 → panel 2 × 0,85 m,\n" +
                "• komponent Billboard obraca panele w stronę kamery.\n\n" +
                "Do klikania paneli w świecie potrzebny jest komponent <b>Panel Input Configuration</b> – tu panele tylko wyświetlają dane.",
                Hint("nowe wartości paneli", "panele zwrócone do kamery / stałe"), scene3D: true, eventSystem: false);
            Floor(40f);
            var uxml = LoadUi<VisualTreeAsset>("WorldPanel.uxml");
            var docs = new List<UIDocument>();
            var titles = new[] { "Wieża łuczników", "Kuźnia", "Magazyn" };
            var mats = new[] { M_Blue, M_Orange, M_Green };
            for (int i = 0; i < 3; i++)
            {
                float a = (i * 120f + 90f) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Cos(a) * 5f, 0f, Mathf.Sin(a) * 5f);
                Prim(PrimitiveType.Cube, "Budynek_" + titles[i], pos + new Vector3(0f, 1.2f, 0f), new Vector3(2f, 2.4f, 2f), mats[i]);
                var doc = Document("Panel_" + titles[i], s_PanelWorld, uxml);
                doc.transform.position = pos + new Vector3(0f, 3.5f, 0f);
                doc.transform.localScale = Vector3.one * 0.5f;
                doc.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
                doc.worldSpaceSize = new Vector2(400f, 170f);
                doc.gameObject.AddComponent<Billboard>().cam = cam;
                docs.Add(doc);
            }
            var demo = Manager<ToolkitWorldDemo>();
            demo.cam = cam;
            demo.panels = docs.ToArray();
            demo.titles = titles;
        }
    }
}
