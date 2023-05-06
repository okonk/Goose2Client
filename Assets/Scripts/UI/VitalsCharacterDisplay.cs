using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goose2Client
{
    public class VitalsCharacterDisplay : MonoBehaviour
    {
        [SerializeField] private Image body;
        [SerializeField] private Image hair;
        [SerializeField] private Image eyes;
        [SerializeField] private Image chest;
        [SerializeField] private Image helmet;

        private void Start()
        {
            GameManager.Instance.CharacterUpdated += OnCharacterUpdated;
        }

        public void OnCharacterUpdated(Character character)
        {
            SetImage(body, "Body", character.BodyId, character.BodyColor);

            if (character.BodyId < 100)
            {
                SetImage(hair, "Hair", character.HairId, character.HairColor);
                SetImage(eyes, "Eyes", character.FaceId, new Color(0, 0, 0, 0));
                SetImage(chest, "Chest", character.DisplayedEquipment[0][0], ColorH.RGBA(character.DisplayedEquipment[0]));
                SetImage(helmet, "Helm", character.DisplayedEquipment[1][0], ColorH.RGBA(character.DisplayedEquipment[1]));
            }
            else
            {
                ClearImage(hair);
                ClearImage(eyes);
                ClearImage(chest);
                ClearImage(helmet);
            }
        }

        private void SetImage(Image image, string animationType, int animationId, Color color)
        {
            AnimationFrame frame = (animationId == 0 ? null : GameManager.Instance.AnimationManager.GetFrame(animationType, animationId));
            if (frame == null)
            {
                ClearImage(image);
                return;
            }

            int yOffset = -20;
            if (animationType == "Body" && animationId >= 100)
                yOffset = 0;

            var sprite = Helpers.GetSprite(frame.GraphicId, frame.FileId);
            image.sprite = sprite;
            image.color = Color.white;
            image.material = Instantiate(image.material);
            image.material.SetColor("_Tint", color);
            image.rectTransform.sizeDelta = new Vector2(frame.Width * 1.25f, frame.Height * 1.25f);
            image.rectTransform.localPosition = new Vector3(0, yOffset);
        }

        private void ClearImage(Image image)
        {
            image.color = new Color(0, 0, 0, 0);
        }
    }
}