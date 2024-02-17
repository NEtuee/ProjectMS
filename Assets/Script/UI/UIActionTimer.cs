using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionTimer
{
    private Action _resultAction;

    private float _startTime;
    private float _endTime;

    private bool _isPlay = false;

    public UIActionTimer(Action action)
    {
        _resultAction = action;
    }

    public void Play(float duration)
    {
        _isPlay = true;
        _startTime = GlobalTimer.Instance().getScaledGlobalTime();
        _endTime = _startTime + duration;
    }

    public void Stop()
    {
        _isPlay = false;
    }

    public void Update()
    {
        if (_isPlay == false)
        {
            return;
        }
        
        var curTime = GlobalTimer.Instance().getScaledGlobalTime();
        if (curTime >= _endTime)
        {
            _resultAction?.Invoke();
            _isPlay = false;
        }
    }
}

public class UIActionLerpTimer
{
    private Action<float> _updateAction;

    private float _startTime;
    private float _endTime;
    private float _diff;

    private bool _isPlay = false;

    public UIActionLerpTimer(Action<float> action)
    {
        _updateAction = action;
    }

    public void Play(float duration)
    {
        _isPlay = true;
        _startTime = GlobalTimer.Instance().getScaledGlobalTime();
        _endTime = _startTime + duration;
        _diff = _endTime - _startTime;
    }

    public void Stop()
    {
        _isPlay = false;
    }

    public void Update()
    {
        if (_isPlay == false)
        {
            return;
        }
        
        var curTime = GlobalTimer.Instance().getScaledGlobalTime();
        var curDiff = _endTime - curTime;

        var t = curDiff / _diff;

        if (t >= 1.0f)
        {
            t = 1.0f;
            _isPlay = false;
        }
        
        _updateAction?.Invoke(t);
        
    }
}

