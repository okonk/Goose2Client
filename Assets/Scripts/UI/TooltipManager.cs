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

        [SerializeField] private SpellTooltip spellTooltip;

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

        public void ShowItemTooltip(ItemStats stats, GameObject parent)
        {
            itemTooltip.SetItem(stats, parent);

            itemTooltip.gameObject.SetActive(true);
        }

        public void HideItemTooltip()
        {
            itemTooltip.Hide();
        }

        public void ShowSpellTooltip(SpellInfo spell, GameObject parent)
        {
            spellTooltip.SetSpell(spell, parent);

            spellTooltip.gameObject.SetActive(true);
        }

        public void HideSpellTooltip()
        {
            spellTooltip.Hide();
        }

        public void ShowMapItemTooltip(ItemStats stats, GameObject parent)
        {
            mapItemTooltip.SetItem(stats, parent);

            mapItemTooltip.gameObject.SetActive(true);
        }

        public void HideMapItemTooltip()
        {
            mapItemTooltip.Hide();
        }

        public void HideMapItemTooltipIfMatching(ItemStats stats)
        {
            if (mapItemTooltip.Item == stats)
                HideMapItemTooltip();
        }

        public void ShowTextTooltip(string text, GameObject parent)
        {
            textTooltip.SetText(text, parent);

            textTooltip.gameObject.SetActive(true);
        }

        public void HideTextTooltip()
        {
            textTooltip.Hide();
        }
    }
}