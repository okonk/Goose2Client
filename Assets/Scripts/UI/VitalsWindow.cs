using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class VitalsWindow : MonoBehaviour
    {
        [SerializeField] private Image hpBarFill;
        [SerializeField] private Image mpBarFill;
        [SerializeField] private TextMeshProUGUI hpBarText;
        [SerializeField] private TextMeshProUGUI mpBarText;
        [SerializeField] private TextMeshProUGUI levelText;

        [SerializeField] private TextTooltipEventHandler hpBarTooltip;
        [SerializeField] private TextTooltipEventHandler mpBarTooltip;
        [SerializeField] private TextTooltipEventHandler levelTooltip;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<StatusInfoPacket>(this.OnStatusInfo);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<StatusInfoPacket>(this.OnStatusInfo);
        }

        private void OnStatusInfo(object packetObj)
        {
            var packet = (StatusInfoPacket)packetObj;

            var hpPercent = packet.CurrentHP / (float)packet.MaxHP;
            hpBarFill.fillAmount = hpPercent;
            hpBarText.text = $"{packet.CurrentHP:N0}";
            hpBarTooltip.TooltipText = $"Health: {packet.CurrentHP:N0} / {packet.MaxHP:N0}";

            var mpPercent = packet.CurrentMP / (float)packet.MaxMP;
            mpBarFill.fillAmount = mpPercent;
            mpBarText.text = $"{packet.CurrentMP:N0}";
            mpBarTooltip.TooltipText = $"Mana: {packet.CurrentMP:N0} / {packet.MaxMP:N0}";

            levelText.text = packet.Level.ToString();
            levelTooltip.TooltipText = $"Level: {packet.Level}";
        }
    }
}