using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 14: pomiar praw UX na sobie.
    ///  • Test Fittsa – kliknij "Start", a potem jak najszybciej cel (różne odległości D i szerokości W).
    ///    Czas ruchu T rośnie liniowo z indeksem trudności ID = log2(D/W + 1): T = a + b·ID.
    ///  • Test Hicka – po kliknięciu "Start" pojawia się litera; kliknij przycisk z tą literą.
    ///    Liczba opcji n = 2, 4 lub 8; czas reakcji rośnie z log2(n + 1).
    ///  • Spacja – przełącz test, Shift – wyczyść wyniki.
    /// Dopasowanie prostej metodą najmniejszych kwadratów pokazuje stałe a i b.
    /// </summary>
    public class UxLawsDemo : DemoActionBase
    {
        public RectTransform arena;
        public Button arenaBackground;
        public TMP_Text title;
        public TMP_Text prompt;
        public TMP_Text results;
        public Sprite circle;

        enum Test { Fitts, Hick }
        Test m_Test = Test.Fitts;

        Button m_Start;
        Button m_Target;
        readonly List<Button> m_Choices = new List<Button>();
        readonly List<(float x, float t, string key)> m_Fitts = new List<(float, float, string)>();
        readonly List<(float x, float t, string key)> m_Hick = new List<(float, float, string)>();
        int m_Errors;
        bool m_Running;
        float m_T0;
        float m_ShowAt = -1f;
        int m_HickN;
        int m_HickAnswer;

        static readonly float[] Widths = { 40f, 80f, 160f };
        static readonly float[] Distances = { 190f, 380f };
        static readonly int[] HickN = { 2, 4, 8 };
        const string Letters = "ABCDEFGH";

        void Start()
        {
            m_Start = UiFactory.Button("Start", arena, "Start", 26f, new Vector2(160f, 64f));
            m_Start.onClick.AddListener(OnStart);
            m_Target = UiFactory.Button("Cel", arena, "", 1f, new Vector2(80f, 80f));
            var img = m_Target.GetComponent<Image>();
            img.sprite = circle;
            img.type = Image.Type.Simple;
            img.color = UiFactory.Skin.accent;
            m_Target.onClick.AddListener(OnTarget);
            for (int i = 0; i < 8; i++)
            {
                int idx = i;
                var b = UiFactory.Button("Opcja_" + Letters[i], arena, Letters[i].ToString(), 34f, new Vector2(110f, 90f));
                b.onClick.AddListener(() => OnChoice(idx));
                m_Choices.Add(b);
            }
            arenaBackground.onClick.AddListener(() =>
            {
                if (!m_Running) return;
                m_Errors++;
                UiSound.Play(UiSound.Kind.Error, 0.3f);
            });
            Setup();
        }

        public override void OnMainAction()
        {
            m_Test = m_Test == Test.Fitts ? Test.Hick : Test.Fitts;
            Setup();
            DemoHud.Log("test: " + (m_Test == Test.Fitts ? "Fittsa" : "Hicka"));
        }

        public override void OnSecondaryAction()
        {
            m_Fitts.Clear();
            m_Hick.Clear();
            m_Errors = 0;
            DemoHud.Log("wyczyszczono wyniki");
        }

        void Setup()
        {
            m_Running = false;
            m_ShowAt = -1f;
            m_Target.gameObject.SetActive(false);
            foreach (var c in m_Choices) c.gameObject.SetActive(false);
            m_Start.gameObject.SetActive(true);
            ((RectTransform)m_Start.transform).anchoredPosition = m_Test == Test.Fitts ? Vector2.zero : new Vector2(0f, -200f);
            title.text = m_Test == Test.Fitts
                ? "<b>Prawo Fittsa</b>  T = a + b·log<sub>2</sub>(D/W + 1)"
                : "<b>Prawo Hicka</b>  T = a + b·log<sub>2</sub>(n + 1)";
            prompt.text = m_Test == Test.Fitts ? "Kliknij Start, a potem cel." : "Kliknij Start i czekaj na literę.";
        }

        void OnStart()
        {
            m_Start.gameObject.SetActive(false);
            m_Running = true;
            if (m_Test == Test.Fitts)
            {
                float w = Widths[Random.Range(0, Widths.Length)];
                float d = Distances[Random.Range(0, Distances.Length)];
                float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * 0.6f).normalized;
                var rt = (RectTransform)m_Target.transform;
                rt.sizeDelta = new Vector2(w, w);
                rt.anchoredPosition = dir * d;
                m_Target.name = $"Cel_D{d:0}_W{w:0}";
                m_Target.gameObject.SetActive(true);
                m_T0 = Time.unscaledTime;
                prompt.text = "Kliknij cel!";
            }
            else
            {
                m_HickN = HickN[Random.Range(0, HickN.Length)];
                float spacing = 125f;
                for (int i = 0; i < m_Choices.Count; i++)
                {
                    bool on = i < m_HickN;
                    m_Choices[i].gameObject.SetActive(on);
                    if (!on) continue;
                    var rt = (RectTransform)m_Choices[i].transform;
                    rt.anchoredPosition = new Vector2((i - (m_HickN - 1) * 0.5f) * spacing, -120f);
                }
                m_HickAnswer = Random.Range(0, m_HickN);
                prompt.text = "Uwaga…";
                m_ShowAt = Time.unscaledTime + Random.Range(0.6f, 1.6f); // losowe opóźnienie – nie da się "zgadnąć" momentu
            }
        }

        void OnTarget()
        {
            if (!m_Running || m_Test != Test.Fitts) return;
            float t = Time.unscaledTime - m_T0;
            var rt = (RectTransform)m_Target.transform;
            float w = rt.sizeDelta.x, d = rt.anchoredPosition.magnitude;
            float id = Mathf.Log(d / w + 1f, 2f);
            m_Fitts.Add((id, t, $"D={d:0} W={w:0}"));
            DemoHud.Log($"Fitts: D={d:0}, W={w:0}, ID={id:0.00} bit, T={t * 1000f:0} ms");
            Setup();
        }

        void OnChoice(int i)
        {
            if (!m_Running || m_Test != Test.Hick || m_ShowAt > 0f) return;
            if (i != m_HickAnswer)
            {
                m_Errors++;
                UiSound.Play(UiSound.Kind.Error, 0.3f);
                return;
            }
            float t = Time.unscaledTime - m_T0;
            float x = Mathf.Log(m_HickN + 1f, 2f);
            m_Hick.Add((x, t, $"n={m_HickN}"));
            DemoHud.Log($"Hick: n={m_HickN}, T={t * 1000f:0} ms");
            Setup();
        }

        void Update()
        {
            if (m_Test == Test.Hick && m_Running && m_ShowAt > 0f && Time.unscaledTime >= m_ShowAt)
            {
                m_ShowAt = -1f;
                prompt.text = $"Kliknij: <size=160%><b>{Letters[m_HickAnswer]}</b></size>";
                m_T0 = Time.unscaledTime;
            }

            var data = m_Test == Test.Fitts ? m_Fitts : m_Hick;
            var sb = new System.Text.StringBuilder();
            sb.Append(m_Test == Test.Fitts ? "<b>Wyniki – Fitts</b> (średni czas)\n" : "<b>Wyniki – Hick</b> (średni czas)\n");
            foreach (var g in data.GroupBy(d => d.key).OrderBy(g => g.First().x))
                sb.Append($"{g.Key,-14} x={g.First().x:0.00}  T={g.Average(d => d.t) * 1000f:0} ms  (prób: {g.Count()})\n");
            if (Fit(data, out float a, out float b))
                sb.Append($"\nDopasowanie: T = {a * 1000f:0} ms + {b * 1000f:0} ms · x\n");
            else
                sb.Append("\nWykonaj kilka prób dla różnych wartości, aby dopasować prostą.\n");
            sb.Append($"Błędne kliknięcia: {m_Errors}");
            results.text = sb.ToString();
            DemoHud.SetStatus("ux", m_Test == Test.Fitts
                ? "<b>Fitts:</b> x = ID = log2(D/W + 1) [bit]\nWiększy i bliższy cel → krótszy czas."
                : "<b>Hick:</b> x = log2(n + 1)\nWięcej opcji → dłuższa decyzja.");
        }

        /// <summary>Regresja liniowa T = a + b·x (metoda najmniejszych kwadratów).</summary>
        static bool Fit(List<(float x, float t, string key)> data, out float a, out float b)
        {
            a = b = 0f;
            if (data.Count < 3) return false;
            float mx = data.Average(d => d.x), mt = data.Average(d => d.t);
            float sxx = data.Sum(d => (d.x - mx) * (d.x - mx));
            if (sxx < 1e-4f) return false;
            b = data.Sum(d => (d.x - mx) * (d.t - mt)) / sxx;
            a = mt - b * mx;
            return true;
        }
    }
}
