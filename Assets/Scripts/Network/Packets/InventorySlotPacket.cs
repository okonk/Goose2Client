using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class InventorySlotPacket : PacketHandler
    {
        public int SlotNumber { get; set; }

        public int ItemId { get; set; }
        public int ItemFile { get; set; }

        public string ItemName { get; set; }

        public int StackSize { get; set; }

        public int GraphicId { get; set; }

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

            var packet = new InventorySlotPacket()
            {
                SlotNumber = p.GetInt32() - 1
            };

            if (p.LengthRemaining() > 0)
            {
                packet.GraphicId = p.GetInt32();
                packet.ItemId = packet.GraphicId; // temporary..
                packet.ItemFile = p.GetInt32();
                var title = p.GetString();
                packet.ItemName = p.GetString();
                var surname = p.GetString();
                packet.StackSize = p.GetInt32();
                packet.GraphicId = p.GetInt32();
            }

            return packet;
        }
    }
}
