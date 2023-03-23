using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System;

namespace Goose2Client
{
    public class SpellSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image image;

        [SerializeField] public SpellInfo info;

        public bool HasSpell => info != null;

        public int SlotNumber { get; set; }
        public IWindow Window { get; set; }

        public Action<SpellInfo> OnDoubleClick { get; set; }
        public Action<int, int> OnMoveSpell { get; set; }

        internal void SetSpell(SpellInfo info)
        {
            if (string.IsNullOrEmpty(info.Name))
            {
                ClearSpell();
                return;
            }

            this.info = info;

            image.sprite = Helpers.GetSprite(info.GraphicId, info.GraphicFile);
            image.color = Color.white;
        }

        internal void ClearSpell()
        {
            info = null;
            image.color = new Color(0, 0, 0, 0); // make the image invisible rather than disabling the object, so drag drop still works
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HasSpell)
                TooltipManager.Instance.ShowTextTooltip(info.Name);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTextTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!HasSpell) return;

            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
                OnDoubleClick?.Invoke(this.info);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var fromSlot = eventData.pointerDrag?.GetComponent<SpellSlot>();
            if (fromSlot == null || !fromSlot.HasSpell) return;

            OnMoveSpell?.Invoke(fromSlot.SlotNumber, SlotNumber);
        }
    }
}