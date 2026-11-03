using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Podpowiedź (tooltip) pojawiająca się po krótkim opóźnieniu nad przedmiotem.
    /// Podąża za kursorem i nie wychodzi poza ekran (zmiana pivota przy prawej/dolnej krawędzi).
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance { get; private set; }

        public RectTransform panel;
        public CanvasGroup group;
        public TMP_Text title;
        public TMP_Text body;
        public Canvas canvas;
        public float delay = 0.4f;
        public Vector2 offset = new Vector2(18f, -18f);

        InventoryItem m_Item;
        float m_RequestTime;

        void Awake()
        {
            Instance = this;
            group.alpha = 0f;
            group.blocksRaycasts = false; // podpowiedź nie może przechwytywać kursora
        }

        public static void Request(InventoryItem item)
        {
            if (Instance == null) return;
            Instance.m_Item = item;
            Instance.m_RequestTime = Time.unscaledTime;
            Instance.title.text = item.itemName;
            Instance.body.text = item.description;
        }

        public static void Cancel(InventoryItem item)
        {
            if (Instance != null && Instance.m_Item == item)
                Instance.m_Item = null;
        }

        public static void Hide()
        {
            if (Instance != null) Instance.m_Item = null;
        }

        void Update()
        {
            bool visible = m_Item != null && Time.unscaledTime - m_RequestTime >= delay;
            group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, 10f * Time.unscaledDeltaTime);
            if (group.alpha <= 0f) return;

            Vector2 p = DemoInput.PointerPosition;
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            Vector2 size = panel.rect.size * scale;
            // domyślnie podpowiedź na prawo i w dół od kursora; przy krawędzi – po drugiej stronie
            float px = p.x + offset.x + size.x > Screen.width ? 1f : 0f;
            float py = p.y + offset.y - size.y < 0f ? 0f : 1f;
            panel.pivot = new Vector2(px, py);
            panel.position = p + new Vector2(px > 0.5f ? -offset.x : offset.x, py > 0.5f ? offset.y : -offset.y);
        }
    }
}
