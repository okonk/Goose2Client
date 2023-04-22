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
        [SerializeField] private GameObject statsLineContainer;

        private RectTransform rectTransform;

        private GameObject parent;

        private static Color descriptionColor = Color.white;
        private static Color acColor = ColorH.RGBA(216, 208, 176);
        private static Color weaponDamageColor = ColorH.RGBA(232, 244, 112);
        private static Color hpMpColor = ColorH.RGBA(192, 204, 136);
        private static Color statColor = ColorH.RGBA(136, 204, 192);
        private static Color resistanceColor = ColorH.RGBA(208, 144, 144);
        private static Color requirementColor = ColorH.RGBA(232, 120, 112);
        private static Color effectColor = ColorH.RGBA(208, 168, 112);
        private static Color valueColor = ColorH.RGBA(232, 224, 112);

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        internal void SetItem(ItemStats itemStats, GameObject parent)
        {
            this.parent = parent;

            image.sprite = Helpers.GetSprite(itemStats.GraphicId, itemStats.GraphicFile);
            image.color = Color.white;
            image.material = Instantiate(image.material);
            image.material.SetColor("_Tint", ColorH.RGBA(itemStats.GraphicR, itemStats.GraphicG, itemStats.GraphicB, itemStats.GraphicA));

            nameText.text = $"{itemStats.Title} {itemStats.Name} {itemStats.Surname}".Trim();
            itemTypeText.text = GetItemTypeText(itemStats);
            flagsText.text = string.Join(", ", GetFlagsStrings(itemStats.Flags));

            while (statsLineContainer.transform.childCount > 0)
                DestroyImmediate(statsLineContainer.transform.GetChild(0).gameObject);

            if (itemStats.Description != null) AddStatLine(itemStats.Description, descriptionColor);

            if (itemStats.MaxDamage != 0) AddStatLine($"{itemStats.MinDamage:N0}-{itemStats.MaxDamage:N0} Damage / {itemStats.Delay / 10.0f:F1}s Delay", weaponDamageColor);
            if (itemStats.AC != 0) AddStatLine($"{itemStats.AC:N0} Armor", acColor);

            if (itemStats.HP != 0) AddStatLine($"{FormatNumber(itemStats.HP)} Health", hpMpColor);
            if (itemStats.MP != 0) AddStatLine($"{FormatNumber(itemStats.MP)} Mana", hpMpColor);
            if (itemStats.SP != 0) AddStatLine($"{FormatNumber(itemStats.SP)} Spirit", hpMpColor);

            if (itemStats.Strength != 0) AddStatLine($"{FormatNumber(itemStats.Strength)} Strength", statColor);
            if (itemStats.Stamina != 0) AddStatLine($"{FormatNumber(itemStats.Stamina)} Stamina", statColor);
            if (itemStats.Intelligence != 0) AddStatLine($"{FormatNumber(itemStats.Intelligence)} Intelligence", statColor);
            if (itemStats.Dexterity != 0) AddStatLine($"{FormatNumber(itemStats.Dexterity)} Dexterity", statColor);

            if (itemStats.FireResist != 0) AddStatLine($"{FormatNumber(itemStats.FireResist)} Fire Resistance", resistanceColor);
            if (itemStats.WaterResist != 0) AddStatLine($"{FormatNumber(itemStats.WaterResist)} Water Resistance", resistanceColor);
            if (itemStats.EarthResist != 0) AddStatLine($"{FormatNumber(itemStats.EarthResist)} Earth Resistance", resistanceColor);
            if (itemStats.AirResist != 0) AddStatLine($"{FormatNumber(itemStats.AirResist)} Air Resistance", resistanceColor);
            if (itemStats.SpiritResist != 0) AddStatLine($"{FormatNumber(itemStats.SpiritResist)} Spirit Resistance", resistanceColor);

            if (itemStats.ClassRestrictions1 != 0)
            {
                int offset = itemStats.ClassRestrictions1 < 50 ? 0 : -50;

                string classes = GameManager.Instance.Classes[itemStats.ClassRestrictions1 + offset];
                if (itemStats.ClassRestrictions2 != 0) classes += $"{(itemStats.ClassRestrictions3 == 0 ? " or" : ",")} {GameManager.Instance.Classes[itemStats.ClassRestrictions2 + offset]}";
                if (itemStats.ClassRestrictions3 != 0) classes += $" or {GameManager.Instance.Classes[itemStats.ClassRestrictions3 + offset]}";

                AddStatLine($"You must {(offset != 0 ? "NOT " : "")}be a {classes} to use this item", requirementColor);
            }

            if (itemStats.MinLevel != 0 && itemStats.MaxLevel != 0) AddStatLine($"Requires level {itemStats.MinLevel} to {itemStats.MaxLevel}", requirementColor);
            else if (itemStats.MinLevel == 0 && itemStats.MaxLevel != 0) AddStatLine($"Requires level 1 to {itemStats.MaxLevel}", requirementColor);
            else if (itemStats.MinLevel != 0 && itemStats.MaxLevel == 0) AddStatLine($"Requires level {itemStats.MinLevel}", requirementColor);

            if (!string.IsNullOrEmpty(itemStats.SpellEffect))
            {
                var effectLines = itemStats.SpellEffect.Split(';');
                var effectChance = itemStats.SpellEffectChance == 100 ? "" : $" ({itemStats.SpellEffectChance}%)";

                AddStatLine($"Effect: {effectLines[0]}{effectChance}", effectColor);

                foreach (var effectLine in effectLines.Skip(1))
                    AddStatLine($"    {effectLine}", effectColor);
            }

            AddEmptyLine();

            if (itemStats.Value == 0)
            {
                AddStatLine("No Value", valueColor);
            }
            else
            {
                var currency = itemStats.Flags.HasFlag(ItemFlags.Donation) ? "credits" : "gold";
                AddStatLine($"Value: {itemStats.Value:N0} {currency}", valueColor);
            }
        }

        private string FormatNumber(int value)
        {
            if (value < 0)
                return $"{value:N0}";

            return $"+{value:N0}";
        }

        private void AddStatLine(string text, Color color)
        {
            var prefab = ResourceManager.LoadFromBundle<GameObject>("ui-prefabs", "ItemTooltipStatLine");
            var lineObject = Instantiate(prefab, statsLineContainer.transform);
            var lineText = lineObject.GetComponent<TextMeshProUGUI>();
            lineText.text = text;
            lineText.color = color;
        }

        private void AddEmptyLine()
        {
            AddStatLine(" ", Color.black);
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

        private string GetItemTypeText(ItemStats itemStats)
        {
            return itemStats.UseType switch {
                ItemUseType.Armor => $"{GetMaterialText(itemStats.MaterialType)} {GetSlotText(itemStats.SlotType)}".Trim(),
                ItemUseType.Weapon => GetMaterialText(itemStats.MaterialType),
                ItemUseType.HairDye => "Hair Dye",
                ItemUseType.Letter => "Letter",
                ItemUseType.Money => "Money",
                ItemUseType.Recipe => "Recipe",
                ItemUseType.OneTime => " ",
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
                _ => " "
            };
        }

        private string GetSlotText(ItemSlotType slot)
        {
            return slot switch {
                ItemSlotType.None => " ",
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