using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Scena 10: UI w przestrzeni świata i rodzaje interfejsu w grach.
    ///  • paski zdrowia nad przeciwnikami – Canvas World Space + Billboard (UI przestrzenne, "spatial"),
    ///  • monitor terminala z tekstem – Canvas World Space na obiekcie (UI diegetyczne – istnieje w świecie gry),
    ///  • znacznik celu i strzałka przy krawędzi ekranu – Canvas Overlay + WorldToScreenPoint (UI niediegetyczne),
    ///  • Spacja – trafienie losowego przeciwnika (pasek pojawia się i znika po chwili),
    ///  • Shift – kamera krąży / stoi.
    /// </summary>
    public class WorldSpaceDemo : DemoActionBase
    {
        public Camera cam;
        public Transform[] enemies;
        public WorldHealthBar[] bars;
        public TMP_Text terminalText;
        public OffscreenIndicator indicator;
        public Vector3 orbitCenter = new Vector3(0f, 1f, 0f);
        public float orbitRadius = 14f;
        public float orbitHeight = 6f;

        bool m_Orbit = true;
        float m_Angle = 200f;
        Vector3[] m_Start;
        int m_Hits;

        void Start()
        {
            m_Start = new Vector3[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
                m_Start[i] = enemies[i].position;
        }

        public override void OnMainAction()
        {
            if (bars.Length == 0) return;
            int i = Random.Range(0, bars.Length);
            float dmg = Mathf.Round(Random.Range(10f, 35f));
            bars[i].Damage(dmg);
            m_Hits++;
            UiSound.Play(UiSound.Kind.Hit, 0.5f);
            DemoHud.Log($"{enemies[i].name}: −{dmg:0} HP");
        }

        public override void OnSecondaryAction()
        {
            m_Orbit = !m_Orbit;
            DemoHud.Log(m_Orbit ? "kamera krąży" : "kamera stoi");
        }

        void Update()
        {
            if (m_Orbit)
                m_Angle += 12f * Time.deltaTime;
            float a = m_Angle * Mathf.Deg2Rad;
            cam.transform.position = orbitCenter + new Vector3(Mathf.Cos(a) * orbitRadius, orbitHeight, Mathf.Sin(a) * orbitRadius);
            cam.transform.LookAt(orbitCenter);

            for (int i = 0; i < enemies.Length; i++)
            {
                float t = Time.time * 0.6f + i * 1.7f;
                enemies[i].position = m_Start[i] + new Vector3(Mathf.Sin(t) * 1.5f, 0f, Mathf.Cos(t * 0.7f) * 1.5f);
            }

            if (terminalText != null)
                terminalText.text =
                    "<color=#7CFC9A>TERMINAL-07</color>\n" +
                    $"Czas: {System.DateTime.Now:HH:mm:ss}\n" +
                    $"Trafienia: {m_Hits}\n" +
                    $"Przeciwnicy: {enemies.Length}\n" +
                    "<color=#7CFC9A>> system OK_</color>";

            DemoHud.SetStatus("world",
                "<b>Rodzaje UI w tej scenie</b>\n" +
                "• paski nad przeciwnikami – przestrzenne (World Space + Billboard)\n" +
                "• ekran terminala – diegetyczne (część świata gry)\n" +
                "• znacznik celu – niediegetyczne (Overlay + WorldToScreenPoint)\n" +
                $"Cel widoczny na ekranie: {(indicator != null && indicator.OnScreen ? "TAK" : "NIE – strzałka przy krawędzi")}\n" +
                $"Kamera: {(m_Orbit ? "krąży" : "stoi")}");
        }
    }
}
