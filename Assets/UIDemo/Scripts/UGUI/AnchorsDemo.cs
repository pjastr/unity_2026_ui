using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 02: RectTransform – kotwice (anchors), pivot, pozycja i rozmiar.
    /// Ramka "ekranu" zmienia rozmiar (Spacja – kolejne proporcje, Shift – płynna animacja),
    /// a elementy wewnątrz zachowują się zgodnie z ustawieniem kotwic.
    /// Romby w kolorze elementu pokazują położenie anchorMin i anchorMax w ramce.
    /// </summary>
    public class AnchorsDemo : DemoActionBase
    {
        public RectTransform frame;
        public TMP_Text frameLabel;
        public RectTransform[] items;
        public TMP_Text[] itemLabels;
        public Sprite markerSprite;

        static readonly Vector2[] Sizes =
        {
            new Vector2(900f, 506f),  // 16:9
            new Vector2(1000f, 428f), // 21:9
            new Vector2(640f, 480f),  // 4:3
            new Vector2(300f, 560f),  // telefon pionowo
        };
        static readonly string[] SizeNames = { "16:9", "21:9 (ultrapanoramiczny)", "4:3", "telefon w pionie" };

        int m_Index;
        bool m_Animate;
        float m_T;
        Vector2 m_Target;
        Image[] m_MinMarkers;
        Image[] m_MaxMarkers;

        void Start()
        {
            m_Target = Sizes[0];
            frame.sizeDelta = m_Target;
            m_MinMarkers = new Image[items.Length];
            m_MaxMarkers = new Image[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var color = items[i].GetComponent<Image>().color;
                color.a = 1f;
                m_MinMarkers[i] = Marker("AnchorMin_" + items[i].name, color);
                m_MaxMarkers[i] = Marker("AnchorMax_" + items[i].name, color);
            }
        }

        Image Marker(string name, Color c)
        {
            var img = UiFactory.Icon(name, frame, markerSprite, c, 18f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            img.gameObject.AddComponent<Outline>().effectColor = Color.black;
            return img;
        }

        public override void OnMainAction()
        {
            m_Animate = false;
            m_Index = (m_Index + 1) % Sizes.Length;
            m_Target = Sizes[m_Index];
            DemoHud.Log("rozmiar rodzica: " + SizeNames[m_Index]);
        }

        public override void OnSecondaryAction()
        {
            m_Animate = !m_Animate;
            DemoHud.Log(m_Animate ? "płynna zmiana rozmiaru" : "animacja zatrzymana");
        }

        void Update()
        {
            if (m_Animate)
            {
                m_T += Time.deltaTime * 0.35f;
                float a = (Mathf.Sin(m_T * Mathf.PI * 2f) + 1f) * 0.5f;
                float b = (Mathf.Sin(m_T * Mathf.PI * 1.3f + 1f) + 1f) * 0.5f;
                m_Target = new Vector2(Mathf.Lerp(320f, 1000f, a), Mathf.Lerp(300f, 560f, b));
            }
            frame.sizeDelta = Vector2.Lerp(frame.sizeDelta, m_Target, 1f - Mathf.Exp(-8f * Time.deltaTime));
            Vector2 size = frame.rect.size;
            if (frameLabel != null)
                frameLabel.text = $"Rodzic (ramka): {size.x:0} × {size.y:0} px";

            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                m_MinMarkers[i].rectTransform.anchoredPosition = (it.anchorMin - new Vector2(0.5f, 0.5f)) * size;
                m_MaxMarkers[i].rectTransform.anchoredPosition = (it.anchorMax - new Vector2(0.5f, 0.5f)) * size;
                m_MaxMarkers[i].enabled = it.anchorMin != it.anchorMax;
                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null)
                    itemLabels[i].text = Describe(it);
            }

            var first = items.Length > 0 ? items[0] : null;
            DemoHud.SetStatus("anchors",
                "<b>RectTransform</b>\n" +
                "anchorMin/anchorMax – punkty w rodzicu (0–1)\n" +
                "kotwice razem → stały rozmiar i odległość od punktu\n" +
                "kotwice rozsunięte → element rozciąga się z rodzicem\n" +
                "pivot – punkt odniesienia pozycji, obrotu i skali\n" +
                $"Rodzic: {size.x:0} × {size.y:0} px" +
                (first != null ? $"\n{first.name}: rect = {first.rect.width:0} × {first.rect.height:0} px" : ""));
        }

        static string Describe(RectTransform rt)
        {
            string a = rt.anchorMin == rt.anchorMax
                ? $"kotwica ({rt.anchorMin.x:0.##}; {rt.anchorMin.y:0.##})"
                : $"min ({rt.anchorMin.x:0.##}; {rt.anchorMin.y:0.##}) max ({rt.anchorMax.x:0.##}; {rt.anchorMax.y:0.##})";
            return $"<b>{rt.name}</b>\n{a}\npivot ({rt.pivot.x:0.##}; {rt.pivot.y:0.##})\n" +
                   $"rozmiar {rt.rect.width:0}×{rt.rect.height:0}";
        }
    }
}
