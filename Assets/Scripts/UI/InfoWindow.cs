using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InfoWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI[] lines;

        public int WindowId { get; private set; }
        public WindowFrames WindowFrame => WindowFrames.GenericInfo;

        public int NpcId { get; private set; }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Listen<WindowLinePacket>(this.OnWindowLine);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Remove<WindowLinePacket>(this.OnWindowLine);
        }

        public void OnMakeWindow(MakeWindowPacket packet)
        {
            NpcId = packet.NpcId;
            titleText.text = packet.Title;
            WindowId = packet.WindowId;
        }

        private void OnEndWindow(object packetObj)
        {
            var packet = (EndWindowPacket)packetObj;

            if (packet.WindowId != this.WindowId) return;

            panel.SetActive(true);

            var titleBar = GetComponentInChildren<TitleBar>();
            titleBar.canvas = GetComponentInParent<Canvas>();
        }

        private void OnWindowLine(object packetObj)
        {
            var packet = (WindowLinePacket)packetObj;

            if (packet.WindowId != this.WindowId) return;

            lines[packet.LineNumber].text = packet.Text + " ";
        }

        public void CloseWindow()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Close, this.WindowId, NpcId);
            panel.SetActive(false);

            Destroy(gameObject);
        }
    }
}