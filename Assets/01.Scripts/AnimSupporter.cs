using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Anim
{
    public AnimationClip Idle;
    public AnimationClip Move;
    public AnimationClip Attack1;
    public AnimationClip Attack2;
    public AnimationClip Attack3;
    public AnimationClip Die;
}

public enum AnimState
{
    idle,
    move,
    trace,
    attack,
    die
}
