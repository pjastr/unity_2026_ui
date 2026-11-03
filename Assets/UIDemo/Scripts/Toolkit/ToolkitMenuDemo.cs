using UnityEngine;
using UnityEngine.UIElements;

namespace UIDemo
{
    /// <summary>
    /// Scena 16: podstawy UI Toolkit – UIDocument (UXML: MainMenu.uxml, USS: MainMenu.uss) i obsługa w C#.
    ///  • Q&lt;Button&gt;("play").clicked, RegisterValueChangedCallback, RegisterCallback&lt;PointerEnterEvent&gt;,
    ///  • pseudoklasy :hover, :focus, :active i przejścia (transition-*) zdefiniowane w USS – bez kodu,
    ///  • Spacja – motyw jasny/ciemny (dodanie arkusza MainMenuLight.uss nadpisującego zmienne USS),
    ///  • Shift – klasa "compact" na karcie (ToggleInClassList) – inny wygląd z tej samej struktury.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ToolkitMenuDemo : DemoActionBase
    {
        public StyleSheet lightTheme;

        VisualElement m_Root;
        VisualElement m_Card;
        Label m_Info;
        bool m_Light;

        void Start()
        {
            m_Root = GetComponent<UIDocument>().rootVisualElement;
            m_Card = m_Root.Q<VisualElement>("card");
            m_Info = m_Root.Q<Label>("info");

            foreach (var name in new[] { "play", "settings", "credits", "quit" })
            {
                var b = m_Root.Q<Button>(name);
                if (b == null) continue;
                string text = b.text;
                b.clicked += () =>
                {
                    m_Info.text = $"Kliknięto: <b>{text}</b>";
                    UiSound.Play(UiSound.Kind.Click, 0.4f);
                    DemoHud.Log($"Button \"{name}\".clicked");
                };
            }

            // wszystkie przyciski z klasą USS "menu-button" – dźwięk przy najechaniu i przy zaznaczeniu
            m_Root.Query<Button>(className: "menu-button").ForEach(b =>
            {
                b.RegisterCallback<PointerEnterEvent>(_ => UiSound.Play(UiSound.Kind.Hover, 0.25f));
                b.RegisterCallback<FocusInEvent>(_ => m_Info.text = $"Zaznaczony (focus): {b.text}");
            });

            m_Root.Q<Slider>("volume")?.RegisterValueChangedCallback(e =>
                m_Info.text = $"Głośność: {e.newValue:0}");
            m_Root.Q<Toggle>("subtitles")?.RegisterValueChangedCallback(e =>
                DemoHud.Log($"Toggle napisy: {e.newValue}"));
            m_Root.Q<DropdownField>("difficulty")?.RegisterValueChangedCallback(e =>
                DemoHud.Log($"DropdownField: {e.previousValue} → {e.newValue}"));

            m_Root.Q<Button>("play")?.Focus(); // punkt startowy nawigacji klawiaturą / padem
        }

        public override void OnMainAction()
        {
            m_Light = !m_Light;
            if (m_Light) m_Root.styleSheets.Add(lightTheme);
            else m_Root.styleSheets.Remove(lightTheme);
            DemoHud.Log(m_Light ? "motyw jasny (MainMenuLight.uss)" : "motyw ciemny");
        }

        public override void OnSecondaryAction()
        {
            m_Card.ToggleInClassList("compact");
            DemoHud.Log("klasa .compact: " + m_Card.ClassListContains("compact"));
        }

        void Update()
        {
            if (m_Root == null) return;
            var focused = m_Root.focusController?.focusedElement as VisualElement;
            DemoHud.SetStatus("uitk",
                "<b>UIDocument</b> → Panel Settings → UXML (MainMenu.uxml)\n" +
                $"Arkusze stylów na root: {m_Root.styleSheets.count}\n" +
                $"Klasy karty: {string.Join(" ", m_Card.GetClasses())}\n" +
                $"Zaznaczony element: {(focused != null ? focused.name : "–")}\n" +
                $"Motyw: {(m_Light ? "jasny" : "ciemny")}\n" +
                "Okno: Window > UI Toolkit > Debugger – podgląd drzewa i stylów");
        }
    }
}
