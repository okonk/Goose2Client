using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class SpellTargetManager : MonoBehaviour
    {
        private static SpellTargetManager instance;

        public static SpellTargetManager Instance
        {
            get { return instance; }
        }

        public bool IsTargeting { get; private set; }

        public Character Target { get; private set; }

        private SpellInfo spellToCast;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            GameManager.Instance.SpellTargetManager = this;
        }

        private void Start()
        {
            var inputManager = PlayerInputManager.Instance;

            inputManager.TargetDown = OnTargetDown;
            inputManager.TargetUp = OnTargetUp;
            inputManager.TargetHome = OnTargetHome;
            inputManager.ConfirmTarget = OnConfirmTarget;
            inputManager.CancelTarget = OnCancelTarget;
        }

        public void Cast(SpellInfo info)
        {
            spellToCast = info;
            IsTargeting = true;

            SetTarget(Target);

            PlayerInputManager.Instance.SwitchToMapping("Targeting");
        }

        private void SetTarget(Character nextTarget)
        {
            if (nextTarget == null)
                Target = GameManager.Instance.Character;
            else
                Target = nextTarget;

            var prefab = ResourceManager.LoadFromBundle<GameObject>("prefabs", "SpellTarget");
            var target = Instantiate(prefab, Target.gameObject.transform);

            ResizeTarget(target);
        }

        private void SwitchTarget(Character nextTarget)
        {
            if (Target == null)
            {
                SetTarget(nextTarget);
                return;
            }

            var existingTarget = Target.GetComponentInChildren<SpellTarget>();
            existingTarget.transform.SetParent(nextTarget.transform, false);

            Target = nextTarget;

            ResizeTarget(existingTarget.gameObject);
        }

        private void ResizeTarget(GameObject target)
        {
            var heightScaled = Math.Max(1, Target.Height / 32f);
            var widthScaled = Math.Max(1, heightScaled * 0.75f);
            var yOffset = (heightScaled - 1) * 0.5f;

            target.transform.localPosition = new Vector3(0.5f, yOffset);
            target.transform.localScale = new Vector3(widthScaled, heightScaled, 1);
        }

        private void RemoveTarget()
        {
            IsTargeting = false;
            PlayerInputManager.Instance.SwitchToMapping("Player");

            if (Target == null) return;

            var target = Target.GetComponentInChildren<SpellTarget>();
            Destroy(target.gameObject);
        }

        private void OnTargetDown(InputValue value)
        {
            var nextTarget = GetNextSpellCastTarget(searchDown: true);
            if (nextTarget == null) return;

            SwitchTarget(nextTarget);
        }

        private void OnTargetUp(InputValue value)
        {
            var nextTarget = GetNextSpellCastTarget(searchDown: false);
            if (nextTarget == null) return;

            SwitchTarget(nextTarget);
        }

        private void OnTargetHome(InputValue value)
        {
            SwitchTarget(GameManager.Instance.Character);
        }

        private void OnConfirmTarget(InputValue value)
        {
            RemoveTarget();

            if (Target != null)
            {
                GameManager.Instance.SpellCooldownManager.Cast(spellToCast.SlotNumber);

                GameManager.Instance.NetworkClient.CastSpell(spellToCast.SlotNumber, Target.LoginId);
            }
        }

        private void OnCancelTarget(InputValue value)
        {
            RemoveTarget();
        }

        private Character GetNextSpellCastTarget(bool searchDown)
        {
            var map = GameManager.Instance.CurrentMap;

            int currentPosition = Target.Y * map.Width + Target.X;
            int lowestPosition = currentPosition;
            int highestPosition = currentPosition;
            int closestPosition = currentPosition;

            var lowestTarget = Target;
            var highestTarget = Target;
            var closestTarget = Target;

            var characters = GameManager.Instance.MapManager.Characters;
            var player = GameManager.Instance.Character;

            var viewRangeX = 10;
            var viewRangeY = 8;

            foreach (var character in characters)
            {
                // Filter out things off screen and current target
                if (character == Target ||
                    Math.Abs(character.X - player.X) > viewRangeX ||
                    Math.Abs(character.Y - player.Y) > viewRangeY)
                {
                    continue;
                }

                int characterPosition = character.Y * map.Width + character.X;

                if (characterPosition < lowestPosition)
                {
                    lowestPosition = characterPosition;
                    lowestTarget = character;
                }
                else if (characterPosition > highestPosition)
                {
                    highestPosition = characterPosition;
                    highestTarget = character;
                }

                if ((searchDown && characterPosition > currentPosition && (closestPosition == currentPosition || currentPosition - closestPosition < currentPosition - characterPosition))
                    || (!searchDown && characterPosition < currentPosition && (closestPosition == currentPosition || closestPosition - currentPosition < characterPosition - currentPosition)))
                {
                    closestPosition = characterPosition;
                    closestTarget = character;
                }
            }

            int nextTarget = closestPosition;
            var next = closestTarget;
            if (nextTarget == currentPosition)
            {
                nextTarget = searchDown ? lowestPosition : highestPosition;
                next = searchDown ? lowestTarget : highestTarget;
            }

            if (nextTarget != currentPosition)
            {
                return next;
                //spellCastTarget = Tiles[nextTarget].Character ?? player;
            }

            return null;
        }
    }
}