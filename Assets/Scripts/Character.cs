using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    class Character : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Facing { get; private set; }

        private Dictionary<AnimationSlot, CharacterAnimation> animations = new();

        public void MakeCharacter(MakeCharacterPacket packet)
        {
            this.X = packet.MapX;
            this.Y = packet.MapY;

            CreateAnimation(AnimationSlot.Body, "Body", packet.BodyId);

            if (packet.BodyId < 100)
            {
                CreateAnimation(AnimationSlot.Hair, "Hair", packet.HairId);
                CreateAnimation(AnimationSlot.Face, "Eyes", packet.FaceId);
                CreateAnimation(AnimationSlot.Chest, "Chest", packet.DisplayedEquipment[0][0]);
                CreateAnimation(AnimationSlot.Head, "Helm", packet.DisplayedEquipment[1][0]);
                CreateAnimation(AnimationSlot.Legs, "Legs", packet.DisplayedEquipment[2][0]);
                CreateAnimation(AnimationSlot.Feet, "Feet", packet.DisplayedEquipment[3][0]);
                CreateAnimation(AnimationSlot.Shield, "Hand", packet.DisplayedEquipment[4][0]);
                CreateAnimation(AnimationSlot.Weapon, "Hand", packet.DisplayedEquipment[5][0]);
            }

            SetFacing(packet.Facing);
        }

        private void CreateAnimation(AnimationSlot slot, string type, int id)
        {
            if (id <= 0) return;

            var animationPrefab = Resources.Load<GameObject>("Prefabs/CharacterAnimation");
            var animation = Instantiate(animationPrefab, gameObject.transform);
            animation.name = type;

            var characterAnimationScript = animation.GetComponent<CharacterAnimation>();
            characterAnimationScript.SetGraphic(type, id.ToString());
            characterAnimationScript.SetSortOrder(1 + (int)slot);

            this.animations[slot] = characterAnimationScript;
        }

        private void SetFacing(Direction direction)
        {
            this.Facing = direction;

            foreach (var animation in animations.Values)
                animation.SetFloat(Constants.Direction, (int)direction);
        }

        public void Move(int x, int y)
        {
            if (y < Y)
            {
                SetFacing(Direction.Up);
            }
            else if (x > X)
            {
                SetFacing(Direction.Right);
            }
            else if (y > Y)
            {
                SetFacing(Direction.Down);
            }
            else if (x < X)
            {
                SetFacing(Direction.Left);
            }

            this.X = x;
            this.Y = y;

            var map = GameManager.Instance.CurrentMap;
            var position = new Vector3(x + 0.5f, map.Height - y - 1);

            this.transform.position = position;
        }
    }
}