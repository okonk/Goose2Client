using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public abstract class BaseMultipleWindowManager<T> : MonoBehaviour where T : BaseMultipleWindow
    {
        private Dictionary<int, T> windows = new();

        public abstract string PrefabPath { get; }

        public abstract WindowFrames WindowFrame { get; }

        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Listen<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Listen<WindowLinePacket>(this.OnWindowLine);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<MakeWindowPacket>(this.OnMakeWindow);
            GameManager.Instance.PacketManager.Remove<EndWindowPacket>(this.OnEndWindow);
            GameManager.Instance.PacketManager.Remove<WindowLinePacket>(this.OnWindowLine);
        }

        private void OnMakeWindow(object packetObj)
        {
            var packet = (MakeWindowPacket)packetObj;

            if (packet.WindowFrame != WindowFrame) return;

            if (!windows.TryGetValue(packet.WindowId, out var windowScript))
            {
                var prefab = Resources.Load<GameObject>(PrefabPath);
                var window = Instantiate(prefab, gameObject.transform);

                windowScript = window.GetComponent<T>();

                windowScript.OnCloseWindow = OnCloseWindow;

                windows[packet.WindowId] = windowScript;
            }

            windowScript.OnMakeWindow(packet);
        }

        public void OnCloseWindow(BaseMultipleWindow window)
        {
            windows.Remove(window.WindowId);
        }

        private void OnEndWindow(object packetObj)
        {
            var packet = (EndWindowPacket)packetObj;

            if (windows.TryGetValue(packet.WindowId, out var window))
                window.packetBuffer.Enqueue(packet);
        }

        private void OnWindowLine(object packetObj)
        {
            var packet = (WindowLinePacket)packetObj;

            if (windows.TryGetValue(packet.WindowId, out var window))
                window.packetBuffer.Enqueue(packet);
        }
    }
}