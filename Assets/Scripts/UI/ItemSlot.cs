using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Goose2Client
{
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI countText;

        internal void SetItem(InventorySlotPacket packet)
        {
            var idString = packet.GraphicId.ToString();
            var sprite = Resources.LoadAll<Sprite>($"Spritesheets/{packet.GraphicFile}").FirstOrDefault(s => s.name == idString);

            image.gameObject.SetActive(true);
            image.sprite = sprite;
            image.color = Color.white;
            image.material.SetColor("_Tint", ColorH.RGBA(packet.GraphicR, packet.GraphicG, packet.GraphicB, packet.GraphicA));

            if (packet.StackSize > 1)
            {
                countText.text = packet.StackSize.ToString();
                countText.gameObject.SetActive(true);
            }
        }
    }
}