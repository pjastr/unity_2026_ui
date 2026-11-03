using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 15: dostępność interfejsu.
    ///  • suwak skali tekstu 50–200% (wartość startowa z AccessibilitySettings.fontScale systemu),
    ///  • tryb wysokiego kontrastu (czarne tło, biały tekst, żółte przyciski),
    ///  • "kolor + kształt" – stan wskaźników przekazany też ikoną i słowem, nie tylko kolorem,
    ///  • napisy dialogów z tłem i nazwą mówiącego,
    ///  • Spacja – symulacja daltonizmu dla kolorów wskaźników (brak → protanopia → deuteranopia → tritanopia),
    ///  • Shift – włącz/wyłącz hierarchię dla czytnika ekranu (AccessibilityHierarchy; Unity 6.3: Narrator, VoiceOver, TalkBack).
    /// </summary>
    public class AccessibilityDemo : DemoActionBase
    {
        [Header("Treść")]
        public RectTransform contentRoot;
        public Image[] panels;
        public Button[] menuButtons;
        public Image[] statusDots;
        public Image[] statusIcons;
        public TMP_Text[] statusLabels;
        public string[] statusNames;
        public bool[] statusOk;
        public Image subtitleBackground;
        public TMP_Text subtitleText;

        [Header("Opcje")]
        public Slider textScale;
        public TMP_Text textScaleValue;
        public Toggle highContrast;
        public Toggle shapeAndColor;
        public Toggle subtitleBox;
        public Sprite iconOk;
        public Sprite iconError;

        readonly Dictionary<TMP_Text, float> m_BaseSizes = new Dictionary<TMP_Text, float>();
        readonly Dictionary<Graphic, Color> m_BaseColors = new Dictionary<Graphic, Color>();
        readonly Dictionary<LayoutElement, float> m_BaseHeights = new Dictionary<LayoutElement, float>();
        int m_Sim;
        AccessibilityHierarchy m_Hierarchy;
        float m_SubtitleTime;
        int m_SubtitleIndex;

        static readonly string[] SimNames = { "brak", "protanopia (brak czopków L)", "deuteranopia (brak czopków M)", "tritanopia (brak czopków S)" };

        // uproszczone macierze symulacji (przybliżenie – do celów poglądowych)
        static readonly float[][] SimMatrices =
        {
            new[] { 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f },
            new[] { 0.567f, 0.433f, 0f, 0.558f, 0.442f, 0f, 0f, 0.242f, 0.758f },
            new[] { 0.625f, 0.375f, 0f, 0.7f, 0.3f, 0f, 0f, 0.3f, 0.7f },
            new[] { 0.95f, 0.05f, 0f, 0f, 0.433f, 0.567f, 0f, 0.475f, 0.525f },
        };

        static readonly (string who, string line)[] Dialog =
        {
            ("Kapitan", "Zbliża się burza. Musimy dotrzeć do portu przed zmrokiem."),
            ("Mira", "Silnik znowu się przegrzewa! Daj mi dwie minuty."),
            ("Kapitan", "Masz minutę. Trzymaj się mocno."),
            ("[odgłos grzmotu]", ""),
        };

        void Start()
        {
            foreach (var t in contentRoot.GetComponentsInChildren<TMP_Text>(true))
                m_BaseSizes[t] = t.fontSize;
            foreach (var g in contentRoot.GetComponentsInChildren<Graphic>(true))
                m_BaseColors[g] = g.color;
            foreach (var le in contentRoot.GetComponentsInChildren<LayoutElement>(true))
                m_BaseHeights[le] = le.preferredHeight;
            foreach (var p in panels)
                if (p != null && !m_BaseColors.ContainsKey(p)) m_BaseColors[p] = p.color;

            float sys = AccessibilitySettings.fontScale;
            textScale.value = sys > 0.1f ? Mathf.Clamp(sys * 100f, 50f, 200f) : 100f;
            textScale.onValueChanged.AddListener(_ => ApplyAll());
            highContrast.onValueChanged.AddListener(_ => ApplyAll());
            shapeAndColor.onValueChanged.AddListener(_ => ApplyAll());
            subtitleBox.onValueChanged.AddListener(_ => ApplyAll());
            foreach (var b in menuButtons)
            {
                var label = b.GetComponentInChildren<TMP_Text>().text;
                b.onClick.AddListener(() => DemoHud.Log("kliknięto: " + label));
            }
            ApplyAll();
            ShowSubtitle();
        }

        void ApplyAll()
        {
            float k = textScale.value / 100f;
            textScaleValue.text = $"{textScale.value:0}%";
            foreach (var kv in m_BaseSizes)
                if (kv.Key != null) kv.Key.fontSize = kv.Value * k;
            // elementy o stałej wysokości (przyciski, przełączniki) rosną razem z tekstem
            foreach (var kv in m_BaseHeights)
                if (kv.Key != null && kv.Value > 0f) kv.Key.preferredHeight = kv.Value * Mathf.Max(1f, k);

            bool hc = highContrast.isOn;
            foreach (var kv in m_BaseColors)
                if (kv.Key != null) kv.Key.color = kv.Value;
            if (hc)
            {
                foreach (var p in panels) p.color = new Color(0f, 0f, 0f, 0.97f);
                foreach (var t in m_BaseSizes.Keys) t.color = Color.white;
                foreach (var b in menuButtons)
                {
                    b.GetComponent<Image>().color = new Color(1f, 0.86f, 0f);
                    b.GetComponentInChildren<TMP_Text>().color = Color.black;
                }
            }

            for (int i = 0; i < statusDots.Length; i++)
            {
                bool ok = statusOk[i];
                statusDots[i].color = Simulate(ok ? new Color(0.25f, 0.75f, 0.3f) : new Color(0.85f, 0.25f, 0.2f));
                statusIcons[i].gameObject.SetActive(shapeAndColor.isOn);
                statusIcons[i].sprite = ok ? iconOk : iconError;
                statusIcons[i].color = hc ? Color.white : new Color(0.95f, 0.95f, 0.95f);
                statusLabels[i].text = shapeAndColor.isOn ? $"{statusNames[i]}: <b>{(ok ? "OK" : "błąd")}</b>" : statusNames[i];
            }

            subtitleBackground.enabled = subtitleBox.isOn;
            subtitleBackground.color = new Color(0f, 0f, 0f, hc ? 1f : 0.75f);
        }

        Color Simulate(Color c)
        {
            var m = SimMatrices[m_Sim];
            return new Color(
                m[0] * c.r + m[1] * c.g + m[2] * c.b,
                m[3] * c.r + m[4] * c.g + m[5] * c.b,
                m[6] * c.r + m[7] * c.g + m[8] * c.b, c.a);
        }

        void ShowSubtitle()
        {
            var (who, line) = Dialog[m_SubtitleIndex % Dialog.Length];
            subtitleText.text = string.IsNullOrEmpty(line)
                ? $"<i>{who}</i>"
                : $"<color=#FFD24A><b>{who}:</b></color> {line}";
            m_SubtitleIndex++;
            m_SubtitleTime = Time.time;
        }

        public override void OnMainAction()
        {
            m_Sim = (m_Sim + 1) % SimNames.Length;
            ApplyAll();
            DemoHud.Log("symulacja daltonizmu: " + SimNames[m_Sim]);
        }

        public override void OnSecondaryAction()
        {
            if (AssistiveSupport.activeHierarchy == null || AssistiveSupport.activeHierarchy != m_Hierarchy)
            {
                m_Hierarchy = BuildHierarchy();
                AssistiveSupport.activeHierarchy = m_Hierarchy;
                DemoHud.Log("aktywowano AccessibilityHierarchy (" + menuButtons.Length + " przyciski + suwak)");
            }
            else
            {
                AssistiveSupport.activeHierarchy = null;
                DemoHud.Log("wyłączono AccessibilityHierarchy");
            }
        }

        /// <summary>Węzły dla czytnika ekranu: etykieta, rola, prostokąt na ekranie i reakcja na zatwierdzenie.</summary>
        AccessibilityHierarchy BuildHierarchy()
        {
            var h = new AccessibilityHierarchy();
            foreach (var b in menuButtons)
            {
                var button = b;
                var node = h.AddNode(b.GetComponentInChildren<TMP_Text>().text);
                node.role = AccessibilityRole.Button;
                node.frameGetter = () => ScreenRect((RectTransform)button.transform);
                node.invoked += () => { button.onClick.Invoke(); return true; };
            }
            var slider = h.AddNode("Skala tekstu");
            slider.role = AccessibilityRole.Slider;
            slider.value = $"{textScale.value:0} procent";
            slider.frameGetter = () => ScreenRect((RectTransform)textScale.transform);
            slider.incremented += () => { textScale.value += 10f; slider.value = $"{textScale.value:0} procent"; };
            slider.decremented += () => { textScale.value -= 10f; slider.value = $"{textScale.value:0} procent"; };
            return h;
        }

        /// <summary>Prostokąt elementu w pikselach ekranu (początek układu w lewym górnym rogu).</summary>
        static Rect ScreenRect(RectTransform rt)
        {
            var c = new Vector3[4];
            rt.GetWorldCorners(c); // Canvas Overlay – współrzędne ekranu (początek w lewym dolnym rogu)
            return new Rect(c[0].x, Screen.height - c[1].y, c[2].x - c[0].x, c[1].y - c[0].y);
        }

        void Update()
        {
            if (Time.time - m_SubtitleTime > 3.5f)
                ShowSubtitle();

            DemoHud.SetStatus("a11y",
                "<b>AccessibilitySettings (system)</b>\n" +
                $"fontScale = {AccessibilitySettings.fontScale:0.00}   isBoldTextEnabled = {AccessibilitySettings.isBoldTextEnabled}\n" +
                $"isClosedCaptioningEnabled = {AccessibilitySettings.isClosedCaptioningEnabled}\n" +
                "<b>AssistiveSupport</b>\n" +
                $"isScreenReaderEnabled = {AssistiveSupport.isScreenReaderEnabled}\n" +
                $"activeHierarchy = {(AssistiveSupport.activeHierarchy != null ? "ustawiona" : "brak")}\n" +
                $"Symulacja daltonizmu: {SimNames[m_Sim]}\n" +
                $"Skala tekstu: {textScale.value:0}%   Wysoki kontrast: {highContrast.isOn}   Kolor + kształt: {shapeAndColor.isOn}");
        }

        void OnDestroy()
        {
            if (AssistiveSupport.activeHierarchy == m_Hierarchy && m_Hierarchy != null)
                AssistiveSupport.activeHierarchy = null;
        }
    }
}
