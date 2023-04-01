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
    public class ItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI countText;

        [SerializeField] public ItemStats stats;

        public bool HasItem => stats != null;
        public int StackSize => stats?.StackSize ?? 0;

        public int SlotNumber { get; set; }
        public IWindow Window { get; set; }

        public Action<ItemStats> OnDoubleClick { get; set; }
        public Action<IWindow, int, int> OnDropItem { get; set; }

        internal void SetItem(ItemStats stats)
        {
            this.stats = stats;

            image.sprite = Helpers.GetSprite(stats.GraphicId, stats.GraphicFile);
            image.color = Color.white;
            image.material = Instantiate(image.material);
            image.material.SetColor("_Tint", ColorH.RGBA(stats.GraphicR, stats.GraphicG, stats.GraphicB, stats.GraphicA));

            countText.text = stats.StackSize.ToString();
            countText.gameObject.SetActive(stats.StackSize > 1);
        }

        internal void ClearItem()
        {
            stats = null;
            image.color = new Color(0, 0, 0, 0); // make the image invisible rather than disabling the object, so drag drop still works
            countText.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HasItem)
                TooltipManager.Instance.ShowItemTooltip(stats);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideItemTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!HasItem) return;

            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
                OnDoubleClick?.Invoke(this.stats);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var fromSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
            if (fromSlot == null || !fromSlot.HasItem) return;

            OnDropItem?.Invoke(fromSlot.Window, fromSlot.SlotNumber, SlotNumber);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DropTargetManager.Instance.gameObject.SetActive(true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DropTargetManager.Instance.gameObject.SetActive(false);
        }
    }
}