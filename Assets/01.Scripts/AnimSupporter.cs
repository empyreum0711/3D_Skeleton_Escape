using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Anim
{
    public AnimationClip Idle;          //기본 상태
    public AnimationClip Move;          //이동
    public AnimationClip Attack1;       //공격
    public AnimationClip Attack2;       //공격
    public AnimationClip Attack3;       //공격
    public AnimationClip Die;           //사망
}

public enum AnimState
{
    idle,                           //기본
    move,                           //이동
    trace,                          //추적
    attack,                         //공격
    die                             //사망
}
