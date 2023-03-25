using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Goose2Client
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public NetworkClient NetworkClient { get; private set; }
        public PacketManager PacketManager { get; private set; }
        public AnimationManager AnimationManager { get; private set; }

        public Character Character { get; set; }

        public Dictionary<int, string> Classes { get; private set; } = new Dictionary<int, string>();

        public SpellCooldownManager SpellCooldownManager { get; private set; }

        public MapFile CurrentMap { get; set; }

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
            AnimationManager = new AnimationManager();
            SpellCooldownManager = new SpellCooldownManager();
        }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ClassUpdatePacket>(this.OnClassUpdate);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<ClassUpdatePacket>(this.OnClassUpdate);
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

        private void OnClassUpdate(object packetObj)
        {
            var packet = (ClassUpdatePacket)packetObj;

            this.Classes[packet.ClassId] = packet.Name;
        }
    }
}