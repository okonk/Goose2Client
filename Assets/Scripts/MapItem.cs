using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class MapItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ItemStats Item { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipManager.Instance.ShowMapItemTooltip(Item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideMapItemTooltip();
        }
    }
}