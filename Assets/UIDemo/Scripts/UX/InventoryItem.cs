using UnityEngine;
using UnityEngine.EventSystems;

namespace UIDemo
{
    /// <summary>
    /// Przedmiot ekwipunku, który można przeciągać (IBeginDragHandler, IDragHandler, IEndDragHandler).
    /// Podczas przeciągania przedmiot jest przenoszony na wierzch (dragLayer) i nie blokuje raycastów,
    /// dzięki czemu slot pod kursorem otrzymuje zdarzenie OnDrop.
    /// Najechanie kursorem pokazuje podpowiedź (Tooltip) z opóźnieniem.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class InventoryItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public string itemName = "Przedmiot";
        [TextArea] public string description = "";
        public RectTransform dragLayer;

        public InventorySlot Slot { get; set; }
        public static InventoryItem Dragged { get; private set; }

        CanvasGroup m_Group;
        Transform m_StartParent;

        void Awake()
        {
            m_Group = GetComponent<CanvasGroup>();
            Slot = GetComponentInParent<InventorySlot>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            m_StartParent = transform.parent;
            Dragged = this;
            transform.SetParent(dragLayer, true); // rysowany nad wszystkimi slotami
            m_Group.blocksRaycasts = false;        // slot pod kursorem dostanie OnDrop
            m_Group.alpha = 0.85f;
            transform.localScale = Vector3.one * 1.1f;
            Tooltip.Hide();
            UiSound.Play(UiSound.Kind.Hover, 0.4f);
        }

        public void OnDrag(PointerEventData e)
        {
            transform.position = e.position; // Canvas Overlay: pozycja w pikselach ekranu
        }

        public void OnEndDrag(PointerEventData e)
        {
            m_Group.blocksRaycasts = true;
            m_Group.alpha = 1f;
            transform.localScale = Vector3.one;
            Dragged = null;
            if (transform.parent == dragLayer)
            {
                // upuszczono poza slotem – powrót na miejsce
                PlaceIn(m_StartParent);
                UiSound.Play(UiSound.Kind.Back, 0.4f);
                DemoHud.Log($"{itemName}: upuszczono poza slotem – powrót");
            }
        }

        public void PlaceIn(Transform slotTransform)
        {
            transform.SetParent(slotTransform, false);
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8f, 8f);
            rt.offsetMax = new Vector2(-8f, -8f);
            rt.localScale = Vector3.one;
            Slot = slotTransform.GetComponent<InventorySlot>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (Dragged == null)
                Tooltip.Request(this);
        }

        public void OnPointerExit(PointerEventData e) => Tooltip.Cancel(this);
    }
}
