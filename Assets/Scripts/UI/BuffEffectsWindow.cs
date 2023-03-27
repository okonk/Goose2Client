using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class BuffEffectsWindow : MonoBehaviour
    {
        [SerializeField] private BuffEffect[] slots;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<BuffBarPacket>(this.OnBuffBar);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.SlotNumber = i;
                slot.OnDoubleClick += KillBuff;
            }
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<BuffBarPacket>(this.OnBuffBar);
        }

        private void OnBuffBar(object packetObj)
        {
            var packet = (BuffBarPacket)packetObj;

            slots[packet.SlotNumber].SetEffect(packet);
        }

        public void KillBuff(int index)
        {
            GameManager.Instance.NetworkClient.KillBuff(index + 1);
        }
    }
}