using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI countText;

        [SerializeField] private ItemStats stats;

        internal void SetItem(ItemStats stats)
        {
            this.stats = stats;

            var idString = stats.GraphicId.ToString();
            var sprite = Resources.LoadAll<Sprite>($"Spritesheets/{stats.GraphicFile}").FirstOrDefault(s => s.name == idString);

            image.gameObject.SetActive(true);
            image.sprite = sprite;
            image.color = Color.white;
            image.material.SetColor("_Tint", ColorH.RGBA(stats.GraphicR, stats.GraphicG, stats.GraphicB, stats.GraphicA));

            if (stats.StackSize > 1)
            {
                countText.text = stats.StackSize.ToString();
                countText.gameObject.SetActive(true);
            }
        }

        internal void ClearItem()
        {
            stats = null;
            image.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipManager.Instance.ShowItemTooltip(stats);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideItemTooltip();
        }
    }
}