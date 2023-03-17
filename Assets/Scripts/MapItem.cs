using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class MapItem : MonoBehaviour
    {
        public ItemStats Item { get; set; }

        public void OnMouseEnter()
        {
            TooltipManager.Instance.ShowMapItemTooltip(Item);
        }

        public void OnMouseExit()
        {
            TooltipManager.Instance.HideMapItemTooltip();
        }
    }
}