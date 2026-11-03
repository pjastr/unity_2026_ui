using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Scena 05: TextMeshPro – znaczniki rich text, Auto Size, tryby Overflow i długość tekstów w różnych językach.
    ///  • Spacja – kolejny język (PL → EN → DE): ten sam napis ma różną długość,
    ///  • Shift – kolejny sposób radzenia sobie z tekstem, który się nie mieści:
    ///    Overflow → Ellipsis → Truncate → Auto Size → Zawijanie wierszy.
    /// </summary>
    public class TextDemo : DemoActionBase
    {
        [Tooltip("Pola tekstowe o stałym rozmiarze (np. przyciski)")]
        public TMP_Text[] fixedBoxes;
        public TMP_Text languageLabel;
        public TMP_Text outlineSample;

        int m_Lang;
        int m_Mode;

        static readonly string[] LangNames = { "polski", "angielski", "niemiecki" };

        // [język][napis]
        static readonly string[][] Texts =
        {
            new[] { "Kontynuuj", "Ustawienia grafiki", "Zapisz i wyjdź do menu", "Wczytywanie zapisanej gry…" },
            new[] { "Continue", "Graphics settings", "Save and quit to menu", "Loading saved game…" },
            new[] { "Fortsetzen", "Grafikeinstellungen", "Speichern und zum Hauptmenü zurückkehren", "Gespeichertes Spiel wird geladen…" },
        };

        static readonly string[] ModeNames =
        {
            "Overflow (tekst wychodzi poza pole)", "Ellipsis (…)", "Truncate (ucięcie)",
            "Auto Size (min 14, max 30)", "Zawijanie wierszy (Text Wrapping = Normal)",
        };

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Lang = (m_Lang + 1) % Texts.Length;
            Apply();
            DemoHud.Log("język: " + LangNames[m_Lang]);
        }

        public override void OnSecondaryAction()
        {
            m_Mode = (m_Mode + 1) % ModeNames.Length;
            Apply();
            DemoHud.Log("tryb: " + ModeNames[m_Mode]);
        }

        void Apply()
        {
            for (int i = 0; i < fixedBoxes.Length; i++)
            {
                var t = fixedBoxes[i];
                t.text = Texts[m_Lang][i % Texts[m_Lang].Length];
                t.enableAutoSizing = false;
                t.fontSize = 30f;
                t.textWrappingMode = TextWrappingModes.NoWrap;
                switch (m_Mode)
                {
                    case 0: t.overflowMode = TextOverflowModes.Overflow; break;
                    case 1: t.overflowMode = TextOverflowModes.Ellipsis; break;
                    case 2: t.overflowMode = TextOverflowModes.Truncate; break;
                    case 3:
                        t.overflowMode = TextOverflowModes.Truncate;
                        t.enableAutoSizing = true;
                        t.fontSizeMin = 14f;
                        t.fontSizeMax = 30f;
                        break;
                    default:
                        t.overflowMode = TextOverflowModes.Truncate;
                        t.textWrappingMode = TextWrappingModes.Normal;
                        t.fontSize = 22f;
                        break;
                }
            }
            if (languageLabel != null)
                languageLabel.text = $"Język: <b>{LangNames[m_Lang]}</b>   Tryb: <b>{ModeNames[m_Mode]}</b>";
        }

        void Update()
        {
            if (outlineSample != null)
            {
                // obrys liczony w shaderze SDF – zmiana bez utraty ostrości
                outlineSample.outlineWidth = 0.1f + 0.1f * Mathf.Sin(Time.time * 2f);
                outlineSample.characterSpacing = 4f * Mathf.Sin(Time.time * 0.8f);
            }

            var sb = new System.Text.StringBuilder($"<b>Pola o stałym rozmiarze</b> (szerokość {fixedBoxes[0].rectTransform.rect.width:0} px)\n");
            foreach (var t in fixedBoxes)
            {
                float pref = t.GetPreferredValues(t.text, 10000f, 100f).x;
                bool fits = pref <= t.rectTransform.rect.width + 0.5f;
                string mark = fits ? "<color=#5c5>mieści się</color>" : "<color=#f66>za długi</color>";
                sb.Append($"\n{t.text.Length,3} zn., potrzeba {pref:0} px – {mark}");
                if (t.enableAutoSizing) sb.Append($", rozmiar {t.fontSize:0.0}");
            }
            sb.Append("\n\nTekst przetłumaczony bywa dłuższy o 20–35% – projektuj z zapasem.");
            DemoHud.SetStatus("text", sb.ToString());
        }
    }
}
