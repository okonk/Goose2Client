using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class CharacterAnimationStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterAnimation = animator.GetComponent<CharacterAnimation>();

            var currentAnimation = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            int height = GameManager.Instance.AnimationManager.GetHeight(currentAnimation);

            if (height != characterAnimation.Height)
                characterAnimation.SetPosition(height);
        }
    }
}