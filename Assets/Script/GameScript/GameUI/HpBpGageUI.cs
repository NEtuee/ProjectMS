using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBpGageUI : IUIElement
{
    private HpBpGageUIBinder _binder;
    
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
        _binder = binder as HpBpGageUIBinder; 
    }

    public void Initialize()
    {
        _binder.HpGageImage.type = Image.Type.Filled;
        _binder.BpGageImgae.type = Image.Type.Filled;
    }

    public void InitValue(float hpPercentage, float bpPercentage)
    {
        UpdateByManager(hpPercentage, bpPercentage);
    }

    public void UpdateByManager(float hpPercentage, float bpPercentage)
    {
        _binder.HpGageImage.fillAmount = hpPercentage;
        _binder.BpGageImgae.fillAmount = bpPercentage;
    }
}
