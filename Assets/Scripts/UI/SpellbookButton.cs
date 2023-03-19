using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class SpellbookButton : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private bool isNext = false;

        [SerializeField] private SpellbookWindow window;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isNext)
                window.OnNextClicked();
            else
                window.OnBackClicked();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var fromSlot = eventData.pointerDrag?.GetComponent<SpellSlot>();
            if (fromSlot == null || !fromSlot.HasSpell) return;

            window.MoveSpell(fromSlot.SlotNumber, isNext);
        }
    }
}