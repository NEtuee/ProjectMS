using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : IUIElement
{
    private TargetFollowerBinder _binder;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "크로스헤어 UI에 바인더가 셋팅되지 않았습니다.";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as TargetFollowerBinder;
    }

    public void Initialize()
    {
    }

    public void UpdateByManager(Vector3 characterPosition)
    {
        var characterScreenPos = Camera.main.WorldToScreenPoint(characterPosition + _binder.Offset);
        characterScreenPos.z = 0.0f;
        //Debug.Log(characterScreenPos);

        var targetPosition = characterScreenPos;

        var deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        Vector3 velocity = Vector3.zero;
        var currentPosition = _binder.FollowObject.transform.position;
        _binder.FollowObject.transform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, _binder.Smooth, Mathf.Infinity, deltaTime);
    }
}
