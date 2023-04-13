using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goose2Client
{
    public class AnimationManager
    {
        private Dictionary<string, int> animationHeights;

        public AnimationManager()
        {
            animationHeights = new Dictionary<string, int>();

            var data = ResourceManager.Load<TextAsset>($"Animations/AnimationHeights");
            foreach (var line in data.text.Split('\n'))
            {
                if (line.Length == 0) continue;

                var comma = line.IndexOf(',');
                var name = line.Substring(0, comma);
                var height = int.Parse(line.Substring(comma + 1));

                animationHeights[name] = height;
            }
        }

        public int GetHeight(string name)
        {
            if (animationHeights.TryGetValue(name, out int height))
                return height;

            return 64;
        }
    }
}