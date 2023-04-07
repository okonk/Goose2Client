using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class TitleBar : MonoBehaviour, IDragHandler
    {
        [SerializeField] public Canvas canvas;
        [SerializeField] private Transform transformToMove;

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                transformToMove.localPosition += (Vector3)(eventData.delta / canvas.scaleFactor);
        }
    }
}