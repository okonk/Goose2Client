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

    public class ToolbarItem : MonoBehaviour
    {
        [SerializeField] private ToolbarItemType itemType;

        public void OnPointerClick(PointerEventData eventData)
        {
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
            GameManager.Instance.NetworkClient.OpenCombineBag();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}