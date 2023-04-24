using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class TitleBar : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] public Canvas canvas;
        [SerializeField] private Transform transformToMove;
        [SerializeField] private string windowName;

        private void Awake()
        {
            if (windowName == null) return;

            var windowSettings = GameManager.Instance.CharacterSettings.GetWindowSettings(windowName);
            if (windowSettings != null)
                transformToMove.localPosition = windowSettings.Position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            transformToMove.localPosition += (Vector3)(eventData.delta / canvas.scaleFactor);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || windowName == null) return;

            GameManager.Instance.CharacterSettings.SetWindowSetting(windowName, transformToMove.localPosition);
        }
    }
}