using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class SpellbookWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private SpellbookPage[] pages;

        private int pageIndex = 0;

        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Spellbook;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<SpellbookSlotPacket>(this.OnSpellbookSlot);

            for (int i = 0; i < pages.Length; i++)
            {
                var page = pages[i];

                for (int j = 0; j < page.slots.Length; j++)
                {
                    var slot = page.slots[j];
                    slot.SlotNumber = page.startSlotIndex + j;
                    slot.Window = this;
                    slot.OnDoubleClick += UseSpell;
                }

                pages[i].gameObject.SetActive(pageIndex == i);
            }

            backButton.SetActive(false);
            nextButton.SetActive(true);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<SpellbookSlotPacket>(this.OnSpellbookSlot);
        }

        private SpellSlot GetSlot(int index)
        {
            foreach (var page in pages)
            {
                if (index >= page.startSlotIndex && index < page.startSlotIndex + page.slots.Length)
                    return page.slots[index - page.startSlotIndex];
            }

            return null;
        }

        private void OnSpellbookSlot(object packetObj)
        {
            var packet = (SpellbookSlotPacket)packetObj;

            var info = SpellInfo.FromPacket(packet);
            GetSlot(packet.SlotNumber).SetSpell(info);
        }

        private void UseSpell(SpellInfo info)
        {
            Debug.Log($"Use spell {info.SlotNumber}/{info.Name}");
            //GameManager.Instance.NetworkClient.UseItem(stats.SlotNumber + 1);
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
    }
}