using UnityEngine;

namespace Goose2Client
{
    public static class ColorH
    {
        public static Color RGBA(int r, int g, int b, int a)
        {
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public static Color RGBA(int[] arr)
        {
            return new Color(arr[1] / 255.0f, arr[2] / 255.0f, arr[3] / 255.0f, arr[4] / 255.0f);
        }
    }
}