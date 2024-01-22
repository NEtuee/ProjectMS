using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPointUI : IUIElement
{
    private DashPointUIBinder _binder;

    private bool[] _dashPointGen;
    private AnimationPlayer[] _animationPlayers;
    private AnimationPresetInfo[] _animationAnimations;

    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "대쉬 포인터 UI에 바인더가 셋팅되지 않았습니다.";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as DashPointUIBinder;
    }

    public void Initialize()
    {
        var imageCount = _binder.DashPointImages.Length;
        
        _dashPointGen = new bool[imageCount];
        _animationPlayers = new AnimationPlayer[imageCount];
        _animationAnimations = new AnimationPresetInfo[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            _animationPlayers[i] = new AnimationPlayer();
            _animationPlayers[i].initialize();
            _dashPointGen[i] = false;
        }
        
        _animationAnimations[0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Empty/"), "Sprites/UI/AP/Empty");
        _animationAnimations[1] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Full/"),"Sprites/UI/AP/Full");
        _animationAnimations[2] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Gen/"),"Sprites/UI/AP/Gen");
        _animationAnimations[3] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Lost/"),"Sprites/UI/AP/Lost");
    }

    public void InitValue(float dashPoint)
    {
        for (int i = 0; i < _dashPointGen.Length; i++)
        {
            float current = (float)i + 1;
            
            _dashPointGen[i] = dashPoint >= current;
            if (_dashPointGen[i] == true)
            {
                _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[1]._path, _animationAnimations[1]._customPreset);
            }
            else
            {
                _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[0]._path, _animationAnimations[0]._customPreset);
            }
        }
    }

    public void UpdateByManager(float deltaTime, float dashPoint)
    {
        for (int i = 0; i < _dashPointGen.Length; i++)
        {
            float current = (float)i + 1;
            
            var currentState = dashPoint >= current;
            if (currentState != _dashPointGen[i])
            {
                if (currentState == true)
                {
                    _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[2]._path, _animationAnimations[2]._customPreset);
                }
                else
                {
                    _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[3]._path, _animationAnimations[3]._customPreset);
                }
            }
            _dashPointGen[i] = currentState; 

            if (_animationPlayers[i].progress(deltaTime, null) == true)
            {
                if (_animationPlayers[i].getCurrentAnimationName() == _animationAnimations[2]._path)
                {
                    _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[1]._path, _animationAnimations[1]._customPreset);
                }
                else if (_animationPlayers[i].getCurrentAnimationName() == _animationAnimations[3]._path)
                {
                    _animationPlayers[i].changeAnimationByCustomPreset(_animationAnimations[0]._path, _animationAnimations[0]._customPreset);
                }
            }

            _binder.DashPointImages[i].sprite = _animationPlayers[i].getCurrentSprite();
            _binder.DashPointImages[i].SetNativeSize();
        }
    }
    
    private struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }
}
