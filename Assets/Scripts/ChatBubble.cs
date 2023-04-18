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

        private void Start()
        {
            Invoke(nameof(DestroyBubble), 3);
        }

        internal void SetText(string message)
        {
            this.text.text = message;
            this.text.ForceMeshUpdate();

            var textSize = this.text.GetRenderedValues(false);
            var padding = new Vector2(7f, 5f) / 32f;

            float offsetX = 0.25f;

            float maxWidth = 250 / 32f;

            if (textSize.x >= maxWidth)
            {
                var rect = this.text.GetComponent<RectTransform>();
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);

                this.text.enableWordWrapping = true;
                this.text.ForceMeshUpdate();
                textSize = this.text.GetRenderedValues(false);
            }
            else
            {
                offsetX = -textSize.x / 2;
            }

            background.size = textSize + padding;
            text.transform.localPosition = new Vector3(offsetX, textSize.y / 2);
        }

        private void DestroyBubble()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}