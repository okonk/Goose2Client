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
    public class HotbarSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextTooltipEventHandler tooltip;

        public int SlotNumber { get; set; }
        public IWindow Window { get; set; }

        private ItemStats itemStats;
        private SpellInfo spellInfo;

        public bool IsEmpty => itemStats == null && spellInfo == null;

        public void OnDrop(PointerEventData eventData)
        {
            var fromSpell = eventData.pointerDrag?.GetComponent<SpellSlot>();
            if (fromSpell != null && fromSpell.HasSpell)
            {
                spellInfo = fromSpell.info;
                image.sprite = Helpers.GetSprite(spellInfo.GraphicId, spellInfo.GraphicFile);
                image.color = Color.white;
                tooltip.TooltipText = $"Spell: {spellInfo.Name}";
                return;
            }

            var fromItem = eventData.pointerDrag?.GetComponent<ItemSlot>();
            if (fromItem != null && fromItem.HasItem)
            {
                itemStats = fromItem.stats;
                image.sprite = Helpers.GetSprite(itemStats.GraphicId, itemStats.GraphicFile);
                image.color = Color.white;
                image.material = Instantiate(image.material);
                image.material.SetColor("_Tint", ColorH.RGBA(itemStats.GraphicR, itemStats.GraphicG, itemStats.GraphicB, itemStats.GraphicA));
                tooltip.TooltipText = $"Item: {itemStats.Name}";
                return;
            }

            var fromHotbar = eventData.pointerDrag?.GetComponent<HotbarSlot>();
            if (fromHotbar != null && !fromHotbar.IsEmpty)
            {
                image.sprite = fromHotbar.image.sprite;
                image.material = fromHotbar.image.material;
                cooldownOverlay.fillAmount = fromHotbar.cooldownOverlay.fillAmount;
                cooldownOverlay.gameObject.SetActive(fromHotbar.cooldownOverlay.gameObject.activeSelf);
                tooltip.TooltipText = fromHotbar.tooltip.TooltipText;
                itemStats = fromHotbar.itemStats;
                spellInfo = fromHotbar.spellInfo;

                fromHotbar.image.color = new Color(0, 0, 0, 0);
                fromHotbar.cooldownOverlay.gameObject.SetActive(false);
                itemStats = null;
                spellInfo = null;
                tooltip.TooltipText = null;

                return;
            }
        }
    }
}