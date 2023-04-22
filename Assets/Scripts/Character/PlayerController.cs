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

        private bool attackPressed = false;
        private float attackDelayTime = 0;

        private void Start()
        {
            this.character = GetComponent<Character>();

            PlayerInputManager.Instance.Attack = OnAttack;
            PlayerInputManager.Instance.Move = OnMove;
            PlayerInputManager.Instance.PickUp = OnPickUp;
            PlayerInputManager.Instance.RefreshPosition = OnRefreshPosition;

            PlayerInputManager.Instance.EmoteHeart = i => OnEmote(1080, 8);
            PlayerInputManager.Instance.EmoteQuestion = i => OnEmote(1081, 8);
            PlayerInputManager.Instance.EmoteTrash = i => OnEmote(1082, 8);
            PlayerInputManager.Instance.EmoteDots = i => OnEmote(1083, 8);
            PlayerInputManager.Instance.EmotePoop = i => OnEmote(1084, 9);
            PlayerInputManager.Instance.EmoteSurprised = i => OnEmote(1085, 9);
            PlayerInputManager.Instance.EmoteSleep = i => OnEmote(1086, 9);
            PlayerInputManager.Instance.EmoteAnnoyed = i => OnEmote(1087, 9);
            PlayerInputManager.Instance.EmoteSweat = i => OnEmote(1088, 10);
            PlayerInputManager.Instance.EmoteMusic = i => OnEmote(1089, 10);
            PlayerInputManager.Instance.EmoteDollar = i => OnEmote(1090, 10);
            PlayerInputManager.Instance.EmoteWink = i => OnEmote(1091, 10);
        }

        private void OnEmote(int animationId, int graphicId)
        {
            GameManager.Instance.NetworkClient.Emote(animationId, graphicId);
        }

        private void OnRefreshPosition(InputValue input)
        {
            Debug.Log("Refresh position");
            GameManager.Instance.NetworkClient.Command("/refresh");
        }

        private void OnAttack(bool pressed)
        {
            attackPressed = pressed;
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

        private void OnPickUp(InputValue value)
        {
            GameManager.Instance.NetworkClient.Pickup();
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

        private void AttackUpdate()
        {
            bool canAttack = attackDelayTime == 0;
            if (!attackPressed && canAttack)
                return;

            attackDelayTime += Time.deltaTime;

            if (attackDelayTime >= MapManager.WeaponSpeed)
                attackDelayTime = 0;

            if (attackPressed && canAttack && !character.IsMounted)
            {
                character.Attack();
                GameManager.Instance.NetworkClient.Attack();
            }
        }
    }
}