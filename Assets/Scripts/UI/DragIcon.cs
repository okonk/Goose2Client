using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Goose2Client
{
    public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private bool dragOnSurfaces = true;

        [SerializeField] private Image imageSource;

        private GameObject m_DraggingIcon;
        private RectTransform m_DraggingPlane;

        public void OnBeginDrag(PointerEventData eventData)
        {
            var canvas = FindInParents<Canvas>(gameObject);
            if (canvas == null || imageSource.color.a == 0)
                return;

            // We have clicked something that can be dragged.
            // What we want to do is create an icon for this.
            m_DraggingIcon = new GameObject("Dragged Icon");

            // this is needed otherwise the icon blocks the drop event...
            var group = m_DraggingIcon.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            m_DraggingIcon.transform.SetParent(canvas.transform, false);
            m_DraggingIcon.transform.SetAsLastSibling();

            var image = m_DraggingIcon.AddComponent<Image>();

            image.sprite = imageSource.sprite;
            var rect = (RectTransform)image.transform;
            var fromRect = (RectTransform)imageSource.transform;

            rect.sizeDelta = fromRect.sizeDelta;
            rect.pivot = new Vector2(0, 1);

            if (dragOnSurfaces)
                m_DraggingPlane = transform as RectTransform;
            else
                m_DraggingPlane = canvas.transform as RectTransform;

            SetDraggedPosition(eventData);
        }

        public void OnDrag(PointerEventData data)
        {
            if (m_DraggingIcon != null)
                SetDraggedPosition(data);
        }

        private void SetDraggedPosition(PointerEventData data)
        {
            if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
                m_DraggingPlane = data.pointerEnter.transform as RectTransform;

            var rt = m_DraggingIcon.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out Vector3 globalMousePos))
            {
                //rt.position = new Vector3(globalMousePos.x + 1, globalMousePos.y - 1, globalMousePos.z);
                rt.position = globalMousePos;
                rt.rotation = m_DraggingPlane.rotation;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_DraggingIcon != null)
                Destroy(m_DraggingIcon);
        }

        public static T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;
            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
            return comp;
        }
    }
}
