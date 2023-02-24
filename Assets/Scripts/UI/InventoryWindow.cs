using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField] private ItemSlot[] slots;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<InventorySlotPacket>(this.OnInventorySlot);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<InventorySlotPacket>(this.OnInventorySlot);
        }

        private void OnInventorySlot(object packetObj)
        {
            var packet = (InventorySlotPacket)packetObj;

            slots[packet.SlotNumber].SetItem(packet);
        }
    }
}