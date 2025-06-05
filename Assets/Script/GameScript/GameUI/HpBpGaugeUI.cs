using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBpGaugeUI : IUIElement
{
    private HpBpGaugeUIBinder _binder;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "hp bp 게이지 바인더가 연결되어 있지 않습니다.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as HpBpGaugeUIBinder; 
    }

    public void Initialize()
    {
        _binder.HpGaugeImage.type = Image.Type.Filled;
        _binder.BpGaugeImage.type = Image.Type.Filled;
    }

    public void InitValue(float hpPercentage, float bpPercentage)
    {
        UpdateByManager(hpPercentage, bpPercentage, 0.0f, 0.0f);
    }

    public void UpdateByManager(float hpPercentage, float bpPercentage, float catchPercentage, float stunPercentage)
    {
        _binder.HpGaugeImage.fillAmount = hpPercentage;
        _binder.BpGaugeImage.fillAmount = bpPercentage;
        _binder.QteImage.fillAmount = MathEx.clamp01f(catchPercentage + stunPercentage);
    }
}
