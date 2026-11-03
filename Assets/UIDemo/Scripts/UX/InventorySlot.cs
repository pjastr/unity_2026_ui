using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Slot ekwipunku (IDropHandler). Upuszczenie przedmiotu na zajęty slot zamienia przedmioty miejscami.
    /// Slot "Kosz" usuwa przedmiot. Podświetlenie slotu przy najechaniu w trakcie przeciągania.
    /// </summary>
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isTrash;
        public Image background;
        public Color normalColor = new Color(0.2f, 0.24f, 0.32f, 1f);
        public Color hoverColor = new Color(0.45f, 0.55f, 0.75f, 1f);

        public InventoryItem Item => GetComponentInChildren<InventoryItem>();

        void Start()
        {
            if (background != null) background.color = normalColor;
        }

        public void OnDrop(PointerEventData e)
        {
            var dragged = e.pointerDrag != null ? e.pointerDrag.GetComponent<InventoryItem>() : null;
            if (dragged == null) return;
            if (background != null) background.color = normalColor;

            if (isTrash)
            {
                DemoHud.Log($"{dragged.itemName}: wyrzucono do kosza");
                UiSound.Play(UiSound.Kind.Error, 0.4f);
                Destroy(dragged.gameObject);
                return;
            }

            var current = Item;
            var from = dragged.Slot;
            if (current != null && current != dragged && from != null)
            {
                current.PlaceIn(from.transform); // zamiana miejscami
                DemoHud.Log($"zamiana: {dragged.itemName} ↔ {current.itemName}");
            }
            else
            {
                DemoHud.Log($"{dragged.itemName} → {name}");
            }
            dragged.PlaceIn(transform);
            UiSound.Play(UiSound.Kind.Click, 0.4f);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (InventoryItem.Dragged != null && background != null)
                background.color = isTrash ? new Color(0.8f, 0.3f, 0.3f) : hoverColor;
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (background != null) background.color = normalColor;
        }
    }
}
