using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 06: kontrolki uGUI i ich zdarzenia: Button (onClick w kodzie i w Inspectorze), Toggle, ToggleGroup,
    /// Slider, TMP_Dropdown, TMP_InputField. Każde zdarzenie trafia do dziennika (prawy górny róg).
    ///  • Spacja – kolejny typ przejścia stanów (Transition): Color Tint → Sprite Swap → None,
    ///  • Shift – interactable = false/true dla wszystkich kontrolek (stan nieaktywny).
    /// </summary>
    public class ControlsDemo : DemoActionBase
    {
        public Button codeButton;
        public Button inspectorButton;
        public Button disabledButton;
        public Toggle subtitlesToggle;
        public Toggle[] difficulty;
        public Slider volumeSlider;
        public TMP_Text volumeLabel;
        public TMP_Dropdown qualityDropdown;
        public TMP_InputField nameInput;
        public TMP_Text resultText;
        public Sprite highlightedSprite;
        [Tooltip("Gałąź UI z kontrolkami tej sceny")]
        public Transform controlsRoot;

        Selectable[] m_All;
        int m_Transition;
        bool m_Interactable = true;
        int m_Clicks;

        void Start()
        {
            m_All = (controlsRoot != null ? controlsRoot : transform).GetComponentsInChildren<Selectable>(true);

            // 1) zdarzenie dodane w kodzie
            codeButton.onClick.AddListener(OnCodeButton);
            // 2) inspectorButton – zdarzenie zapisane w scenie (pole On Click () w Inspectorze → OnInspectorButton)
            disabledButton.interactable = false;

            subtitlesToggle.onValueChanged.AddListener(on => DemoHud.Log($"Toggle Napisy: isOn = {on}"));
            for (int i = 0; i < difficulty.Length; i++)
            {
                int idx = i;
                difficulty[i].onValueChanged.AddListener(on =>
                {
                    if (on) DemoHud.Log($"ToggleGroup: wybrano \"{difficulty[idx].GetComponentInChildren<TMP_Text>().text}\"");
                });
            }
            volumeSlider.onValueChanged.AddListener(v =>
            {
                volumeLabel.text = $"Głośność: {v:0}%";
            });
            qualityDropdown.onValueChanged.AddListener(i => DemoHud.Log($"Dropdown: value = {i} ({qualityDropdown.options[i].text})"));
            nameInput.characterLimit = 16;
            nameInput.onValueChanged.AddListener(s => resultText.text = string.IsNullOrEmpty(s) ? "Witaj, graczu!" : $"Witaj, {s}!");
            nameInput.onEndEdit.AddListener(s => DemoHud.Log($"InputField onEndEdit: \"{s}\""));
            volumeLabel.text = $"Głośność: {volumeSlider.value:0}%";
            resultText.text = "Witaj, graczu!";
        }

        void OnCodeButton()
        {
            m_Clicks++;
            resultText.text = $"Kliknięto przycisk ({m_Clicks}×)";
            DemoHud.Log("Button.onClick – słuchacz dodany w kodzie (AddListener)");
        }

        /// <summary>Metoda publiczna podpięta w Inspectorze (On Click ()) – zdarzenie serializowane w scenie.</summary>
        public void OnInspectorButton()
        {
            resultText.text = "Zdarzenie z Inspectora!";
            DemoHud.Log("Button.onClick – zdarzenie serializowane (On Click () w Inspectorze)");
        }

        public override void OnMainAction()
        {
            m_Transition = (m_Transition + 1) % 3;
            foreach (var s in m_All)
            {
                if (s == null) continue;
                switch (m_Transition)
                {
                    case 0:
                        s.transition = Selectable.Transition.ColorTint;
                        break;
                    case 1:
                        s.transition = Selectable.Transition.SpriteSwap;
                        var st = s.spriteState;
                        st.highlightedSprite = highlightedSprite;
                        st.selectedSprite = highlightedSprite;
                        st.pressedSprite = highlightedSprite;
                        s.spriteState = st;
                        if (s.targetGraphic is Image img) img.color = Color.white;
                        break;
                    default:
                        s.transition = Selectable.Transition.None;
                        break;
                }
            }
            DemoHud.Log("Transition = " + (m_Transition == 0 ? "Color Tint" : m_Transition == 1 ? "Sprite Swap" : "None"));
        }

        public override void OnSecondaryAction()
        {
            m_Interactable = !m_Interactable;
            foreach (var s in m_All)
                if (s != null && s != disabledButton)
                    s.interactable = m_Interactable;
            DemoHud.Log("interactable = " + m_Interactable);
        }

        void Update()
        {
            string sel = UnityEngine.EventSystems.EventSystem.current != null &&
                         UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null
                ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name : "brak";
            DemoHud.SetStatus("controls",
                "<b>Wartości kontrolek</b>\n" +
                $"Toggle Napisy: {subtitlesToggle.isOn}\n" +
                $"Slider: {volumeSlider.value:0.0} (min {volumeSlider.minValue:0}, max {volumeSlider.maxValue:0})\n" +
                $"Dropdown: {qualityDropdown.value} – {qualityDropdown.captionText.text}\n" +
                $"InputField: \"{nameInput.text}\" ({nameInput.text.Length}/{nameInput.characterLimit})\n" +
                $"Zaznaczony (EventSystem): {sel}\n" +
                $"Transition: {(m_Transition == 0 ? "Color Tint" : m_Transition == 1 ? "Sprite Swap" : "None")}   interactable: {m_Interactable}");
        }
    }
}
