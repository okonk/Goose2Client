using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class OptionsWindow : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Toggle targetFilteringToggle;

        public void ToggleWindow()
        {
            panel.SetActive(!panel.activeSelf);

            if (!panel.activeSelf)
                GameManager.Instance.CharacterSettings.Save();
        }

        private void Start()
        {
            targetFilteringToggle.isOn = GameManager.Instance.CharacterSettings.GetOption<bool>(Options.TargetFiltering, true);
        }

        public void CloseWindow()
        {
            panel.SetActive(false);

            GameManager.Instance.CharacterSettings.Save();
        }

        public void OnTargetFilteringChanged()
        {
            GameManager.Instance.CharacterSettings.Options[Options.TargetFiltering] = targetFilteringToggle.isOn;
        }
    }
}