using System;
using System.Collections.Generic;

namespace Goose2Client
{
    class ServerMessagePacket : PacketHandler
    {
        public int ChatType { get; set; }

        public string Message { get; set; }

        public override string Prefix { get; } = "$";

        public override object Parse(PacketParser p)
        {
            return new ServerMessagePacket()
            {
                ChatType = Convert.ToInt32(p.GetSubstring(1)),
                Message = p.GetRemaining()
            };
        }
    }
}
