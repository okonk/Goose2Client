using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace Goose2Client
{
    public class LoadingScene : MonoBehaviour
    {
        private void Start()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;

            StartCoroutine(LoadAsync());
        }

        private void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            var atlas = ResourceManager.LoadFromBundle<SpriteAtlas>("spriteatlas", "SpriteAtlas");

            callback(atlas);
        }

        private IEnumerator OnFinishedLoading()
        {
            var asyncLoad = SceneManager.LoadSceneAsync("LoginScene");

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator LoadAsync()
        {
            var bundlesToLoad = new[] { "spriteatlas", "prefabs", "ui-prefabs", "maps", "body-1", "spell-1080" };

            foreach (var name in bundlesToLoad)
            {
                yield return LoadAssetBundleAsync(name);
            }

            yield return OnFinishedLoading();
        }

        private IEnumerator LoadAssetBundleAsync(string name)
        {
            var assetBundleLoad = ResourceManager.LoadAssetBundleAsync(name);

            while (!assetBundleLoad.isDone)
            {
                yield return null;
            }

            ResourceManager.CacheAssetBundle(name, assetBundleLoad.assetBundle);
        }
    }
}