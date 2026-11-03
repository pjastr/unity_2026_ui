using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 08: układ automatyczny – Vertical/Horizontal/Grid Layout Group, Layout Element,
    /// Content Size Fitter i Scroll View (ScrollRect + Viewport z maską + Content).
    ///  • Spacja – dodaj przedmiot (do listy i siatki); dymek z tekstem dopasowuje się do treści,
    ///  • Shift – ograniczenie siatki (Constraint): Flexible → 4 kolumny → 6 kolumn.
    /// </summary>
    public class LayoutDemo : DemoActionBase
    {
        public RectTransform listContent;
        public RectTransform gridContent;
        public GridLayoutGroup grid;
        public ScrollRect listScroll;
        public ScrollRect gridScroll;
        public TMP_Text bubbleText;
        public Sprite[] icons;
        public int startItems = 12;

        int m_Count;
        int m_Constraint;

        static readonly string[] Names =
        {
            "Miecz", "Tarcza", "Mikstura zdrowia", "Mikstura many", "Łuk", "Strzały", "Klucz", "Mapa",
            "Hełm", "Pierścień", "Zwój teleportacji", "Kilof", "Lina", "Pochodnia", "Kryształ", "Chleb",
        };

        static readonly Color[] Colors =
        {
            new Color(0.95f, 0.4f, 0.35f), new Color(0.35f, 0.7f, 1f), new Color(0.45f, 0.85f, 0.45f),
            new Color(1f, 0.8f, 0.3f), new Color(0.75f, 0.5f, 1f), new Color(0.3f, 0.85f, 0.85f),
        };

        void Start()
        {
            for (int i = 0; i < startItems; i++)
                AddItem(false);
            ApplyConstraint();
        }

        public override void OnMainAction()
        {
            AddItem(true);
        }

        public override void OnSecondaryAction()
        {
            m_Constraint = (m_Constraint + 1) % 3;
            ApplyConstraint();
        }

        void ApplyConstraint()
        {
            switch (m_Constraint)
            {
                case 0: grid.constraint = GridLayoutGroup.Constraint.Flexible; break;
                case 1: grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; break;
                default: grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 6; break;
            }
            DemoHud.Log("GridLayoutGroup.constraint = " + grid.constraint +
                        (grid.constraint == GridLayoutGroup.Constraint.Flexible ? "" : $" ({grid.constraintCount})"));
        }

        void AddItem(bool announce)
        {
            int i = m_Count++;
            string name = Names[i % Names.Length];
            int amount = 1 + (i * 7) % 9;
            Color color = Colors[i % Colors.Length];
            Sprite icon = icons != null && icons.Length > 0 ? icons[i % icons.Length] : null;

            // wiersz listy: [ikona] nazwa ...... xN   (Horizontal Layout Group + Layout Element)
            var row = UiFactory.Panel($"Wiersz_{i + 1}_{name}", listContent, UiFactory.Skin.control);
            UiFactory.Prefer(row.gameObject, -1f, 52f);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(10, 14, 6, 6);
            h.spacing = 12f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = true;
            var ic = UiFactory.Icon("Ikona", row.transform, icon, color, 40f);
            UiFactory.Prefer(ic.gameObject, 40f, 40f);
            var label = UiFactory.Text("Nazwa", row.transform, name, 24f, null, TextAlignmentOptions.MidlineLeft);
            UiFactory.Prefer(label.gameObject, -1f, -1f, 1f); // Flexible Width = 1 – zajmuje wolne miejsce
            var count = UiFactory.Text("Ilosc", row.transform, "x" + amount, 24f, UiFactory.Skin.accent, TextAlignmentOptions.MidlineRight);
            UiFactory.Prefer(count.gameObject, 60f);

            // komórka siatki (Grid Layout Group – rozmiar komórki narzuca grupa)
            var cell = UiFactory.Panel($"Komorka_{i + 1}", gridContent, UiFactory.Skin.control);
            var cic = UiFactory.Icon("Ikona", cell.transform, icon, color, 56f);
            cic.rectTransform.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(56f, 56f));
            var ccount = UiFactory.Text("Ilosc", cell.transform, amount.ToString(), 20f, UiFactory.Skin.text, TextAlignmentOptions.BottomRight, FontStyles.Bold);
            ccount.rectTransform.Fill(6f);

            if (announce)
            {
                bubbleText.text = $"Dodano: <b>{name}</b> ×{amount}.\nPrzedmiotów w ekwipunku: {m_Count}." +
                                  (m_Count % 3 == 0 ? "\nDymek rośnie razem z tekstem – Content Size Fitter." : "");
                UiSound.Play(UiSound.Kind.Success, 0.35f);
                // przewiń listę na dół, żeby pokazać nowy element
                Canvas.ForceUpdateCanvases();
                listScroll.verticalNormalizedPosition = 0f;
                gridScroll.verticalNormalizedPosition = 0f;
                DemoHud.Log("dodano: " + name);
            }
        }

        void Update()
        {
            DemoHud.SetStatus("layout",
                $"<b>Przedmiotów:</b> {m_Count}\n" +
                $"Lista – Content: {listContent.rect.height:0} px wysokości (Vertical Layout Group + Content Size Fitter)\n" +
                $"Siatka – Cell Size {grid.cellSize.x:0}×{grid.cellSize.y:0}, Spacing {grid.spacing.x:0}, constraint = {grid.constraint}" +
                (grid.constraint == GridLayoutGroup.Constraint.Flexible ? "" : $" ({grid.constraintCount})") + "\n" +
                $"ScrollRect listy: verticalNormalizedPosition = {listScroll.verticalNormalizedPosition:0.00}\n" +
                "Przewijanie: kółko myszy, przeciąganie, pasek.");
        }
    }
}
