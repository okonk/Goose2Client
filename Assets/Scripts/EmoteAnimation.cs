using System.Collections;
using System.Collections.Generic;
using Goose2Client;
using UnityEngine;

public class EmoteAnimation : SpellAnimation
{
    protected override void SetPosition(int height, float x, float y)
    {
        float bodyOffset = y / 32f - 0.75f;
        transform.localPosition = new Vector3(0.5f, bodyOffset);
    }
}
