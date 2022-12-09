using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

namespace Goose2Client
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        private Dictionary<int, GameObject> characters = new();

        private MapFile map;

        public static GameObject CharacterAnimationPrefab;
        private static GameObject CharacterPrefab;

        private GameObject roofLayer;

        private void Start()
        {
            CharacterAnimationPrefab = Resources.Load<GameObject>("Prefabs/CharacterAnimation");
            CharacterPrefab = Resources.Load<GameObject>("Prefabs/Character");

            this.map = GameManager.Instance.CurrentMap;

            GameManager.Instance.PacketManager.Listen<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Listen<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Listen<SetYourCharacterPacket>(this.OnSetYourCharacter);
            GameManager.Instance.PacketManager.Listen<MoveCharacterPacket>(this.OnMoveCharacter);
            GameManager.Instance.PacketManager.Listen<ChangeHeadingPacket>(this.OnChangeHeading);
            GameManager.Instance.PacketManager.Listen<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Listen<SendCurrentMapPacket>(this.OnSendCurrentMap);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Remove<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Remove<SetYourCharacterPacket>(this.OnSetYourCharacter);
            GameManager.Instance.PacketManager.Remove<MoveCharacterPacket>(this.OnMoveCharacter);
            GameManager.Instance.PacketManager.Remove<ChangeHeadingPacket>(this.OnChangeHeading);
            GameManager.Instance.PacketManager.Remove<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Remove<SendCurrentMapPacket>(this.OnSendCurrentMap);
        }

        private void OnMakeCharacter(object packet)
        {
            var makeCharacterPacket = (MakeCharacterPacket)packet;

            var position = new Vector3(makeCharacterPacket.MapX, map.Height - makeCharacterPacket.MapY);
            var character = Instantiate(CharacterPrefab, position, Quaternion.identity);
            character.name = makeCharacterPacket.Name;
            characters[makeCharacterPacket.LoginId] = character;

            var characterScript = character.GetComponent<Character>();
            characterScript.MakeCharacter(makeCharacterPacket);
        }

        private void OnPing(object packet)
        {
            GameManager.Instance.NetworkClient.Pong();
        }

        private void OnSetYourCharacter(object packet)
        {
            var setYourCharacter = (SetYourCharacterPacket)packet;

            if (!characters.TryGetValue(setYourCharacter.LoginId, out var character))
                return;

            SetCameraFollow(character);

            var playerController = character.gameObject.AddComponent<PlayerController>();
            playerController.MapManager = this;

            var characterScript = character.GetComponent<Character>();
            if (map[characterScript.X, characterScript.Y].IsRoof)
                this.roofLayer.SetActive(false);
        }

        private void OnMoveCharacter(object packet)
        {
            var moveCharacter = (MoveCharacterPacket)packet;

            if (!characters.TryGetValue(moveCharacter.LoginId, out var character))
                return;

            var characterScript = character.GetComponent<Character>();

            characterScript.Move(moveCharacter.MapX, moveCharacter.MapY);
        }

        private void OnChangeHeading(object packet)
        {
            var changeHeadingPacket = (ChangeHeadingPacket)packet;

            if (!characters.TryGetValue(changeHeadingPacket.LoginId, out var character))
                return;

            var characterScript = character.GetComponent<Character>();

            characterScript.SetFacing(changeHeadingPacket.Direction);
        }

        private void OnEraseCharacter(object packet)
        {
            var eraseCharacter = (EraseCharacterPacket)packet;

            if (!characters.TryGetValue(eraseCharacter.LoginId, out var character))
                return;

            Destroy(character);
            characters.Remove(eraseCharacter.LoginId);
        }

        private void OnSendCurrentMap(object packet)
        {
            SetCameraFollow(null); // needed otherwise unity gives error about using a destroyed object

            var sendCurrentMap = (SendCurrentMapPacket)packet;
            GameManager.Instance.ChangeMap(sendCurrentMap.MapFileName, sendCurrentMap.MapName);
        }

        private void SetCameraFollow(GameObject character)
        {
            var camera = cameraObject.GetComponent<CinemachineVirtualCamera>();
            camera.Follow = character?.transform;
        }

        public bool IsValidMove(int x, int y)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return false;

            if (characters.Values.Select(c => c.GetComponent<Character>()).Any(c => c.X == x && c.Y == y))
                return false;

            if (map[x, y].IsBlocked)
                return false;

            return true;
        }

        public void PlayerMoved(int fromX, int fromY, int toX, int toY)
        {
            var fromRoof = map[fromX, fromY].IsRoof;
            var toRoof = map[toX, toY].IsRoof;

            if (fromRoof && !toRoof)
                this.roofLayer.SetActive(true);
            else if (toRoof && !fromRoof)
                this.roofLayer.SetActive(false);
        }

        public void OnMapLoaded(GameObject mapObject)
        {
            this.roofLayer = GameObject.Find("Roofs");
        }
    }
}