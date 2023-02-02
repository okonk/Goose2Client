using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class BattleTextLine : MonoBehaviour
    {
        private float aliveTime = 0;

        private static Color yellow = ColorH.RGBA(248, 208, 0);
        private static Color green = ColorH.RGBA(136, 204, 64);
        private static Color red = ColorH.RGBA(154, 0, 0);

        public void Create(BattleTextType textType, string text, float x, float y)
        {
            var color = Color.white;
            switch (textType)
            {
                case BattleTextType.Red1:
                case BattleTextType.Red2:
                case BattleTextType.Red4:
                case BattleTextType.Red5:
                case BattleTextType.Red61:
                    color = red;
                    break;
                case BattleTextType.Green7:
                case BattleTextType.Green8:
                    color = green;
                    break;
                case BattleTextType.Yellow60:
                    color = yellow;
                    break;
                case BattleTextType.Stunned10:
                case BattleTextType.Stunned50:
                    text = "STUNNED";
                    break;
                case BattleTextType.Rooted11:
                case BattleTextType.Rooted51:
                    text = "ROOTED";
                    break;
                case BattleTextType.Dodge20:
                    text = "DODGE";
                    break;
                case BattleTextType.Miss21:
                    text = "MISS";
                    break;
            }

            gameObject.name = $"BattleText ({text})";

            var tmp = GetComponent<TMPro.TextMeshPro>();
            tmp.text = text;
            tmp.color = color;
            tmp.sortingLayerID = SortingLayer.NameToID(Constants.NamesLayer);
            tmp.fontMaterial = Resources.Load<Material>("Materials/NameFont");

            tmp.transform.localPosition = new Vector3(x, y);
        }

        public void Update()
        {
            aliveTime += Time.deltaTime;

            if (aliveTime >= 1)
                Destroy(gameObject);
            else
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + Time.deltaTime);
        }
    }
}
