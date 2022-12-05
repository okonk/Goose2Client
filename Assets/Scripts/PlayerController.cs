using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class PlayerController : MonoBehaviour
    {
        private Character character;
        private Vector2 moveInput;

        public MapManager MapManager { get; set; }

        private void Start()
        {
            this.character = GetComponent<Character>();

            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = Resources.Load<InputActionAsset>("Input System/Controls");
            playerInput.actions.Enable();
        }

        private void OnMove(InputValue value)
        {
            this.moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            if (character.Moving)
                return;

            int dx = (int)moveInput.x;
            int dy = dx == 0 ? -1 * (int)moveInput.y : 0;

            if (dx != 0 || dy != 0)
            {
                int x = dx + character.X;
                int y = dy + character.Y;

                if (!MapManager.IsValidMove(x, y))
                    return;

                this.character.Move(x, y);

                if (dx < 0)
                    GameManager.Instance.NetworkClient.Move(Direction.Left);
                else if (dx > 0)
                    GameManager.Instance.NetworkClient.Move(Direction.Right);
                else if (dy < 0)
                    GameManager.Instance.NetworkClient.Move(Direction.Up);
                else
                    GameManager.Instance.NetworkClient.Move(Direction.Down);
            }
        }
    }
}