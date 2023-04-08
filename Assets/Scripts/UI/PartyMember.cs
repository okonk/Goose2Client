using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class PartyMember : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image hpBar;
        [SerializeField] private Image mpBar;
        [SerializeField] private GameObject content;

        public int PlayerId { get; private set; }

        public void OnGroupUpdate(GroupUpdatePacket packet)
        {
            PlayerId = packet.LoginId;
            content.SetActive(PlayerId != 0);

            if (PlayerId == 0) return;

            nameText.text = packet.Name;

            var character = GameManager.Instance.MapManager.GetCharacter(PlayerId);
            var hp = character?.HPPercent ?? 1;
            var mp = character?.MPPercent ?? 1;

            UpdateHPMP(hp, mp);
        }

        public void UpdateHPMP(float hp, float mp)
        {
            hpBar.fillAmount = hp;
            mpBar.fillAmount = mp;
        }
    }
}