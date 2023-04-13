using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.U2D;

namespace Goose2Client
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] public SpriteAtlas atlas;

        private static GameManager instance;

        public CharacterSettings CharacterSettings { get; private set; }

        public NetworkClient NetworkClient { get; private set; }
        public PacketManager PacketManager { get; private set; }
        public AnimationManager AnimationManager { get; private set; }

        public MapManager MapManager { get; set; }

        public SpellTargetManager SpellTargetManager { get; set; }

        public Character Character { get; set; }

        public Dictionary<int, string> Classes { get; private set; } = new Dictionary<int, string>();

        public SpellCooldownManager SpellCooldownManager { get; private set; }

        public MapFile CurrentMap { get; set; }

        public ChatWindow ChatWindow { get; set; }

        public bool IsTyping { get { return ChatWindow?.Typing ?? false; } }

        public bool IsTargeting { get { return SpellTargetManager?.IsTargeting ?? false; } }

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

        public void LoadSettings(string characterName)
        {
            this.CharacterSettings = new CharacterSettings(characterName);
        }

        public void ChangeMap(string mapFile, string mapName)
        {
            NetworkClient.Pause = true;

            StartCoroutine(LoadMapAsync(mapFile, mapName));
        }

        private IEnumerator LoadMapAsync(string mapFile, string mapName)
        {
            var ui = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(o => o.name == "UI");
            Debug.Log($"Moving UI to map loading: {ui != null}");
            ui?.SetActive(false);

            var lastScene = SceneManager.GetActiveScene();

            var asyncLoad = SceneManager.LoadSceneAsync("LoadingMapScene", LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            var mapLoadScene = SceneManager.GetSceneByName("LoadingMapScene");

            if (ui != null)
                SceneManager.MoveGameObjectToScene(ui, mapLoadScene);

            var unload = SceneManager.UnloadSceneAsync(lastScene);
            while (!unload.isDone)
            {
                yield return null;
            }

            var canvas = mapLoadScene.GetRootGameObjects().FirstOrDefault(o => o.name == "Canvas");
            var loadingMapScript = canvas.GetComponentInChildren<LoadingMapScene>();

            loadingMapScript.Load(mapFile, mapName);
        }

        private void OnClassUpdate(object packetObj)
        {
            var packet = (ClassUpdatePacket)packetObj;

            this.Classes[packet.ClassId] = packet.Name;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}