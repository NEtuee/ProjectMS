using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowUI : IUIElement
{
    private ArrowUIBinder _binder;
    private Vector3[,] _screenSectors = new Vector3[4,2];
    private Camera _mainCamera;
    private Vector2 _cameraSizeHalf;
    private readonly float _magnitude = 0.9f;
    private bool _active;
    private Vector3 _direction;

    public bool CheckValidBinderLink(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as ArrowUIBinder;
    }

    public void Initialize()
    {
        _mainCamera = Camera.main;
    }

    public void ActiveArrow()
    {
        _active = true;
    }

    public void UpdateByManager()
    {
        if (_active == false)
            return;

        if(MasterManager.instance._stageProcessor.isValid() == false)
        {
            _binder.Arrow.SetActive(false);
            return;
        }

        if(MasterManager.instance._stageProcessor.getNextPointDirection(ref _direction) == false)
        {
            _binder.Arrow.SetActive(false);
            return;
        }

        Vector3 nextPoint = new Vector3();
        MasterManager.instance._stageProcessor.getNextPoint(ref nextPoint);
        if(CameraControlEx.Instance().IsInCameraBound(nextPoint, out Vector3 normal))
        {
            _binder.Arrow.SetActive(false);
            return;
        }

        UpdateCameraSize();
        var center = _mainCamera.transform.position;
        
        CalculateScreenSectors(center);
        var position = GetSectorPosition(center, center + _direction * 1000);

        _binder.Arrow.SetActive(true);
        _binder.Arrow.transform.position = position;
        _binder.Arrow.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(_direction));
    }

    public void DisableArrow()
    {
        _binder.Arrow.SetActive(false);
        _active = false;
    }
    
    private void UpdateCameraSize()
    {
        if (_mainCamera == null)
        {
            return;
        }

        float orthoSize = _mainCamera.orthographicSize;
        float aspectRatio = _mainCamera.aspect;
        float height = orthoSize * 2f;
        float width = height * aspectRatio;

        _cameraSizeHalf.x = width * 0.5f;
        _cameraSizeHalf.y = height * 0.5f;
    }
    
    private void CalculateScreenSectors(Vector3 center)
    {
        Vector2 cameraSize = _cameraSizeHalf * _magnitude;

        //left
        _screenSectors[0,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[0,1] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;

        //right
        _screenSectors[1,0] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[1,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;

        //up
        _screenSectors[2,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[2,1] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;

        //down
        _screenSectors[3,0] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;
        _screenSectors[3,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;
    }
    
    private Vector3 GetSectorPosition(Vector3 center, Vector3 targetPosition)
    {
        for(int index = 0; index < 4; ++index)
        {
            if(MathEx.findLineIntersection(_screenSectors[index,0],_screenSectors[index,1],center,targetPosition,out var outInterSectionPosition) == false)
                continue;
            
            return new Vector3(outInterSectionPosition.x,outInterSectionPosition.y,0f);
        }

        return center;
    }
}
