using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 09: HUD i informacja zwrotna (feedback, "juice").
    ///  • Spacja – otrzymaj obrażenia: pasek zdrowia spada od razu, a "ślad" (ghost bar) dogania go powoli,
    ///    pasek drży, ekran błyska czerwoną winietą, nad celem pojawiają się liczby obrażeń,
    ///  • Shift – umiejętność "Leczenie" (+25 HP) z czasem odnowienia 4 s (Image Filled Radial 360).
    /// HUD adaptacyjny: pełny pasek zdrowia po 3 s bez zmian staje się półprzezroczysty;
    /// przy niskim zdrowiu pasek pulsuje. Powiadomienia (toasty) ustawiają się w kolejce.
    /// </summary>
    public class HudDemo : DemoActionBase
    {
        [Header("Zdrowie")]
        public Image healthFront;
        public Image healthGhost;
        public RectTransform healthBar;
        public TMP_Text healthText;
        public CanvasGroup healthGroup;
        public float maxHealth = 100f;

        [Header("Umiejętność")]
        public Image abilityIcon;
        public Image abilityCooldown;
        public TMP_Text abilityText;
        public float cooldown = 4f;

        [Header("Efekty")]
        public Image vignette;
        public RectTransform target;
        public RectTransform floatingRoot;
        public RectTransform toastRoot;

        float m_Health;
        float m_CooldownLeft;
        float m_LastChange = -10f;
        float m_Shake;
        float m_Flash;
        Vector2 m_BarPos;
        readonly List<(CanvasGroup group, float born)> m_Toasts = new List<(CanvasGroup, float)>();

        void Start()
        {
            m_Health = maxHealth;
            healthFront.fillAmount = healthGhost.fillAmount = 1f;
            m_BarPos = healthBar.anchoredPosition;
            Toast("Witaj! Spacja – obrażenia, Shift – leczenie.");
        }

        public override void OnMainAction()
        {
            if (m_Health <= 0f)
            {
                m_Health = maxHealth;
                Toast("Odrodzenie – pełne zdrowie.");
                UiSound.Play(UiSound.Kind.Success);
                return;
            }
            float dmg = Mathf.Round(Random.Range(8f, 26f));
            bool crit = Random.value < 0.2f;
            if (crit) dmg *= 2f;
            m_Health = Mathf.Max(0f, m_Health - dmg);
            m_LastChange = Time.time;
            m_Shake = 0.35f;
            m_Flash = crit ? 0.9f : 0.55f;
            SpawnNumber(crit ? $"<size=150%>{dmg:0}!</size>" : dmg.ToString("0"), crit ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.35f, 0.3f));
            UiSound.Play(UiSound.Kind.Hit, 0.6f);
            DemoHud.Log($"obrażenia: {dmg:0}{(crit ? " (krytyczne)" : "")}");
            if (m_Health <= 0f)
                Toast("Pokonany! Spacja – odrodzenie.");
            else if (m_Health < 30f)
                Toast("Niski poziom zdrowia – użyj leczenia (Shift).");
        }

        public override void OnSecondaryAction()
        {
            if (m_CooldownLeft > 0f)
            {
                UiSound.Play(UiSound.Kind.Error);
                Toast($"Leczenie niedostępne – jeszcze {m_CooldownLeft:0.0} s.");
                return;
            }
            float heal = Mathf.Min(25f, maxHealth - m_Health);
            m_Health += heal;
            m_LastChange = Time.time;
            m_CooldownLeft = cooldown;
            SpawnNumber("+" + heal.ToString("0"), new Color(0.4f, 1f, 0.5f));
            UiSound.Play(UiSound.Kind.Success, 0.5f);
            DemoHud.Log($"leczenie: +{heal:0}");
        }

        void SpawnNumber(string text, Color color)
        {
            var t = UiFactory.Text("Liczba", floatingRoot, text, 46f, color, TextAlignmentOptions.Center, FontStyles.Bold);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.outlineWidth = 0.25f;
            t.outlineColor = new Color32(0, 0, 0, 220);
            var rt = t.rectTransform;
            rt.sizeDelta = new Vector2(200f, 60f);
            Vector2 start = target != null ? target.anchoredPosition + new Vector2(Random.Range(-60f, 60f), 70f) : Vector2.zero;
            rt.anchoredPosition = start;
            var f = t.gameObject.AddComponent<FloatingText>();
            f.velocity = new Vector2(Random.Range(-40f, 40f), 160f);
        }

        void Toast(string message)
        {
            var panel = UiFactory.Panel("Powiadomienie", toastRoot, new Color(0.08f, 0.1f, 0.14f, 0.92f));
            UiFactory.Prefer(panel.gameObject, 620f, 56f);
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            var t = UiFactory.Text("Tekst", panel.transform, message, 24f, null, TextAlignmentOptions.Center);
            t.rectTransform.Fill(8f);
            m_Toasts.Add((group, Time.time));
            while (m_Toasts.Count > 3)
            {
                Destroy(m_Toasts[0].group.gameObject);
                m_Toasts.RemoveAt(0);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float ratio = m_Health / maxHealth;

            // pasek: przód od razu, ślad z opóźnieniem 0,4 s i stałą prędkością
            healthFront.fillAmount = ratio;
            if (healthGhost.fillAmount < ratio)
                healthGhost.fillAmount = ratio; // leczenie – ślad od razu
            else if (Time.time - m_LastChange > 0.4f)
                healthGhost.fillAmount = Mathf.MoveTowards(healthGhost.fillAmount, ratio, 0.5f * dt);
            healthText.text = $"{m_Health:0} / {maxHealth:0}";

            // kolor: zielony → żółty → czerwony; pulsowanie przy niskim zdrowiu
            Color c = ratio > 0.5f ? Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.3f, 0.85f, 0.4f), (ratio - 0.5f) * 2f)
                                   : Color.Lerp(new Color(0.95f, 0.25f, 0.2f), new Color(1f, 0.85f, 0.2f), ratio * 2f);
            if (ratio < 0.3f && ratio > 0f)
                c = Color.Lerp(c, Color.white, 0.35f * (0.5f + 0.5f * Mathf.Sin(Time.time * 10f)));
            healthFront.color = c;

            // drżenie paska
            m_Shake = Mathf.Max(0f, m_Shake - dt);
            healthBar.anchoredPosition = m_BarPos + (m_Shake > 0f ? Random.insideUnitCircle * 10f * m_Shake / 0.35f : Vector2.zero);

            // HUD adaptacyjny – pełne zdrowie bez zmian przez 3 s → półprzezroczysty
            float targetAlpha = ratio >= 0.999f && Time.time - m_LastChange > 3f ? 0.25f : 1f;
            healthGroup.alpha = Mathf.MoveTowards(healthGroup.alpha, targetAlpha, 2f * dt);

            // winieta obrażeń
            m_Flash = Mathf.Max(0f, m_Flash - 1.8f * dt);
            float lowHealth = ratio < 0.3f ? 0.25f + 0.1f * Mathf.Sin(Time.time * 4f) : 0f;
            vignette.color = new Color(0.85f, 0.05f, 0.05f, Mathf.Max(m_Flash, lowHealth));

            // odnowienie umiejętności
            m_CooldownLeft = Mathf.Max(0f, m_CooldownLeft - dt);
            abilityCooldown.fillAmount = m_CooldownLeft / cooldown;
            abilityText.text = m_CooldownLeft > 0f ? m_CooldownLeft.ToString("0.0") : "Shift";
            abilityIcon.color = m_CooldownLeft > 0f ? new Color(0.5f, 0.5f, 0.5f) : Color.white;

            // powiadomienia: pojawienie się, 2,5 s widoczności, zanikanie
            for (int i = m_Toasts.Count - 1; i >= 0; i--)
            {
                var (group, born) = m_Toasts[i];
                float age = Time.time - born;
                group.alpha = age < 0.2f ? age / 0.2f : age < 2.7f ? 1f : 1f - (age - 2.7f) / 0.5f;
                if (age > 3.2f)
                {
                    Destroy(group.gameObject);
                    m_Toasts.RemoveAt(i);
                }
            }

            DemoHud.SetStatus("hud",
                $"<b>Zdrowie:</b> {m_Health:0}/{maxHealth:0}   front.fillAmount = {healthFront.fillAmount:0.00}, ślad = {healthGhost.fillAmount:0.00}\n" +
                $"Alfa paska (HUD adaptacyjny): {healthGroup.alpha:0.00}\n" +
                $"Odnowienie: {m_CooldownLeft:0.0} s (Image Filled, Radial 360)\n" +
                $"Powiadomienia w kolejce: {m_Toasts.Count}");
        }
    }
}
