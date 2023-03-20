using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InventoryWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

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

        private void UseItem(ItemStats stats)
        {
            GameManager.Instance.NetworkClient.UseItem(stats.SlotNumber + 1);
        }

        private void DropItem(int fromWindowId, int fromSlot, int toSlot)
        {
            if (fromWindowId == this.WindowId)
            {
                GameManager.Instance.NetworkClient.Change(fromSlot + 1, toSlot + 1);
            }
            else
            {
                // window to inventory
            }
        }
    }
}