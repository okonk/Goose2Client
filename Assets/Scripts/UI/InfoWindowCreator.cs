using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InfoWindowCreator : MonoBehaviour
    {
        private void Start()
        {
            GameManager.Instance.PacketManager.Listen<MakeWindowPacket>(this.OnMakeWindow);
        }

        private void OnDestroy()
        {
            GameManager.Instance.PacketManager.Remove<MakeWindowPacket>(this.OnMakeWindow);
        }

        private void OnMakeWindow(object packetObj)
        {
            var packet = (MakeWindowPacket)packetObj;

            if (packet.WindowFrame != WindowFrames.GenericInfo) return;

            var prefab = Resources.Load<GameObject>("Prefabs/UI/InfoWindow");
            var infoWindow = Instantiate(prefab, gameObject.transform);

            var infoWindowScript = infoWindow.GetComponent<InfoWindow>();

            infoWindowScript.OnMakeWindow(packet);
        }
    }
}