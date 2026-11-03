using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Scena 13: ekwipunek z przeciąganiem (drag &amp; drop) i podpowiedziami (tooltip).
    ///  • przeciągnij przedmiot do innego slotu (zamiana, gdy zajęty) albo do kosza,
    ///  • Spacja – automatyczne sortowanie (prawo Teslera: złożoność przejmuje gra, a nie gracz),
    ///  • Shift – opóźnienie podpowiedzi: 0 s → 0,4 s → 1 s (zbyt szybkie migają, zbyt wolne irytują).
    /// </summary>
    public class DragDropDemo : DemoActionBase
    {
        public InventorySlot[] slots;
        public Tooltip tooltip;

        static readonly float[] Delays = { 0f, 0.4f, 1f };
        int m_Delay = 1;

        void Start() => tooltip.delay = Delays[m_Delay];

        public override void OnMainAction()
        {
            var items = slots.Where(s => !s.isTrash).Select(s => s.Item).Where(i => i != null)
                .OrderBy(i => i.itemName).ToList();
            var targets = slots.Where(s => !s.isTrash).ToList();
            for (int i = 0; i < items.Count; i++)
                items[i].PlaceIn(targets[i].transform);
            UiSound.Play(UiSound.Kind.Success, 0.4f);
            DemoHud.Log("posortowano przedmioty alfabetycznie");
        }

        public override void OnSecondaryAction()
        {
            m_Delay = (m_Delay + 1) % Delays.Length;
            tooltip.delay = Delays[m_Delay];
            DemoHud.Log($"opóźnienie podpowiedzi: {Delays[m_Delay]:0.0} s");
        }

        void Update()
        {
            int count = slots.Count(s => !s.isTrash && s.Item != null);
            var dragged = InventoryItem.Dragged;
            DemoHud.SetStatus("dd",
                $"<b>Przedmiotów:</b> {count} / {slots.Count(s => !s.isTrash)} slotów\n" +
                $"Przeciągany: {(dragged != null ? dragged.itemName : "–")}\n" +
                $"Opóźnienie podpowiedzi: {tooltip.delay:0.0} s\n" +
                "Interfejsy: IBeginDragHandler, IDragHandler, IEndDragHandler (przedmiot), IDropHandler (slot)");
        }
    }
}
