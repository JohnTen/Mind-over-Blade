using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

[System.Serializable]
public class AnimationData
{
    public string name;  //动画名称，Animation name
    public float timeScale; //动画播放速度 The speed of playing animation
    public float playTimes; //动画播放的循环次数
    public float duration; //动画播放的持续时间 The duration of current animation
    public bool isFadeOut; //动画是否淡出 Is animation fading out
    public bool isPlaying; //动画是否已经开始 Is animation started
    public bool canChange; //动画当前是否可以改变 Can animation change
}

public class Animations : MonoBehaviour
{
    public AnimationData[] _animationsData;
    public UnityArmatureComponent _amature;
    public Dictionary<string, AnimationData> animationDictionary = new Dictionary<string, AnimationData>();
    const float DefaultTimeScale = 1;

    private void Awake()
    {
        _amature = this.GetComponent<UnityArmatureComponent>();
        _animationsData = new AnimationData[_amature.animation.animationNames.Count];
    }

    private void Start()
    {
        _amature.AddDBEventListener(EventObject.START, this.OnAnimationEventHandler);
        _amature.AddDBEventListener(EventObject.FADE_OUT_COMPLETE, this.OnAnimationEventHandler);
        AnimationsDataGet();
    }

    private void Update()
    {
    }

    /// <summary>
    /// 初始化animation中的数据，Set the default data of animations in Animation arry;
    /// </summary>

    private void AnimationsDataGet()
    {
        for (int i = 0; i < _amature.animation.animationNames.Count; i++)
        {
            _animationsData[i].name = _amature.animation.animationNames[i];
            _animationsData[i].timeScale = _amature.animation.timeScale;
            _animationsData[i].playTimes = _amature.animation.animations[_animationsData[i].name].playTimes;
            _animationsData[i].duration = _amature.animation.animations[_animationsData[i].name].duration;
            animationDictionary.Add(_animationsData[i].name, _animationsData[i]);
        }
    }

    /// <summary>
    /// 设置当前动画的TimeScale,并仅在该动画播放期间改变TimeScale；Set TimeScale while animation is playing
    /// </summary>
    /// <param name="animationName">Animation name.</param>
    /// <param name="timeScale">Time scale.</param>

    public void AnimationDataSet(string animationName, float timeScale)
    {
        animationDictionary[animationName].timeScale = timeScale;
        if(animationDictionary[animationName].isPlaying)
        {
            _amature.animation.timeScale = timeScale;
        }
        else
        {
            _amature.animation.timeScale = DefaultTimeScale;
        }
    }

    private void OnAnimationEventHandler(string type, EventObject eventObject)
    {
        for(int i = 0; i < _amature.animation.animationNames.Count; i++)
        {
            if (eventObject.animationState.name == _animationsData[i].name && _animationsData[i]!=null)
            {
                if (type == EventObject.FADE_OUT_COMPLETE)
                {
                    _animationsData[i].isFadeOut = true;
                    _animationsData[i].isPlaying = false;
                }
                if(type == EventObject.START)
                {
                    _animationsData[i].isPlaying = true;
                    _animationsData[i].isFadeOut = false;
                }
            }
        }
    }
}
