using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AnimatorOverrideController overrideController;

    private void Awake()
    {
        this.animator = GetComponent<Animator>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        this.overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    public void SetGraphic(string type, string id)
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            var o = overrides[i];

            var splits = o.Key.name.Split('-', 3);
            splits[0] = type;
            splits[1] = id;
            var newClip = Resources.Load<AnimationClip>($"Animations/{string.Join('-', splits)}");

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(o.Key, newClip);
        }

        overrideController.ApplyOverrides(overrides);
    }

    public void SetSortOrder(int order)
    {
        this.spriteRenderer.sortingOrder = order;
    }

    public void SetFloat(string key, float value)
    {
        this.animator.SetFloat(key, value);
    }
}
