using System.Collections.Generic;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Krótkie dźwięki interfejsu generowane w kodzie (bez plików audio): najechanie, kliknięcie,
    /// błąd, sukces. Informacja zwrotna (feedback) dźwiękiem – próg Doherty'ego: reakcja natychmiast.
    /// Głośność całości reguluje AudioListener.volume (scena "Ustawienia").
    /// </summary>
    public static class UiSound
    {
        public enum Kind { Hover, Click, Back, Error, Success, Hit }

        static AudioSource s_Source;
        static readonly Dictionary<Kind, AudioClip> s_Clips = new Dictionary<Kind, AudioClip>();
        public static bool Enabled = true;

        public static void Play(Kind kind, float volume = 0.5f)
        {
            if (!Enabled || !Application.isPlaying)
                return;
            if (s_Source == null)
            {
                var go = new GameObject("UiSound");
                Object.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.HideInHierarchy;
                s_Source = go.AddComponent<AudioSource>();
                s_Source.playOnAwake = false;
                s_Source.spatialBlend = 0f;
                s_Source.ignoreListenerPause = true; // dźwięki menu słychać także w pauzie
            }
            if (!s_Clips.TryGetValue(kind, out var clip) || clip == null)
            {
                clip = Create(kind);
                s_Clips[kind] = clip;
            }
            s_Source.PlayOneShot(clip, volume);
        }

        static AudioClip Create(Kind kind)
        {
            const int rate = 44100;
            float f0, f1, dur;
            switch (kind)
            {
                case Kind.Hover: f0 = 1400f; f1 = 1500f; dur = 0.03f; break;
                case Kind.Click: f0 = 900f; f1 = 600f; dur = 0.06f; break;
                case Kind.Back: f0 = 600f; f1 = 400f; dur = 0.08f; break;
                case Kind.Error: f0 = 220f; f1 = 180f; dur = 0.22f; break;
                case Kind.Success: f0 = 660f; f1 = 1320f; dur = 0.18f; break;
                default: f0 = 140f; f1 = 70f; dur = 0.12f; break; // Hit
            }
            int n = Mathf.CeilToInt(rate * dur);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(f0, f1, t);
                phase += 2f * Mathf.PI * f / rate;
                float env = Mathf.Min(1f, t * 40f) * (1f - t) * (1f - t);
                float s = Mathf.Sin(phase);
                if (kind == Kind.Error) s = Mathf.Sign(s) * 0.6f;               // "brzęczyk"
                if (kind == Kind.Hit) s += 0.3f * (Random.value * 2f - 1f);     // szum uderzenia
                data[i] = s * env * 0.6f;
            }
            var clip = AudioClip.Create("UiSound_" + kind, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
