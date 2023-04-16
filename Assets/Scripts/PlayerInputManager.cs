using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput playerInput;

        private static PlayerInputManager instance;

        public static PlayerInputManager Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();

            for (int i = 0; i < 10; i++)
            {
                int index = i - 1;
                if (index == -1) index = 9;

                var hotkeyAction = playerInput.actions.FindAction($"Hotkey{i}");
                hotkeyAction.performed += (c) => Hotkey?.Invoke(index, true);
                hotkeyAction.canceled += (c) => Hotkey?.Invoke(index, false);
            }

            var attackAction = playerInput.actions.FindAction("Attack");
            attackAction.performed += (c) => Attack?.Invoke(true);
            attackAction.canceled += (c) => Attack?.Invoke(false);
        }

        public void SwitchToMapping(string mapping)
        {
            playerInput.SwitchCurrentActionMap(mapping);
        }

        public Action<bool> Attack { get; set; }
        public Action<InputValue> Move { get; set; }
        public Action<InputValue> PickUp { get; set; }
        public Action<int, bool> Hotkey { get; set; }
        public Action<InputValue> ToggleSpellbook { get; set; }
        public Action<InputValue> ToggleInventory { get; set; }
        public Action<InputValue> ToggleCharacterWindow { get; set; }

        public Action<InputValue> TargetUp { get; set; }
        public Action<InputValue> TargetDown { get; set; }
        public Action<InputValue> TargetHome { get; set; }
        public Action<InputValue> ConfirmTarget { get; set; }
        public Action<InputValue> CancelTarget { get; set; }

        public Action<InputValue> StartChat { get; set; }
        public Action<InputValue> GuildCommand { get; set; }
        public Action<InputValue> SlashCommand { get; set; }
        public Action<InputValue> TellCommand { get; set; }
        public Action<InputValue> ReplyCommand { get; set; }

        private void OnMove(InputValue value)
            => Move?.Invoke(value);

        private void OnPickUp(InputValue value)
            => PickUp?.Invoke(value);

        private void OnToggleSpellbook(InputValue value)
            => ToggleSpellbook?.Invoke(value);

        private void OnToggleInventory(InputValue value)
            => ToggleInventory?.Invoke(value);

        private void OnToggleCharacterWindow(InputValue value)
            => ToggleCharacterWindow?.Invoke(value);

        private void OnTargetUp(InputValue value)
            => TargetUp?.Invoke(value);
        private void OnTargetDown(InputValue value)
            => TargetDown?.Invoke(value);
        private void OnTargetHome(InputValue value)
            => TargetHome?.Invoke(value);
        private void OnConfirmTarget(InputValue value)
            => ConfirmTarget?.Invoke(value);
        private void OnCancelTarget(InputValue value)
            => CancelTarget?.Invoke(value);

        private void OnStartChat(InputValue value)
            => StartChat?.Invoke(value);
        private void OnGuildCommand(InputValue value)
            => GuildCommand?.Invoke(value);
        private void OnSlashCommand(InputValue value)
            => SlashCommand?.Invoke(value);
        private void OnTellCommand(InputValue value)
            => TellCommand?.Invoke(value);
        private void OnReplyCommand(InputValue value)
            => ReplyCommand?.Invoke(value);
    }
}