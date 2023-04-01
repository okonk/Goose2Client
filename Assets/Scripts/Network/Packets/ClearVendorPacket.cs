using System;
using System.Collections.Generic;

namespace Goose2Client
{
    public class ClearVendorPacket : PacketHandler
    {
        public override string Prefix { get; } = "VCL";

        public override object Parse(PacketParser p)
        {
            return new ClearVendorPacket();
        }
    }
}
