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

        private bool[] buttonPressed;
        private float buttonRepeatDelayTime;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Hotbar;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ExperienceBarPacket>(this.OnExperienceBar);
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Listen<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Listen<SpellbookSlotPacket>(this.OnSpellbookSlot);

            PlayerInputManager.Instance.Hotkey = OnButtonPressed;

            var settings = GameManager.Instance.CharacterSettings.Hotkeys;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
                slot.OnUseSlot = UseSlot;

                LoadSlot(slot, settings[i]);
            }

            buttonPressed = new bool[slots.Length];
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<ExperienceBarPacket>(this.OnExperienceBar);
            GameManager.Instance.PacketManager.Remove<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Remove<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Remove<SpellbookSlotPacket>(this.OnSpellbookSlot);
        }

        private void LoadSlot(HotbarSlot slot, HotkeySetting setting)
        {
            if (setting == null || setting.SlotNumber == -1) return;

            if (setting.Type == HotkeySetting.SlotType.Item)
            {
                slot.SetItem(new ItemStats
                {
                    SlotNumber = setting.SlotNumber,
                    Name = "Loaded slot",
                    GraphicId = 3002,
                    GraphicFile = 13,
                    GraphicR = 1
                });
            }
            else
            {
                slot.SetSpell(new SpellInfo
                {
                    SlotNumber = setting.SlotNumber,
                    Name = "Loaded slot",
                    GraphicId = 3002,
                    GraphicFile = 13
                });
            }
        }

        private void SaveSlots()
        {
            var settings = GameManager.Instance.CharacterSettings.Hotkeys;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];

                HotkeySetting setting = null;
                if (slot.itemSlot != -1)
                    setting = new(slot.itemSlot, HotkeySetting.SlotType.Item);
                else if (slot.spellSlot != -1)
                    setting = new(slot.spellSlot, HotkeySetting.SlotType.Spell);
                else
                    setting = new(-1, HotkeySetting.SlotType.Item);

                settings[i] = setting;
            }

            GameManager.Instance.CharacterSettings.Save();
        }

        public void SaveSlotsDelayed()
        {
            StopCoroutine(nameof(SaveSlotsDelayedInternal));

            StartCoroutine(nameof(SaveSlotsDelayedInternal));
        }

        private IEnumerator SaveSlotsDelayedInternal()
        {
            yield return new WaitForSecondsRealtime(5);

            SaveSlots();
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
            if (GameManager.Instance.IsTargeting) return;

            var slot = slots[slotNumber];
            if (!slot.CanUse())
                return;

            if (slot.ItemStats != null)
                inventoryWindow.UseItem(slot.ItemStats);
            else if (slot.SpellInfo != null)
                spellbookWindow.UseSpell(slot.SpellInfo);
        }

        public void OnButtonPressed(int index, bool pressed)
        {
            if (pressed) buttonRepeatDelayTime = 0.1f;
            buttonPressed[index] = pressed;
        }

        private void Update()
        {
            buttonRepeatDelayTime += Time.deltaTime;

            if (buttonRepeatDelayTime < 0.1) return;

            buttonRepeatDelayTime = 0;

            for (int i = 0; i < buttonPressed.Length; i++)
            {
                if (buttonPressed[i])
                    UseSlot(i);
            }
        }
    }
}