using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public static class Helpers
    {
        public static Sprite GetSprite(int id, int file)
        {
            return ResourceManager.LoadSprite($"{file}-{id}");
        }

        public static string FormatDuration(this TimeSpan t)
        {
            string cd = "";
            if (t.Hours != 0)
                cd += t.Hours + "h ";

            if (t.Minutes != 0)
                cd += t.Minutes + "m ";

            var seconds = Math.Round(t.Seconds + t.Milliseconds / 1000.0f, 0);
            if (seconds != 0)
             cd += seconds + "s";

            return cd;
        }

        public static int GetStackSplitAmount(int initialStack)
        {
            if (initialStack == 1) return initialStack;

            int splitStack = initialStack;
            if (Keyboard.current.ctrlKey.isPressed)
            {
                splitStack = 1;
            }
            else if (Keyboard.current.shiftKey.isPressed)
            {
                splitStack = initialStack / 2;
            }

            return splitStack;
        }
    }
}