using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public abstract class BaseMultipleWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI[] lines;

        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject nextButton;
        [SerializeField] private GameObject closeButton;

        public Action<BaseMultipleWindow> OnCloseWindow { get; set; }

        public Queue<PacketHandler> packetBuffer = new();

        public int WindowId { get; private set; }
        public abstract WindowFrames WindowFrame { get; }

        public int NpcId { get; private set; }

        public void OnMakeWindow(MakeWindowPacket packet)
        {
            NpcId = packet.NpcId;
            titleText.text = packet.Title;
            WindowId = packet.WindowId;

            backButton?.SetActive(packet.Buttons[(int)WindowButtons.Back - 1]);
            nextButton?.SetActive(packet.Buttons[(int)WindowButtons.Next - 1]);
            closeButton?.SetActive(packet.Buttons[(int)WindowButtons.Close - 1]);

            foreach (var line in lines)
                line.text = " ";
        }

        private void OnEndWindow(EndWindowPacket packet)
        {
            panel.SetActive(true);

            var titleBar = GetComponentInChildren<TitleBar>();
            titleBar.canvas = GetComponentInParent<Canvas>();
        }

        private void OnWindowLine(WindowLinePacket packet)
        {
            lines[packet.LineNumber].text = packet.Text + " ";
        }

        public void CloseWindow()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Close, this.WindowId, NpcId);
            panel.SetActive(false);

            OnCloseWindow?.Invoke(this);

            Destroy(gameObject);
        }

        public void NextClicked()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Next, this.WindowId, NpcId);
        }

        public void BackClicked()
        {
            GameManager.Instance.NetworkClient.WindowButtonClick(WindowButtons.Back, this.WindowId, NpcId);
        }

        private void Update()
        {
            HandlePackets();
        }

        private void HandlePackets()
        {
            while (packetBuffer.Count != 0)
            {
                var packet = packetBuffer.Dequeue();

                if (packet is EndWindowPacket endWindowPacket)
                    OnEndWindow(endWindowPacket);
                else if (packet is WindowLinePacket windowLinePacket)
                    OnWindowLine(windowLinePacket);
                else
                    throw new ArgumentException($"Got unexpected packet type {packet.GetType()}");
            }
        }
    }
}