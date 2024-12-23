using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;

public enum CameraModeType
{
    ArenaMode,
    TargetCenterMode,
    TwoTargetMode,
    PositionMode,
    Count,
};

public abstract class CameraModeBase
{
    public static Vector2Int _screenPixel = new Vector2Int(800, 600);
    
    public static float _pixelPerUnit = 100f;

    protected Vector3 _cameraPosition;
    protected Vector3 _currentTargetPosition;
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

    public void setCurrentTargetPosition(Vector3 targetPosition)
    {
        _currentTargetPosition = targetPosition;
    }

    public Vector3 getCameraPosition()
    {
        return _cameraPosition;
    }

    public virtual Vector3 getVirtualCameraPosition()
    {
        return _cameraPosition;
    }

    public void setCameraPosition(Vector3 cameraPosition)
    {
        _cameraPosition = cameraPosition;
    }
};

public class CameraTwoTargetMode : CameraModeBase
{
    public float _targetPositionRatio = 0.25f;
    public static float _cameraMoveSpeedRate = 7.3f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        //_cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = MathEx.damp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate, deltaTime);
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

public class CameraPositionMode : CameraModeBase
{
    public float _targetPositionRatio = 0.25f;
    public static float _cameraMoveSpeedRate = 8.0f;

    public override CameraModeType getCameraModeType() => CameraModeType.PositionMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        _cameraPosition = Vector3.Lerp(_cameraPosition, _currentTargetPosition, _cameraMoveSpeedRate * deltaTime);
    }

    public override void release()
    {
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
    public static float _cameraMoveSpeedRate = 7.3f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        if(_currentTarget != null)
            _currentCenterPosition = position;
        else            
            _cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = MathEx.damp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate, deltaTime);
        GizmoHelper.instance.drawLine(_currentCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }

    public override Vector3 getVirtualCameraPosition()
    {
        return getCameraTargetPosition();
    }

    public Vector3 getCameraTargetPosition()
    {
        if((_targetEntity == null || _targetEntity.isDead()) && _currentTarget != null)
        {
            return _currentTarget.transform.position;
        }

        return getCameraPosition();
    }

    public void updateCameraCenter()
    {
        if((_targetEntity == null || _targetEntity.isDead()) && _currentTarget != null)
        {
            _currentCenterPosition = _currentTarget.transform.position;
            _currentCenterPosition = MasterManager.instance._stageProcessor.getCameraCenterPosition(_currentCenterPosition);
        }
    }
};




public class CameraControlEx : Singleton<CameraControlEx>
{
    private const float _backgroundTextureSize = 1024f;

    public float _cameraBoundRate = 0.75f;
    private Camera _currentCamera;
    private ObjectBase _currentTarget;
    private ObjectBase _currentUVTarget;

    private CameraModeBase _currentCameraMode;
    private Vector3 _cameraTargetPosition;

    private CameraModeBase[] _cameraModes;
    private GameEntityBase _cameraTargetEntity;

    private PostProcessProfileControl _postProcessProfileControl;

    private float _mainCamSize;
    private float _currentMainCamSize;
    private float _currentZoomSpeed;
	private float _camWidth;
	private float _camHeight;

    private bool _enableShake = false;
    private float _shakeScale = 0f;
    private float _shakeTime = 0f;
    private float _shakeTimer = 0f;
    private float _shakeSpeed = 0f;

    private Vector3 _shakePosition = Vector3.zero;
    private Vector3 _uvTargetWorldPosition = Vector3.zero;

    private Vector2 _cameraBoundHalf = Vector2.zero;

    private bool _debugCameraMode = false;
    private Vector3 _debugCameraPosition = Vector3.zero;
    private bool _debugModeInput = false;
    private float _debugCameraMoveSpeed = 2f;
    private float _debugCameraMoveSpeedMultiflier = 4f;

    public void initialize()
    {
        _currentCamera = Camera.main;

        _mainCamSize = _currentCamera.orthographicSize;
        _currentMainCamSize = _mainCamSize;
        _currentZoomSpeed = 4f;

        updateCameraSize();

        _cameraModes = new CameraModeBase[(int)CameraModeType.Count];

        _postProcessProfileControl = new PostProcessProfileControl();
        _postProcessProfileControl.updateMaterial(false);

        _currentUVTarget = null;

        setCameraMode(CameraModeType.TargetCenterMode);
    }

    public void updateCameraSize()
    {
        _camHeight = _currentCamera.orthographicSize * 2f;
        _camWidth = _camHeight * (800f / 600f);
        _cameraBoundHalf = new Vector2(_camWidth, _camHeight) * 0.5f * _cameraBoundRate;
    }

    public void updateDebugMode()
    {
        if(_debugCameraMode)
        {
            bool debugModeInput = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Comma);
            if(debugModeInput && _debugModeInput == false)
            {
                ToastMessage._instance.ShowToastMessage("Free Camera Mode Disabled", 1f, Color.red);
                _debugCameraMode = false;
            }

            _debugModeInput = debugModeInput;

            if(_debugCameraMode)
                updateDebugCamera();
        }
        else
        {
            bool debugModeInput = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Comma);
            if(debugModeInput && _debugModeInput == false)
            {
                _debugCameraPosition = _currentCamera.transform.position;
                _debugCameraMode = true;

                ToastMessage._instance.ShowToastMessage("Free Camera Mode Enabled", 1f, Color.green);
            }

            _debugModeInput = debugModeInput;
        }
    }

    public void progress(float deltaTime)
    {
        if(_debugCameraMode)
            return;

        updateCameraMode(deltaTime);
        _postProcessProfileControl.processBlend(deltaTime);

        if(MathEx.equals(_currentCamera.orthographicSize,_currentMainCamSize,float.Epsilon) == true)
			_currentCamera.orthographicSize = _currentMainCamSize;
		else	
			_currentCamera.orthographicSize = Mathf.Lerp(_currentCamera.orthographicSize,_currentMainCamSize,_currentZoomSpeed * deltaTime);

        updateCameraSize();

        if (_enableShake)
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

        updateCameraUVTarget();

        GizmoHelper.instance.drawRectangle(_currentCamera.transform.position,_cameraBoundHalf,Color.green);
    }

    public Vector2 worldToBackgroundUV(Vector3 worldPosition)
    {
        float convertedByPPU = _backgroundTextureSize * 0.01f;
        Vector2 position = worldPosition - _currentCamera.transform.position;

        return position * (1f / convertedByPPU) + Vector2.one * 0.5f;
    }

    private void updateDebugCamera()
    {
        float debugCameraMoveSpeed = _debugCameraMoveSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
            debugCameraMoveSpeed *= _debugCameraMoveSpeedMultiflier;

        if(Input.GetKey(KeyCode.UpArrow))
            _debugCameraPosition += Vector3.up * debugCameraMoveSpeed * Time.deltaTime;
        if(Input.GetKey(KeyCode.DownArrow))
            _debugCameraPosition += Vector3.down * debugCameraMoveSpeed * Time.deltaTime;
        if(Input.GetKey(KeyCode.LeftArrow))
            _debugCameraPosition += Vector3.left * debugCameraMoveSpeed * Time.deltaTime;
        if(Input.GetKey(KeyCode.RightArrow))
            _debugCameraPosition += Vector3.right * debugCameraMoveSpeed * Time.deltaTime;

        _currentCamera.transform.position = _debugCameraPosition;
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
        {
			_currentCamera.orthographicSize = _currentMainCamSize + scale;
            _currentZoomSpeed = 4f;
        }
	}

    public void setZoomSize(float zoomSize, float speed)
    {
        _currentMainCamSize = zoomSize;
        _currentZoomSpeed = speed;
    }

    public void setDefaultZoomSize()
    {
        _currentMainCamSize = _mainCamSize;
        _currentZoomSpeed = 4f;
    }

    public void setZoomSizeForce(float zoomSize, float speed)
    {
        setZoomSize(zoomSize, speed);
        _currentCamera.orthographicSize = zoomSize;
    }

    public void clearCamera(Vector3 position)
    {
        _currentCameraMode = null;
        _currentTarget = null;
        _currentUVTarget = null;

        setDefaultZoomSize();

        Vector3 currentPosition = position;
        currentPosition.z = -10f;
        setCameraPosition(currentPosition);

        CameraControlEx.Instance().setCameraMode(CameraModeType.PositionMode);
        CameraControlEx.Instance().setCameraTargetPosition(_currentCamera.transform.position);
    }

    public void updateCameraUVTarget()
    {
        if(_currentUVTarget != null)
            _uvTargetWorldPosition = _currentUVTarget.transform.position;

        _postProcessProfileControl.applyCenterUVPosition(worldToBackgroundUV(_uvTargetWorldPosition));
    }

    public void setCameraUVTarget(ObjectBase uvTarget)
    {
        _currentUVTarget = uvTarget;
        updateCameraUVTarget();
    }

    public void setCameraMode(CameraModeType mode)
    {
        if(_currentCamera == null )
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
                case CameraModeType.PositionMode:
                    _cameraModes[index] = new CameraPositionMode();
                break;
                default:
                    DebugUtil.assert(false, "잘못된 카메라 모드입니다. 이게 뭐징");
                break;
            }
        }

        _currentCameraMode = _cameraModes[index];
        _currentCameraMode.setCurrentTarget(_currentTarget);
        _currentCameraMode.setCurrentTargetPosition(_cameraTargetPosition);
        _currentCameraMode.initialize(_currentCamera.transform.position);
    }

    public void setCameraTarget(ObjectBase obj)
    {
        _currentTarget = obj;
        _currentCameraMode?.setCurrentTarget(_currentTarget);
        _currentCameraMode?.initialize(_currentTarget.transform.position);
    }

    public void setCameraTargetPosition(Vector3 targetPosition)
    {
        _cameraTargetPosition = targetPosition;
        _currentCameraMode?.setCurrentTargetPosition(_cameraTargetPosition);
    }

    private void updateCameraMode(float deltaTime)
    {
        if(_currentCameraMode == null)
            return;

        if(_currentTarget is GameEntityBase)
            _currentCameraMode.setCurrentTargetEntity(_cameraTargetEntity);
        else
            _currentCameraMode.setCurrentTargetEntity(null);

        _currentCameraMode.progress(deltaTime,_currentTarget == null ? _cameraTargetPosition : _currentTarget.transform.position);
    }

    public Vector3 getRealCameraPosition()
    {
        return getCameraPosition() + _shakePosition;
    }

    public Vector3 getCameraPosition()
    {
        Vector3 currentPosition = _currentCameraMode == null ? _currentCamera.transform.position : _currentCameraMode.getCameraPosition();
        currentPosition.z = -10f;
        return currentPosition;
    }

    public Vector3 getVirtualCameraPosition()
    {
        Vector3 currentPosition = _currentCameraMode == null ? _currentCamera.transform.position : _currentCameraMode.getVirtualCameraPosition();
        currentPosition.z = -10f;
        return currentPosition;
    }

    public void setCameraPosition(Vector3 position)
    {
        if(_debugCameraMode)
            return;

        _currentCamera.transform.position = position + _shakePosition;
    }

    public void SyncPosition()
    {
        if(_debugCameraMode)
            return;

        if(_currentCamera == null || _currentCameraMode == null)
            return;

        _currentCamera.transform.position = getCameraPosition() + _shakePosition;
    }

    public bool isCameraTargetObject(ObjectBase targetObject)
    {
        return _currentTarget == targetObject;
    }

    public bool IsInCameraBound(Vector3 position, out Vector3 normal)
    {
        Vector3 inPosition = new Vector3();
        return IsInCameraBound(position, Camera.main.transform.position, out inPosition, out normal);
    }

    public bool IsInCameraBound(Vector3 position, Vector3 cameraPosition, out Vector3 cameraInPosition, out Vector3 normal)
	{
		var bound = GetCamBounds(cameraPosition);
        cameraInPosition = position;

        normal = Vector3.zero;
        
        if(position.x < bound.x)
        {
            cameraInPosition.x = bound.x;
            normal = Vector3.right;
        }
        else if(position.x > bound.y)
        {
            cameraInPosition.x = bound.y;
            normal = Vector3.left;
        }

        if(position.y < bound.z)
        {
            cameraInPosition.y = bound.z;
            normal = Vector3.up;
        }
        else if(position.y > bound.w)
        {
            cameraInPosition.y = bound.w;
            normal = Vector3.down;
        }

		return (position.x >= bound.x && position.x <= bound.y && position.y >= bound.z && position.y <= bound.w);
	}

    public Vector3 getRandomPositionInCamera()
    {
        return new Vector3(Random.Range(-_camWidth * 0.5f,_camWidth * 0.5f),Random.Range(-_camHeight * 0.5f,_camHeight * 0.5f));
    }

    public Vector4 GetCamBounds(in Vector3 position) //x min x max y min y max
	{
        float xmin = position.x - _cameraBoundHalf.x;
        float xmax = position.x + _cameraBoundHalf.x;
        float ymin = position.y - _cameraBoundHalf.y;
        float ymax = position.y + _cameraBoundHalf.y;

        return new Vector4(xmin, xmax, ymin, ymax);
	}

    public PostProcessProfileControl getPostProcessProfileControl()
    {
        return _postProcessProfileControl;
    }
    
    public Camera getCurrentCamera()
    {
        return _currentCamera;
    }
}
