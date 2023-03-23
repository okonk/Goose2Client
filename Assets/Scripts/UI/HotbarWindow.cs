using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class HotbarWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private HotbarSlot[] slots;

        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI xpBarText;
        [SerializeField] private TextTooltipEventHandler xpBarTooltip;

        public int WindowId => (int)WindowFrame;
        public WindowFrames WindowFrame => WindowFrames.Hotbar;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ExperienceBarPacket>(this.OnExperienceBar);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.Window = this;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<ExperienceBarPacket>(this.OnExperienceBar);
        }

        private void OnExperienceBar(object packetObj)
        {
            var packet = (ExperienceBarPacket)packetObj;

            xpBarFill.fillAmount = packet.Percentage;
            xpBarText.text = $"{packet.ExperienceToNextLevel:N0}";

            if (packet.Percentage == 1 && packet.Experience == packet.ExperienceToNextLevel)
            {
                xpBarTooltip.TooltipText = $"Experience: {packet.Experience:N0} / Sold: {packet.ExperienceSold:N0}";
            }
            else
            {
                xpBarTooltip.TooltipText = $"Experience: {packet.Experience:N0} / {packet.Experience + packet.ExperienceToNextLevel:N0}";
            }
        }
    }
}