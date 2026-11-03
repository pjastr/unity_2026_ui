using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>Liczba obrażeń: unosi się, zwalnia, powiększa na starcie i zanika (po czym się usuwa).</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class FloatingText : MonoBehaviour
    {
        public Vector2 velocity = new Vector2(0f, 150f);
        public float lifetime = 1.1f;

        TMP_Text m_Text;
        RectTransform m_Rt;
        float m_Age;

        void Awake()
        {
            m_Text = GetComponent<TMP_Text>();
            m_Rt = (RectTransform)transform;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            m_Age += dt;
            m_Rt.anchoredPosition += velocity * dt;
            velocity *= Mathf.Exp(-2.5f * dt);
            float pop = m_Age < 0.12f ? Mathf.Lerp(1.6f, 1f, m_Age / 0.12f) : 1f;
            m_Rt.localScale = Vector3.one * pop;
            var c = m_Text.color;
            c.a = Mathf.Clamp01(1f - (m_Age - lifetime * 0.6f) / (lifetime * 0.4f));
            m_Text.color = c;
            if (m_Age >= lifetime)
                Destroy(gameObject);
        }
    }
}
