using System;
using System.Collections.Generic;
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

        public static T Load<T>(string path, string id) where T: UnityEngine.Object
        {
            var key = path + "/" + id;
            if (cache.TryGetValue(key, out var obj))
                return (T)obj;

            var resources = Resources.LoadAll<T>(path);
            foreach (var resource in resources)
                cache[path + "/" + resource.name] = resource;

            return (T)cache[key];
        }
    }
}