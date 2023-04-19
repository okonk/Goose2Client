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
        public Action<InputValue> ToggleMount { get; set; }

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

        public Action<InputValue> EmoteHeart { get; set; }
        public Action<InputValue> EmoteQuestion { get; set; }
        public Action<InputValue> EmoteDots { get; set; }
        public Action<InputValue> EmotePoop { get; set; }
        public Action<InputValue> EmoteSurprised { get; set; }
        public Action<InputValue> EmoteSleep { get; set; }
        public Action<InputValue> EmoteAnnoyed { get; set; }
        public Action<InputValue> EmoteSweat { get; set; }
        public Action<InputValue> EmoteMusic { get; set; }
        public Action<InputValue> EmoteWink { get; set; }
        public Action<InputValue> EmoteTrash { get; set; }
        public Action<InputValue> EmoteDollar { get; set; }

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
        private void OnToggleMount(InputValue value)
            => ToggleMount?.Invoke(value);

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

        private void OnEmoteHeart(InputValue value)
            => EmoteHeart?.Invoke(value);
        private void OnEmoteQuestion(InputValue value)
            => EmoteQuestion?.Invoke(value);
        private void OnEmoteDots(InputValue value)
            => EmoteDots?.Invoke(value);
        private void OnEmotePoop(InputValue value)
            => EmotePoop?.Invoke(value);
        private void OnEmoteSurprised(InputValue value)
            => EmoteSurprised?.Invoke(value);
        private void OnEmoteSleep(InputValue value)
            => EmoteSleep?.Invoke(value);
        private void OnEmoteAnnoyed(InputValue value)
            => EmoteAnnoyed?.Invoke(value);
        private void OnEmoteSweat(InputValue value)
            => EmoteSweat?.Invoke(value);
        private void OnEmoteMusic(InputValue value)
            => EmoteMusic?.Invoke(value);
        private void OnEmoteWink(InputValue value)
            => EmoteWink?.Invoke(value);
        private void OnEmoteTrash(InputValue value)
            => EmoteTrash?.Invoke(value);
        private void OnEmoteDollar(InputValue value)
            => EmoteDollar?.Invoke(value);
    }
}