using System;

namespace Goose2Client
{
    public static class Constants
    {
        public const int SpellbookSlotsPerPage = 30;

        public const string Walking = "Walking";
        public const string Direction = "Direction";
        public const string BodyState = "Body State";
        public const string Equipped = "Equipped";
        public const string Cast = "DoCast";
        public const string Attack = "DoAttack";

        public const string NamesLayer = "Names";
        public const string HealthBarsLayer = "HealthBars";
        public const string TooltipsLayer = "Tooltips";
    }

    public enum ItemUseType
    {
        NoUse = 0,
        OneTime,
        Armor,
        Weapon,
        Scroll,
        HairDye,
        Letter,
        Money,
        Recipe,
    }

    public enum ItemSlots
    {
        Helmet = 0,
        Shield,
        OneHanded,
        TwoHanded,
        Ring,
        Necklace,
        Pauldrons,
        Cloak,
        Belt,
        Gloves,
        Chest,
        Pants,
        Shoes,
        Mount,
        Misc = 20,
    }

    public enum ItemMaterial
    {
        None = 0,
        Plate = 10,
        Leather,
        Cloth,
        Mail,
        OneHandedSword,
        TwoHandedSword,
        OneHandedBlunt,
        TwoHandedBlunt,
        OneHandedPierce,
        TwoHandedPierce,
        Fist,
    }

    public enum ItemSlotType
    {
        None = 0,
        Helmet,
        Chestpiece,
        Pauldrons,
        Gloves,
        Pants,
        Shoes,
        Cloak,
        Belt,
        Necklace,
        Bracelet,
        Weapon,
        Shield,
        Mount
    }

    [Flags]
    public enum ItemFlags
    {
        Lore = 1,
        BindOnPickup = 2,
        ShortTerm = 4,
        Unique = 8,
        Event = 16,
        Costume = 32,
        Donation = 64,
        BindOnEquip = 128,
        BreakOnDeath = 256,
    }

    public enum ChatType
    {
        Chat = 1,
        Guild,
        Group,
        Melee,
        Spells,
        Tell,
        Server,
        Client = 8,
    }
}