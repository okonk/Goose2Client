using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goose2Client
{
    public class AnimationFrame
    {
        public int Id { get; }
        public int Width { get; }
        public int Height { get; }

        public AnimationFrame(int id, int w, int h)
        {
            Id = id;
            Width = w;
            Height = h;
        }
    }

    public class AnimationManager
    {
        private Dictionary<string, int> animationHeights = new();

        private Dictionary<string, AnimationFrame> animationFirstFrame = new();

        public AnimationManager()
        {
            LoadHeights();
            LoadFrames();
        }

        private void LoadHeights()
        {
            var data = ResourceManager.Load<TextAsset>($"AnimationHeights");
            foreach (var line in data.text.Split('\n'))
            {
                if (line.Length == 0) continue;

                var comma = line.IndexOf(',');
                var name = line.Substring(0, comma);
                var height = int.Parse(line.Substring(comma + 1));

                animationHeights[name] = height;
            }
        }

        private void LoadFrames()
        {
            var data = ResourceManager.Load<TextAsset>($"AnimationToFirstFrame");
            foreach (var line in data.text.Split('\n'))
            {
                if (line.Length == 0) continue;

                var tokens = line.Split(',');
                var name = tokens[0];
                var id = int.Parse(tokens[1]);
                var w = int.Parse(tokens[2]);
                var h = int.Parse(tokens[3]);

                animationFirstFrame[name] = new AnimationFrame(id, w, h);
            }
        }

        public AnimationFrame GetFrame(string animationType, int animationId)
        {
            var id = $"{animationType}-{animationId}";
            if (animationFirstFrame.TryGetValue(id, out var frame))
                return frame;

            return null;
        }

        public int GetHeight(string name)
        {
            if (animationHeights.TryGetValue(name, out int height))
                return height;

            return 64;
        }
    }
}