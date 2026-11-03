using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Informacja zwrotna dla elementu interaktywnego: dźwięk przy najechaniu/zaznaczeniu i kliknięciu
    /// oraz płynne powiększenie, gdy element jest najechany lub zaznaczony (nawigacja padem/klawiaturą).
    /// Interfejsy EventSystems: IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler,
    /// IPointerClickHandler, ISubmitHandler.
    /// </summary>
    public class UiFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
    {
        [Tooltip("Skala, gdy element jest najechany lub zaznaczony")]
        public float hoverScale = 1.06f;
        public float speed = 14f;
        public bool sounds = true;

        bool m_Hover;
        bool m_Selected;
        Selectable m_Selectable;

        void Awake() => m_Selectable = GetComponent<Selectable>();

        void OnDisable()
        {
            m_Hover = m_Selected = false;
            transform.localScale = Vector3.one;
        }

        bool Interactable => m_Selectable == null || m_Selectable.IsInteractable();

        public void OnPointerEnter(PointerEventData e)
        {
            m_Hover = true;
            if (sounds && Interactable) UiSound.Play(UiSound.Kind.Hover, 0.25f);
        }

        public void OnPointerExit(PointerEventData e) => m_Hover = false;

        public void OnSelect(BaseEventData e)
        {
            m_Selected = true;
            // przy nawigacji padem/klawiaturą dźwięk przy każdej zmianie zaznaczenia
            if (sounds && !m_Hover && Interactable) UiSound.Play(UiSound.Kind.Hover, 0.25f);
        }

        public void OnDeselect(BaseEventData e) => m_Selected = false;

        public void OnPointerClick(PointerEventData e) => ClickSound();

        public void OnSubmit(BaseEventData e) => ClickSound();

        void ClickSound()
        {
            if (!sounds) return;
            UiSound.Play(Interactable ? UiSound.Kind.Click : UiSound.Kind.Error, 0.45f);
        }

        void Update()
        {
            float target = (m_Hover || m_Selected) && Interactable ? hoverScale : 1f;
            float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
