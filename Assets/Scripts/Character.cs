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

        private Animator animator;

        public void MakeCharacter(MakeCharacterPacket packet)
        {
            this.animator = GetComponent<Animator>();

            this.X = packet.MapX;
            this.Y = packet.MapY;

            SetFacing(packet.Facing);


        }

        private void SetFacing(Direction direction)
        {
            this.Facing = direction;

            this.animator.SetFloat(Constants.Direction, (int)direction);
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