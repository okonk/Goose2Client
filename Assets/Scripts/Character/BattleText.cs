using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class BattleText : MonoBehaviour
    {
        private int battleTextPosition = 0;

        public void AddText(BattleTextType textType, string text, int spriteHeight)
        {
            int battleTextCount = gameObject.transform.childCount;
            if (battleTextCount >= 18) return;

            bool spread = false;
            switch (textType)
            {
                case BattleTextType.Red1:
                case BattleTextType.Red2:
                case BattleTextType.Red4:
                case BattleTextType.Red5:
                    spread = true;
                    break;
                case BattleTextType.Green7:
                case BattleTextType.Green8:
                    spread = true;
                    break;
            }

            float x = 0;
            float y = 0;
            if (spread)
            {
                if (battleTextCount != 0)
                {
                    this.battleTextPosition = (this.battleTextPosition + 1) % 9;
                }
                else
                {
                    this.battleTextPosition = 0;
                }

                y += (Math.Min(battleTextCount / 3, 2) * 8) / 32f;

                if (this.battleTextPosition % 3 != 0)
                {
                    x += (this.battleTextPosition % 3 != 1 ? 12 : -4) / 32f;
                }
                else
                {
                    x += 4 / 32f;
                }
            }

            var prefab = Resources.Load<GameObject>("Prefabs/BattleTextLine");
            var battleTextLine = Instantiate(prefab, gameObject.transform);

            var battleTextScript = battleTextLine.GetComponent<BattleTextLine>();
            battleTextScript.Create(textType, text, x, y);
        }
    }
}