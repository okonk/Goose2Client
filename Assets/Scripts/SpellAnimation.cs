using System.Collections;
using System.Collections.Generic;
using Goose2Client;
using UnityEngine;

public class SpellAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AnimatorOverrideController overrideController;

    public int Height { get; private set; } = 64;

    private void Awake()
    {
        this.animator = GetComponent<Animator>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        this.overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    private void SetPosition(int height)
    {
        this.Height = height;

        int yOffset = -System.Math.Max((height - 48) / 2, 0) - 16;
        transform.localPosition = new Vector3(0.5f, yOffset / 32f);
    }

    public void SetAnimation(int id)
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        var o = overrides[0];

        var clip = Resources.Load<AnimationClip>($"Animations/{id}");

        if (clip == null)
            clip = Resources.Load<AnimationClip>($"Animations/Blank");

        overrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(o.Key, clip);

        overrideController.ApplyOverrides(overrides);

        this.spriteRenderer.material.SetColor("_Color", Color.white);
        this.spriteRenderer.material.SetColor("_Tint", new Color(0,0,0,0));

        var animationName = $"{id}";
        int height = GameManager.Instance.AnimationManager.GetHeight(animationName);
        SetPosition(height);

        Invoke(nameof(Stop), clip.length);
    }

    public void Stop()
    {
        Destroy(gameObject);
    }
}
