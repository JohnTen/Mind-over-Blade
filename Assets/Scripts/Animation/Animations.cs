using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

[System.Serializable]
public class Animation
{
    public string name;  //动画名称，Animation name
    public float timeScale; //动画播放速度 The speed of playing animation
    public float totalTime; //动画播放的总时间 The total playing time of current animation
    public bool isCompleted; //动画是否播放完成 Is animation completed
    public bool isStart; //动画是否已经开始 Is animation started
    public bool canChange; //动画当前是否可以改变 Can animation change
}

public class Animations : MonoBehaviour
{
    public int sizeOfAnimationsClass;
    public Animation[] animations;
    public UnityArmatureComponent _amature;

    private void Start()
    {
        animations = new Animation[_amature.animation.animationNames.Count];
        print(sizeOfAnimationsClass);
    }
}
