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
    public class HotbarSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image cooldownOverlay;

        public Action<int> OnUseSlot { get; set; }

        public int SlotNumber { get; set; }
        public IWindow Window { get; set; }

        private int itemSlot = -1;
        private int spellSlot = -1;

        public ItemStats ItemStats { get; private set; }
        public SpellInfo SpellInfo { get; private set; }

        public bool IsEmpty => ItemStats == null && SpellInfo == null;

        public bool CanUse()
        {
            if (IsEmpty)
                return false;

            if (SpellInfo != null && GameManager.Instance.SpellCooldownManager.GetCooldownRemaining(SpellInfo) > TimeSpan.Zero)
                return false;

            return true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
                OnUseSlot?.Invoke(SlotNumber);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var fromSpell = eventData.pointerDrag?.GetComponent<SpellSlot>();
            if (fromSpell != null && fromSpell.HasSpell)
            {
                SetSpell(fromSpell.info);
                return;
            }

            var fromItem = eventData.pointerDrag?.GetComponent<ItemSlot>();
            if (fromItem != null && fromItem.HasItem && (fromItem.Window.WindowFrame == WindowFrames.Inventory || fromItem.Window.WindowFrame == WindowFrames.Equipped))
            {
                SetItem(fromItem.stats);
                return;
            }

            var fromHotbar = eventData.pointerDrag?.GetComponent<HotbarSlot>();
            if (fromHotbar != null && !fromHotbar.IsEmpty)
            {
                var currentItem = ItemStats;
                var currentSpell = SpellInfo;

                if (fromHotbar.ItemStats != null)
                    SetItem(fromHotbar.ItemStats);
                else
                    SetSpell(fromHotbar.SpellInfo);

                if (currentItem != null)
                    fromHotbar.SetItem(currentItem);
                else if (currentSpell != null)
                    fromHotbar.SetSpell(currentSpell);
                else
                    fromHotbar.Clear();

                return;
            }
        }

        public void SetItem(ItemStats stats)
        {
            Clear();

            ItemStats = stats;
            itemSlot = ItemStats.SlotNumber;
            image.sprite = Helpers.GetSprite(ItemStats.GraphicId, ItemStats.GraphicFile);
            image.color = Color.white;
            image.material = Instantiate(image.material);
            image.material.SetColor("_Tint", ColorH.RGBA(ItemStats.GraphicR, ItemStats.GraphicG, ItemStats.GraphicB, ItemStats.GraphicA));
            countText.text = ItemStats.StackSize.ToString();
            countText.gameObject.SetActive(ItemStats.StackSize > 1);
        }

        public void SetSpell(SpellInfo spell)
        {
            Clear(keepSpellSlot: spellSlot);

            if (string.IsNullOrEmpty(spell.Name))
                return;

            SpellInfo = spell;
            spellSlot = spell.SlotNumber;
            image.sprite = Helpers.GetSprite(SpellInfo.GraphicId, SpellInfo.GraphicFile);
            image.color = Color.white;
            cooldownOverlay.fillAmount = GetCooldownRemainingPercent();
            cooldownOverlay.gameObject.SetActive(true);
        }

        public void Clear(int keepItemSlot = -1, int keepSpellSlot = -1)
        {
            image.color = new Color(0, 0, 0, 0);
            cooldownOverlay.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            ItemStats = null;
            SpellInfo = null;
            itemSlot = keepItemSlot;
            spellSlot = keepSpellSlot;
        }

        public void OnInventorySlot(ItemStats stats)
        {
            if (itemSlot == stats.SlotNumber)
                SetItem(stats);
        }

        public void OnClearInventorySlot(int slotNumber)
        {
            if (itemSlot == slotNumber)
                Clear(keepItemSlot: itemSlot);
        }

        public void OnSpellbookSlot(SpellInfo spell)
        {
            if (spellSlot == spell.SlotNumber)
                SetSpell(spell);
        }

        private void Update()
        {
            if (SpellInfo == null) return;

            UpdateSpellCooldown();
        }

        private void UpdateSpellCooldown()
        {
            if (SpellInfo.Cooldown == TimeSpan.Zero) return;

            cooldownOverlay.fillAmount = GetCooldownRemainingPercent();
        }

        private float GetCooldownRemainingPercent()
        {
            var remaining = GameManager.Instance.SpellCooldownManager.GetCooldownRemaining(SpellInfo);

            if (remaining == TimeSpan.Zero)
                return 0;

            return (float)(remaining.TotalMilliseconds / SpellInfo.Cooldown.TotalMilliseconds);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (SpellInfo != null)
            {
                var remaining = GameManager.Instance.SpellCooldownManager.GetCooldownRemaining(SpellInfo);
                if (remaining == TimeSpan.Zero)
                    TooltipManager.Instance.ShowTextTooltip(SpellInfo.Name);
                else
                    TooltipManager.Instance.ShowTextTooltip($"{SpellInfo.Name} ({remaining.FormatDuration()} remaining)");
            }
            else if (ItemStats != null)
            {
                TooltipManager.Instance.ShowTextTooltip(ItemStats.Name);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTextTooltip();
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