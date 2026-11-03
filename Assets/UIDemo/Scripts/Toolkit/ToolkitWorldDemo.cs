using UnityEngine;
using UnityEngine.UIElements;

namespace UIDemo
{
    /// <summary>
    /// Scena 18: UI Toolkit w przestrzeni świata (Unity 6.2+).
    /// Panel Settings z Render Mode = World Space; każdy panel to UIDocument na obiekcie sceny
    /// (World Space Size Mode = Fixed, rozmiar w pikselach panelu; skala obiektu ustala rozmiar w metrach).
    ///  • Spacja – nowe losowe wartości na panelach,
    ///  • Shift – panele zwrócone do kamery (Billboard) / stała orientacja.
    /// Interakcja z panelami świata wymaga komponentu Panel Input Configuration (tu panele tylko wyświetlają).
    /// </summary>
    public class ToolkitWorldDemo : DemoActionBase
    {
        public Camera cam;
        public UIDocument[] panels;
        public string[] titles;
        public Vector3 orbitCenter = new Vector3(0f, 2f, 0f);
        public float orbitRadius = 13f;

        float m_Angle = 230f;
        bool m_Billboard = true;
        float[] m_Values;
        float[] m_Targets;

        void Start()
        {
            m_Values = new float[panels.Length];
            m_Targets = new float[panels.Length];
            Randomize();
            for (int i = 0; i < panels.Length; i++)
            {
                var root = panels[i].rootVisualElement;
                var title = root.Q<Label>("title");
                if (title != null && titles != null && i < titles.Length)
                    title.text = titles[i];
            }
        }

        void Randomize()
        {
            for (int i = 0; i < m_Targets.Length; i++)
                m_Targets[i] = Random.Range(10f, 100f);
        }

        public override void OnMainAction()
        {
            Randomize();
            UiSound.Play(UiSound.Kind.Click, 0.4f);
            DemoHud.Log("nowe wartości paneli");
        }

        public override void OnSecondaryAction()
        {
            m_Billboard = !m_Billboard;
            foreach (var p in panels)
            {
                var b = p.GetComponent<Billboard>();
                if (b != null) b.enabled = m_Billboard;
                if (!m_Billboard) p.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            }
            DemoHud.Log(m_Billboard ? "panele zwrócone do kamery" : "stała orientacja paneli");
        }

        void Update()
        {
            m_Angle += 10f * Time.deltaTime;
            float a = m_Angle * Mathf.Deg2Rad;
            cam.transform.position = orbitCenter + new Vector3(Mathf.Cos(a) * orbitRadius, 4f, Mathf.Sin(a) * orbitRadius);
            cam.transform.LookAt(orbitCenter);

            for (int i = 0; i < panels.Length; i++)
            {
                m_Values[i] = Mathf.MoveTowards(m_Values[i], m_Targets[i], 40f * Time.deltaTime);
                var root = panels[i].rootVisualElement;
                if (root == null) continue;
                var bar = root.Q<ProgressBar>("bar");
                if (bar != null)
                {
                    bar.value = m_Values[i];
                    bar.title = $"{m_Values[i]:0}%";
                }
                var desc = root.Q<Label>("desc");
                if (desc != null)
                    desc.text = $"Odległość od kamery: {Vector3.Distance(cam.transform.position, panels[i].transform.position):0.0} m";
            }

            var first = panels.Length > 0 ? panels[0] : null;
            DemoHud.SetStatus("world",
                "<b>UI Toolkit – World Space</b>\n" +
                (first != null
                    ? $"Panel Settings: {first.panelSettings.name}, Render Mode = {first.panelSettings.renderMode}\n" +
                      $"UIDocument: worldSpaceSizeMode = {first.worldSpaceSizeMode}, size = {first.worldSpaceSize.x:0}×{first.worldSpaceSize.y:0} px\n" +
                      $"skala obiektu = {first.transform.lossyScale.x:0.000}\n"
                    : "") +
                $"Billboard: {(m_Billboard ? "tak" : "nie")}");
        }
    }
}
