using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class BankWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ItemSlot[] slots;

        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;

        public int WindowId { get; private set; }

        public WindowFrames WindowFrame => WindowFrames.Bank;

        public int NpcId { get; private set; }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Listen<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Listen<BankSlotPacket>(this.OnBankSlot);
            GameManager.Instance.PacketManager.Listen<ClearBankSlotPacket>(this.OnClearBankSlot);

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
            GameManager.Instance.PacketManager.Remove<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Remove<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Remove<BankSlotPacket>(this.OnBankSlot);
            GameManager.Instance.PacketManager.Remove<ClearBankSlotPacket>(this.OnClearBankSlot);
        }

        private void OnMakeWindow(object packetObj)
        {
            var packet = (MakeWindowPacket)packetObj;

            if (packet.WindowFrame != WindowFrames.Bank) return;

            NpcId = packet.NpcId;
            titleText.text = packet.Title;
            WindowId = packet.WindowId;

            backButton.SetActive(packet.Buttons[(int)WindowButtons.Back - 1]);
            nextButton.SetActive(packet.Buttons[(int)WindowButtons.Next - 1]);
        }

        private void OnEndWindow(object packetObj)
        {
            var packet = (EndWindowPacket)packetObj;

            if (packet.WindowId == this.WindowId)
                panel.SetActive(true);
        }

        private void OnBankSlot(object packetObj)
        {
            var packet = (BankSlotPacket)packetObj;

            var stats = ItemStats.FromPacket(packet);
            slots[packet.SlotNumber].SetItem(stats);
        }

        private void OnClearBankSlot(object packetObj)
        {
            var packet = (ClearBankSlotPacket)packetObj;

            slots[packet.SlotNumber].ClearItem();
        }

        private void DropItem(IWindow fromWindow, int fromSlot, int toSlot)
        {
            if (fromWindow.WindowFrame == WindowFrames.Inventory)
            {
                GameManager.Instance.NetworkClient.MoveInventoryToWindow(fromSlot, this.WindowId, toSlot);
            }
            else if (fromWindow.WindowFrame != WindowFrames.Vendor)
            {
                GameManager.Instance.NetworkClient.MoveWindowToWindow(fromWindow.WindowId, fromSlot, this.WindowId, toSlot);
            }
        }

        public void CloseWindow()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Close, this.WindowId, NpcId);
            panel.SetActive(false);
        }

        public void NextClicked()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Next, this.WindowId, NpcId);
        }

        public void BackClicked()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Back, this.WindowId, NpcId);
        }
    }
}