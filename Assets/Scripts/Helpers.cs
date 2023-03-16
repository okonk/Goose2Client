using System.Linq;
using UnityEngine;

namespace Goose2Client
{
    public static class Helpers
    {
        public static Sprite GetSprite(int id, int file)
        {
            var idString = id.ToString();
            var sprite = Resources.LoadAll<Sprite>($"Spritesheets/{file}").FirstOrDefault(s => s.name == idString);

            return sprite;
        }
    }
}