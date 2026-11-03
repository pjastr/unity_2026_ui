using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Pomocnicze metody tworzące elementy uGUI (TextMeshPro) w jednolitym stylu.
    /// Używa ich generator scen (w edytorze) oraz skrypty, które tworzą UI w trakcie gry
    /// (liczby obrażeń, powiadomienia, elementy listy…). Przed użyciem trzeba ustawić <see cref="Skin"/>
    /// – robi to DemoHud w Awake().
    /// </summary>
    public static class UiFactory
    {
        public static UiSkin Skin;

        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;

        // ------------------------------------------------------------------ Canvas

        /// <summary>Canvas z CanvasScaler (Scale With Screen Size 1920×1080, Match 0,5) i GraphicRaycaster.</summary>
        public static Canvas Canvas(string name, int sortingOrder, RenderMode mode = RenderMode.ScreenSpaceOverlay,
            Camera camera = null, bool raycaster = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = mode;
            canvas.sortingOrder = sortingOrder;
            if (mode != RenderMode.ScreenSpaceOverlay)
                canvas.worldCamera = camera;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            if (raycaster)
                go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ------------------------------------------------------------------ RectTransform

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent != null ? parent.gameObject.layer : 5;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Kotwice w jednym punkcie (anchor), pivot, pozycja i rozmiar w pikselach odniesienia.</summary>
        public static RectTransform At(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Kotwice rozciągnięte (anchorMin–anchorMax) z marginesami: left, bottom, right, top.</summary>
        public static RectTransform Stretch(this RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
            float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>Wypełnia cały prostokąt rodzica (z opcjonalnym marginesem).</summary>
        public static RectTransform Fill(this RectTransform rt, float margin = 0f)
            => rt.Stretch(Vector2.zero, Vector2.one, margin, margin, margin, margin);

        // ------------------------------------------------------------------ grafika

        public static Image Panel(string name, Transform parent, Color color, bool sliced = true)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            if (sliced && Skin != null && Skin.rounded != null)
            {
                img.sprite = Skin.rounded;
                img.type = Image.Type.Sliced;
            }
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Image Icon(string name, Transform parent, Sprite sprite, Color color, float size)
        {
            var rt = Rect(name, parent);
            rt.sizeDelta = new Vector2(size, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        // ------------------------------------------------------------------ tekst

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (Skin != null && Skin.font != null)
                t.font = Skin.font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color ?? (Skin != null ? Skin.text : Color.white);
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.richText = true;
            t.raycastTarget = false;
            return t;
        }

        static void StyleTmp(TMP_Text t, float size, Color color)
        {
            if (Skin != null && Skin.font != null)
                t.font = Skin.font;
            t.fontSize = size;
            t.color = color;
        }

        // ------------------------------------------------------------------ kontrolki

        static TMP_DefaultControls.Resources TmpResources()
        {
            var r = new TMP_DefaultControls.Resources();
            if (Skin == null) return r;
            r.standard = Skin.rounded;
            r.background = Skin.builtinBackground;
            r.inputField = Skin.builtinInputField;
            r.knob = Skin.builtinKnob;
            r.checkmark = Skin.builtinCheckmark;
            r.dropdown = Skin.builtinDropdownArrow;
            r.mask = Skin.builtinMask;
            return r;
        }

        static DefaultControls.Resources UguiResources()
        {
            var r = new DefaultControls.Resources();
            if (Skin == null) return r;
            r.standard = Skin.rounded;
            r.background = Skin.builtinBackground;
            r.inputField = Skin.builtinInputField;
            r.knob = Skin.builtinKnob;
            r.checkmark = Skin.builtinCheckmark;
            r.dropdown = Skin.builtinDropdownArrow;
            r.mask = Skin.builtinMask;
            return r;
        }

        /// <summary>Kolory stanów (Color Tint) – wyraźne dla stanu "Selected" (nawigacja padem/klawiaturą).</summary>
        public static void SetColors(Selectable s)
        {
            var c = s.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1f, 0.93f, 0.75f);
            c.pressedColor = new Color(0.72f, 0.72f, 0.72f);
            c.selectedColor = new Color(1f, 0.85f, 0.45f);
            c.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.55f);
            c.colorMultiplier = 1f;
            c.fadeDuration = 0.08f;
            s.colors = c;
        }

        public static Button Button(string name, Transform parent, string label, float fontSize = 26f, Vector2? size = null)
        {
            var go = TMP_DefaultControls.CreateButton(TmpResources());
            go.name = name;
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size ?? new Vector2(300f, 64f);
            var img = go.GetComponent<Image>();
            img.color = Skin != null ? Skin.control : img.color;
            var b = go.GetComponent<Button>();
            SetColors(b);
            var t = go.GetComponentInChildren<TMP_Text>();
            t.text = label;
            StyleTmp(t, fontSize, Skin != null ? Skin.text : Color.white);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            return b;
        }

        public static Toggle Toggle(string name, Transform parent, string label, bool isOn, float fontSize = 24f)
        {
            var go = DefaultControls.CreateToggle(UguiResources());
            go.name = name;
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(360f, 44f);
            var toggle = go.GetComponent<Toggle>();
            toggle.isOn = isOn;
            SetColors(toggle);

            // tło i znacznik nieco większe niż domyślne 20 px
            var bg = (RectTransform)go.transform.Find("Background");
            bg.anchorMin = bg.anchorMax = new Vector2(0f, 0.5f);
            bg.pivot = new Vector2(0f, 0.5f);
            bg.anchoredPosition = new Vector2(4f, 0f);
            bg.sizeDelta = new Vector2(34f, 34f);

            // etykieta: zamiana tekstu Legacy na TextMeshPro
            var labelTr = go.transform.Find("Label");
            var legacy = labelTr.GetComponent<Text>();
            Object.DestroyImmediate(legacy);
            var t = labelTr.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = label;
            StyleTmp(t, fontSize, Skin != null ? Skin.text : Color.white);
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            var lrt = (RectTransform)labelTr;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(48f, 0f);
            lrt.offsetMax = Vector2.zero;
            return toggle;
        }

        public static Slider Slider(string name, Transform parent, float min, float max, float value, bool wholeNumbers = false)
        {
            var go = DefaultControls.CreateSlider(UguiResources());
            go.name = name;
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(360f, 30f);
            var s = go.GetComponent<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.wholeNumbers = wholeNumbers;
            s.value = value;
            SetColors(s);
            var fill = go.transform.Find("Fill Area/Fill");
            if (fill != null && Skin != null)
                fill.GetComponent<Image>().color = Skin.accent;
            var handle = go.transform.Find("Handle Slide Area/Handle");
            if (handle != null)
                ((RectTransform)handle).sizeDelta = new Vector2(30f, 0f);
            return s;
        }

        public static TMP_Dropdown Dropdown(string name, Transform parent, List<string> options, float fontSize = 24f)
        {
            var go = TMP_DefaultControls.CreateDropdown(TmpResources());
            go.name = name;
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(360f, 52f);
            var dd = go.GetComponent<TMP_Dropdown>();
            dd.ClearOptions();
            dd.AddOptions(options);
            SetColors(dd);
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
                StyleTmp(t, fontSize, new Color(0.12f, 0.12f, 0.14f));
            var template = (RectTransform)go.transform.Find("Template");
            if (template != null)
                template.sizeDelta = new Vector2(0f, 260f);
            var item = (RectTransform)go.transform.Find("Template/Viewport/Content/Item");
            if (item != null)
                item.sizeDelta = new Vector2(item.sizeDelta.x, 40f);
            var content = (RectTransform)go.transform.Find("Template/Viewport/Content");
            if (content != null)
                content.sizeDelta = new Vector2(content.sizeDelta.x, 44f);
            return dd;
        }

        public static TMP_InputField InputField(string name, Transform parent, string placeholder, float fontSize = 24f)
        {
            var go = TMP_DefaultControls.CreateInputField(TmpResources());
            go.name = name;
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(360f, 52f);
            var input = go.GetComponent<TMP_InputField>();
            SetColors(input);
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
                StyleTmp(t, fontSize, new Color(0.12f, 0.12f, 0.14f));
            if (input.placeholder is TMP_Text ph)
            {
                ph.text = placeholder;
                ph.color = new Color(0.35f, 0.35f, 0.4f, 0.8f);
                ph.fontStyle = FontStyles.Italic;
            }
            input.pointSize = fontSize;
            return input;
        }

        /// <summary>ScrollRect + Viewport (RectMask2D) + Content. Zwraca ScrollRect; content – kontener elementów.</summary>
        public static ScrollRect ScrollView(string name, Transform parent, out RectTransform content, bool vertical = true)
        {
            var root = Panel(name, parent, Skin != null ? Skin.panel : new Color(0, 0, 0, 0.6f));
            root.raycastTarget = true; // przewijanie kółkiem/przeciąganiem nad całym obszarem
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = !vertical;
            scroll.vertical = vertical;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            var viewport = Rect("Viewport", root.transform).Fill(8f);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", viewport);
            if (vertical)
            {
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
                content.sizeDelta = new Vector2(0f, 0f);
            }
            else
            {
                content.anchorMin = new Vector2(0f, 0f);
                content.anchorMax = new Vector2(0f, 1f);
                content.pivot = new Vector2(0f, 0.5f);
                content.sizeDelta = new Vector2(0f, 0f);
            }
            content.anchoredPosition = Vector2.zero;
            scroll.viewport = viewport;
            scroll.content = content;
            return scroll;
        }

        // ------------------------------------------------------------------ układ

        public static VerticalLayoutGroup VLayout(GameObject go, float spacing, int padding, bool fitHeight = false,
            TextAnchor align = TextAnchor.UpperLeft, bool controlWidth = true)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childAlignment = align;
            v.childControlWidth = controlWidth;
            v.childControlHeight = true;
            v.childForceExpandWidth = controlWidth;
            v.childForceExpandHeight = false;
            if (fitHeight)
                go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return v;
        }

        public static HorizontalLayoutGroup HLayout(GameObject go, float spacing, int padding,
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(padding, padding, padding, padding);
            h.childAlignment = align;
            h.childControlWidth = false;
            h.childControlHeight = false;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            return h;
        }

        /// <summary>Preferowany rozmiar elementu dla grup układu.</summary>
        public static LayoutElement Prefer(GameObject go, float width = -1f, float height = -1f, float flexibleWidth = -1f)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (width >= 0f) le.preferredWidth = width;
            if (height >= 0f) le.preferredHeight = height;
            if (flexibleWidth >= 0f) le.flexibleWidth = flexibleWidth;
            return le;
        }

        /// <summary>Kolor w zapisie #RRGGBB do znaczników rich text.</summary>
        public static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    }
}
