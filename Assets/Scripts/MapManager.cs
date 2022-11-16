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

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Listen<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Listen<SetYourCharacterPacket>(this.OnSetYourCharacter);
        }

        private void OnMakeCharacter(object packet)
        {
            var makeCharacterPacket = (MakeCharacterPacket)packet;

            var characterPrefab = Resources.Load<GameObject>("Prefabs/Character");

            var map = GameManager.Instance.CurrentMap;
            var position = new Vector3(makeCharacterPacket.MapX + 0.5f, map.Height - makeCharacterPacket.MapY - 1);
            var character = Instantiate(characterPrefab, position, Quaternion.identity);
            character.name = makeCharacterPacket.Name;

            characters[makeCharacterPacket.LoginId] = character;
        }

        private void OnPing(object packet)
        {
            GameManager.Instance.NetworkClient.Pong();
        }

        private void OnSetYourCharacter(object packet)
        {
            var setYourCharacter = (SetYourCharacterPacket)packet;

            var character = characters[setYourCharacter.LoginId];

            var camera = cameraObject.GetComponent<CinemachineVirtualCamera>();
            camera.Follow = character.transform;
        }
    }
}