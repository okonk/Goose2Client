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

        public bool Moving { get { return (Vector2)transform.position != targetPosition; } }

        public void MakeCharacter(MakeCharacterPacket packet)
        {
            this.targetPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

            var bodyObject = CreateAnimation(AnimationSlot.Body, "Body", packet.BodyId, ColorH.RGBA(packet.BodyR, packet.BodyG, packet.BodyB, packet.BodyA));

            if (packet.BodyId < 100)
            {
                var hair = CreateAnimation(AnimationSlot.Hair, "Hair", packet.HairId, ColorH.RGBA(packet.HairR, packet.HairG, packet.HairB, packet.HairA));
                Debug.Log($"height of {packet.Name} is {bodyObject.GetHeight()} hair is {hair.GetHeight()}");

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

            this.MoveSpeed = packet.MoveSpeed;
        }

        private CharacterAnimation CreateAnimation(AnimationSlot slot, string type, int id, Color color)
        {
            if (id <= 0) return null;

            var animation = Instantiate(MapManager.CharacterAnimationPrefab, gameObject.transform);
            animation.name = $"{type} ({id})";
            animation.transform.localPosition = new Vector3(0.5f, -0.5f);

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

        private void SetFacing(Direction direction)
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
    }
}