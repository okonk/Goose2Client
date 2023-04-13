using System;
using System.Linq;
using UnityEngine;

namespace Goose2Client
{
    public static class Helpers
    {
        public static Sprite GetSprite(int id, int file)
        {
            var idString = id.ToString();
            var sprite = ResourceManager.Load<Sprite>($"Spritesheets/{file}", idString);

            return sprite;
        }

        public static string FormatDuration(this TimeSpan t)
        {
            string cd = "";
            if (t.Hours != 0)
                cd += t.Hours + "h ";

            if (t.Minutes != 0)
                cd += t.Minutes + "m ";

            var seconds = (int)Math.Round(t.Seconds + t.Milliseconds / 1000.0d, 0);
            if (seconds != 0)
             cd += seconds + "s";

            return cd;
        }
    }
}