using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 12: menu ustawień zapisywane w PlayerPrefs.
    /// Zmiany działają od razu (podgląd) i są zapisywane; zmiana rozdzielczości wymaga potwierdzenia
    /// z automatycznym powrotem po 10 s (dobra praktyka UX – gracz nie zostaje z nieczytelnym obrazem).
    ///  • Spacja – przywróć ustawienia domyślne,
    ///  • Shift – wczytaj ponownie z PlayerPrefs (jak po ponownym uruchomieniu gry).
    /// </summary>
    public class SettingsDemo : DemoActionBase
    {
        public Slider volume;
        public TMP_Text volumeValue;
        public Button testSound;
        public Toggle fullscreen;
        public TMP_Dropdown resolution;
        public TMP_Dropdown quality;
        public Slider uiScale;
        public TMP_Text uiScaleValue;
        public CanvasScaler settingsScaler;
        public Slider textSize;
        public TMP_Text textSample;
        public Toggle subtitles;
        public Button resetButton;
        public CanvasGroup confirmPanel;
        public TMP_Text confirmText;
        public Button keepButton;
        public Button revertButton;

        const string KVolume = "ui_volume", KFull = "ui_fullscreen", KRes = "ui_resolution", KQuality = "ui_quality",
            KScale = "ui_scale", KText = "ui_textsize", KSubs = "ui_subtitles";

        readonly List<Resolution> m_Resolutions = new List<Resolution>();
        int m_PreviousRes;
        float m_ConfirmLeft;
        bool m_Loading;

        void Start()
        {
            // lista rozdzielczości bez powtórzeń (różne częstotliwości odświeżania)
            var labels = new List<string>();
            foreach (var r in Screen.resolutions)
            {
                string label = $"{r.width} × {r.height}";
                if (labels.Contains(label)) continue;
                labels.Add(label);
                m_Resolutions.Add(r);
            }
            if (m_Resolutions.Count == 0)
            {
                m_Resolutions.Add(new Resolution { width = Screen.width, height = Screen.height });
                labels.Add($"{Screen.width} × {Screen.height}");
            }
            resolution.ClearOptions();
            resolution.AddOptions(labels);
            quality.ClearOptions();
            quality.AddOptions(new List<string>(QualitySettings.names));

            volume.onValueChanged.AddListener(v => { if (m_Loading) return; AudioListener.volume = v / 100f; volumeValue.text = $"{v:0}%"; Save(KVolume, v); });
            testSound.onClick.AddListener(() => UiSound.Play(UiSound.Kind.Success, 0.8f));
            fullscreen.onValueChanged.AddListener(on => { if (m_Loading) return; Screen.fullScreen = on; Save(KFull, on ? 1 : 0); DemoHud.Log("Screen.fullScreen = " + on); });
            resolution.onValueChanged.AddListener(OnResolutionChanged);
            quality.onValueChanged.AddListener(i => { if (m_Loading) return; QualitySettings.SetQualityLevel(i, true); Save(KQuality, i); DemoHud.Log("Jakość: " + QualitySettings.names[i]); });
            uiScale.onValueChanged.AddListener(v => { if (m_Loading) return; ApplyUiScale(v); Save(KScale, v); });
            textSize.onValueChanged.AddListener(v => { if (m_Loading) return; textSample.fontSize = v; Save(KText, v); });
            subtitles.onValueChanged.AddListener(on => { if (m_Loading) return; Save(KSubs, on ? 1 : 0); });
            resetButton.onClick.AddListener(ResetDefaults);
            keepButton.onClick.AddListener(KeepResolution);
            revertButton.onClick.AddListener(RevertResolution);

            SetConfirm(false);
            Load();
        }

        static void Save(string key, float v) { PlayerPrefs.SetFloat(key, v); PlayerPrefs.Save(); }
        static void Save(string key, int v) { PlayerPrefs.SetInt(key, v); PlayerPrefs.Save(); }

        int CurrentResolutionIndex()
        {
            for (int i = 0; i < m_Resolutions.Count; i++)
                if (m_Resolutions[i].width == Screen.width && m_Resolutions[i].height == Screen.height)
                    return i;
            return m_Resolutions.Count - 1;
        }

        void Load()
        {
            m_Loading = true;
            volume.value = PlayerPrefs.GetFloat(KVolume, 80f);
            fullscreen.isOn = PlayerPrefs.GetInt(KFull, Screen.fullScreen ? 1 : 0) == 1;
            resolution.value = Mathf.Clamp(PlayerPrefs.GetInt(KRes, CurrentResolutionIndex()), 0, m_Resolutions.Count - 1);
            quality.value = Mathf.Clamp(PlayerPrefs.GetInt(KQuality, QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
            uiScale.value = PlayerPrefs.GetFloat(KScale, 1f);
            textSize.value = PlayerPrefs.GetFloat(KText, 26f);
            subtitles.isOn = PlayerPrefs.GetInt(KSubs, 1) == 1;
            resolution.RefreshShownValue();
            quality.RefreshShownValue();
            m_Loading = false;

            AudioListener.volume = volume.value / 100f;
            volumeValue.text = $"{volume.value:0}%";
            ApplyUiScale(uiScale.value);
            textSample.fontSize = textSize.value;
            m_PreviousRes = resolution.value;
            DemoHud.Log("wczytano ustawienia z PlayerPrefs");
        }

        void ResetDefaults()
        {
            foreach (var k in new[] { KVolume, KFull, KRes, KQuality, KScale, KText, KSubs })
                PlayerPrefs.DeleteKey(k);
            PlayerPrefs.Save();
            Load();
            UiSound.Play(UiSound.Kind.Back);
            DemoHud.Log("przywrócono ustawienia domyślne");
        }

        void ApplyUiScale(float s)
        {
            // większa skala UI = mniejsza rozdzielczość odniesienia Canvas Scalera
            settingsScaler.referenceResolution = new Vector2(UiFactory.RefWidth, UiFactory.RefHeight) / Mathf.Max(0.1f, s);
            uiScaleValue.text = $"{s * 100f:0}%";
        }

        void OnResolutionChanged(int index)
        {
            if (m_Loading) return;
            var r = m_Resolutions[index];
            Screen.SetResolution(r.width, r.height, fullscreen.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            DemoHud.Log($"Screen.SetResolution({r.width}, {r.height})" + (Application.isEditor ? " – w edytorze bez efektu" : ""));
            m_ConfirmLeft = 10f;
            SetConfirm(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(keepButton.gameObject);
        }

        void KeepResolution()
        {
            m_PreviousRes = resolution.value;
            Save(KRes, resolution.value);
            SetConfirm(false);
            DemoHud.Log("zachowano rozdzielczość");
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(resolution.gameObject);
        }

        void RevertResolution()
        {
            m_Loading = true;
            resolution.value = m_PreviousRes;
            resolution.RefreshShownValue();
            m_Loading = false;
            var r = m_Resolutions[m_PreviousRes];
            Screen.SetResolution(r.width, r.height, fullscreen.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            SetConfirm(false);
            DemoHud.Log("przywrócono poprzednią rozdzielczość");
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(resolution.gameObject);
        }

        void SetConfirm(bool on)
        {
            confirmPanel.alpha = on ? 1f : 0f;
            confirmPanel.interactable = on;
            confirmPanel.blocksRaycasts = on;
        }

        public override void OnMainAction() => ResetDefaults();

        public override void OnSecondaryAction() => Load();

        void Update()
        {
            if (confirmPanel.interactable)
            {
                m_ConfirmLeft -= Time.unscaledDeltaTime;
                confirmText.text = $"Zachować nową rozdzielczość?\nPowrót do poprzedniej za {Mathf.CeilToInt(m_ConfirmLeft)} s";
                if (m_ConfirmLeft <= 0f)
                    RevertResolution();
            }

            DemoHud.SetStatus("settings",
                "<b>PlayerPrefs</b>\n" +
                $"{KVolume} = {PlayerPrefs.GetFloat(KVolume, -1f):0}\n" +
                $"{KFull} = {PlayerPrefs.GetInt(KFull, -1)}\n" +
                $"{KRes} = {PlayerPrefs.GetInt(KRes, -1)}\n" +
                $"{KQuality} = {PlayerPrefs.GetInt(KQuality, -1)}\n" +
                $"{KScale} = {PlayerPrefs.GetFloat(KScale, -1f):0.00}\n" +
                $"{KText} = {PlayerPrefs.GetFloat(KText, -1f):0}\n" +
                $"{KSubs} = {PlayerPrefs.GetInt(KSubs, -1)}\n" +
                "(−1 – brak zapisanej wartości, użyto domyślnej)\n" +
                $"AudioListener.volume = {AudioListener.volume:0.00}   Ekran: {Screen.width}×{Screen.height}");
        }
    }
}
