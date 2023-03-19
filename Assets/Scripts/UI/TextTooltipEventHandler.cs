using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class TextTooltipEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] public string TooltipText { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipManager.Instance.ShowTextTooltip(TooltipText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTextTooltip();
        }
    }
}