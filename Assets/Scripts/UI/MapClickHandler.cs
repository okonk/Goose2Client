using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Goose2Client
{
    public class MapClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private Tilemap tilemap;

        public void Start()
        {
            tilemap = GetComponent<Tilemap>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 worldPos = eventData.pointerCurrentRaycast.worldPosition;
            Vector3Int coord = tilemap.WorldToCell(worldPos);

            if (eventData.button == PointerEventData.InputButton.Left)
                GameManager.Instance.NetworkClient.LeftClick(coord.x, coord.y);
            else if (eventData.button == PointerEventData.InputButton.Right)
                GameManager.Instance.NetworkClient.RightClick(coord.x, coord.y);
        }
    }
}