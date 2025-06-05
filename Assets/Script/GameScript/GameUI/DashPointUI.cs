using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashPointUI : IUIElement
{
    private DashPointUIBinder _binder;

    private int _prevDashPointInt;

    private AnimationPlayer[] _animationPlayers = new AnimationPlayer[4];
    private AnimationCustomPreset _customPreset;
    
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
        _customPreset = ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AkaneStatusBar/DP/1/AttackRecovering/");

        for (int i = 0; i < 4; ++i)
        {
            _animationPlayers[i] = new AnimationPlayer();
            _animationPlayers[i].initialize();
            _animationPlayers[i].changeAnimationByCustomPreset("Sprites/UI/AkaneStatusBar/DP/1/AttackRecovering/", _customPreset);
        }
    }

    public void InitValue(float dashPoint)
    {
        _prevDashPointInt = (int)dashPoint;
        UpdateDashPoint(_prevDashPointInt);
    }

    public void UpdateByManager(float deltaTime, float dashPoint, float blood)
    {
        var currentDashPoint = (int)dashPoint;
        if (_prevDashPointInt != currentDashPoint)
        {
            UpdateDashPoint(currentDashPoint);
            _prevDashPointInt = currentDashPoint;
        }

        for (int i = 0; i < 4; ++i)
        {
            _animationPlayers[i].progress(deltaTime, null);
            _binder.DashPointImages[i].sprite = _animationPlayers[i].getCurrentSprite();
            _binder.DashPointImages[i].SetNativeSize();
        }
    }

    private void UpdateDashPoint(int point)
    {
        switch (point)
        {
            case 0 :
                SetDashPoint0();
                break;
            case 1 :
                SetDashPoint1();
                break;
            case 2 :
                SetDashPoint2();
                break;
            case 3 :
                SetDashPoint3();
                break;
            case 4 :
                SetDashPoint4();
                break;
            default:
                SetDashPoint4();
                _binder._additionalText.gameObject.SetActive(true);
                _binder._additionalText.text = "+" + (point - 4);
                break;
        }
    }

    private void SetDashPoint0()
    {
        _binder._additionalText.gameObject.SetActive(false);
        _binder.DashPointImages[0].gameObject.SetActive(false);
        _binder.DashPointImages[1].gameObject.SetActive(false);
        _binder.DashPointImages[2].gameObject.SetActive(false);
        _binder.DashPointImages[3].gameObject.SetActive(false);
    }

    private void SetDashPoint1()
    {
        _binder._additionalText.gameObject.SetActive(false);
        _binder.DashPointImages[0].gameObject.SetActive(true);
        _binder.DashPointImages[1].gameObject.SetActive(false);
        _binder.DashPointImages[2].gameObject.SetActive(false);
        _binder.DashPointImages[3].gameObject.SetActive(false);

        _binder.DashPointImages[0].color = _binder._color1;
    }
    
    private void SetDashPoint2()
    {
        _binder._additionalText.gameObject.SetActive(false);
        _binder.DashPointImages[0].gameObject.SetActive(true);
        _binder.DashPointImages[1].gameObject.SetActive(true);
        _binder.DashPointImages[2].gameObject.SetActive(false);
        _binder.DashPointImages[3].gameObject.SetActive(false);
        
        _binder.DashPointImages[0].color = _binder._color2;
        _binder.DashPointImages[1].color = _binder._color1;
    }
    
    private void SetDashPoint3()
    {
        _binder._additionalText.gameObject.SetActive(false);
        _binder.DashPointImages[0].gameObject.SetActive(true);
        _binder.DashPointImages[1].gameObject.SetActive(true);
        _binder.DashPointImages[2].gameObject.SetActive(true);
        _binder.DashPointImages[3].gameObject.SetActive(false);
        
        _binder.DashPointImages[0].color = _binder._color3;
        _binder.DashPointImages[1].color = _binder._color2;
        _binder.DashPointImages[2].color = _binder._color1;
    }
    
    private void SetDashPoint4()
    {
        _binder._additionalText.gameObject.SetActive(false);
        _binder.DashPointImages[0].gameObject.SetActive(true);
        _binder.DashPointImages[1].gameObject.SetActive(true);
        _binder.DashPointImages[2].gameObject.SetActive(true);
        _binder.DashPointImages[3].gameObject.SetActive(true);
        
        _binder.DashPointImages[0].color = _binder._color4;
        _binder.DashPointImages[1].color = _binder._color3;
        _binder.DashPointImages[2].color = _binder._color2;
        _binder.DashPointImages[3].color = _binder._color1;
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
