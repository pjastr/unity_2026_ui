using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIDemo
{
    /// <summary>
    /// Wspólny "menedżer" każdej sceny demonstracyjnej:
    ///  • panel z tytułem i opisem (po polsku) – przełączany klawiszem C,
    ///  • panel stanu (prawy górny róg): wartości na żywo + dziennik zdarzeń,
    ///  • pasek podpowiedzi sterowania (dół ekranu),
    ///  • skróty: Spacja – akcja sceny, Shift – akcja dodatkowa, przytrzymaj E – reset,
    ///    1/2 – poprzednia/następna scena.
    /// Panele HUD nie przechwytują kliknięć (Raycast Target wyłączony) – nie przeszkadzają w obsłudze UI sceny.
    /// Skróty są wyłączone, gdy gracz pisze w polu tekstowym.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DemoHud : MonoBehaviour
    {
        public static DemoHud Instance { get; private set; }

        [Header("Treść")]
        public string title = "Tytuł sceny";
        [TextArea(5, 30)] public string description = "Opis";
        [Tooltip("Co robi Spacja / Shift w tej scenie")]
        [TextArea(1, 4)] public string sceneHint = "<b>Spacja</b> – akcja sceny";

        [Header("Referencje (ustawiane przez generator)")]
        public UiSkin skin;
        public GameObject descriptionPanel;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public TMP_Text hintText;
        public GameObject statusPanel;
        public TMP_Text statusText;

        [Header("Dziennik")]
        public int maxLogLines = 6;

        readonly List<string> m_Log = new List<string>();
        readonly List<string> m_StatusKeys = new List<string>();
        readonly Dictionary<string, string> m_Status = new Dictionary<string, string>();
        readonly StringBuilder m_Sb = new StringBuilder();
        bool m_Dirty = true;

        void Awake()
        {
            Instance = this;
            if (skin != null)
                UiFactory.Skin = skin;
            Time.timeScale = 1f;          // na wypadek przejścia ze sceny z pauzą
            AudioListener.pause = false;
            Refresh();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Refresh()
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            if (hintText != null)
            {
                int idx = SceneManager.GetActiveScene().buildIndex;
                int count = SceneManager.sceneCountInBuildSettings;
                string sceneInfo = idx >= 0 && count > 0 ? $"Scena {idx + 1}/{count}" : "Scena";
                hintText.text =
                    $"<b>{sceneInfo}</b>   {sceneHint}\n" +
                    "<b>C</b> – opis   <b>przytrzymaj E</b> – reset   <b>1/2</b> – poprzednia/następna scena";
            }
        }

        void Update()
        {
            if (DemoInput.IsTyping)
                return;

            if (DemoInput.PressedThisFrame(DemoInput.Jump))
                DemoActionBase.InvokeMain();

            if (DemoInput.PressedThisFrame(DemoInput.Sprint))
                DemoActionBase.InvokeSecondary();

            if (DemoInput.PerformedThisFrame(DemoInput.Interact))
                ReloadScene();

            if (DemoInput.PressedThisFrame(DemoInput.Crouch) && descriptionPanel != null)
                descriptionPanel.SetActive(!descriptionPanel.activeSelf);

            if (DemoInput.PressedThisFrame(DemoInput.Next))
                LoadRelative(+1);
            if (DemoInput.PressedThisFrame(DemoInput.Previous))
                LoadRelative(-1);
        }

        void LateUpdate()
        {
            if (!m_Dirty || statusText == null)
                return;
            m_Dirty = false;
            m_Sb.Clear();
            foreach (var k in m_StatusKeys)
            {
                if (string.IsNullOrEmpty(m_Status[k]))
                    continue;
                if (m_Sb.Length > 0) m_Sb.Append('\n');
                m_Sb.Append(m_Status[k]);
            }
            if (m_Log.Count > 0)
            {
                if (m_Sb.Length > 0) m_Sb.Append("\n\n");
                m_Sb.Append("<b>Dziennik zdarzeń</b>");
                foreach (var line in m_Log)
                    m_Sb.Append('\n').Append(line);
            }
            statusText.text = m_Sb.ToString();
            if (statusPanel != null)
                statusPanel.SetActive(m_Sb.Length > 0);
        }

        public static void ReloadScene()
        {
            Time.timeScale = 1f;
            var s = SceneManager.GetActiveScene();
            if (s.buildIndex >= 0)
                SceneManager.LoadScene(s.buildIndex);
            else
                SceneManager.LoadScene(s.name);
        }

        public static void LoadRelative(int delta)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (count <= 0)
            {
                Debug.LogWarning("[UI Demo] Brak scen w Build Settings – użyj menu UI Demo > Generuj wszystkie sceny.");
                return;
            }
            Time.timeScale = 1f;
            int idx = SceneManager.GetActiveScene().buildIndex;
            if (idx < 0) idx = 0;
            int next = ((idx + delta) % count + count) % count;
            SceneManager.LoadScene(next);
        }

        // ---------------- panel stanu ----------------

        /// <summary>Ustawia (lub zastępuje) blok tekstu w panelu stanu. Klucz porządkuje bloki.</summary>
        public static void SetStatus(string key, string text)
        {
            if (Instance == null)
                return;
            var h = Instance;
            if (!h.m_Status.ContainsKey(key))
                h.m_StatusKeys.Add(key);
            if (h.m_Status.TryGetValue(key, out var old) && old == text)
                return;
            h.m_Status[key] = text;
            h.m_Dirty = true;
        }

        public static void Log(string line)
        {
            if (Instance == null)
            {
                Debug.Log("[UI Demo] " + line);
                return;
            }
            var h = Instance;
            h.m_Log.Add($"<color=#9fd3ff>{Time.unscaledTime:0.0}s</color>  {line}");
            while (h.m_Log.Count > h.maxLogLines)
                h.m_Log.RemoveAt(0);
            h.m_Dirty = true;
        }
    }
}
