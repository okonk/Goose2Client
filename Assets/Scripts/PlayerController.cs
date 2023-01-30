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

        private float weaponSpeed = 1.0f;
        private bool attackPressed = false;
        private float attackDelayTime = 0;

        private void Start()
        {
            this.character = GetComponent<Character>();

            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = Resources.Load<InputActionAsset>("Input System/Controls");
            playerInput.actions.Enable();

            GameManager.Instance.PacketManager.Listen<WeaponSpeedPacket>(this.OnWeaponSpeed);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<WeaponSpeedPacket>(this.OnWeaponSpeed);
        }

        private void OnAttack(InputValue value)
        {
            attackPressed = value.isPressed;
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
                    GetMoveDelta(lastInput, out var direction, out int _, out int _);

                    SetFacing(direction);
                }

                movePressedTime = 0;
            }
            else
            {
                movePressed = true;
            }

            SetInputLastPressed();
        }

        private void Update()
        {
            MoveUpdate();
            AttackUpdate();
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

        private void MoveUpdate()
        {
            if (character.Moving || !movePressed)
                return;

            movePressedTime += Time.deltaTime;

            if (movePressedTime < 0.1) return;

            GetMoveDelta(moveInput, out var direction, out int dx, out int dy);

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
                SetFacing(direction);
            }
        }

        private void SetFacing(Direction direction)
        {
            character.SetFacing(direction);
            GameManager.Instance.NetworkClient.Face(direction);
        }

        private void GetMoveDelta(Vector2 input, out Direction direction, out int dx, out int dy)
        {
            dx = (int)Mathf.Round(input.x);
            dy = -1 * (int)Mathf.Round(input.y);

            direction = GetDirection(dx, dy);
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

        private void OnWeaponSpeed(object packet)
        {
            var weaponSpeedPacket = (WeaponSpeedPacket)packet;

            this.weaponSpeed = weaponSpeedPacket.Speed / 1000f;
        }

        private void AttackUpdate()
        {
            bool canAttack = attackDelayTime == 0;
            if (!attackPressed && canAttack)
                return;

            attackDelayTime += Time.deltaTime;

            if (attackDelayTime >= weaponSpeed)
                attackDelayTime = 0;

            if (attackPressed && canAttack)
            {
                character.Attack();
                GameManager.Instance.NetworkClient.Attack();
            }
        }
    }
}