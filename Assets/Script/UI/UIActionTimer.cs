using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionTimer
{
    private Action _resultAction;

    private double _startTime;
    private double _endTime;

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
    private Action<double> _updateAction;
    private Action _endAction;

    private double _startTime;
    private double _endTime;
    private double _diff;

    public bool IsPlay => _isPlay;
    private bool _isPlay = false;

    public UIActionLerpTimer(Action<double> action, Action endAction = null)
    {
        _updateAction = action;
        _endAction = endAction;
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
        t = 1.0f - t;

        if (t >= 1.0f)
        {
            t = 1.0f;
            _isPlay = false;
            _endAction?.Invoke();
            return;
        }
        
        _updateAction?.Invoke(t);
        
    }
}

