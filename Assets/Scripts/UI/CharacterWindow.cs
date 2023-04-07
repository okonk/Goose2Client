using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goose2Client
{
    public class CharacterWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelClassText;
        [SerializeField] private TextMeshProUGUI guildText;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private TextMeshProUGUI experienceSoldText;

        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI acText;

        [SerializeField] private TextMeshProUGUI fireResistText;
        [SerializeField] private TextMeshProUGUI waterResistText;
        [SerializeField] private TextMeshProUGUI earthResistText;
        [SerializeField] private TextMeshProUGUI airResistText;
        [SerializeField] private TextMeshProUGUI spiritResistText;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Equipped;

        private int firstSlotNumber = 31;

        private void OnToggleCharacterWindow(InputValue value)
        {
            if (GameManager.Instance.IsTyping) return;

            panel.SetActive(!panel.activeSelf);
        }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Listen<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Listen<StatusInfoPacket>(this.OnStatusInfo);
            GameManager.Instance.PacketManager.Listen<ExperienceBarPacket>(this.OnExperienceBar);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i + firstSlotNumber;
                slot.Window = this;
                slot.OnDoubleClick += UseItem;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Remove<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Remove<StatusInfoPacket>(this.OnStatusInfo);
            GameManager.Instance.PacketManager.Remove<ExperienceBarPacket>(this.OnExperienceBar);
        }

        private void OnInventorySlot(object packetObj)
        {
            var packet = (InventorySlotPacket)packetObj;

            if (packet.SlotNumber < firstSlotNumber) return;

            var stats = ItemStats.FromPacket(packet);
            slots[packet.SlotNumber - firstSlotNumber].SetItem(stats);
        }

        private void OnClearInventorySlot(object packetObj)
        {
            var packet = (ClearInventorySlotPacket)packetObj;

            if (packet.SlotNumber < firstSlotNumber) return;

            slots[packet.SlotNumber - firstSlotNumber].ClearItem();
        }

        public void UseItem(ItemStats stats)
        {
            GameManager.Instance.NetworkClient.UseItem(stats.SlotNumber);
        }

        private void OnStatusInfo(object packetObj)
        {
            var packet = (StatusInfoPacket)packetObj;

            if (GameManager.Instance.Character != null)
                nameText.text = GameManager.Instance.Character.FullName;

            levelClassText.text = $"Level {packet.Level} {packet.ClassName}";
            guildText.text = string.IsNullOrWhiteSpace(packet.GuildName) ? "No Guild" : packet.GuildName;

            strengthText.text = packet.Strength.ToString();
            staminaText.text = packet.Stamina.ToString();
            intelligenceText.text = packet.Intelligence.ToString();
            dexterityText.text = packet.Dexterity.ToString();
            acText.text = packet.ArmorClass.ToString();

            fireResistText.text = packet.FireResist.ToString();
            waterResistText.text = packet.WaterResist.ToString();
            earthResistText.text = packet.EarthResist.ToString();
            airResistText.text = packet.AirResist.ToString();
            spiritResistText.text = packet.SpiritResist.ToString();

            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.gameObject.GetComponentInParent<RectTransform>());
        }

        private void OnExperienceBar(object packetObj)
        {
            var packet = (ExperienceBarPacket)packetObj;

            experienceText.text = $"{packet.Experience:N0}";
            experienceSoldText.text = $"{packet.ExperienceSold:N0}";
        }

        public void CloseWindow()
        {
            panel.SetActive(false);
        }
    }
}