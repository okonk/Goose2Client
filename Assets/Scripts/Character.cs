using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    class Character : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public float MoveSpeed { get; set; }
        public Direction Facing { get; private set; }

        private Dictionary<AnimationSlot, CharacterAnimation> animations = new();

        private Vector2Int targetPosition;
        private GameObject nameObject;

        [SerializeField] private CharacterHealthBar healthBars;
        [SerializeField] private GameObject healthBarsObject;

        public bool Moving { get { return (Vector2)transform.position != targetPosition; } }

        private BattleText battleText;
        private CharacterAnimation body;

        public void MakeCharacter(MakeCharacterPacket packet)
        {
            this.targetPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

            this.body = CreateAnimation(AnimationSlot.Body, "Body", packet.BodyId, ColorH.RGBA(packet.BodyR, packet.BodyG, packet.BodyB, packet.BodyA));

            CreateName(packet.Name, packet.Title, packet.Surname, body.Height, body.transform.localPosition.y);
            UpdateHealthBarPosition(body.Height, body.transform.localPosition.y);
            UpdateHPMP(packet.HPPercent, 100);

            if (packet.BodyId < 100)
            {
                var hair = CreateAnimation(AnimationSlot.Hair, "Hair", packet.HairId, ColorH.RGBA(packet.HairR, packet.HairG, packet.HairB, packet.HairA));

                SetUnderwear(packet.BodyId, packet.DisplayedEquipment);

                CreateAnimation(AnimationSlot.Face, "Eyes", packet.FaceId, Color.clear);
                CreateAnimation(AnimationSlot.Chest, "Chest", packet.DisplayedEquipment[0][0], ColorH.RGBA(packet.DisplayedEquipment[0]));
                CreateAnimation(AnimationSlot.Head, "Helm", packet.DisplayedEquipment[1][0], ColorH.RGBA(packet.DisplayedEquipment[1]));
                CreateAnimation(AnimationSlot.Legs, "Legs", packet.DisplayedEquipment[2][0], ColorH.RGBA(packet.DisplayedEquipment[2]));
                CreateAnimation(AnimationSlot.Feet, "Feet", packet.DisplayedEquipment[3][0], ColorH.RGBA(packet.DisplayedEquipment[3]));
                CreateAnimation(AnimationSlot.Shield, "Hand", packet.DisplayedEquipment[4][0], ColorH.RGBA(packet.DisplayedEquipment[4]));
                CreateAnimation(AnimationSlot.Weapon, "Hand", packet.DisplayedEquipment[5][0], ColorH.RGBA(packet.DisplayedEquipment[5]));
            }

            SetFacing(packet.Facing);

            var equipped = packet.BodyState == 3 ? 0 : 1;
            foreach (var animation in animations.Values)
                animation.SetFloat(Constants.Equipped, equipped);

            SetBodyState(packet.BodyState);

            this.MoveSpeed = packet.MoveSpeed;
            this.X = packet.MapX;
            this.Y = packet.MapY;

            this.battleText = GetComponentInChildren<BattleText>();
        }

        private void CreateName(string name, string title, string surname, int bodyHeight, float yOffset)
        {
            var textObject = new GameObject("Name Text");
            textObject.transform.SetParent(gameObject.transform);

            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 1);

            var text = textObject.AddComponent<TMPro.TextMeshPro>();
            text.text = $"{title} {name} {surname}".Trim();
            text.sortingLayerID = SortingLayer.NameToID(Constants.NamesLayer);
            text.fontSize = 2.5f;
            text.fontMaterial = Resources.Load<Material>("Materials/NameFont");

            var contentSizeFitter = textObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            contentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            text.transform.localPosition = new Vector3(0.5f, bodyHeight / 32f);

            this.nameObject = textObject;
        }

        private void UpdateHealthBarPosition(int bodyHeight, float yOffset)
        {
            healthBarsObject.transform.localPosition = new Vector3(0.5f, (bodyHeight - 13) / 32f);
        }

        private void SetUnderwear(int bodyId, int[][] equips)
        {
            // male
            if (bodyId == 1 && equips[2][0] == 0)
            {
                equips[2][0] = 3;
                return;
            }

            // female
            if (bodyId == 2)
            {
                if (equips[0][0] == 0)
                    equips[0][0] = 8;

                if (equips[2][0] == 0)
                    equips[2][0] = 4;
            }
        }

        private CharacterAnimation CreateAnimation(AnimationSlot slot, string type, int id, Color color)
        {
            if (id <= 0) return null;

            var animation = Instantiate(MapManager.CharacterAnimationPrefab, gameObject.transform);
            animation.name = $"{type} ({id})";

            var characterAnimationScript = animation.GetComponent<CharacterAnimation>();
            characterAnimationScript.SetGraphic(type, id);
            characterAnimationScript.SetColor(color);
            characterAnimationScript.SetSortOrder(GetSortOrder(slot, Facing));

            this.animations[slot] = characterAnimationScript;

            return characterAnimationScript;
        }

        private int GetSortOrder(AnimationSlot slot, Direction direction)
        {
            int order = (int)slot + 2;
            if (slot < AnimationSlot.Shield)
            {
                return order;
            }
            else if (slot == AnimationSlot.Shield)
            {
                return direction switch {
                    Direction.Right => 0,
                    Direction.Up => 0,
                    Direction.Down => order,
                    Direction.Left => order,
                };
            }

            return direction switch {
                Direction.Right => order,
                Direction.Up => 1,
                Direction.Down => order,
                Direction.Left => 0,
            };
        }

        public void SetFacing(Direction direction)
        {
            this.Facing = direction;

            foreach (var animation in animations.Values)
                animation.SetFloat(Constants.Direction, (int)direction);

            if (animations.TryGetValue(AnimationSlot.Weapon, out var weapon))
                weapon.SetSortOrder(GetSortOrder(AnimationSlot.Weapon, Facing));

            if (animations.TryGetValue(AnimationSlot.Shield, out var shield))
                shield.SetSortOrder(GetSortOrder(AnimationSlot.Shield, Facing));
        }

        private void SetMoving(bool moving)
        {
            foreach (var animation in animations.Values)
                animation.SetBool(Constants.Walking, moving);
        }

        public void Attack()
        {
            foreach (var animation in animations.Values)
                animation.TriggerAttack();
        }

        public void Cast()
        {
            foreach (var animation in animations.Values)
                animation.TriggerCast();
        }

        private void SetBodyState(int bodyState)
        {
            foreach (var animation in animations.Values)
                animation.SetFloat(Constants.BodyState, bodyState);
        }

        public void Move(int x, int y)
        {
            var map = GameManager.Instance.CurrentMap;
            this.targetPosition = new Vector2Int(x, map.Height - y);

            if (targetPosition.y < transform.position.y)
            {
                SetFacing(Direction.Down);
            }
            else if (targetPosition.x > transform.position.x)
            {
                SetFacing(Direction.Right);
            }
            else if (targetPosition.y > transform.position.y)
            {
                SetFacing(Direction.Up);
            }
            else if (targetPosition.x < transform.position.x)
            {
                SetFacing(Direction.Left);
            }

            SetMoving(true);
            this.X = x;
            this.Y = y;
        }

        public void SetPosition(int x, int y)
        {
            SetMoving(false);

            var map = GameManager.Instance.CurrentMap;
            this.targetPosition = new Vector2Int(x, map.Height - y);
            transform.position = (Vector3Int)this.targetPosition;
            this.X = x;
            this.Y = y;
        }

        public void Update()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (Moving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, 1000 / MoveSpeed * Time.deltaTime);

                if (!Moving)
                {
                    SetMoving(false);
                }
            }
        }

        public void UpdateHPMP(int hpPercent, int mpPercent)
        {
            this.healthBars.SetHPPercent(hpPercent);
            this.healthBars.SetMPPercent(mpPercent);
        }

        public void AddBattleText(BattleTextType textType, string text)
        {
            battleText.AddText(textType, text, this.body.Height);
        }
    }
}