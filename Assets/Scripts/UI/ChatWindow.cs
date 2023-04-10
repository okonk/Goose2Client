using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goose2Client
{
    public class ChatWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Transform chatContainer;

        [SerializeField] private TMP_InputField inputField;

        [SerializeField] private Image panel;

        [SerializeField] private Image scrollBarHandle;
        [SerializeField] private Image scrollBarBackground;

        [SerializeField] private ScrollRect scrollRect;

        private Dictionary<ChatType, Color> chatTypeToColor = new();

        public bool Typing { get { return inputField.isFocused; } }

        private string replyToName = null;

        private Dictionary<string, string> commandAliases = new();
        public Dictionary<string, Action<string, string>> CommandHandlers { get; set; } = new();
        private List<string> inputHistory = new();
        private int inputHistoryIndex = 0;

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<ChatPacket>(this.OnChat);
            GameManager.Instance.PacketManager.Listen<HashMessagePacket>(this.OnHashMessage);
            GameManager.Instance.PacketManager.Listen<ServerMessagePacket>(this.OnServerMessage);
            GameManager.Instance.PacketManager.Listen<TellPacket>(this.OnTell);

            SetAlpha(0.7f);

            GameManager.Instance.ChatWindow = this;

            chatTypeToColor[ChatType.Chat] = Colors.White;
            chatTypeToColor[ChatType.Guild] = Colors.Yellow;
            chatTypeToColor[ChatType.Group] = Colors.Yellow;
            chatTypeToColor[ChatType.Melee] = Colors.Red;
            chatTypeToColor[ChatType.Spells] = Colors.Blue;
            chatTypeToColor[ChatType.Tell] = Colors.Yellow;
            chatTypeToColor[ChatType.Server] = Colors.Green;

            CommandHandlers["/quit"] = OnQuitCommand;

            commandAliases["/t"] = "/tell";
            commandAliases["/ga"] = "/groupadd";
            commandAliases["/gr"] = "/groupremove";
            commandAliases["/gu"] = "/guild";
            commandAliases["/g"] = "/group";
            commandAliases["/"] = "/who";
            commandAliases["/r"] = "/random 1000";
            commandAliases["/h"] = "Hello there!";

            PlayerInputManager.Instance.StartChat = (i) => ChatFocused("");
            PlayerInputManager.Instance.SlashCommand = (i) => ChatFocused("/");
            PlayerInputManager.Instance.GuildCommand = (i) => ChatFocused("/guild ");
            PlayerInputManager.Instance.TellCommand = (i) => ChatFocused("/tell ");
            PlayerInputManager.Instance.ReplyCommand = (i) => ChatFocused($"/tell {(replyToName == null ? "" : replyToName + " ")}");
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<ChatPacket>(this.OnChat);
            GameManager.Instance.PacketManager.Remove<HashMessagePacket>(this.OnHashMessage);
            GameManager.Instance.PacketManager.Remove<ServerMessagePacket>(this.OnServerMessage);
            GameManager.Instance.PacketManager.Remove<TellPacket>(this.OnTell);
        }

        private void OnChat(object packetObj)
        {
            var packet = (ChatPacket)packetObj;

            AddChatLine(packet.Message, ChatType.Chat);
        }

        private void OnHashMessage(object packetObj)
        {
            var packet = (HashMessagePacket)packetObj;

            AddChatLine(packet.Message, ChatType.Chat);
        }

        private void OnServerMessage(object packetObj)
        {
            var packet = (ServerMessagePacket)packetObj;

            AddChatLine(packet.Message, packet.ChatType);
        }

        private void OnTell(object packetObj)
        {
            var packet = (TellPacket)packetObj;

            replyToName = packet.Name;

            AddChatLine($"[tell from] {packet.Name}: {packet.Message}", ChatType.Tell);
        }

        public void AddChatLine(string message, ChatType chatType)
        {
            var oldScrollValue = scrollRect.verticalNormalizedPosition;

            var prefab = Resources.Load<GameObject>("Prefabs/UI/ChatText");
            var chatText = Instantiate(prefab, chatContainer);

            message = message.Replace('`', '\u2665');

            var tmp = chatText.GetComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.color = chatTypeToColor[chatType];

            if (oldScrollValue == 0)
                ApplyScrollPosition(chatText, 0);
        }

        private void ApplyScrollPosition(GameObject chatText, float verticalPos)
        {
            Canvas.ForceUpdateCanvases();

            chatText.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            scrollRect.verticalNormalizedPosition = verticalPos;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetAlpha(1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetAlpha(0.7f);
        }

        private void SetAlpha(float value)
        {
            var color = new Color(1, 1, 1, value);
            panel.color = color;
            inputField.GetComponent<Image>().color = color;

            var scrollColor = new Color(1, 1, 1, (value == 1 ? 1f : 0.2f));
            scrollBarHandle.color = scrollColor;
            scrollBarBackground.color = scrollColor;
        }

        public void ChatSubmitted()
        {
            var enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
            if (!enterPressed)
            {
                ClearAndRemoveFocus();
                return;
            }

            string message = inputField.text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message[0] == '/')
                    HandleCommand(message);
                else
                    GameManager.Instance.NetworkClient.ChatMessage(message);

                if (inputHistory.LastOrDefault() != message)
                    inputHistory.Add(message);

                inputHistoryIndex = inputHistory.Count;
            }

            ClearAndRemoveFocus();
        }

        private void ChatEscapePressed()
        {
            ClearAndRemoveFocus();
            inputHistoryIndex = inputHistory.Count;
        }

        private void ClearAndRemoveFocus()
        {
            inputField.text = "";

            var eventSystem = EventSystem.current;
            if (!eventSystem.alreadySelecting)
                eventSystem.SetSelectedGameObject(null);

            PlayerInputManager.Instance.SwitchToMapping("Player");
        }

        public void ChatUpPressed()
        {
            if (inputHistory.Count == 0) return;

            inputHistoryIndex = Math.Max(0, inputHistoryIndex - 1);
            inputField.text = inputHistory[inputHistoryIndex];
        }

        public void ChatDownPressed()
        {
            if (inputHistory.Count == 0) return;

            inputHistoryIndex++;

            if (inputHistoryIndex < inputHistory.Count)
            {
                inputField.text = inputHistory[inputHistoryIndex];
            }
            else
            {
                inputField.text = "";
                inputHistoryIndex = inputHistory.Count;
            }
        }

        private void HandleCommand(string commandText)
        {
            int space = commandText.IndexOf(' ');

            string command = commandText;
            string arguments = null;
            if (space != -1)
            {
                command = commandText.Substring(0, space);
                arguments = commandText.Substring(space + 1);
            }

            if (commandAliases.TryGetValue(command.ToLowerInvariant(), out string replacedCommand))
                command = replacedCommand;

            if (command[0] == '/' && CommandHandlers.TryGetValue(command.ToLowerInvariant(), out Action<string, string> action))
            {
                action(command, arguments);
            }
            else
            {
                string fullCommand = $"{command} {arguments}".TrimEnd();
                if (fullCommand[0] == '/')
                    GameManager.Instance.NetworkClient.Command(fullCommand);
                else
                    GameManager.Instance.NetworkClient.ChatMessage(fullCommand);
            }
        }

        private void OnQuitCommand(string command, string arguments)
        {
            GameManager.Instance.Quit();
        }

        private void ChatFocused(string text)
        {
            inputField.text = text;
            inputField.caretPosition = inputField.text.Length;
            inputField.ActivateInputField();

            PlayerInputManager.Instance.SwitchToMapping("UI");
        }

        public void Update()
        {
            if (!Typing) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ChatEscapePressed();

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                ChatUpPressed();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                ChatDownPressed();
        }

        public void OnSelect()
        {
            ChatFocused("");
        }
    }
}