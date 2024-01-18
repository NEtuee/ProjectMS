using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrosshairUI : IUIElement
{
    public enum State
    {
        Idle,
        IdleToDetect,
        DetectToIdle,
        Detect
    }
    
    private CrosshairUIBinder _binder;

    private readonly List<GameObject> _guidLineObjects = new List<GameObject>();
    private readonly List<SpriteRenderer> _gageObjects = new List<SpriteRenderer>();

    private readonly float _startPosition = -0.36f;
    private readonly float _endPosition = -0.12f;
    private float _squreStartPosition;
    private float _posDiff;

    private List<CharacterEntityBase> _detectList = new List<CharacterEntityBase>();

    private float _attackRadius;
    private float _attackAngle;
    private float _attackStartDistance;
    private float _attackRayRadius;

    private CollisionInfoData _collisionInfoData;
    private CollisionInfo _collisionInfo;

    private bool _detectFlag = false;
    private bool _enemyDetect = false;
    private bool _prevDetect = false;

    private State _state = State.Idle;
    private bool _tweeningNow = false;

    private float _completeTime;
    
    private Color _mainCursorTargetColor;
    private Color _subCursorTargetColor;
    private float _colorCompleteTime;

    private Coroutine _idleToDetectColorCoroutine;
    
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
        _binder = binder as CrosshairUIBinder; 
    }

    public void Initialize()
    {
        _posDiff = _endPosition - _startPosition;
        _binder.SubMarker.transform.localPosition = new Vector3(_startPosition, 0, 0);

        _squreStartPosition = _startPosition * _startPosition;
        
        AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AttackPreset") as AttackPreset;
        AttackPresetData presetData = preset.getPresetData("PlayerAttackTest");

        if (presetData != null)
        {
            _attackRadius = presetData._attackRadius;
            _attackAngle = presetData._attackAngle;
            _attackStartDistance = presetData._attackStartDistance;
            _attackRayRadius = presetData._attackRayRadius;
            
            _collisionInfoData = new CollisionInfoData(_attackRadius, _attackAngle, _attackStartDistance, _attackRayRadius, CollisionType.Attack);
            _collisionInfo = new CollisionInfo(_collisionInfoData);
        }

        _mainCursorTargetColor = _binder.MainCusor.color = _binder.IdleColor;
        _subCursorTargetColor = _binder.SubCusor.color = _binder.SubIdleColor;

        _colorCompleteTime = Time.time;
    }

    public void InitValue(GameEntityBase ownerEntity, Vector3 targetPosition)
    {
        UpdateByManager(ownerEntity,false, targetPosition);
    }

    public void UpdateByManager(GameEntityBase ownerEntity, bool isDead, Vector3 targetPosition)
    {
        var mainCamera = Camera.main;

        if (mainCamera == null || isDead == true)
        {
            return;
        }

        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldMousePosition.z = 0.0f;

        var characterToMouse = worldMousePosition - targetPosition;

        Vector3 toMouseVector = worldMousePosition - targetPosition;
        Quaternion rotation = Quaternion.Euler(0f, 0f, MathEx.directionToAngle(toMouseVector.normalized));

        var deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        _binder.HeadObject.transform.position = worldMousePosition;
        _binder.HeadObject.transform.rotation = rotation;
        
        _collisionInfoData.setCollisionInfoData(characterToMouse.magnitude, _attackAngle, _attackStartDistance, CollisionType.Attack);
        _collisionInfo.updateCollisionInfo(targetPosition, characterToMouse.normalized);
        
        CollisionManager.Instance().collisionRequest(_collisionInfo, ownerEntity, OnDetectEnemy, OnCollisionEnd);

        _binder.DebugDetect = _enemyDetect;
        
        TweenPosition();

        TweenColor();

        CheckCloseToOwner(worldMousePosition, targetPosition);
    }

    private void OnDetectEnemy(CollisionSuccessData collisionSuccessData)
    {
        if (collisionSuccessData._target == null)
        {
            return;
        }

        var targetEntity = collisionSuccessData._target as GameEntityBase;
        if (targetEntity == null || targetEntity.isDead() == true)
        {
            return;
        }

        _detectFlag = true;
    }

    private void OnCollisionEnd()
    {
        _prevDetect = _enemyDetect;
        _enemyDetect = _detectFlag;
        _detectFlag = false;

        UpdateState();
    }

    private void UpdateState()
    {
        if (_prevDetect == false && _enemyDetect == true &&
            (_tweeningNow == false || _state == State.DetectToIdle))
        {
            _state = State.IdleToDetect;
            StartIdleToDetect();
            return;
        }
        
        if (_prevDetect == true && _enemyDetect == false &&
            (_tweeningNow == false || _state == State.IdleToDetect))
        {
            _state = State.DetectToIdle;
            StartDetectToIdle();
            return;
        }
        
        if (_prevDetect == false && _enemyDetect == false && _tweeningNow == false)
        {
            _state = State.Idle;
            return;
        }
        
        if (_prevDetect == true && _enemyDetect == true && _tweeningNow == false)
        {
            _state = State.Detect;
            return;
        }
    }

    private void StartIdleToDetect()
    {
        _tweeningNow = true;
        var t = _endPosition - _binder.SubMarker.transform.localPosition.x / _posDiff;

        _completeTime = Time.time + _binder.LerpTime * t;
        
        _idleToDetectColorCoroutine = _binder.StartCoroutine(StartColorLerp());
    }

    private void StartDetectToIdle()
    {
        _tweeningNow = true;
        var t = _endPosition - _binder.SubMarker.transform.localPosition.x / _posDiff;

        t = 1 - t;

        if (_idleToDetectColorCoroutine != null)
        {
            _binder.StopCoroutine(_idleToDetectColorCoroutine);
            _idleToDetectColorCoroutine = null;
        }
        
        _completeTime = Time.time +_binder.LerpTime * t;
        
        _mainCursorTargetColor = _binder.IdleColor;
        _subCursorTargetColor = _binder.SubIdleColor;
        
        _colorCompleteTime = Time.time + _binder.ColorLerpTime;
    }

    private IEnumerator StartColorLerp()
    {
        _binder.MainCusor.color = _binder.DetectColor;
        _binder.SubCusor.color = _binder.DetectColor;
        _mainCursorTargetColor = _binder.DetectColor;
        _subCursorTargetColor = _binder.DetectColor;

        yield return new WaitForSeconds(_binder.DetectHighlightTime);
        
        _mainCursorTargetColor = _binder.DetectIdleColor;
        _subCursorTargetColor = _binder.DetectIdleColor;

        _colorCompleteTime = Time.time + _binder.ColorLerpTime;

        _idleToDetectColorCoroutine = null;
    }

    private void TweenPosition()
    {
        if (_tweeningNow == false)
        {
            return;
        }

        var t =  (_completeTime - Time.time) / _binder.LerpTime;
        t = Mathf.Clamp(t, 0f, 1f);
        t = 1 - t;
        
        if (_completeTime <= Time.time)
        {
            _tweeningNow = false;

            if (_state == State.IdleToDetect)
            {
                t = 1;
            }
            else if (_state == State.DetectToIdle)
            {
                t = 0;
            }
        }
        else
        {
            if (_state == State.DetectToIdle)
            {
                t = 1 - t; 
            }   
        }

        var curPosition = MathEx.easeOutExpo(_startPosition, _endPosition, t);
        _binder.SubMarker.transform.localPosition = new Vector3(curPosition, 0, 0);
    }

    private void TweenColor()
    {
        var t = (_colorCompleteTime - Time.time) / _binder.ColorLerpTime;
        t = Mathf.Clamp(t, 0f, 1f);
        t = 1 - t;

        _binder.MainCusor.color = Color.Lerp(_binder.MainCusor.color, _mainCursorTargetColor, t);
        _binder.SubCusor.color = Color.Lerp(_binder.SubCusor.color, _subCursorTargetColor, t);
    }

    private void CheckCloseToOwner(Vector3 mousePosition, Vector3 ownerPosition)
    {
        if (_enemyDetect == true)
        {
            return;
        }
        
        var sqrDist = Vector3.SqrMagnitude(mousePosition - ownerPosition);
        if (sqrDist < _squreStartPosition)
        {
            _binder.SubMarker.transform.position = ownerPosition;
        }
    }
}
