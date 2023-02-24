using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class WindowTransparency : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private CanvasGroup canvasGroup;

        private void Start()
        {
            this.canvasGroup = gameObject.GetComponentInParent<CanvasGroup>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            canvasGroup.alpha = 1;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            canvasGroup.alpha = 0.7f;
        }
    }
}