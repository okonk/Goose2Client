using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class SetYourCharacterPacket : PacketHandler
    {
        public int LoginId { get; set; }

        public override string Prefix { get; } = "SUC";

        public override object Parse(PacketParser p)
        {
            return new SetYourCharacterPacket()
            {
                LoginId = p.GetInt32()
            };
        }
    }
}
