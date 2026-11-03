using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 04: komponent Image – tryby Simple, Sliced (9-slice), Tiled, Filled oraz maski.
    ///  • Spacja – animacja rozmiaru obrazów Simple/Sliced/Tiled (włącz/wyłącz),
    ///  • Shift – kolejna metoda wypełnienia (Fill Method) obrazu Filled.
    /// Ten sam sprite (zaokrąglony prostokąt z Border = 24 px) w trybie Simple ma rozciągnięte narożniki,
    /// a w trybie Sliced narożniki pozostają ostre i okrągłe przy każdym rozmiarze.
    /// </summary>
    public class ImageTypesDemo : DemoActionBase
    {
        public RectTransform[] resizable;
        public Image filled;
        public Image filledBar;
        public RectTransform maskedContent;
        public RectTransform portraitContent;

        bool m_Animate = true;
        int m_Fill = 4;
        Vector2[] m_BaseSizes;

        static readonly Image.FillMethod[] Methods =
        {
            Image.FillMethod.Horizontal, Image.FillMethod.Vertical,
            Image.FillMethod.Radial90, Image.FillMethod.Radial180, Image.FillMethod.Radial360,
        };

        void Start()
        {
            m_BaseSizes = new Vector2[resizable.Length];
            for (int i = 0; i < resizable.Length; i++)
                m_BaseSizes[i] = resizable[i].sizeDelta;
            ApplyFill();
        }

        public override void OnMainAction()
        {
            m_Animate = !m_Animate;
            DemoHud.Log(m_Animate ? "animacja rozmiaru: włączona" : "animacja rozmiaru: wyłączona");
        }

        public override void OnSecondaryAction()
        {
            m_Fill = (m_Fill + 1) % Methods.Length;
            ApplyFill();
        }

        void ApplyFill()
        {
            filled.type = Image.Type.Filled;
            filled.fillMethod = Methods[m_Fill];
            filled.fillOrigin = 0;
            filled.fillClockwise = true;
            DemoHud.Log("Fill Method = " + Methods[m_Fill]);
        }

        void Update()
        {
            float t = Time.time;
            if (m_Animate)
            {
                float k = 0.55f + 0.45f * Mathf.Sin(t * 1.3f);
                float k2 = 0.55f + 0.45f * Mathf.Sin(t * 0.9f + 1.2f);
                for (int i = 0; i < resizable.Length; i++)
                    resizable[i].sizeDelta = new Vector2(m_BaseSizes[i].x * (0.45f + 0.55f * k), m_BaseSizes[i].y * (0.45f + 0.55f * k2));
            }

            // wypełnienie 0 → 1 w pętli (np. odnowienie umiejętności)
            float fill = Mathf.Repeat(t / 3f, 1f);
            filled.fillAmount = fill;
            if (filledBar != null)
                filledBar.fillAmount = 0.5f + 0.5f * Mathf.Sin(t * 0.7f);

            // zawartość przesuwana pod maską
            if (maskedContent != null)
                maskedContent.anchoredPosition = new Vector2(Mathf.Sin(t * 0.6f) * 120f, Mathf.Cos(t * 0.45f) * 60f);
            if (portraitContent != null)
                portraitContent.localRotation = Quaternion.Euler(0f, 0f, t * 25f);

            var sliced = resizable.Length > 1 ? resizable[1] : null;
            DemoHud.SetStatus("img",
                "<b>Image Type</b>\n" +
                "Simple – sprite rozciągnięty w całości\n" +
                "Sliced – 9-slice: narożniki stałe (Border w Sprite Editorze)\n" +
                "Tiled – sprite powtarzany\n" +
                $"Filled – fillMethod = {filled.fillMethod}, fillAmount = {filled.fillAmount:0.00}\n" +
                (sliced != null ? $"rozmiar obrazów: {sliced.rect.width:0} × {sliced.rect.height:0} px\n" : "") +
                "<b>Maski:</b> Rect Mask 2D (prostokąt, szybka) i Mask (kształt ze sprite'a – koło)");
        }
    }
}
