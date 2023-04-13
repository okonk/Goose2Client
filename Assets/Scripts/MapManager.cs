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
        private Dictionary<int, Character> characters = new();

        private Dictionary<int, GameObject> mapObjects = new();

        public IEnumerable<Character> Characters => characters.Values;

        private MapFile map;

        public static GameObject CharacterAnimationPrefab;
        private static GameObject CharacterPrefab;
        public static GameObject SpellAnimationPrefab;

        private GameObject roofLayer;

        private Character character;

        public float WeaponSpeed { get; set; } = 1.0f;

        private void Start()
        {
            CharacterAnimationPrefab = ResourceManager.Load<GameObject>("Prefabs/CharacterAnimation");
            CharacterPrefab = ResourceManager.Load<GameObject>("Prefabs/Character");
            SpellAnimationPrefab = ResourceManager.Load<GameObject>("Prefabs/SpellAnimation");

            GameManager.Instance.MapManager = this;

            this.map = GameManager.Instance.CurrentMap;

            GameManager.Instance.PacketManager.Listen<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Listen<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Listen<UpdateCharacterPacket>(this.OnUpdateCharacter);
            GameManager.Instance.PacketManager.Listen<SetYourCharacterPacket>(this.OnSetYourCharacter);
            GameManager.Instance.PacketManager.Listen<MoveCharacterPacket>(this.OnMoveCharacter);
            GameManager.Instance.PacketManager.Listen<ChangeHeadingPacket>(this.OnChangeHeading);
            GameManager.Instance.PacketManager.Listen<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Listen<SendCurrentMapPacket>(this.OnSendCurrentMap);
            GameManager.Instance.PacketManager.Listen<VitalsPercentagePacket>(this.OnVitalsPercentage);
            GameManager.Instance.PacketManager.Listen<AttackPacket>(this.OnAttack);
            GameManager.Instance.PacketManager.Listen<WeaponSpeedPacket>(this.OnWeaponSpeed);
            GameManager.Instance.PacketManager.Listen<SetYourPositionPacket>(this.OnSetYourPosition);
            GameManager.Instance.PacketManager.Listen<SpellCharacterPacket>(this.OnSpellCharacter);
            GameManager.Instance.PacketManager.Listen<SpellTilePacket>(this.OnSpellTile);
            GameManager.Instance.PacketManager.Listen<BattleTextPacket>(this.OnBattleText);
            GameManager.Instance.PacketManager.Listen<CastPacket>(this.OnCast);
            GameManager.Instance.PacketManager.Listen<MapObjectPacket>(this.OnMapObject);
            GameManager.Instance.PacketManager.Listen<EraseObjectPacket>(this.OnEraseMapObject);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<PingPacket>(this.OnPing);
            GameManager.Instance.PacketManager.Remove<MakeCharacterPacket>(this.OnMakeCharacter);
            GameManager.Instance.PacketManager.Remove<UpdateCharacterPacket>(this.OnUpdateCharacter);
            GameManager.Instance.PacketManager.Remove<SetYourCharacterPacket>(this.OnSetYourCharacter);
            GameManager.Instance.PacketManager.Remove<MoveCharacterPacket>(this.OnMoveCharacter);
            GameManager.Instance.PacketManager.Remove<ChangeHeadingPacket>(this.OnChangeHeading);
            GameManager.Instance.PacketManager.Remove<EraseCharacterPacket>(this.OnEraseCharacter);
            GameManager.Instance.PacketManager.Remove<SendCurrentMapPacket>(this.OnSendCurrentMap);
            GameManager.Instance.PacketManager.Remove<VitalsPercentagePacket>(this.OnVitalsPercentage);
            GameManager.Instance.PacketManager.Remove<AttackPacket>(this.OnAttack);
            GameManager.Instance.PacketManager.Remove<WeaponSpeedPacket>(this.OnWeaponSpeed);
            GameManager.Instance.PacketManager.Remove<SetYourPositionPacket>(this.OnSetYourPosition);
            GameManager.Instance.PacketManager.Remove<SpellCharacterPacket>(this.OnSpellCharacter);
            GameManager.Instance.PacketManager.Remove<SpellTilePacket>(this.OnSpellTile);
            GameManager.Instance.PacketManager.Remove<BattleTextPacket>(this.OnBattleText);
            GameManager.Instance.PacketManager.Remove<CastPacket>(this.OnCast);
            GameManager.Instance.PacketManager.Remove<MapObjectPacket>(this.OnMapObject);
            GameManager.Instance.PacketManager.Remove<EraseObjectPacket>(this.OnEraseMapObject);
        }

        private void OnApplicationQuit()
        {
            GameManager.Instance.NetworkClient.Quit();
        }

        private void OnMakeCharacter(object packet)
        {
            var makeCharacterPacket = (MakeCharacterPacket)packet;

            var position = new Vector3(makeCharacterPacket.MapX, map.Height - makeCharacterPacket.MapY);
            var character = Instantiate(CharacterPrefab, position, Quaternion.identity);
            character.name = makeCharacterPacket.Name;

            var characterScript = character.GetComponent<Character>();

            characters[makeCharacterPacket.LoginId] = characterScript;

            characterScript.MakeCharacter(makeCharacterPacket);
        }

        private void OnUpdateCharacter(object packet)
        {
            var updateCharacterPacket = (UpdateCharacterPacket)packet;

            if (!characters.TryGetValue(updateCharacterPacket.LoginId, out var character))
                return;

            character.UpdateCharacter(updateCharacterPacket);
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

            SetCameraFollow(character.gameObject);

            var playerController = character.gameObject.AddComponent<PlayerController>();
            playerController.MapManager = this;

            if (map[character.X, character.Y].IsRoof)
                this.roofLayer.SetActive(false);

            this.character = character;

            GameManager.Instance.Character = character;
        }

        private void OnMoveCharacter(object packet)
        {
            var moveCharacter = (MoveCharacterPacket)packet;

            if (!characters.TryGetValue(moveCharacter.LoginId, out var character))
                return;

            character.Move(moveCharacter.MapX, moveCharacter.MapY);
        }

        private void OnChangeHeading(object packet)
        {
            var changeHeadingPacket = (ChangeHeadingPacket)packet;

            if (!characters.TryGetValue(changeHeadingPacket.LoginId, out var character))
                return;

            character.SetFacing(changeHeadingPacket.Direction);
        }

        private void OnEraseCharacter(object packet)
        {
            var eraseCharacter = (EraseCharacterPacket)packet;

            if (!characters.TryGetValue(eraseCharacter.LoginId, out var character))
                return;

            Destroy(character.gameObject);
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

        private void ShowSpell(int id, Transform parent, int x = 0, int y = 0)
        {
            var animation = Instantiate(MapManager.SpellAnimationPrefab, parent);
            animation.name = $"Spell ({id})";

            var spellAnimationScript = animation.GetComponent<SpellAnimation>();
            spellAnimationScript.SetAnimation(id, x, y);
        }

        public void OnMapLoaded(GameObject mapObject)
        {
            this.roofLayer = GameObject.Find("Roofs");
        }

        private void OnVitalsPercentage(object packet)
        {
            var vitalsPercentage = (VitalsPercentagePacket)packet;

            if (!characters.TryGetValue(vitalsPercentage.LoginId, out var character))
                return;

            character.UpdateHPMP(vitalsPercentage.HPPercentage, vitalsPercentage.MPPercentage);
        }

        private void OnAttack(object packet)
        {
            var attack = (AttackPacket)packet;

            if (!characters.TryGetValue(attack.LoginId, out var character))
                return;

            character.Attack();
        }

        private void OnCast(object packet)
        {
            var cast = (CastPacket)packet;

            if (!characters.TryGetValue(cast.LoginId, out var character))
                return;

            character.Cast();
        }

        private void OnWeaponSpeed(object packet)
        {
            var weaponSpeedPacket = (WeaponSpeedPacket)packet;

            this.WeaponSpeed = weaponSpeedPacket.Speed / 1000f;
        }

        private void OnSetYourPosition(object packet)
        {
            var setPosition = (SetYourPositionPacket)packet;

            this.character.SetPosition(setPosition.MapX, setPosition.MapY);
        }

        private void OnSpellCharacter(object packet)
        {
            var spellCharacter = (SpellCharacterPacket)packet;

            if (!characters.TryGetValue(spellCharacter.LoginId, out var character))
                return;

            ShowSpell(spellCharacter.AnimationId, character.gameObject.transform);
        }

        private void OnSpellTile(object packet)
        {
            var spellTile = (SpellTilePacket)packet;

            ShowSpell(spellTile.AnimationId, gameObject.transform, spellTile.TileX, map.Height - spellTile.TileY);
        }

        private void OnBattleText(object packet)
        {
            var battleText = (BattleTextPacket)packet;

            if (!characters.TryGetValue(battleText.LoginId, out var character))
                return;

            character.AddBattleText(battleText.BattleTextType, battleText.Text);
        }

        private void OnMapObject(object packetObj)
        {
            var packet = (MapObjectPacket)packetObj;

            var itemPrefab = ResourceManager.Load<GameObject>("Prefabs/MapItem");
            var item = Instantiate(itemPrefab, gameObject.transform);
            item.name = $"MapItem {packet.Name} ({packet.GraphicId})";

            var script = item.GetComponent<MapItem>();
            script.Item = ItemStats.FromPacket(packet);

            var renderer = item.GetComponent<SpriteRenderer>();
            renderer.sprite = Helpers.GetSprite(packet.GraphicId, packet.GraphicFile);
            renderer.color = Color.white;
            renderer.material.SetColor("_Tint", ColorH.RGBA(packet.GraphicR, packet.GraphicG, packet.GraphicB, packet.GraphicA));

            item.transform.localPosition = new Vector3(packet.TileX + 0.5f, map.Height - packet.TileY - 0.5f);

            mapObjects[packet.TileY * map.Height + packet.TileX] = item;
        }

        private void OnEraseMapObject(object packetObj)
        {
            var packet = (EraseObjectPacket)packetObj;

            if (mapObjects.TryGetValue(packet.TileY * map.Height + packet.TileX, out var item))
                Destroy(item);
        }

        public Character GetCharacter(int loginId)
        {
            if (characters.TryGetValue(loginId, out var character))
                return character;

            return null;
        }
    }
}