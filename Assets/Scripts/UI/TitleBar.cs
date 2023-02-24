using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class TitleBar : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private Vector2 mouseStartPosition;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                transform.parent.localPosition = (Vector3)(eventData.position - mouseStartPosition);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                mouseStartPosition = eventData.pressPosition - (Vector2)transform.parent.localPosition;
        }
    }
}