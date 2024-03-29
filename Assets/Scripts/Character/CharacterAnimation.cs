using System.Collections;
using System.Collections.Generic;
using Goose2Client;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AnimatorOverrideController overrideController;

    public int Id { get; private set; }
    public Color Color { get; private set; }
    public int Height { get; private set; } = 64;

    private void Awake()
    {
        this.animator = GetComponent<Animator>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        this.overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    public void SetPosition(int height)
    {
        this.Height = height;

        int yOffset = -System.Math.Max((height - 48) / 2, 0) - 16;
        transform.localPosition = new Vector3(0, yOffset / 32f);
    }

    public void SetGraphic(string type, int id)
    {
        this.Id = id;

        var assetBundle = ResourceManager.LoadAssetBundle($"{type.ToLowerInvariant()}-{id}");

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
                clip = ResourceManager.Load<AnimationClip>(assetBundle, $"{string.Join('-', splits)}");
            }

            if (clip == null)
                clip = ResourceManager.Load<AnimationClip>($"Animations/Blank");

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(o.Key, clip);
        }

        overrideController.ApplyOverrides(overrides);

        var animationName = $"{type}-{id}-IdleNoEquip-Left";
        int height = GameManager.Instance.AnimationManager.GetHeight(animationName);
        SetPosition(height);
    }

    public void SetColor(Color color)
    {
        this.Color = color;

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

    public void TriggerAttack()
    {
        this.animator.SetTrigger(Constants.Attack);
    }

    public void TriggerCast()
    {
        this.animator.SetTrigger(Constants.Cast);
    }

    public void Stop()
    {
        this.animator.StopPlayback();
    }
}
