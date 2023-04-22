using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Goose2Client
{
    public class SpellTooltip : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI text;

        public SpellInfo Spell { get; private set; }

        private RectTransform rectTransform;

        private GameObject parent;

        private float lastTooltipUpdate = 0;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private string GetTooltipText()
        {
            var remaining = GameManager.Instance.SpellCooldownManager.GetCooldownRemaining(Spell);

            string tooltip = Spell.Name;
            if (remaining != TimeSpan.Zero)
            {
                var remainingString = remaining.FormatDuration();
                if (remainingString.Length > 0)
                    tooltip +=  $" ({remainingString} remaining)";
            }

            return tooltip;
        }

        internal void SetSpell(SpellInfo spell, GameObject parent)
        {
            Spell = spell;
            this.parent = parent;

            this.text.text = GetTooltipText();
            this.lastTooltipUpdate = 0;
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

            this.lastTooltipUpdate += Time.unscaledDeltaTime;
            if (this.lastTooltipUpdate >= 0.5)
            {
                this.text.text = GetTooltipText();
                this.lastTooltipUpdate = 0;
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