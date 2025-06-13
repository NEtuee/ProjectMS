using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashPointUI : IUIElement
{
    private DashPointUIBinder _binder;

    private int _prevDashPointInt;

    private int maxDP;

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

    }

    public void InitValue(float dashPoint)
    {
        maxDP = (int)dashPoint;
        SetDashPoint(maxDP);
    }

    public void UpdateByManager(float deltaTime, float dashPoint)
    {
        var currentDashPoint = (int)dashPoint;
        if (_prevDashPointInt != currentDashPoint)
        {
            SetDashPoint(currentDashPoint);
            _prevDashPointInt = currentDashPoint;
        }
    }

    private void SetDashPoint(int point)
    {
        var activatedDP = point;
        var deactivatedDP = maxDP - point;
        var additionalDP = Mathf.Max(0, point - maxDP);
        for (int i = 0; i < activatedDP; i++)
        {
            _binder.DashPointImages[i].gameObject.SetActive(true);
        }

        for (int i = activatedDP; i < maxDP; i++)
        {
            _binder.DashPointImages[i].gameObject.SetActive(false);
        }

        if (additionalDP > 0)
        {
            _binder._additionalText.gameObject.SetActive(true);
            _binder._additionalText.text = "+" + (additionalDP);
        }
        else
        {
            _binder._additionalText.gameObject.SetActive(false);
        }

        _prevDashPointInt = point;
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