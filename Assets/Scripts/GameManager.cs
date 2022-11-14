using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goose2Client
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public NetworkClient NetworkClient { get; private set; }
        public PacketManager PacketManager { get; private set; }

        public static GameManager Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            NetworkClient = new NetworkClient();
            PacketManager = new PacketManager();
        }

        public void Update()
        {
            this.NetworkClient.Update();
        }

        public void ChangeMap(string mapFile, string mapName)
        {
            NetworkClient.Pause = true;

            StartCoroutine(LoadMapAsync(mapFile, mapName));
        }

        private IEnumerator LoadMapAsync(string mapFile, string mapName)
        {
            var asyncLoad = SceneManager.LoadSceneAsync("LoadingMapScene");

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            var mapLoadScene = SceneManager.GetSceneByName("LoadingMapScene");
            var canvas = mapLoadScene.GetRootGameObjects().FirstOrDefault(o => o.name == "Canvas");
            var loadingMapScript = canvas.GetComponentInChildren<LoadingMapScene>();

            loadingMapScript.Load(mapFile, mapName);
        }
    }
}