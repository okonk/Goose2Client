using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Goose2Client
{
    public class BackgroundTransparency : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image image;

        private void Start()
        {
            this.image = gameObject.GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.color = Color.white;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.color = new Color(1, 1, 1, 0.7f);
        }
    }
}