using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class TitleBar : MonoBehaviour, IDragHandler
    {
        [SerializeField] private Canvas canvas;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                transform.parent.localPosition += (Vector3)(eventData.delta / canvas.scaleFactor);
        }
    }
}