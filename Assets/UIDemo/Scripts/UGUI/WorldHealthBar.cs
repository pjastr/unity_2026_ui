using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Pasek zdrowia w przestrzeni świata (Canvas World Space nad postacią).
    /// Widoczny tylko po otrzymaniu obrażeń i tylko w pobliżu kamery – mniej "szumu" na ekranie.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        public Image fill;
        public Image ghost;
        public TMP_Text label;
        public CanvasGroup group;
        public float maxHealth = 100f;
        public float hideAfter = 2.5f;
        public float maxDistance = 25f;
        public bool alwaysVisible;

        public float Health { get; private set; }
        float m_LastHit = -100f;

        void Awake() => Health = maxHealth;

        public void Damage(float amount)
        {
            Health = Mathf.Max(0f, Health - amount);
            if (Health <= 0f) Health = maxHealth; // demonstracja – "odrodzenie"
            m_LastHit = Time.time;
        }

        void Update()
        {
            float ratio = Health / maxHealth;
            fill.fillAmount = ratio;
            if (ghost != null)
                ghost.fillAmount = Mathf.MoveTowards(Mathf.Max(ghost.fillAmount, ratio), ratio,
                    Time.time - m_LastHit > 0.4f ? 0.6f * Time.deltaTime : 0f);
            if (label != null)
                label.text = $"{Health:0}";

            var cam = Camera.main;
            float dist = cam != null ? Vector3.Distance(cam.transform.position, transform.position) : 0f;
            bool show = alwaysVisible || (Time.time - m_LastHit < hideAfter && dist < maxDistance);
            group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, 4f * Time.deltaTime);
        }
    }
}
