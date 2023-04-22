using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Goose2Client
{
    public class CharacterClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Character character;

        public void Start()
        {
            character = transform.GetComponentInParent<Character>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                GameManager.Instance.NetworkClient.LeftClick(character.X, character.Y);
            else if (eventData.button == PointerEventData.InputButton.Right)
                GameManager.Instance.NetworkClient.RightClick(character.X, character.Y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var mapItem = GameManager.Instance.MapManager.GetMapItem(character.X, character.Y);
            if (mapItem != null)
                TooltipManager.Instance.ShowMapItemTooltip(mapItem.Item, mapItem.gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideMapItemTooltip();
        }
    }
}