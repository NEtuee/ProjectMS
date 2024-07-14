using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossHpUI : IUIElement
{
    private BossHpUIBinder _binder;

    private bool _active = false;
    private GameEntityBase _target;

    private UIActionLerpTimer _appearTimer;
    private UIActionLerpTimer _hpFillTimer;

    private float _prevPercentage = 0.0f;
    private float _fromPercentage = 0.0f;
    private float _toPercentage = 0.0f;
    private UIActionLerpTimer _hitTimer;

    private UIActionLerpTimer _shakeTimer;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "보스 hp ui 바인더가 없음";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as BossHpUIBinder;
    }

    public void Initialize()
    {
        _binder.Root.gameObject.SetActive(false);
        _binder.InGauge.fillAmount = 0.0f;
        _binder.OutGauge.fillAmount = 0.0f;

        _appearTimer = new UIActionLerpTimer(UpdateAppearHpBar, PlayFillHpBar);
        _hpFillTimer = new UIActionLerpTimer(UpdateFillHpBar);

        _hitTimer = new UIActionLerpTimer(UpdateHitTimer);

        _shakeTimer = new UIActionLerpTimer(UpdateShake, EndShake);
    }

    public void UpdateByManager()
    {
        if (_active == false || _target == null)
        {
            return;
        }
        
        _appearTimer.Update();
        _hpFillTimer.Update();

        if (_appearTimer.IsPlay == false && _hpFillTimer.IsPlay == false)
        {
            var curHpPercentage = _target.getStatusPercentage("HP");
            if (Mathf.Abs(_prevPercentage - curHpPercentage) > Mathf.Epsilon)
            {
                _fromPercentage = _prevPercentage;
                _toPercentage = curHpPercentage;
                _prevPercentage = curHpPercentage;
                _binder.OutGauge.fillAmount = curHpPercentage;
                PlayHitTimer();

                if (Mathf.Abs(_toPercentage - 1.0f) > Mathf.Epsilon)
                {
                    PlayShake();
                }
            }
            
            _hitTimer.Update();
            _shakeTimer.Update();
        }
    }

    public void Active(GameEntityBase target)
    {
        _active = true;
        _binder.Root.gameObject.SetActive(true);
        _target = target;
        
        PlayAppearHpBar();
    }

    public void Disable()
    {
        _active = false;
        _target = null;
        _binder.Root.gameObject.SetActive(false);
        
        _appearTimer.Stop();
        _hpFillTimer.Stop();
        _hitTimer.Stop();
    }

    private void PlayAppearHpBar()
    {
        //Debug.Log("PlayAppearHp");
        _binder.CanvasGroup.alpha = 0.0f;
        _appearTimer.Play(0.5f);
    }

    private void UpdateAppearHpBar(double t)
    {
        //Debug.Log("UpdateAppearHp");
        _binder.CanvasGroup.alpha = (float)t;
    }

    private void PlayFillHpBar()
    {
        //Debug.Log("PlayFillHp");
        _binder.InGauge.fillAmount = 0.0f;
        _binder.OutGauge.fillAmount = 0.0f;
        _hpFillTimer.Play(0.5f);
    }

    private void UpdateFillHpBar(double t)
    {
        //Debug.Log("UpdateFillHp");
        var value = MathEx.easeOutExpo(0, 1, (float)t);
        _binder.InGauge.fillAmount = value;
        _binder.OutGauge.fillAmount = value;
    }

    private void PlayHitTimer()
    {
        _hitTimer.Play(0.8f);
    }

    private void UpdateHitTimer(double t)
    {
        var value = MathEx.easeOutExpo(_fromPercentage, _toPercentage, (float)t);
        _binder.InGauge.fillAmount = value;
    }

    private void PlayShake()
    {
        _shakeTimer.Play(0.15f);
    }
    
    private void UpdateShake(double t)
    {
        _binder.Root.localPosition = (Vector2)(Random.insideUnitCircle * 7.0f);
    }

    private void EndShake()
    {
        _binder.Root.localPosition = Vector3.zero;
    }
}
