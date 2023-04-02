using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Goose2Client
{
    public class CharacterClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private Character character;

        public void Start()
        {
            character = transform.parent.GetComponent<Character>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                GameManager.Instance.NetworkClient.LeftClick(character.X, character.Y);
            else if (eventData.button == PointerEventData.InputButton.Right)
                GameManager.Instance.NetworkClient.RightClick(character.X, character.Y);
        }
    }
}