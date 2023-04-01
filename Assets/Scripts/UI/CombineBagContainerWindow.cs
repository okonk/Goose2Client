using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class CombineBagContainerWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

        public int WindowId => 22;

        public WindowFrames WindowFrame => WindowFrames.TenSlot;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Listen<CombineBagSlotPacket>(this.OnCombineBagSlot);
            GameManager.Instance.PacketManager.Listen<ClearCombineBagSlotPacket>(this.OnClearCombineBagSlot);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
                slot.OnDropItem += DropItem;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Remove<CombineBagSlotPacket>(this.OnCombineBagSlot);
            GameManager.Instance.PacketManager.Remove<ClearCombineBagSlotPacket>(this.OnClearCombineBagSlot);
        }

        private void OnEndWindow(object packetObj)
        {
            var packet = (EndWindowPacket)packetObj;

            if (packet.WindowId == this.WindowId)
                panel.SetActive(true);
        }

        private void OnCombineBagSlot(object packetObj)
        {
            var packet = (CombineBagSlotPacket)packetObj;

            var stats = ItemStats.FromPacket(packet);
            slots[packet.SlotNumber].SetItem(stats);
        }

        private void OnClearCombineBagSlot(object packetObj)
        {
            var packet = (ClearCombineBagSlotPacket)packetObj;

            slots[packet.SlotNumber].ClearItem();
        }

        private void DropItem(int fromWindowId, int fromSlot, int toSlot)
        {
            if (fromWindowId == (int)WindowFrames.Inventory)
            {
                GameManager.Instance.NetworkClient.MoveInventoryToWindow(fromSlot, this.WindowId, toSlot);
            }
            else
            {
                GameManager.Instance.NetworkClient.MoveWindowToWindow(fromWindowId, fromSlot, this.WindowId, toSlot);
            }
        }

        public void CloseWindow()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Close, this.WindowId, 0);
            panel.SetActive(false);
        }

        public void CombineClicked()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Combine, this.WindowId, 0);
        }
    }
}