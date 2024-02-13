using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHpObject : MonoBehaviour
{
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
    
    public SpriteRenderer Renderer;

    private GameEntityBase _target;
    private bool _follow;
    private bool _dead;

    private AnimationPlayer _animationPlayer = new AnimationPlayer();
    private AnimationPresetInfo _appearInfo;
    private AnimationPresetInfo _disappearInfo;

    private Action<EnemyHpObject> _onDead;
    private Vector3 _offset;

    private int _prevHp = -1;

    public void Init()
    {
        _appearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/appear/"), "Sprites/UI/1hpalert/appear");
        _disappearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/disappear/"), "Sprites/UI/1hpalert/disappear");
        
        _animationPlayer.initialize();
        _animationPlayer.changeAnimationByCustomPreset(_appearInfo._path, _appearInfo._customPreset);

    }

    public void Active(GameEntityBase target, Vector3 offset, Action<EnemyHpObject> onDead)
    {
        if (target == null)
        {
            return;
        }
        
        _target = target;
        //gameObject.SetActive(true);
        _follow = true;
        _onDead = onDead;
        _offset = offset;
        _dead = false;
    }

    public void UpdateByManager(float deltaTime)
    {
        if (_target == null)
        {
            return;
        }

        Follow();
        ChangeAnimation();
        UpdateAnimation(deltaTime);
    }

    private void Follow()
    {
        if (_follow == false)
        {
            return;
        }
        
        transform.position = _target.transform.position + _offset;
    }

    private void ChangeAnimation()
    {
        var hp = (int)_target.getStatus("HP");
        if (_prevHp == hp)
        {
            return;
        }

        _prevHp = hp;
        if (hp == 1)
        {
            gameObject.SetActive(true);
            _animationPlayer.changeAnimationByCustomPreset(_appearInfo._path, _appearInfo._customPreset);
        }
        else if (hp == 0)
        {
            _follow = false;
            gameObject.SetActive(true);
            _animationPlayer.changeAnimationByCustomPreset(_disappearInfo._path, _disappearInfo._customPreset);
            _dead = true;
        }
    }

    private void UpdateAnimation(float deltaTime)
    {
        if (_animationPlayer.isEnd() == false)
        {
            _animationPlayer.progress(deltaTime, null);
            Renderer.sprite = _animationPlayer.getCurrentSprite();
        }

        if (_dead == true)
        {
            if (_animationPlayer.isEnd() == true)
            {
                gameObject.SetActive(false);
                _target = null;
                _onDead?.Invoke(this);
                return;
            }
        }
    }

    private void Stop()
    {
        _target = null;
        _follow = false;
        _dead = false;
    }
}
