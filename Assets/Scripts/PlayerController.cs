using System;
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

        private bool movePressed = false;
        private bool wasMovingVertical = false;
        private float movePressedTime = 0;

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
            var lastInput = moveInput;

            moveInput = value.Get<Vector2>();
            if (moveInput == Vector2.zero)
            {
                movePressed = false;

                // allow spinning in place
                if (movePressedTime < 0.1)
                {
                    int dx = (int)Mathf.Round(lastInput.x);
                    int dy = -1 * (int)Mathf.Round(lastInput.y);

                    var direction = GetDirection(dx, dy);

                    character.SetFacing(direction);
                    GameManager.Instance.NetworkClient.Face(direction);
                }
            }
            else
            {
                movePressed = true;
                movePressedTime = 0;
            }

            SetInputLastPressed();
        }

        private void SetInputLastPressed()
        {
            bool isMovingHorizontal = Mathf.Abs(moveInput.x) > 0.5f;
            bool isMovingVertical = Mathf.Abs(moveInput.y) > 0.5f;

            if (isMovingVertical && isMovingHorizontal)
            {
                if (wasMovingVertical)
                    moveInput.y = 0;
                else
                    moveInput.x = 0;
            }
            else if (isMovingHorizontal)
            {
                moveInput.y = 0;
                wasMovingVertical = false;
            }
            else if (isMovingVertical)
            {
                moveInput.x = 0;
                wasMovingVertical = true;
            }
        }

        private void Update()
        {
            if (character.Moving || !movePressed)
                return;

            movePressedTime += Time.deltaTime;

            if (movePressedTime < 0.1) return;

            int dx = (int)Mathf.Round(moveInput.x);
            int dy = -1 * (int)Mathf.Round(moveInput.y);

            var direction = GetDirection(dx, dy);

            int x = dx + character.X;
            int y = dy + character.Y;

            if (MapManager.IsValidMove(x, y))
            {
                MapManager.PlayerMoved(character.X, character.Y, x, y);

                this.character.Move(x, y);

                GameManager.Instance.NetworkClient.Move(direction);
            }
            else
            {
                character.SetFacing(direction);
                GameManager.Instance.NetworkClient.Face(direction);
            }
        }

        private Direction GetDirection(int dx, int dy)
        {
            if (dx < 0)
                return Direction.Left;
            else if (dx > 0)
                return Direction.Right;
            else if (dy < 0)
                return Direction.Up;
            else
                return Direction.Down;
        }
    }
}