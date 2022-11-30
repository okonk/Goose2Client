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

    public void SetGraphic(string type, int id)
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            var o = overrides[i];

            AnimationClip clip = null;
            if (id > 0)
            {
                var splits = o.Key.name.Split('-', 3);
                splits[0] = type;
                splits[1] = id.ToString();
                clip = Resources.Load<AnimationClip>($"Animations/{string.Join('-', splits)}");
            }

            if (clip == null)
                clip = Resources.Load<AnimationClip>($"Animations/Blank");

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(o.Key, clip);
        }

        overrideController.ApplyOverrides(overrides);
    }

    public void SetColor(Color color)
    {
        this.spriteRenderer.material.SetColor("_Color", Color.white);
        //color.a = 220 / 256f ;
        this.spriteRenderer.material.SetColor("_Tint", color);
    }

    public void SetSortOrder(int order)
    {
        this.spriteRenderer.sortingOrder = order;
    }

    public void SetFloat(string key, float value)
    {
        this.animator.SetFloat(key, value);
    }

    public void SetBool(string key, bool value)
    {
        this.animator.SetBool(key, value);
    }

    public float GetHeight()
    {
        return this.spriteRenderer.sprite.bounds.center.y;
    }
}
