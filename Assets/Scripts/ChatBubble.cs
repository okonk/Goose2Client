using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Goose2Client
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Start()
        {

        }

        internal void SetText(string text)
        {
            this.text.text = text;
        }
    }
}