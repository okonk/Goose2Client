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
    }
}