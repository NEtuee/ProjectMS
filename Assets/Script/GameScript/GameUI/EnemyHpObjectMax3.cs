using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHpObjectMax3 : MonoBehaviour
{
    public enum Type
    {
        Max2,
        Max3
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

    public SpriteRenderer Blood1;
    public SpriteRenderer Blood2;
    public SpriteRenderer Last;

    private AnimationPlayer _animationPlayerForBlood1 = new AnimationPlayer();
    private AnimationPlayer _animationPlayerForBlood2 = new AnimationPlayer();
    private AnimationPlayer _animationPlayerForLast = new AnimationPlayer();

    private AnimationPresetInfo _lastAppearInfo;
    private AnimationPresetInfo _lastDisappearInfo;

    private AnimationPresetInfo _blood1AppearInfo;
    private AnimationPresetInfo _blood2AppearInfo;
    
    [SerializeField] private GameEntityBase _target;
    private bool _follow;
    private bool _dead;
    private Action<EnemyHpObjectMax3> _onDead;
    private Vector3 _offset;
    private Type _type = Type.Max3;
    
    private int _prevHp = -1;
    
    public void Init()
    {
        _lastAppearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/appear/"), "Sprites/UI/1hpalert/appear");
        _lastDisappearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/disappear/"), "Sprites/UI/1hpalert/disappear");
        
        _animationPlayerForLast.initialize();
        _animationPlayerForLast.changeAnimationByCustomPreset(_lastAppearInfo._path, _lastAppearInfo._customPreset);
        
        _blood1AppearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/blooddrip/dripAappear/"), "Sprites/UI/1hpalert/blooddrip/dripAappear");
        _blood2AppearInfo = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/1hpalert/blooddrip/dripBappear/"), "Sprites/UI/1hpalert/blooddrip/dripBappear");
        
        _animationPlayerForBlood1.initialize();
        _animationPlayerForBlood1.changeAnimationByCustomPreset(_blood1AppearInfo._path, _blood2AppearInfo._customPreset);
        
        _animationPlayerForBlood2.initialize();
        _animationPlayerForBlood2.changeAnimationByCustomPreset(_blood2AppearInfo._path, _blood2AppearInfo._customPreset);
    }
    
    public void Active(GameEntityBase target, Vector3 offset, Type type, Action<EnemyHpObjectMax3> onDead)
    {
        if (target == null)
        {
            return;
        }
        
        _target = target;
        _follow = true;
        _onDead = onDead;
        _offset = offset;
        _dead = false;
        
        gameObject.SetActive(false);
        Blood1.gameObject.SetActive(false);
        Blood2.gameObject.SetActive(false);
        Last.gameObject.SetActive(false);

        var color1 = Blood1.color;
        color1.a = 1f;
        Blood1.color = color1;

        var color2 = Blood2.color;
        color2.a = 1f;
        Blood2.color = color2;

        _type = type;
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

    public void Stop()
    {
        _follow = false;
        gameObject.SetActive(false);
        
        if (_target == null)
        {
            return;
        }

        _target = null;
        _onDead?.Invoke(this);
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
        
        if (_type == Type.Max3 && hp <= 2)
        {
            gameObject.SetActive(true);
            Blood1.gameObject.SetActive(true);
            _animationPlayerForBlood1.changeAnimationByCustomPreset(_blood1AppearInfo._path, _blood2AppearInfo._customPreset);
        }
        
        if (hp <= 1)
        {
            Last.gameObject.SetActive(true);
            _animationPlayerForLast.changeAnimationByCustomPreset(_lastAppearInfo._path, _lastAppearInfo._customPreset);

            if (_type == Type.Max2)
            {
                gameObject.SetActive(true);
                Blood1.gameObject.SetActive(true);
                _animationPlayerForBlood1.changeAnimationByCustomPreset(_blood1AppearInfo._path, _blood2AppearInfo._customPreset);
            }
            else if (_type == Type.Max3)
            {
                Blood2.gameObject.SetActive(true);
                _animationPlayerForBlood2.changeAnimationByCustomPreset(_blood2AppearInfo._path, _blood2AppearInfo._customPreset);
            }
        }

        if (hp <= 0)
        {
            _follow = false;
            _dead = true;
            
            Last.gameObject.SetActive(true);
            _animationPlayerForLast.changeAnimationByCustomPreset(_lastDisappearInfo._path, _lastDisappearInfo._customPreset);
        }
    }
    
    private void UpdateAnimation(float deltaTime)
    {
        if (_animationPlayerForBlood1.isEnd() == false)
        {
            _animationPlayerForBlood1.progress(deltaTime, null);
            Blood1.sprite = _animationPlayerForBlood1.getCurrentSprite();
        }

        if (_animationPlayerForBlood2.isEnd() == false)
        {
            _animationPlayerForBlood2.progress(deltaTime, null);
            Blood2.sprite = _animationPlayerForBlood2.getCurrentSprite();
        }
        
        if (_animationPlayerForLast.isEnd() == false)
        {
            _animationPlayerForLast.progress(deltaTime, null);
            Last.sprite = _animationPlayerForLast.getCurrentSprite();
        }
        
        if (_dead == true)
        {
            // var color1 = Blood1.color;
            // color1.a = 0f;
            // Blood1.color = color1;
            //
            // var color2 = Blood2.color;
            // color2.a = 0f;
            // Blood2.color = color2;
            
            if (_animationPlayerForLast.isEnd() == true)
            {
                gameObject.SetActive(false);
                _target = null;
                _onDead?.Invoke(this);
                return;
            }
        }
    }

}
