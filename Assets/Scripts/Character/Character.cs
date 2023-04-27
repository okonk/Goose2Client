using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class Character : MonoBehaviour
    {
        public int LoginId { get; private set; }
        public CharacterType CharacterType { get; private set; }
        public string Name { get; private set; }
        public string Title { get; private set; }
        public string Surname { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public float MoveSpeed { get; set; }
        public Direction Facing { get; private set; }

        public float HPPercent { get; private set; }
        public float MPPercent { get; private set; }

        public string FullName { get { return $"{Title} {Name} {Surname}".Trim(); } }

        public int Height => body.Height;

        public float BodyX => body.transform.localPosition.x;
        public float BodyY => body.transform.localPosition.y;

        public bool IsMounted => animations.ContainsKey(AnimationSlot.Mount);

        private Dictionary<AnimationSlot, CharacterAnimation> animations = new();

        private Vector2 targetPosition;
        private GameObject nameObject;

        [SerializeField] private CharacterHealthBar healthBars;
        [SerializeField] private GameObject healthBarsObject;
        [SerializeField] private GameObject spriteContainer;

        public bool Moving { get { return (Vector2)transform.position != targetPosition; } }

        private BattleText battleText;
        private CharacterAnimation body;

        public void MakeCharacter(MakeCharacterPacket packet)
        {
            this.targetPosition = new Vector2(transform.position.x, transform.position.y);

            this.body = CreateAnimation(AnimationSlot.Body, "Body", packet.BodyId, ColorH.RGBA(packet.BodyR, packet.BodyG, packet.BodyB, packet.BodyA));
            this.body.gameObject.AddComponent<BoxCollider2D>();
            this.body.gameObject.AddComponent<CharacterClickHandler>();

            CreateName(packet.Name, packet.Title, packet.Surname, body.Height, body.transform.localPosition.y);
            UpdateHealthBarPosition(body.Height, body.transform.localPosition.y);
            UpdateHPMP(packet.HPPercent, 1);

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
                CreateAnimation(AnimationSlot.Mount, "Body", packet.DisplayedEquipment[6][0], ColorH.RGBA(packet.DisplayedEquipment[6]));
            }

            SetFacing(packet.Facing);

            bool mounted = packet.DisplayedEquipment != null && packet.DisplayedEquipment[6][0] != 0;
            var equipped = packet.BodyState == 3 ? 0 : 1;
            foreach (var kvp in animations)
            {
                var animation = kvp.Value;

                if (kvp.Key == AnimationSlot.Mount)
                {
                    animation.SetFloat(Constants.Equipped, 0);
                    animation.SetBool(Constants.Mounted, false);
                    animation.SetFloat(Constants.BodyState, 3);
                }
                else
                {
                    animation.SetFloat(Constants.Equipped, equipped);
                    animation.SetBool(Constants.Mounted, mounted);
                    animation.SetFloat(Constants.BodyState, packet.BodyState);
                }
            }

            this.LoginId = packet.LoginId;
            this.CharacterType = packet.CharacterType;
            this.MoveSpeed = packet.MoveSpeed;
            this.X = packet.MapX;
            this.Y = packet.MapY;
            this.Name = packet.Name;
            this.Title = packet.Title;
            this.Surname = packet.Surname;

            this.battleText = GetComponentInChildren<BattleText>();
        }

        public void UpdateCharacter(UpdateCharacterPacket packet)
        {
            UpdateAnimation(AnimationSlot.Body, "Body", packet.BodyId, ColorH.RGBA(packet.BodyR, packet.BodyG, packet.BodyB, packet.BodyA));

            if (packet.BodyId < 100)
            {
                UpdateAnimation(AnimationSlot.Hair, "Hair", packet.HairId, ColorH.RGBA(packet.HairR, packet.HairG, packet.HairB, packet.HairA));

                SetUnderwear(packet.BodyId, packet.DisplayedEquipment);

                UpdateAnimation(AnimationSlot.Face, "Eyes", packet.FaceId, Color.clear);
                UpdateAnimation(AnimationSlot.Chest, "Chest", packet.DisplayedEquipment[0][0], ColorH.RGBA(packet.DisplayedEquipment[0]));
                UpdateAnimation(AnimationSlot.Head, "Helm", packet.DisplayedEquipment[1][0], ColorH.RGBA(packet.DisplayedEquipment[1]));
                UpdateAnimation(AnimationSlot.Legs, "Legs", packet.DisplayedEquipment[2][0], ColorH.RGBA(packet.DisplayedEquipment[2]));
                UpdateAnimation(AnimationSlot.Feet, "Feet", packet.DisplayedEquipment[3][0], ColorH.RGBA(packet.DisplayedEquipment[3]));
                UpdateAnimation(AnimationSlot.Shield, "Hand", packet.DisplayedEquipment[4][0], ColorH.RGBA(packet.DisplayedEquipment[4]));
                UpdateAnimation(AnimationSlot.Weapon, "Hand", packet.DisplayedEquipment[5][0], ColorH.RGBA(packet.DisplayedEquipment[5]));
                UpdateAnimation(AnimationSlot.Mount, "Body", packet.DisplayedEquipment[6][0], ColorH.RGBA(packet.DisplayedEquipment[6]));
            }
            else
            {
                DestroyAnimation(AnimationSlot.Hair);
                DestroyAnimation(AnimationSlot.Face);
                DestroyAnimation(AnimationSlot.Chest);
                DestroyAnimation(AnimationSlot.Head);
                DestroyAnimation(AnimationSlot.Legs);
                DestroyAnimation(AnimationSlot.Feet);
                DestroyAnimation(AnimationSlot.Shield);
                DestroyAnimation(AnimationSlot.Weapon);
                DestroyAnimation(AnimationSlot.Mount);
            }

            SetFacing(Facing);

            bool mounted = packet.DisplayedEquipment != null && packet.DisplayedEquipment[6][0] != 0;
            var equipped = packet.BodyState == 3 ? 0 : 1;
            foreach (var kvp in animations)
            {
                var animation = kvp.Value;

                if (kvp.Key == AnimationSlot.Mount)
                {
                    animation.SetFloat(Constants.Equipped, 0);
                    animation.SetBool(Constants.Mounted, false);
                    animation.SetFloat(Constants.BodyState, 3);
                }
                else
                {
                    animation.SetFloat(Constants.Equipped, equipped);
                    animation.SetBool(Constants.Mounted, mounted);
                    animation.SetFloat(Constants.BodyState, packet.BodyState);
                }
            }

            this.MoveSpeed = packet.MoveSpeed;
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
            text.fontMaterial = ResourceManager.Load<Material>("Materials/NameFont");

            var contentSizeFitter = textObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            contentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            text.transform.localPosition = new Vector3(0, bodyHeight / 32f);

            this.nameObject = textObject;
        }

        private void UpdateHealthBarPosition(int bodyHeight, float yOffset)
        {
            healthBarsObject.transform.localPosition = new Vector3(0, (bodyHeight - 13) / 32f);
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
            if (bodyId == 11)
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

            var characterAnimationPrefab = ResourceManager.LoadFromBundle<GameObject>("prefabs", "CharacterAnimation");
            var animation = Instantiate(characterAnimationPrefab, spriteContainer.transform);
            animation.name = $"{type} ({id})";

            var characterAnimationScript = animation.GetComponent<CharacterAnimation>();
            characterAnimationScript.SetGraphic(type, id);
            characterAnimationScript.SetColor(color);
            characterAnimationScript.SetSortOrder(GetSortOrder(slot, Facing));

            this.animations[slot] = characterAnimationScript;

            return characterAnimationScript;
        }

        private void UpdateAnimation(AnimationSlot slot, string type, int id, Color color)
        {
            if (!this.animations.TryGetValue(slot, out var animation))
            {
                CreateAnimation(slot, type, id, color);
                return;
            }

            if (id <= 0)
            {
                this.animations.Remove(slot);
                Destroy(animation.gameObject);
                return;
            }

            if (animation.Id == id && animation.Color == color)
                return;

            animation.SetGraphic(type, id);
            animation.SetColor(color);
            animation.SetSortOrder(GetSortOrder(slot, Facing));
        }

        private void DestroyAnimation(AnimationSlot slot)
        {
            if (!this.animations.TryGetValue(slot, out var animation))
                return;

            this.animations.Remove(slot);
            Destroy(animation.gameObject);
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

        private void StopAnimations()
        {
            foreach (var animation in animations.Values)
                animation.Stop();
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

        public void Move(int x, int y)
        {
            var map = GameManager.Instance.CurrentMap;
            var lastTarget = targetPosition;
            transform.position = lastTarget;
            targetPosition = new Vector2(x + 0.5f, map.Height - y);

            var newFacing = Direction.Up;
            if (targetPosition.y < lastTarget.y)
                newFacing = Direction.Down;
            else if (targetPosition.x > lastTarget.x)
                newFacing = Direction.Right;
            else if (targetPosition.y > lastTarget.y)
                newFacing = Direction.Up;
            else if (targetPosition.x < lastTarget.x)
                newFacing = Direction.Left;

            if (Facing != newFacing)
                SetFacing(newFacing);

            SetMoving(true);
            this.X = x;
            this.Y = y;
        }

        public void SetPosition(int x, int y)
        {
            SetMoving(false);

            var map = GameManager.Instance.CurrentMap;
            targetPosition = new Vector2(x + 0.5f, map.Height - y);
            transform.position = targetPosition;
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
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, 1000 / MoveSpeed * Time.unscaledDeltaTime);

                if (!Moving)
                {
                    SetMoving(false);
                }
            }
        }

        public void UpdateHPMP(float hpPercent, float mpPercent)
        {
            this.healthBars.SetHPPercent(hpPercent);
            this.healthBars.SetMPPercent(mpPercent);

            this.HPPercent = hpPercent;
            this.MPPercent = mpPercent;
        }

        public void AddBattleText(BattleTextType textType, string text)
        {
            battleText.AddText(textType, text, this.body.Height);
        }
    }
}