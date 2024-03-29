using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Goose2Client
{
    public class MapItemTooltip : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI bindText;

        public ItemStats Item { get; private set; }

        private RectTransform rectTransform;

        private GameObject parent;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        internal void SetItem(ItemStats itemStats, GameObject parent)
        {
            Item = itemStats;
            this.parent = parent;

            nameText.text = $"{itemStats.Title} {itemStats.Name} {itemStats.Surname}".Trim();

            if (itemStats.StackSize > 1)
                nameText.text += $" ({itemStats.StackSize})";

            bindText.gameObject.SetActive(itemStats.Flags.HasFlag(ItemFlags.BindOnPickup));
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (parent == null || !parent.activeInHierarchy)
            {
                Hide();
                return;
            }

            var position = Input.mousePosition;

            transform.position = position;

            var pivotX = 1.0f;
            var pivotY = 0.0f;

            var sizeX = rectTransform.sizeDelta.x;
            var sizeY = rectTransform.sizeDelta.y;

            var positionX = position.x / canvas.scaleFactor;
            var offScreenX = positionX - sizeX;
            if (offScreenX < 0)
                pivotX = 0;//1 - Math.Abs(offScreenX) / sizeX;

            var positionY = position.y / canvas.scaleFactor;
            var screenHeight = Screen.height / canvas.scaleFactor;
            var offScreenY = positionY + sizeY;
            if (offScreenY > screenHeight)
                pivotY = (offScreenY - screenHeight) / sizeY;

            rectTransform.pivot = new Vector2(pivotX, pivotY);
        }
    }
}