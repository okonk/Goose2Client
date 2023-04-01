using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InventoryWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

        [SerializeField] private TextMeshProUGUI goldText;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Inventory;

        private void OnToggleInventory(InputValue value)
        {
            panel.SetActive(!panel.activeSelf);
        }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Listen<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Listen<StatusInfoPacket>(this.OnStatusInfo);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
                slot.OnDoubleClick += UseItem;
                slot.OnDropItem += DropItem;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<InventorySlotPacket>(this.OnInventorySlot);
            GameManager.Instance.PacketManager.Remove<ClearInventorySlotPacket>(this.OnClearInventorySlot);
            GameManager.Instance.PacketManager.Remove<StatusInfoPacket>(this.OnStatusInfo);
        }

        private void OnStatusInfo(object packetObj)
        {
            var packet = (StatusInfoPacket)packetObj;

            goldText.text = $"{packet.Gold:N0}";
        }

        private void OnInventorySlot(object packetObj)
        {
            var packet = (InventorySlotPacket)packetObj;

            if (packet.SlotNumber >= slots.Length) return;

            var stats = ItemStats.FromPacket(packet);
            slots[packet.SlotNumber].SetItem(stats);
        }

        private void OnClearInventorySlot(object packetObj)
        {
            var packet = (ClearInventorySlotPacket)packetObj;

            if (packet.SlotNumber >= slots.Length) return;

            slots[packet.SlotNumber].ClearItem();
        }

        public void UseItem(ItemStats stats)
        {
            GameManager.Instance.NetworkClient.UseItem(stats.SlotNumber);
        }

        private void DropItem(int fromWindowId, int fromSlot, int toSlot)
        {
            if (fromWindowId == this.WindowId)
            {
                GameManager.Instance.NetworkClient.MoveItemInInventory(fromSlot, toSlot);
            }
            else
            {
                GameManager.Instance.NetworkClient.MoveWindowToInventory(fromWindowId, fromSlot, toSlot);
            }
        }

        public void CloseWindow()
        {
            panel.SetActive(false);
        }
    }
}