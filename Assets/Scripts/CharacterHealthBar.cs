using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class CharacterHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject hpBar;
        [SerializeField] private GameObject mpBar;
        [SerializeField] private GameObject background;

        private bool shouldHide = false;

        public void SetHPPercent(int value)
        {
            SetBar(hpBar, value);

            if (value > 66)
                SetColor(hpBar, ColorH.RGBA(112, 232, 120)); // green
            else if (value > 33)
                SetColor(hpBar, ColorH.RGBA(244, 133, 50)); // orange
            else
                SetColor(hpBar, ColorH.RGBA(191, 64, 64)); // red

            if (value == 100)
                ScheduleHideBars();
        }

        public void SetMPPercent(int value)
        {
            SetBar(mpBar, value);
        }

        private void SetColor(GameObject bar, Color color)
        {
            var spriteRenderer = bar.GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }

        private void SetBar(GameObject bar, int value)
        {
            var maxWidth = background.transform.localScale.x;

            var newWidth = value / 100.0f * maxWidth;

            bar.transform.localScale = new Vector3(newWidth, bar.transform.localScale.y);
            bar.transform.localPosition = new Vector3(-(maxWidth - newWidth) / 2, bar.transform.localPosition.y);

            ShowBars();

            if (value != 100)
                shouldHide = false;
        }

        private void ScheduleHideBars()
        {
            Invoke(nameof(HideBars), 2);

            shouldHide = true;
        }

        private void HideBars()
        {
            if (shouldHide)
                gameObject.SetActive(false);
        }

        private void ShowBars()
        {
            gameObject.SetActive(true);
        }
    }
}