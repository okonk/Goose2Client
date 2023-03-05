using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Goose2Client
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI flagsText;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        internal void SetItem(ItemStats itemStats)
        {
            var idString = itemStats.GraphicId.ToString();
            var sprite = Resources.LoadAll<Sprite>($"Spritesheets/{itemStats.GraphicFile}").FirstOrDefault(s => s.name == idString);
            image.sprite = sprite;
            image.color = Color.white;
            image.material.SetColor("_Tint", ColorH.RGBA(itemStats.GraphicR, itemStats.GraphicG, itemStats.GraphicB, itemStats.GraphicA));

            nameText.text = itemStats.Name;
            itemTypeText.text = GetItemTypeText(itemStats);
            flagsText.text = string.Join(", ", GetFlagsStrings(itemStats.Flags));
        }

        public void Update()
        {
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

        private string GetItemTypeText(ItemStats itemStats)
        {
            return itemStats.UseType switch {
                ItemUseType.Armor => $"{GetMaterialText(itemStats.MaterialType)} {GetSlotText(itemStats.SlotType)}".Trim(),
                ItemUseType.Weapon => GetMaterialText(itemStats.MaterialType),
                ItemUseType.HairDye => "Hair Dye",
                ItemUseType.Letter => "Letter",
                ItemUseType.Money => "Money",
                ItemUseType.Recipe => "Recipe",
                ItemUseType.OneTime => "",
                ItemUseType.Scroll => "Scroll",
                _ => "Miscellaneous",
            };
        }

        private string GetMaterialText(ItemMaterial type)
        {
            return type switch {
                ItemMaterial.Cloth => "Cloth",
                ItemMaterial.Plate => "Plate",
                ItemMaterial.Leather => "Leather",
                ItemMaterial.Mail => "Mail",
                ItemMaterial.OneHandedSword => "One-Handed Sword",
                ItemMaterial.TwoHandedSword => "Two-Handed Sword",
                ItemMaterial.OneHandedBlunt => "One-Handed Blunt",
                ItemMaterial.TwoHandedBlunt => "Two-Handed Blunt",
                ItemMaterial.OneHandedPierce => "One-Handed Pierce",
                ItemMaterial.TwoHandedPierce => "Two-Handed Pierce",
                ItemMaterial.Fist => "Fist Weapon",
                _ => ""
            };
        }

        private string GetSlotText(ItemSlotType slot)
        {
            return slot switch {
                ItemSlotType.None => "",
                _ => slot.ToString()
            };
        }

        private IEnumerable<string> GetFlagsStrings(ItemFlags flags)
        {
            foreach (ItemFlags value in Enum.GetValues(typeof(ItemFlags)))
            {
                if (flags.HasFlag(value)) yield return GetFlagsText(value);
            }
        }

        private string GetFlagsText(ItemFlags flags)
        {
            return flags switch {
                ItemFlags.BindOnEquip => "Bind on Equip",
                ItemFlags.BindOnPickup => "Bind on Pickup",
                ItemFlags.BreakOnDeath => "Break on Death",
                ItemFlags.ShortTerm => "Short Term",
                _ => flags.ToString()
            };
        }
    }
}