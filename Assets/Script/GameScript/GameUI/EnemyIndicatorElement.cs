using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicatorElement : MonoBehaviour
{
    public enum State
    {
        AppearPlay,
        Active,
        DisappearPlay,
        Inactive,
    }
    
    struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }
    
    private SpriteRenderer _spriteRenderer;
    private AnimationPresetInfo _appearInfo;
    private AnimationPresetInfo _disappearInfo;
    private AnimationPlayer _animationPlayer;

    public State _state = State.Inactive;

    private Action _completeDisappear;

    public State GetState()
    {
        return _state;
    }

    public void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _animationPlayer = new AnimationPlayer();
        _animationPlayer.initialize();

        _appearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/ScreenIndicator/Appear/"), "Sprites/UI/ScreenIndicator/Appear");
        _disappearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/ScreenIndicator/Disappear/"), "Sprites/UI/ScreenIndicator/Disappear");
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        _state = State.AppearPlay;
        
        _animationPlayer.changeAnimationByCustomPreset(_appearInfo._path, _appearInfo._customPreset);
    }
    
    public void Disappear(Action completeDisappear)
    {
        _state = State.DisappearPlay;
        
        _animationPlayer.changeAnimationByCustomPreset(_disappearInfo._path, _disappearInfo._customPreset);

        _completeDisappear = completeDisappear;
    }

    private void Update()
    {
        if (_state == State.AppearPlay)
        {
            if (_animationPlayer.progress(Time.deltaTime, null) == true)
            {
                _state = State.Active;
            }

            _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
        }
        else if (_state == State.DisappearPlay)
        {
            if (_animationPlayer.progress(Time.deltaTime, null) == true)
            {
                _state = State.Inactive;
                _completeDisappear?.Invoke();
                gameObject.SetActive(false);
            }

            _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
        }
    }
}
