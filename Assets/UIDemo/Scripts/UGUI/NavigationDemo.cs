using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 07: nawigacja klawiaturą i padem (strzałki / WASD / lewa gałka, Enter / przycisk A, Esc / przycisk B).
    ///  • Spacja – tryb Navigation przycisków siatki: Automatic → Horizontal → Vertical → Explicit (z zawijaniem) → None,
    ///  • Shift – usuń zaznaczenie i włącz/wyłącz automatyczne ponowne zaznaczanie.
    /// Ramka zaznaczenia płynnie przesuwa się do zaznaczonego elementu (EventSystem.currentSelectedGameObject).
    /// </summary>
    public class NavigationDemo : DemoActionBase
    {
        public Selectable[] grid;
        public int columns = 3;
        public Selectable[] sideList;
        public GameObject firstSelected;
        public RectTransform selectionFrame;

        int m_Mode;
        bool m_AutoReselect = true;
        GameObject m_LastSelected;

        static readonly string[] ModeNames = { "Automatic", "Horizontal", "Vertical", "Explicit (z zawijaniem)", "None" };

        void Start()
        {
            Apply();
            Select(firstSelected);
            foreach (var s in grid)
                if (s is Button b)
                {
                    var name = b.name;
                    b.onClick.AddListener(() => DemoHud.Log($"Submit/Click: {name}"));
                }
            foreach (var s in sideList)
                if (s is Button b)
                {
                    var name = b.name;
                    b.onClick.AddListener(() => DemoHud.Log($"Submit/Click: {name}"));
                }
        }

        static void Select(GameObject go)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(go);
        }

        public override void OnMainAction()
        {
            m_Mode = (m_Mode + 1) % ModeNames.Length;
            Apply();
            DemoHud.Log("Navigation = " + ModeNames[m_Mode]);
        }

        public override void OnSecondaryAction()
        {
            m_AutoReselect = !m_AutoReselect;
            Select(null);
            DemoHud.Log("usunięto zaznaczenie; automatyczne ponowne zaznaczanie: " + (m_AutoReselect ? "TAK" : "NIE"));
        }

        void Apply()
        {
            int rows = Mathf.CeilToInt(grid.Length / (float)columns);
            for (int i = 0; i < grid.Length; i++)
            {
                var nav = new Navigation();
                switch (m_Mode)
                {
                    case 0: nav.mode = Navigation.Mode.Automatic; break;
                    case 1: nav.mode = Navigation.Mode.Horizontal; break;
                    case 2: nav.mode = Navigation.Mode.Vertical; break;
                    case 3:
                        nav.mode = Navigation.Mode.Explicit;
                        int r = i / columns, c = i % columns;
                        nav.selectOnUp = grid[((r - 1 + rows) % rows) * columns + c];
                        nav.selectOnDown = grid[((r + 1) % rows) * columns + c];
                        nav.selectOnLeft = grid[r * columns + (c - 1 + columns) % columns];
                        // z prawej krawędzi siatki – przejście do listy bocznej
                        nav.selectOnRight = c == columns - 1 && sideList.Length > 0
                            ? sideList[Mathf.Min(r, sideList.Length - 1)]
                            : grid[r * columns + (c + 1) % columns];
                        break;
                    default: nav.mode = Navigation.Mode.None; break;
                }
                grid[i].navigation = nav;
            }
        }

        void Update()
        {
            var es = EventSystem.current;
            GameObject sel = es != null ? es.currentSelectedGameObject : null;

            // dobra praktyka: gdy nic nie jest zaznaczone, pierwszy ruch padem/klawiaturą zaznacza element
            if (sel == null && m_AutoReselect && DemoInput.ReadVector2(DemoInput.Navigate).sqrMagnitude > 0.25f)
            {
                Select(m_LastSelected != null && m_LastSelected.activeInHierarchy ? m_LastSelected : firstSelected);
                DemoHud.Log("ponownie zaznaczono element (brak zaznaczenia)");
            }
            if (sel != null)
                m_LastSelected = sel;

            // ramka zaznaczenia podąża za zaznaczonym elementem
            if (selectionFrame != null)
            {
                bool visible = sel != null && sel.transform is RectTransform;
                selectionFrame.gameObject.SetActive(visible);
                if (visible)
                {
                    var target = (RectTransform)sel.transform;
                    float k = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
                    selectionFrame.position = Vector3.Lerp(selectionFrame.position, target.TransformPoint(target.rect.center), k);
                    Vector2 size = target.rect.size * target.lossyScale.x / selectionFrame.lossyScale.x + new Vector2(18f, 18f);
                    selectionFrame.sizeDelta = Vector2.Lerp(selectionFrame.sizeDelta, size, k);
                }
            }

            string neighbors = "–";
            if (sel != null && sel.TryGetComponent<Selectable>(out var s))
            {
                string N(Selectable x) => x != null ? x.name : "–";
                neighbors = $"↑ {N(s.FindSelectableOnUp())}   ↓ {N(s.FindSelectableOnDown())}\n" +
                            $"← {N(s.FindSelectableOnLeft())}   → {N(s.FindSelectableOnRight())}";
            }
            DemoHud.SetStatus("nav",
                $"<b>Navigation (siatka):</b> {ModeNames[m_Mode]}\n" +
                $"Zaznaczony: <b>{(sel != null ? sel.name : "brak – pad i klawiatura nie działają!")}</b>\n" +
                $"Sąsiedzi:\n{neighbors}\n" +
                $"Automatyczne ponowne zaznaczanie: {(m_AutoReselect ? "TAK" : "NIE")}\n" +
                $"Ostatnie urządzenie: {DemoInput.LastDeviceName}");
        }
    }
}
