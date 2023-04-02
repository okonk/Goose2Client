using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class DropTarget : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var fromSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
            if (fromSlot != null && fromSlot.HasItem && fromSlot.Window.WindowFrame == WindowFrames.Inventory)
            {
                GameManager.Instance.NetworkClient.Drop(fromSlot.SlotNumber, fromSlot.StackSize);
                return;
            }

            var hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlot>();
            if (hotbarSlot != null && !hotbarSlot.IsEmpty)
            {
                hotbarSlot.Clear();
                return;
            }
        }
    }
}