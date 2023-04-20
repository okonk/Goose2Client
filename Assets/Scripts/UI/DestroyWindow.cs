using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class DestroyWindow : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var fromSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
            if (fromSlot != null && fromSlot.HasItem && fromSlot.Window.WindowFrame == WindowFrames.Inventory)
            {
                GameManager.Instance.NetworkClient.DestroyItem(fromSlot.SlotNumber);
                return;
            }

            var spellSlot = eventData.pointerDrag?.GetComponent<SpellSlot>();
            if (spellSlot != null && spellSlot.HasSpell)
            {
                GameManager.Instance.NetworkClient.DestroySpell(spellSlot.SlotNumber);
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