using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public enum CameraModeType
{
    ArenaMode,
    TargetCenterMode,
    TwoTargetMode,
    Count,
};

public abstract class CameraModeBase
{
    public static Vector2Int _screenPixel = new Vector2Int(800, 600);
    
    public static float _pixelPerUnit = 100f;

    protected Vector3 _cameraPosition;
    protected ObjectBase _currentTarget;
    protected GameEntityBase _targetEntity;


    public abstract CameraModeType getCameraModeType();
    public abstract void initialize(Vector3 position);
    public abstract void progress(float deltaTime, Vector3 targetPosition);
    public abstract void release();

    public void setCurrentTargetEntity(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
    }

    public void setCurrentTarget(ObjectBase obj)
    {
        _currentTarget = obj;
    }

    public Vector3 getCameraPosition()
    {
        return _cameraPosition;
    }
};

public class CameraTwoTargetMode : CameraModeBase
{
    public float _targetPositionRatio = 0.25f;
    public static float _cameraMoveSpeedRate = 8.0f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = Vector3.Lerp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate * deltaTime);
        GizmoHelper.instance.drawLine(_currentCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }

    public void updateCameraCenter()
    {
        if(_targetEntity == null || _targetEntity.isDead())
            _currentCenterPosition = _currentTarget.transform.position;
        else
            _currentCenterPosition = _currentTarget.transform.position + (_targetEntity.transform.position - _currentTarget.transform.position) * _targetPositionRatio;
    }
};

public class CameraArenaMode : CameraModeBase
{
    public static float _cameraCenterRate = 0.1f;
    public static float _cameraMoveSpeedRate = 2f;
    private Vector3 _cameraCenterPosition = Vector3.zero;
    public override CameraModeType getCameraModeType() => CameraModeType.ArenaMode;
    public override void initialize(Vector3 position)
    {
        _cameraCenterPosition = position;

        if(_currentTarget != null && _currentTarget is GameEntityBase)
        {
            GameEntityBase target = ((GameEntityBase)_currentTarget).getCurrentTargetEntity();
            if(target != null && target.isDead() == false)
                _cameraCenterPosition = target.transform.position + (_currentTarget.transform.position - target.transform.position) * 0.5f;
        }

        _cameraPosition = position;
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {            
        Vector3 targetDirection = (targetPosition - _cameraCenterPosition);
        _cameraPosition = Vector3.Lerp(_cameraPosition, _cameraCenterPosition + (targetDirection * _cameraCenterRate), _cameraMoveSpeedRate * deltaTime);
        GizmoHelper.instance.drawLine(_cameraCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }
};

public class CameraTargetCenterMode : CameraModeBase
{
    public static float _cameraMoveSpeedRate = 8.0f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = Vector3.Lerp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate * deltaTime);
        GizmoHelper.instance.drawLine(_currentCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }

    public void updateCameraCenter()
    {
        if(_targetEntity == null || _targetEntity.isDead())
            _currentCenterPosition = _currentTarget.transform.position;
    }
};




public class CameraControlEx : Singleton<CameraControlEx>
{
    public float _cameraBoundRate = 0.85f;
    private Camera _currentCamera;
    private ObjectBase _currentTarget;

    private CameraModeBase _currentCameraMode;
    private Vector3 _cameraTargetPosition;

    private CameraModeBase[] _cameraModes;
    private GameEntityBase _cameraTargetEntity;

    private float _mainCamSize;
    private float _currentMainCamSize;
	private float _camWidth;
	private float _camHeight;

    private bool _enableShake = false;
    private float _shakeScale = 0f;
    private float _shakeTime = 0f;
    private float _shakeTimer = 0f;
    private float _shakeSpeed = 0f;

    private Vector3 _shakePosition = Vector3.zero;

    private Vector2 _cameraBoundHalf = Vector2.zero;

    public void initialize()
    {
        _currentCamera = Camera.main;

        _mainCamSize = _currentCamera.orthographicSize;
        _currentMainCamSize = _mainCamSize;

        _camHeight = _mainCamSize;
		_camWidth = _camHeight * ((float)Screen.width / (float)Screen.height);
        _cameraBoundHalf = new Vector2(_camWidth, _camHeight) * _cameraBoundRate;

        _cameraModes = new CameraModeBase[(int)CameraModeType.Count];

        setCameraMode(CameraModeType.TargetCenterMode);
    }

    public void progress(float deltaTime)
    {
        updateCameraMode(deltaTime);

        if(MathEx.equals(_currentCamera.orthographicSize,_currentMainCamSize,float.Epsilon) == true)
			_currentCamera.orthographicSize = _currentMainCamSize;
		else	
			_currentCamera.orthographicSize = Mathf.Lerp(_currentCamera.orthographicSize,_currentMainCamSize,4f * deltaTime);

        if(_enableShake)
        {
            _shakeTimer += deltaTime;
            if(_shakeTimer >= _shakeTime)
                _shakeTimer = _shakeTime;
            
            float factor = _shakeTimer * (1f / _shakeTime);
            float shakeScale = Mathf.Lerp(_shakeScale, 0f, factor);
            if(factor >= 1f)
                _enableShake = false;

            _shakePosition = MathEx.lemniscate(_shakeTimer * _shakeSpeed) * shakeScale;
        }

        GizmoHelper.instance.drawRectangle(_currentCamera.transform.position,_cameraBoundHalf,Color.green);
    }

    public void setShake(float scale, float speed, float time)
    {
        _shakeScale = scale;
        _shakeTime = time;
        _shakeSpeed = speed;
        _shakeTimer = 0f;

        _enableShake = true;

        _shakePosition = Vector3.zero;
    }

    public void Zoom(float scale)
	{
		if(_currentCamera.orthographicSize > scale)
			_currentCamera.orthographicSize = scale;
	}

    public void setZoomSize(float zoomSize)
    {
        _currentMainCamSize = zoomSize;
    }

    public void setDefaultZoomSize()
    {
        _currentMainCamSize = _mainCamSize;
    }

    public void setCameraMode(CameraModeType mode)
    {
        if(_currentCamera == null || _currentTarget == null)
            return;

        if(_currentCameraMode != null && _currentCameraMode.getCameraModeType() == mode)
            return;

        if(_currentCameraMode != null)
            _currentCameraMode.release();

        int index = (int)mode;
        if(_cameraModes[index] == null)
        {
            switch(mode)
            {
                case CameraModeType.ArenaMode:
                    _cameraModes[index] = new CameraArenaMode();
                break;
                case CameraModeType.TargetCenterMode:
                    _cameraModes[index] = new CameraTargetCenterMode();
                break;
                case CameraModeType.TwoTargetMode:
                    _cameraModes[index] = new CameraTwoTargetMode();
                break;
                default:
                    DebugUtil.assert(false, "잘못된 카메라 모드입니다. 이게 뭐징");
                break;
            }
        }

        _currentCameraMode = _cameraModes[index];
        _currentCameraMode.setCurrentTarget(_currentTarget);
        _currentCameraMode.initialize(_currentCamera.transform.position);
    }

    public void setCameraTarget(ObjectBase obj)
    {
        _currentTarget = obj;
        _currentCameraMode?.setCurrentTarget(_currentTarget);
        _currentCameraMode?.initialize(_currentTarget.transform.position);
    }

    private void updateCameraMode(float deltaTime)
    {
        if(_currentCameraMode == null || _currentTarget == null)
            return;

        if(_currentTarget is GameEntityBase)
            _currentCameraMode.setCurrentTargetEntity(_cameraTargetEntity);
        else
            _currentCameraMode.setCurrentTargetEntity(null);

        _currentCameraMode.progress(deltaTime,_currentTarget.transform.position);
    }

    public void SyncPosition()
    {
        if(_currentCamera == null || _currentCameraMode == null)
            return;

        Vector3 currentPosition = _currentCameraMode.getCameraPosition();
        currentPosition.z = -10f;
        _currentCamera.transform.position = currentPosition + _shakePosition;
    }

    public bool IsInCameraBound(Vector3 pos)
	{
		var bound = GetCamBounds();
		return (pos.x >= bound.x && pos.x <= bound.y && pos.y >= bound.z && pos.y <= bound.w);
	}

    public Vector4 GetCamBounds() //x min x max y min y max
	{
        Vector3 position = _currentCamera.transform.position;
		return new Vector4(position.x - _cameraBoundHalf.x, position.x + _cameraBoundHalf.x,
							position.y - _cameraBoundHalf.y, position.y + _cameraBoundHalf.y);
	}
}
