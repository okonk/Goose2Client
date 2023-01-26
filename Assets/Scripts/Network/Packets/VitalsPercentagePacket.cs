using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class VitalsPercentagePacket : PacketHandler
    {
        public int LoginId { get; set; }

        public int HPPercentage { get; set; }

        public int MPPercentage { get; set; }

        public override string Prefix { get; } = "VPU";

        public override object Parse(PacketParser p)
        {
            return new VitalsPercentagePacket()
            {
                LoginId = p.GetInt32(),
                HPPercentage = p.GetInt32(),
                MPPercentage = p.GetInt32()
            };
        }
    }
}
