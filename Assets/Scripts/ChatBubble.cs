using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Goose2Client
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private SpriteRenderer background;

        public float Height => background.size.y;

        private const float maxWidth = 250 / 32f;
        private static readonly Vector2 padding = new Vector2(7f, 5f) / 32f;

        private void Start()
        {
            Invoke(nameof(DestroyBubble), 3);
        }

        private void DestroyBubble()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }

        internal void SetText(string message)
        {
            text.text = message;
            text.ForceMeshUpdate();

            var textSize = text.GetRenderedValues(false);
            if (textSize.x > maxWidth)
            {
                text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);

                text.enableWordWrapping = true;
                text.ForceMeshUpdate();
                textSize = text.GetRenderedValues(false);
            }

            text.rectTransform.sizeDelta = textSize;
            background.size = textSize + padding;
        }
    }
}