using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : IUIElement
{
    private PauseUIBinder _binder;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "타이틀 메뉴 ui 바인더가 없음";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as PauseUIBinder;
    }

    public void Initialize()
    {
    }

    public bool IsActive()
    {
        return _binder.gameObject.activeSelf;
    }

    public void ActivePauseUI(bool active)
    {
        _binder.gameObject.SetActive(active);
    }
}
