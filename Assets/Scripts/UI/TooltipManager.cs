using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class TooltipManager : MonoBehaviour
    {
        private static TooltipManager instance;

        public static TooltipManager Instance
        {
            get { return instance; }
        }

        [SerializeField] private ItemTooltip itemTooltip;

        [SerializeField] private MapItemTooltip mapItemTooltip;

        [SerializeField] private TextTooltip textTooltip;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public void ShowItemTooltip(ItemStats stats)
        {
            itemTooltip.SetItem(stats);

            itemTooltip.gameObject.SetActive(true);
        }

        public void HideItemTooltip()
        {
            itemTooltip.gameObject.SetActive(false);
        }

        public void ShowMapItemTooltip(ItemStats stats)
        {
            mapItemTooltip.SetItem(stats);

            mapItemTooltip.gameObject.SetActive(true);
        }

        public void HideMapItemTooltip()
        {
            mapItemTooltip.gameObject.SetActive(false);
        }

        public void HideMapItemTooltipIfMatching(ItemStats stats)
        {
            if (mapItemTooltip.Item == stats)
                HideMapItemTooltip();
        }

        public void ShowTextTooltip(string text)
        {
            textTooltip.SetText(text);

            textTooltip.gameObject.SetActive(true);
        }

        public void HideTextTooltip()
        {
            textTooltip.gameObject.SetActive(false);
        }
    }
}