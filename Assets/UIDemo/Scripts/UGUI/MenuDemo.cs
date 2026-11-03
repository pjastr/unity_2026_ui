using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 11: menu, pauza i przejścia między ekranami.
    ///  • stos ekranów: menu główne → ustawienia; Esc (UI/Cancel) zawsze cofa o jeden ekran,
    ///    a zaznaczenie wraca na przycisk, z którego przyszliśmy,
    ///  • pauza: Time.timeScale = 0 – zatrzymuje obracający się sześcian (Time.deltaTime),
    ///    a animacje menu działają dalej (Time.unscaledDeltaTime) – patrz drugi sześcian,
    ///  • okno potwierdzenia (modalne) blokuje kliknięcia pod spodem; domyślnie zaznaczona opcja bezpieczna "Nie",
    ///  • Spacja – pauza / wznowienie w trakcie rozgrywki,
    ///  • Shift – długość przejść: 0,25 s ↔ 0 s (natychmiast).
    /// </summary>
    public class MenuDemo : DemoActionBase
    {
        [Header("Ekrany (CanvasGroup)")]
        public CanvasGroup mainMenu;
        public CanvasGroup settingsMenu;
        public CanvasGroup pauseMenu;
        public CanvasGroup confirmDialog;
        public CanvasGroup gameplayHud;

        [Header("Przyciski")]
        public Button playButton;
        public Button settingsButton;
        public Button quitButton;
        public Button settingsBackButton;
        public Button resumeButton;
        public Button pauseSettingsButton;
        public Button pauseMenuButton;
        public Button confirmYesButton;
        public Button confirmNoButton;

        [Header("Rozgrywka")]
        public Transform scaledSpinner;
        public Transform unscaledSpinner;

        float m_Fade = 0.25f;
        readonly List<CanvasGroup> m_Stack = new List<CanvasGroup>();
        readonly Dictionary<CanvasGroup, GameObject> m_ReturnSelection = new Dictionary<CanvasGroup, GameObject>();
        bool m_InGame;

        void Start()
        {
            foreach (var g in new[] { mainMenu, settingsMenu, pauseMenu, confirmDialog })
                SetVisible(g, false, 0f);
            SetVisible(gameplayHud, false, 0f);

            playButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(() => Push(settingsMenu, settingsBackButton.gameObject));
            quitButton.onClick.AddListener(() => Push(confirmDialog, confirmNoButton.gameObject, modal: true));
            settingsBackButton.onClick.AddListener(Pop);
            resumeButton.onClick.AddListener(Resume);
            pauseSettingsButton.onClick.AddListener(() => Push(settingsMenu, settingsBackButton.gameObject));
            pauseMenuButton.onClick.AddListener(BackToMainMenu);
            confirmNoButton.onClick.AddListener(Pop);
            confirmYesButton.onClick.AddListener(() =>
            {
                DemoHud.Log("Wyjście potwierdzone (w demie – powrót do menu głównego)");
                Pop();
            });

            Push(mainMenu, playButton.gameObject);
        }

        // ------------------------------------------------------------ stos ekranów

        void Push(CanvasGroup screen, GameObject firstSelected, bool modal = false)
        {
            var es = EventSystem.current;
            if (m_Stack.Count > 0)
            {
                var top = m_Stack[m_Stack.Count - 1];
                m_ReturnSelection[top] = es != null ? es.currentSelectedGameObject : null;
                if (modal)
                {
                    top.interactable = false; // poprzedni ekran widoczny, ale nieaktywny
                }
                else
                {
                    SetVisible(top, false, m_Fade);
                }
            }
            m_Stack.Add(screen);
            SetVisible(screen, true, m_Fade);
            if (es != null)
                es.SetSelectedGameObject(firstSelected);
            UiSound.Play(UiSound.Kind.Click, 0.4f);
            DemoHud.Log("Push: " + screen.name);
        }

        void Pop()
        {
            if (m_Stack.Count == 0) return;
            var top = m_Stack[m_Stack.Count - 1];
            m_Stack.RemoveAt(m_Stack.Count - 1);
            SetVisible(top, false, m_Fade);
            UiSound.Play(UiSound.Kind.Back, 0.4f);
            DemoHud.Log("Pop: " + top.name);
            if (m_Stack.Count == 0)
            {
                if (m_InGame) Resume();
                return;
            }
            var prev = m_Stack[m_Stack.Count - 1];
            SetVisible(prev, true, m_Fade);
            if (m_ReturnSelection.TryGetValue(prev, out var sel) && sel != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(sel);
        }

        void StartGame()
        {
            ClearStack();
            m_InGame = true;
            SetVisible(gameplayHud, true, m_Fade);
            Time.timeScale = 1f;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            DemoHud.Log("Rozgrywka – Esc lub Spacja: pauza");
        }

        void Pause()
        {
            if (!m_InGame || m_Stack.Count > 0) return;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Push(pauseMenu, resumeButton.gameObject);
        }

        void Resume()
        {
            ClearStack();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            DemoHud.Log("Wznowiono (Time.timeScale = 1)");
        }

        void BackToMainMenu()
        {
            ClearStack();
            m_InGame = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SetVisible(gameplayHud, false, m_Fade);
            Push(mainMenu, playButton.gameObject);
        }

        void ClearStack()
        {
            foreach (var g in m_Stack)
                SetVisible(g, false, m_Fade);
            m_Stack.Clear();
        }

        // ------------------------------------------------------------ przejścia

        readonly Dictionary<CanvasGroup, Coroutine> m_Fades = new Dictionary<CanvasGroup, Coroutine>();

        void SetVisible(CanvasGroup g, bool visible, float duration)
        {
            if (g == null) return;
            if (m_Fades.TryGetValue(g, out var running) && running != null)
                StopCoroutine(running);
            g.interactable = visible;
            g.blocksRaycasts = visible;
            if (duration <= 0f)
            {
                g.alpha = visible ? 1f : 0f;
                g.transform.localScale = Vector3.one;
                return;
            }
            m_Fades[g] = StartCoroutine(Fade(g, visible ? 1f : 0f, duration));
        }

        static IEnumerator Fade(CanvasGroup g, float to, float duration)
        {
            float from = g.alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime) // działa także przy timeScale = 0
            {
                float k = t / duration;
                k = k * k * (3f - 2f * k); // smoothstep
                g.alpha = Mathf.Lerp(from, to, k);
                float s = to > 0.5f ? Mathf.Lerp(0.95f, 1f, k) : Mathf.Lerp(1f, 0.97f, k);
                g.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            g.alpha = to;
            g.transform.localScale = Vector3.one;
        }

        // ------------------------------------------------------------ wejście

        public override void OnMainAction()
        {
            if (!m_InGame) return;
            if (m_Stack.Count == 0) Pause();
            else Resume();
        }

        public override void OnSecondaryAction()
        {
            m_Fade = m_Fade > 0f ? 0f : 0.25f;
            DemoHud.Log(m_Fade > 0f ? "przejścia: 0,25 s" : "przejścia: natychmiast (bez animacji)");
        }

        void Update()
        {
            if (DemoInput.PressedThisFrame(DemoInput.Cancel) && !DemoInput.IsTyping)
            {
                if (m_Stack.Count == 0) Pause();
                else if (m_Stack[m_Stack.Count - 1] != mainMenu) Pop();
            }

            if (scaledSpinner != null) scaledSpinner.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            if (unscaledSpinner != null) unscaledSpinner.Rotate(0f, 90f * Time.unscaledDeltaTime, 0f, Space.World);

            var names = new List<string>();
            foreach (var g in m_Stack) names.Add(g.name);
            var es = EventSystem.current;
            DemoHud.SetStatus("menu",
                $"<b>Time.timeScale = {Time.timeScale:0}</b>   AudioListener.pause = {AudioListener.pause}\n" +
                $"Stos ekranów: [{string.Join(" → ", names)}]\n" +
                $"Rozgrywka: {(m_InGame ? "tak" : "nie")}   Długość przejść: {m_Fade:0.00} s\n" +
                $"Zaznaczony: {(es != null && es.currentSelectedGameObject != null ? es.currentSelectedGameObject.name : "brak")}\n" +
                "Lewy sześcian: Time.deltaTime (staje w pauzie)\nPrawy sześcian: Time.unscaledDeltaTime");
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
