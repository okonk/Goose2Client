using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public enum ToolbarItemType
    {
        Help,
        ToggleDM,
        Party,
        Refresh,
        CombineBag,
        Exit
    }

    public class ToolbarItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ToolbarItemType itemType;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Toolbar clicked");

            if (eventData.button != PointerEventData.InputButton.Left) return;

            var action = itemType switch
            {
                ToolbarItemType.CombineBag => (Action)OpenCombineBag,
                ToolbarItemType.Exit => Exit,
                _ => DoNothing,
            };

            action();
        }

        public void DoNothing() {}

        public void OpenCombineBag()
        {
            Debug.Log("open combine bag");
            GameManager.Instance.NetworkClient.OpenCombineBag();
        }

        public void Exit()
        {
            GameManager.Instance.Quit();
        }
    }
}