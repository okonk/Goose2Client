using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class ClearCombineBagSlotPacket : PacketHandler
    {
        public int SlotNumber { get; set; }

        public override string Prefix { get; } = "CCS";

        public override object Parse(PacketParser p)
        {
            return new ClearCombineBagSlotPacket()
            {
                SlotNumber = p.GetInt32() - 1
            };
        }
    }
}
