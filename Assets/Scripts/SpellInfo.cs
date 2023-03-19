namespace Goose2Client
{
    public class SpellInfo
    {
        public int SlotNumber { get; set; }
        public string Name { get; set; }
        public int TargetType { get; set; }
        public int GraphicId { get; set; }
        public int GraphicFile { get; set; }
        public long CooldownMilliseconds { get; set; }

        public static SpellInfo FromPacket(SpellbookSlotPacket packet)
        {
            return new SpellInfo
            {
                SlotNumber = packet.SlotNumber,
                Name = packet.Name,
                TargetType = packet.TargetType,
                GraphicId = packet.GraphicId,
                GraphicFile = packet.GraphicFile,
                CooldownMilliseconds = packet.Cooldown,
            };
        }
    }
}