using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goose2Client
{
    public class HotbarWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private HotbarSlot[] slots;

        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI xpBarText;
        [SerializeField] private TextTooltipEventHandler xpBarTooltip;

        [SerializeField] private InventoryWindow inventoryWindow;
        [SerializeField] private SpellbookWindow spellbookWindow;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Hotbar;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ExperienceBarPacket>(this.OnExperienceBar);
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Listen<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Listen<SpellbookSlotPacket>(this.OnSpellbookSlot);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<ExperienceBarPacket>(this.OnExperienceBar);
            GameManager.Instance.PacketManager.Remove<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Remove<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Remove<SpellbookSlotPacket>(this.OnSpellbookSlot);
        }

        private void OnExperienceBar(object packetObj)
        {
            var packet = (ExperienceBarPacket)packetObj;

            xpBarFill.fillAmount = packet.Percentage;
            xpBarText.text = $"{packet.ExperienceToNextLevel:N0}";

            if (packet.Percentage == 1 && packet.Experience == packet.ExperienceToNextLevel)
            {
                xpBarTooltip.TooltipText = $"Experience: {packet.Experience:N0} / Sold: {packet.ExperienceSold:N0}";
            }
            else
            {
                xpBarTooltip.TooltipText = $"Experience: {packet.Experience:N0} / {packet.Experience + packet.ExperienceToNextLevel:N0}";
            }
        }

        private void OnInventorySlot(object packetObj)
        {
            var packet = (InventorySlotPacket)packetObj;

            var stats = ItemStats.FromPacket(packet);

            foreach (var slot in slots)
            {
                slot.OnInventorySlot(stats);
            }
        }

        private void OnClearInventorySlot(object packetObj)
        {
            var packet = (ClearInventorySlotPacket)packetObj;

            foreach (var slot in slots)
            {
                slot.OnClearInventorySlot(packet.SlotNumber);
            }
        }

        private void OnSpellbookSlot(object packetObj)
        {
            var packet = (SpellbookSlotPacket)packetObj;

            var info = SpellInfo.FromPacket(packet);

            foreach (var slot in slots)
            {
                slot.OnSpellbookSlot(info);
            }
        }

        private void UseSlot(int slotNumber)
        {
            var slot = slots[slotNumber];
            if (!slot.CanUse())
                return;

            if (slot.ItemStats != null)
                inventoryWindow.UseItem(slot.ItemStats);
            else if (slot.SpellInfo != null)
                spellbookWindow.UseSpell(slot.SpellInfo);
        }

        private void OnHotkey0(InputValue value)
        {
            UseSlot(9);
        }

        private void OnHotkey1(InputValue value)
        {
            UseSlot(0);
        }

        private void OnHotkey2(InputValue value)
        {
            UseSlot(1);
        }

        private void OnHotkey3(InputValue value)
        {
            UseSlot(2);
        }

        private void OnHotkey4(InputValue value)
        {
            UseSlot(3);
        }

        private void OnHotkey5(InputValue value)
        {
            UseSlot(4);
        }

        private void OnHotkey6(InputValue value)
        {
            UseSlot(5);
        }

        private void OnHotkey7(InputValue value)
        {
            UseSlot(6);
        }

        private void OnHotkey8(InputValue value)
        {
            UseSlot(7);
        }

        private void OnHotkey9(InputValue value)
        {
            UseSlot(8);
        }
    }
}