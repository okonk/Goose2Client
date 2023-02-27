using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class InventorySlotPacket : PacketHandler
    {
        public int SlotNumber { get; set; }
        public int GraphicId { get; set; }
        public int GraphicFile { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int StackSize { get; set; }
        public int Value { get; set; }
        public int Flags { get; set; }
        public string Description { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int Delay { get; set; }
        public int Type { get; set; }
        public int AC { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int SP { get; set; }
        public int Strength { get; set; }
        public int Stamina { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
        public int FireResist { get; set; }
        public int WaterResist { get; set; }
        public int EarthResist { get; set; }
        public int AirResist { get; set; }
        public int SpiritResist { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int ClassRestrictions { get; set; }
        public int Access { get; set; }
        public int Gender { get; set; }
        public string SpellEffect { get; set; }
        public int SpellEffectChance { get; set; }
        public int BodyType { get; set; }
        public int UseType { get; set; }
        public int NotSure { get; set; }
        public int GraphicR { get; set; }
        public int GraphicG { get; set; }
        public int GraphicB { get; set; }
        public int GraphicA { get; set; }

        public override string Prefix { get; } = "SIS";

        public override object Parse(PacketParser p)
        {
            p.Delimeter = '|';

            //SIS1|331405|2270||Stick||1|0|0||0|0|0|16|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0||100|11|3|0|0|0|0|0'

            // return slotId + "|" +
            //         item.GraphicTile + "|" +
            //         item.GraphicFile + "|" +
            //         "" + "|" + // title
            //         item.Name + "|" +
            //         "" + "|" + //surname
            //         stack + "|" +
            //         item.Value + "|" +
            //         item.Flags + "|" +
            //         item.Description + "|" +
            //         item.WeaponDamage + "|" +
            //         item.WeaponDamage + "|" +
            //         (item.WeaponDamage > 0 ? item.WeaponDelay : 0) + "|" +
            //         (int)item.Type + "|" +
            //         item.TotalStats.AC + "|" +
            //         item.TotalStats.HP + "|" +
            //         item.TotalStats.MP + "|" +
            //         item.TotalStats.SP + "|" +
            //         item.TotalStats.Strength + "|" +
            //         item.TotalStats.Stamina + "|" +
            //         item.TotalStats.Intelligence + "|" +
            //         item.TotalStats.Dexterity + "|" +
            //         item.TotalStats.FireResist + "|" +
            //         item.TotalStats.WaterResist + "|" +
            //         item.TotalStats.EarthResist + "|" +
            //         item.TotalStats.AirResist + "|" +
            //         item.TotalStats.SpiritResist + "|" +
            //         item.MinLevel + "|" +
            //         item.MaxLevel + "|" +
            //         ItemTemplate.FigureClassRestrictions(world, item.ClassRestrictions) +
            //         "0" + "|" + // gm access
            //         "0" + "|" + // gender, always 0 since we don't care about gender
            //         (spellEffect == null ? "" : spellEffect.Name + ';' + string.Join(";", spellEffect.GetItemDescription(world))) + "|" +
            //         spellEffectChance + "|" +
            //         item.BodyType + "|" +
            //         (int)item.UseType + "|" +
            //         0 + "|" + // not sure
            //         item.GraphicR + "|" +
            //         item.GraphicG + "|" +
            //         item.GraphicB + "|" +
            //         item.GraphicA;

            return new InventorySlotPacket()
            {
                SlotNumber = p.GetInt32() - 1,
                GraphicId = p.GetInt32(),
                GraphicFile = p.GetInt32(),
                Title = p.GetString(),
                Name = p.GetString(),
                Surname = p.GetString(),
                StackSize = p.GetInt32(),
                Value = p.GetInt32(),
                Flags = p.GetInt32(),
                Description = p.GetString(),
                MinDamage = p.GetInt32(),
                MaxDamage = p.GetInt32(),
                Delay = p.GetInt32(),
                Type = p.GetInt32(),
                AC = p.GetInt32(),
                HP = p.GetInt32(),
                MP = p.GetInt32(),
                SP = p.GetInt32(),
                Strength = p.GetInt32(),
                Stamina = p.GetInt32(),
                Intelligence = p.GetInt32(),
                Dexterity = p.GetInt32(),
                FireResist = p.GetInt32(),
                WaterResist = p.GetInt32(),
                EarthResist = p.GetInt32(),
                AirResist = p.GetInt32(),
                SpiritResist = p.GetInt32(),
                MinLevel = p.GetInt32(),
                MaxLevel = p.GetInt32(),
                ClassRestrictions = p.GetInt32(),
                Access = p.GetInt32(),
                Gender = p.GetInt32(),
                SpellEffect = p.GetString(),
                SpellEffectChance = p.GetInt32(),
                BodyType = p.GetInt32(),
                UseType = p.GetInt32(),
                NotSure = p.GetInt32(),
                GraphicR = p.GetInt32(),
                GraphicG = p.GetInt32(),
                GraphicB = p.GetInt32(),
                GraphicA = p.GetInt32(),
            };
        }
    }
}
