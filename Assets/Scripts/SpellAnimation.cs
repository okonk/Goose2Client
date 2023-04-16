using System.Collections;
using System.Collections.Generic;
using Goose2Client;
using UnityEngine;

public class SpellAnimation : MonoBehaviour
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

    private void SetPosition(int height, int x, int y)
    {
        int yOffset = -System.Math.Max((height - 48) / 2, 0) - 24;
        transform.localPosition = new Vector3(x + 0.5f, y + yOffset / 32f);
    }

    public void SetAnimation(int id, int x, int y)
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        var o = overrides[0];

        var assetBundle = ResourceManager.LoadAssetBundle($"spell-{id}");

        var clip = ResourceManager.Load<AnimationClip>(assetBundle, $"{id}");
        if (clip == null)
            clip = ResourceManager.Load<AnimationClip>($"Animations/Blank");

        overrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(o.Key, clip);

        overrideController.ApplyOverrides(overrides);

        this.spriteRenderer.material.SetColor("_Color", Color.white);
        this.spriteRenderer.material.SetColor("_Tint", new Color(0,0,0,0));

        var animationName = $"{id}";
        int height = GameManager.Instance.AnimationManager.GetHeight(animationName);
        SetPosition(height, x, y);

        Invoke(nameof(Stop), clip.length);
    }

    public void Stop()
    {
        Destroy(gameObject);
    }
}
