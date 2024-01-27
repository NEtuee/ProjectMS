using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashPointUI : IUIElement
{
    private DashPointUIBinder _binder;

    private bool[] _dashPointGen;
    private DashPointIcon[] _dashPointIcon;
    private float _prevBlood = 0f;
    private float _prevDashPoint = 0f;

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
        _dashPointIcon = new DashPointIcon[imageCount];

        _dashPointIcon[0] = new DashPointIcon(
            _binder.DashPointImages[0], 
            "Sprites/UI/AP/newAP/A/absorbing", 
            "Sprites/UI/AP/newAP/A/autocharging", 
            "Sprites/UI/AP/newAP/A/breaking");
        _dashPointIcon[1] = new DashPointIcon(
            _binder.DashPointImages[1], 
            "Sprites/UI/AP/newAP/B/absorbing", 
            "Sprites/UI/AP/newAP/B/autocharging", 
            "Sprites/UI/AP/newAP/B/breaking");
        _dashPointIcon[2] = new DashPointIcon(
            _binder.DashPointImages[2], 
            "Sprites/UI/AP/newAP/C/absorbing", 
            "Sprites/UI/AP/newAP/C/autocharging", 
            "Sprites/UI/AP/newAP/C/breaking");
        _dashPointIcon[3] = new DashPointIcon(
            _binder.DashPointImages[3], 
            "Sprites/UI/AP/newAP/D/absorbing", 
            "Sprites/UI/AP/newAP/D/autocharging", 
            "Sprites/UI/AP/newAP/D/breaking");
    }

    public void InitValue(float dashPoint)
    {
        for (int i = 0; i < _dashPointGen.Length; i++)
        {
            float current = (float)i + 1;
            
            _dashPointGen[i] = dashPoint >= current;
            if (_dashPointGen[i] == true)
            {
                _dashPointIcon[i].AutoCharging();
            }
            else
            {
                _dashPointIcon[i].Breaking();
            }
        }
    }

    private bool _absorbingFlag = false;
    public void UpdateByManager(float deltaTime, float dashPoint, float blood)
    {
        _absorbingFlag = _absorbingFlag == false ? _prevBlood < blood : _absorbingFlag;
        _prevBlood = blood;

        float prevDashPointIndex = MathEx.floor(_prevDashPoint, 0);
        bool releaseAbsorbing = false;
        for (int i = 0; i < _dashPointGen.Length; i++)
        {
            float current = (float)i + 1;

            var currentState = dashPoint >= current;
            if (currentState != _dashPointGen[i])
            {
                if (currentState == true)
                {
                    float delayTime = (current - (prevDashPointIndex + 1f)) * 0.1f;
                    if (_absorbingFlag == true)
                    {
                        releaseAbsorbing = true;
                        _dashPointIcon[i].ChangeIconState(DashPointIconState.Absorbing,delayTime);
                    }
                    else
                    {
                        _dashPointIcon[i].ChangeIconState(DashPointIconState.AutoCharging,delayTime);
                    }
                }
                else
                {
                    _dashPointIcon[i].ChangeIconState(DashPointIconState.Breaking,0f);
                }
            }
            _dashPointGen[i] = currentState;

            _dashPointIcon[i].Progress(deltaTime);
        }

        if (releaseAbsorbing == true)
        {
            _absorbingFlag = false;
        }

        _prevDashPoint = dashPoint;
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
    
    private enum DashPointIconState
    {
        None,
        Absorbing,
        AutoCharging,
        Breaking,
    }

    private class DashPointIcon
    {
        private Image _image;
        private AnimationPlayer _animationPlayer;
        private AnimationPresetInfo _absorbingAnimPreset;
        private AnimationPresetInfo _autoChargingPreset;
        private AnimationPresetInfo _breakingPreset;

        private DashPointIconState _iconState = DashPointIconState.None;
        private bool _iconStateChanged = false;
        private float _iconStateDelay = 0f;

        public DashPointIcon(Image ownerImage, string absorbingAniPath, string autoChargingAniPath, string breakingAniPath)
        {
            _image = ownerImage;
            _animationPlayer = new AnimationPlayer();
            _absorbingAnimPreset = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset($"{absorbingAniPath}/"), absorbingAniPath);
            _autoChargingPreset = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset($"{autoChargingAniPath}/"), autoChargingAniPath);
            _breakingPreset = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset($"{breakingAniPath}/"), breakingAniPath);

            _iconState = DashPointIconState.None;
            _iconStateChanged = false;
            _iconStateDelay = 0f;
        }
        
        public void Progress(float deltaTime)
        {
            UpdateIconState(deltaTime);

            _animationPlayer.progress(deltaTime, null);
            _image.sprite = _animationPlayer.getCurrentSprite();
            _image.SetNativeSize();
        }

        private void UpdateIconState(float deltaTime)
        {
            if(_iconStateChanged == false)
                return;

            _iconStateDelay -= deltaTime;
            if(_iconStateDelay > 0f)
                return;
            
            _iconStateDelay = 0f;
            switch(_iconState)
            {
                case DashPointIconState.None:
                break;
                case DashPointIconState.Absorbing:
                    Absorbing();
                break;
                case DashPointIconState.AutoCharging:
                    AutoCharging();
                break;
                case DashPointIconState.Breaking:
                    Breaking();
                break;
            }

            _iconStateChanged = false;
        }

        public void ChangeIconState(DashPointIconState iconState, float delay)
        {
            _iconState = iconState;
            _iconStateDelay = delay;

            _iconStateChanged = true;
        }

        public void Absorbing()
        {
            if (_animationPlayer.getCurrentAnimationName() == _absorbingAnimPreset._path)
            {
                return;
            }
            
            _animationPlayer.changeAnimationByCustomPreset(_absorbingAnimPreset._path, _absorbingAnimPreset._customPreset);
        }

        public void AutoCharging()
        {
            if (_animationPlayer.getCurrentAnimationName() == _autoChargingPreset._path ||
                _animationPlayer.getCurrentAnimationName() == _absorbingAnimPreset._path)
            {
                return;
            }
            
            _animationPlayer.changeAnimationByCustomPreset(_autoChargingPreset._path, _autoChargingPreset._customPreset);
        }

        public void Breaking()
        {
            if (_animationPlayer.getCurrentAnimationName() == _breakingPreset._path)
            {
                return;
            }
            
            _animationPlayer.changeAnimationByCustomPreset(_breakingPreset._path, _breakingPreset._customPreset);
        }
    }
}
