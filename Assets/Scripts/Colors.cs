using System;
using System.Linq;
using UnityEngine;

namespace Goose2Client
{
    public static class Colors
    {
        public static Color White = ColorH.RGBA(255, 255, 255, 255);
        public static Color Black = ColorH.RGBA(1, 1, 1, 255);
        public static Color Yellow = ColorH.RGBA(248, 208, 0, 255);
        public static Color Green = ColorH.RGBA(136, 204, 64, 255);
        public static Color Red = ColorH.RGBA(254, 81, 28, 255);
        public static Color Blue = ColorH.RGBA(0, 146, 255, 255);
        public static Color Purple = ColorH.RGBA(135, 138, 255, 255);

        public static Color ParseColor(string colorString)
        {
            switch (colorString.ToLowerInvariant())
            {
                case "white": return White;
                case "black": return Black;
                case "yellow": return Yellow;
                case "green": return Green;
                case "red": return Red;
                case "blue": return Blue;
                case "purple": return Purple;
                default:
                    var splits = colorString.Split(',');
                    if (splits.Length >= 3)
                    {
                        byte.TryParse(splits[0], out byte r);
                        byte.TryParse(splits[1], out byte g);
                        byte.TryParse(splits[2], out byte b);
                        byte a = 255;
                        if (splits.Length > 3)
                            byte.TryParse(splits[3], out a);
                        return ColorH.RGBA(r, g, b, a);
                    }
                    else
                    {
                        return White;
                    }
            }
        }
    }
}