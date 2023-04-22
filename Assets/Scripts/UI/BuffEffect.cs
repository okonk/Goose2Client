using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System;

namespace Goose2Client
{
    public class BuffEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image image;

        private string effectName;

        public int SlotNumber { get; set; }

        public Action<int> OnDoubleClick { get; set; }

        public void SetEffect(BuffBarPacket packet)
        {
            if (string.IsNullOrWhiteSpace(packet.Name))
            {
                ClearEffect();
                return;
            }

            effectName = packet.Name;

            image.sprite = Helpers.GetSprite(packet.GraphicId, packet.GraphicFile);
            image.color = Color.white;
        }

        public void ClearEffect()
        {
            effectName = null;
            image.color = new Color(0, 0, 0, 0);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (effectName == null) return;

            TooltipManager.Instance.ShowTextTooltip(effectName, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTextTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (effectName == null) return;

            if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
                OnDoubleClick?.Invoke(SlotNumber);
        }
    }
}