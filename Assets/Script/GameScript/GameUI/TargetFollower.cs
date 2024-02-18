using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : IUIElement
{
    private TargetFollowerBinder _binder;

    private float _clampWidth;
    private float _clampHeight;
    private float _fadeDist;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "팔로우 바인더가 셋팅되지 않았습니다.";
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
        _clampWidth = Screen.width;
        _clampHeight = Screen.height;
        _fadeDist = _binder.HeightAdjust * _binder.HeightAdjust;
    }

    public void UpdateByManager(Vector3 characterPosition)
    {
        
        var characterScreenPos = Camera.main.WorldToScreenPoint(characterPosition);
        characterScreenPos.z = 0.0f;

        var targetPosition = Camera.main.WorldToScreenPoint(characterPosition + _binder.Offset);
        targetPosition.z = 0.0f;

        var deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        // Vector3 velocity = Vector3.zero;
        // var currentPosition = _binder.FollowObject.transform.position;
        // var dampingPos =  Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, _binder.Smooth, Mathf.Infinity, deltaTime);
        // dampingPos = new Vector3(Mathf.Clamp(dampingPos.x, _binder.WidthAdjust, _clampWidth - _binder.WidthAdjust), Mathf.Clamp(dampingPos.y, _binder.HeightAdjust, _clampHeight - _binder.HeightAdjust), 0.0f);
        // _binder.FollowObject.transform.position = dampingPos;

        if ((characterScreenPos - _binder.FollowObject.transform.position).sqrMagnitude <= _fadeDist)
        {
            _binder.Group.alpha = 0.3f;
        }
        else
        {
            _binder.Group.alpha = 1.0f;
        }
        //Debug.Log(dampingPos);
    }
}
