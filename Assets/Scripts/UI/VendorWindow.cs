using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class VendorWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

        [SerializeField] private TextMeshProUGUI titleText;

        public int WindowId { get; private set; }
        public WindowFrames WindowFrame => WindowFrames.Vendor;

        public int NpcId { get; private set;}

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Listen<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Listen<VendorSlotPacket>(this.OnVendorSlot);
            GameManager.Instance.PacketManager.Listen<ClearVendorPacket>(this.OnClearVendor);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
                slot.OnDoubleClick += BuyItem;
                slot.OnDropItem += DropItem;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Remove<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Remove<VendorSlotPacket>(this.OnVendorSlot);
            GameManager.Instance.PacketManager.Remove<ClearVendorPacket>(this.OnClearVendor);
        }

        private void OnMakeWindow(object packetObj)
        {
            var packet = (MakeWindowPacket)packetObj;

            if (packet.WindowFrame != WindowFrames.Vendor) return;

            NpcId = packet.NpcId;
            titleText.text = packet.Title;
            WindowId = packet.WindowId;
        }

        private void OnEndWindow(object packetObj)
        {
            var packet = (EndWindowPacket)packetObj;

            if (packet.WindowId == this.WindowId)
                panel.SetActive(true);
        }

        private void OnVendorSlot(object packetObj)
        {
            var packet = (VendorSlotPacket)packetObj;

            var stats = ItemStats.FromPacket(packet);
            slots[packet.SlotNumber].SetItem(stats);
        }

        private void OnClearVendor(object packetObj)
        {
            foreach (var slot in slots)
                slot.ClearItem();
        }

        public void BuyItem(ItemStats stats)
        {
            GameManager.Instance.NetworkClient.VendorPurchaseItem(NpcId, stats.SlotNumber);
        }

        private void DropItem(IWindow fromWindow, int fromSlot, int toSlot)
        {
            if (fromWindow.WindowFrame == WindowFrames.Inventory)
                GameManager.Instance.NetworkClient.VendorSellItem(NpcId, fromSlot, 1);
        }

        public void CloseWindow()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Close, this.WindowId, NpcId);
            panel.SetActive(false);
        }
    }
}