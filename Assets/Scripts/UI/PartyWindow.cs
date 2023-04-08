using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Goose2Client
{
    public class PartyWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private PartyMember[] members;

        [SerializeField] private Image scrollBarHandle;
        [SerializeField] private Image scrollBarBackground;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<GroupUpdatePacket>(this.OnGroupUpdate);
            GameManager.Instance.PacketManager.Listen<VitalsPercentagePacket>(this.OnVitalsPercentage);
            GameManager.Instance.PacketManager.Listen<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Listen<MakeCharacterPacket>(this.OnMakeCharacter);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<GroupUpdatePacket>(this.OnGroupUpdate);
            GameManager.Instance.PacketManager.Remove<VitalsPercentagePacket>(this.OnVitalsPercentage);
            GameManager.Instance.PacketManager.Remove<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Remove<MakeCharacterPacket>(this.OnMakeCharacter);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetScrollAlpha(1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetScrollAlpha(0f);
        }

        private void SetScrollAlpha(float value)
        {
            var scrollColor = new Color(1, 1, 1, value);
            scrollBarHandle.color = scrollColor;
            scrollBarBackground.color = scrollColor;
        }

        private void OnGroupUpdate(object packetObj)
        {
            var packet = (GroupUpdatePacket)packetObj;

            members[packet.LineNumber].OnGroupUpdate(packet);
        }

        private void OnVitalsPercentage(object packetObj)
        {
            var packet = (VitalsPercentagePacket)packetObj;

            UpdateMember(packet.LoginId, packet.HPPercentage, packet.MPPercentage);
        }

        private void OnEraseCharacter(object packetObj)
        {
            var packet = (EraseCharacterPacket)packetObj;

            UpdateMember(packet.LoginId, 0, 0);
        }

        private void OnMakeCharacter(object packetObj)
        {
            var packet = (MakeCharacterPacket)packetObj;

            UpdateMember(packet.LoginId, packet.HPPercent, 1);
        }

        private void UpdateMember(int id, float hp, float mp)
        {
            foreach (var member in members)
            {
                if (member.PlayerId != id) continue;

                member.UpdateHPMP(hp, mp);
            }
        }
    }
}