using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Goose2Client
{
    public class LoginButton : MonoBehaviour
    {
        [SerializeField] private GameObject nameInput;
        [SerializeField] private GameObject passwordInput;

        [SerializeField] private GameObject messageOverlay;

        public void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                var selectables = Selectable.allSelectablesArray;
                for (int i = 0; i < selectables.Length; i++)
                {
                    if (selectables[i].gameObject == EventSystem.current.currentSelectedGameObject)
                    {
                        var nextSelectable = selectables[(i + 1) % selectables.Length];
                        nextSelectable.Select();
                        break;
                    }
                }
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                if (EventSystem.current.currentSelectedGameObject == passwordInput)
                    OnLoginClicked();
            }
        }

        public void Start()
        {
            GameManager.Instance.PacketManager.Listen<LoginSuccessPacket>(this.OnLoginSuccess);
            GameManager.Instance.PacketManager.Listen<LoginFailPacket>(this.OnLoginFail);
            GameManager.Instance.PacketManager.Listen<SendCurrentMapPacket>(this.OnSendCurrentMap);

            GameManager.Instance.NetworkClient.Connected += this.OnConnected;
            GameManager.Instance.NetworkClient.SocketError += this.OnError;
            GameManager.Instance.NetworkClient.ConnectionError += this.OnError;

            nameInput.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("CharacterName", "");
            passwordInput.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("CharacterPassword", "");

            messageOverlay.SetActive(false);
        }

        public void OnDestroy()
        {
            GameManager.Instance.NetworkClient.Connected -= this.OnConnected;
            GameManager.Instance.NetworkClient.SocketError -= this.OnError;
            GameManager.Instance.NetworkClient.ConnectionError -= this.OnError;

            GameManager.Instance.PacketManager.Remove<LoginSuccessPacket>(this.OnLoginSuccess);
            GameManager.Instance.PacketManager.Remove<LoginFailPacket>(this.OnLoginFail);
            GameManager.Instance.PacketManager.Remove<SendCurrentMapPacket>(this.OnSendCurrentMap);
        }

        private string GetName() => nameInput.GetComponent<TMP_InputField>().text;
        private string GetPassword() => passwordInput.GetComponent<TMP_InputField>().text;

        private void SetMessage(string message)
        {
            messageOverlay.SetActive(true);

            var messageComponent = messageOverlay.GetComponentInChildren<TMP_Text>();
            messageComponent.text = message;
        }

        private void SetCloseButtonInteractable(bool interactable)
        {
            var button = messageOverlay.GetComponentInChildren<UnityEngine.UI.Button>();
            button.interactable = interactable;
        }

        public void OnLoginClicked()
        {
            string name = GetName();
            string password = GetPassword();

            if (name.Length <= 3 || password.Length <= 3)
                return;

            this.SetMessage("Connecting...");
            this.SetCloseButtonInteractable(false);

            GameManager.Instance.NetworkClient.Connect("game.illutia.net", 2006);
        }

        public void OnMessageCloseClicked()
        {
            messageOverlay.SetActive(false);
        }

        private void OnConnected()
        {
            Debug.Log("Connected");

            string name = GetName();
            string password = GetPassword();

            GameManager.Instance.NetworkClient.Login(name, password);
        }

        private void OnLoginSuccess(object packet)
        {
            Debug.Log("Login success");

            this.SetMessage("Connected!");

            var name = GetName();
            PlayerPrefs.SetString("CharacterName", name);
            PlayerPrefs.SetString("CharacterPassword", GetPassword());

            GameManager.Instance.LoadSettings(name);

            GameManager.Instance.NetworkClient.LoginContinued();
        }

        private void OnLoginFail(object packet)
        {
            var failedPacket = (LoginFailPacket)packet;

            Debug.Log($"Login failed: {failedPacket.Message}");

            this.SetMessage(failedPacket.Message);
            this.SetCloseButtonInteractable(true);
        }

        private void OnError(Exception e)
        {
            Debug.Log($"Connecting failed: {e}");

            this.SetMessage(e.Message);
            this.SetCloseButtonInteractable(true);
        }

        private void OnSendCurrentMap(object packet)
        {
            var sendCurrentMap = (SendCurrentMapPacket)packet;
            GameManager.Instance.ChangeMap(sendCurrentMap.MapFileName, sendCurrentMap.MapName);
        }
    }
}