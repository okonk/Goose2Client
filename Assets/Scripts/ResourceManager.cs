using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Goose2Client
{
    public static class ResourceManager
    {
        private static readonly Dictionary<string, object> cache = new();

        public static T Load<T>(string path) where T: UnityEngine.Object
        {
            if (cache.TryGetValue(path, out var obj))
                return (T)obj;

            var resource = Resources.Load<T>(path);
            cache[path] = resource;

            return resource;
        }

        public static Sprite LoadSprite(string id)
        {
            if (cache.TryGetValue(id, out var obj))
                return (Sprite)obj;

            var sprite = GameManager.Instance.atlas.GetSprite(id);
            cache[id] = sprite;

            return sprite;
        }

        public static AssetBundle LoadAssetBundle(string path)
        {
            if (cache.TryGetValue(path, out var obj))
                return (AssetBundle)obj;

            var resource = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, path));
            cache[path] = resource;

            return resource;
        }

        public static T Load<T>(AssetBundle bundle, string path) where T: UnityEngine.Object
        {
            if (cache.TryGetValue(path, out var obj))
                return (T)obj;

            var resource = bundle.LoadAsset<T>(path);
            cache[path] = resource;

            return resource;
        }
    }
}