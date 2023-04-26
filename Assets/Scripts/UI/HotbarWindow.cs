using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goose2Client
{
    public class HotbarWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private HotbarPage[] pages;

        private int pageIndex = 0;

        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;

        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI xpBarText;
        [SerializeField] private TextTooltipEventHandler xpBarTooltip;

        [SerializeField] private InventoryWindow inventoryWindow;
        [SerializeField] private SpellbookWindow spellbookWindow;

        private bool[] buttonPressed;
        private float buttonRepeatDelayTime;

        private const int mountSlotNumber = 30 + 14;
        private bool mounted = false;
        private Dictionary<int, string> mountSlots = new();

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Hotbar;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ExperienceBarPacket>(this.OnExperienceBar);
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Listen<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Listen<SpellbookSlotPacket>(this.OnSpellbookSlot);

            PlayerInputManager.Instance.Hotkey = OnButtonPressed;
            PlayerInputManager.Instance.ToggleMount = OnToggleMount;
            PlayerInputManager.Instance.CycleHotbarPage = OnCyclePage;

            var settings = GameManager.Instance.CharacterSettings.Hotkeys;

            for (int p = 0; p < pages.Length; p++)
            {
                var slots = pages[p].slots;

                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    slot.SlotNumber = i;
                    slot.Window = this;
                    slot.OnUseSlot = UseSlot;

                    LoadSlot(slot, settings[p * slots.Length + i]);
                }

                pages[p].gameObject.SetActive(pageIndex == p);
            }

            backButton.SetActive(false);
            nextButton.SetActive(true);

            buttonPressed = new bool[10];
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

            for (int p = 0; p < pages.Length; p++)
            {
                var slots = pages[p].slots;

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

                    settings[p * slots.Length + i] = setting;
                }
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

        private void SetMount(string mount)
        {
            if (GameManager.Instance.CharacterSettings.MountName == mount) return;

            GameManager.Instance.CharacterSettings.MountName = mount;
            GameManager.Instance.CharacterSettings.Save();
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

            foreach (var page in pages)
            {
                foreach (var slot in page.slots)
                {
                    slot.OnInventorySlot(stats);
                }
            }

            if (stats.SlotNumber < 30)
            {
                if (stats.SlotType == ItemSlotType.Mount)
                    mountSlots[stats.SlotNumber] = stats.Name;
                else if (mountSlots.ContainsKey(stats.SlotNumber))
                    mountSlots.Remove(stats.SlotNumber);
            }

            if (stats.SlotNumber == mountSlotNumber)
            {
                mounted = true;
                SetMount(stats.Name);
            }
        }

        private void OnClearInventorySlot(object packetObj)
        {
            var packet = (ClearInventorySlotPacket)packetObj;

            foreach (var page in pages)
            {
                foreach (var slot in page.slots)
                {
                    slot.OnClearInventorySlot(packet.SlotNumber);
                }
            }

            if (mountSlots.ContainsKey(packet.SlotNumber))
                mountSlots.Remove(packet.SlotNumber);

            if (packet.SlotNumber == mountSlotNumber)
                mounted = false;
        }

        private void OnSpellbookSlot(object packetObj)
        {
            var packet = (SpellbookSlotPacket)packetObj;

            var info = SpellInfo.FromPacket(packet);

            foreach (var page in pages)
            {
                foreach (var slot in page.slots)
                {
                    slot.OnSpellbookSlot(info);
                }
            }
        }

        private void UseSlot(int slotNumber)
        {
            if (GameManager.Instance.IsTargeting) return;

            var slot = pages[pageIndex].slots[slotNumber];
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

        private void OnToggleMount(InputValue inputValue)
        {
            if (mounted)
            {
                GameManager.Instance.NetworkClient.UseItem(mountSlotNumber);
            }
            else if (mountSlots.Count > 0)
            {
                var mount = mountSlots.FirstOrDefault(m => m.Value == GameManager.Instance.CharacterSettings.MountName);
                if (mount.Value == null)
                    mount = mountSlots.First();

                GameManager.Instance.NetworkClient.UseItem(mount.Key);
            }
        }

        public void OnBackClicked()
        {
            var newPageIndex = Math.Max(0, pageIndex - 1);
            ChangePage(newPageIndex);
        }

        public void OnNextClicked()
        {
            var newPageIndex = (pageIndex + 1) % pages.Length;
            ChangePage(newPageIndex);
        }

        private void ChangePage(int newPageIndex)
        {
            if (pageIndex == newPageIndex) return;

            pages[pageIndex].gameObject.SetActive(false);
            pages[newPageIndex].gameObject.SetActive(true);

            pageIndex = newPageIndex;

            backButton.SetActive(pageIndex != 0);
            nextButton.SetActive(pageIndex != pages.Length - 1);
        }

        private void OnCyclePage(InputValue value)
        {
            var newPageIndex = (pageIndex + 1) % pages.Length;
            ChangePage(newPageIndex);
        }
    }
}