using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Goose2Client
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        private Dictionary<int, GameObject> characters = new();

        public static GameObject CharacterAnimationPrefab;
        private static GameObject CharacterPrefab;

        private void Start()
        {
            CharacterAnimationPrefab = Resources.Load<GameObject>("Prefabs/CharacterAnimation");
            CharacterPrefab = Resources.Load<GameObject>("Prefabs/Character");

            GameManager.Instance.PacketManager.Listen<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Listen<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Listen<SetYourCharacterPacket>(this.OnSetYourCharacter);
            GameManager.Instance.PacketManager.Listen<MoveCharacterPacket>(this.OnMoveCharacter);
            GameManager.Instance.PacketManager.Listen<EraseCharacterPacket>(this.OnEraseCharacter);
        }

        private void OnMakeCharacter(object packet)
        {
            var makeCharacterPacket = (MakeCharacterPacket)packet;

            var map = GameManager.Instance.CurrentMap;
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

            var camera = cameraObject.GetComponent<CinemachineVirtualCamera>();
            camera.Follow = character.transform;
        }

        private void OnMoveCharacter(object packet)
        {
            var moveCharacter = (MoveCharacterPacket)packet;

            if (!characters.TryGetValue(moveCharacter.LoginId, out var character))
                return;

            var characterScript = character.GetComponent<Character>();

            characterScript.Move(moveCharacter.MapX, moveCharacter.MapY);
        }

        private void OnEraseCharacter(object packet)
        {
            var eraseCharacter = (EraseCharacterPacket)packet;

            if (!characters.TryGetValue(eraseCharacter.LoginId, out var character))
                return;

            Destroy(character);
            characters.Remove(eraseCharacter.LoginId);
        }
    }
}