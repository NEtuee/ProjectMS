using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrosshairUI : IUIElement
{
    private CrosshairUIBinder _binder;

    private float _attackAngle;
    private float _attackStartDistance;

    private CollisionInfoData _collisionInfoData;
    private CollisionInfo _collisionInfo;

    private float _attackRadius;
    private float _attackRayRadius;
    
    private bool _detectFlag = false;
    private bool _enemyDetect = false;
    private bool _prevDetect = false;

    private int _currentDashPoint;

    private UIActionTimer _detectColorTimer;
    private UIActionTimer _dashColorTimer;
    private UIActionTimer _hitEnemyColorTimer;

    private float _highlightDuration = 0.1f;
    private float _subCursorHighlightDuration = 0.2f;

    public CrosshairUI()
    {
        _detectColorTimer = new UIActionTimer(ChangeToIdleColor);
        _dashColorTimer = new UIActionTimer(ChangeToSubCursorIdleColor);
        _hitEnemyColorTimer = new UIActionTimer(ChangeToSubCursorIdleColor);
    }

    private void ChangeToIdleColor()
    {
        _binder.MainCursor.color = _binder.DetectIdleColor;
    }

    private void ChangeToSubCursorDashColor()
    {
        for (int i = 0; i < _binder.SubCursorDashPointObjectsSpriteRenderers.Length; i++)
        {
            _binder.SubCursorDashPointObjectsSpriteRenderers[i].color = _binder.SubCursorDashColor;
        }
    }
    
    private void ChangeToSubCursorHitColor()
    {
        for (int i = 0; i < _binder.SubCursorDashPointObjectsSpriteRenderers.Length; i++)
        {
            _binder.SubCursorDashPointObjectsSpriteRenderers[i].color = _binder.SubCursorHitEnemyColor;
        }
    }
    
    private void ChangeToSubCursorIdleColor()
    {
        for (int i = 0; i < _binder.SubCursorDashPointObjectsSpriteRenderers.Length; i++)
        {
            _binder.SubCursorDashPointObjectsSpriteRenderers[i].color = _binder.DashPointColors[i];
        }
    }

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
        AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AttackPreset") as AttackPreset;
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

        ChangeToSubCursorIdleColor();
    }

    public void InitValue(GameEntityBase ownerEntity, Vector3 targetPosition, float dashPoint)
    {
        UpdateByManager(ownerEntity,false, targetPosition, dashPoint);
    }

    public void UpdateByManager(GameEntityBase ownerEntity, bool isDead, Vector3 targetPosition, float dashPoint)
    {
        var mainCamera = Camera.main;
        if (mainCamera == null || isDead == true || _binder == null || _binder.gameObject.activeInHierarchy == false)
        {
            return;
        }

        var screenHeight = Screen.height;
        var screenWidth = Screen.width;

        var mousePosition = Input.mousePosition;
        var clampMousePosition = new Vector3(Mathf.Clamp(mousePosition.x, 0.0f, screenWidth), Mathf.Clamp(mousePosition.y, 0.0f, screenHeight), 0.0f);
        
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(clampMousePosition);
        worldMousePosition.z = 0.0f;

        Vector3 toMouseVector = worldMousePosition - targetPosition;
        Quaternion rotation = Quaternion.Euler(0f, 0f, MathEx.directionToAngle(toMouseVector.normalized));
        
        _binder.HeadObject.transform.position = worldMousePosition;
        _binder.HeadObject.transform.rotation = rotation;

        var characterToMouse = worldMousePosition - targetPosition;

        _collisionInfoData.setCollisionInfoData(characterToMouse.magnitude, _attackAngle, _attackStartDistance, CollisionType.Attack);
        _collisionInfo.updateCollisionInfo(targetPosition, characterToMouse.normalized);
        
        CollisionRequestData requestData;
        requestData._collision = _collisionInfo;
        requestData._collisionDelegate = OnDetectEnemy;
        requestData._collisionEndEvent = OnCollisionEnd;
        requestData._position = targetPosition;
        requestData._direction = characterToMouse.normalized;
        requestData._requestObject = ownerEntity;
        CollisionManager.Instance().collisionRequest(requestData);

        _binder.DebugDetect = _enemyDetect;

        var isQte = ownerEntity.checkCurrentActionFlag(ActionFlags.QTE);
        var isStunShot = ActionKeyInputManager.Instance().keyCheck("StunShot");
        var isGuardBreak = ActionKeyInputManager.Instance().keyCheck("GuardBreak2");
        
        UpdateMainCursor(isQte, isStunShot, isGuardBreak);
        UpdateSubCursor(targetPosition, rotation, dashPoint, isQte, isStunShot, isGuardBreak);
        
        _detectColorTimer.Update();
        _dashColorTimer.Update();
        _hitEnemyColorTimer.Update();
    }

    public void OnDash()
    {
        _dashColorTimer.Play(_subCursorHighlightDuration);
        ChangeToSubCursorDashColor();
    }

    public void OnHitEnemy()
    {
        _dashColorTimer.Stop();
        _hitEnemyColorTimer.Play(_subCursorHighlightDuration);
        ChangeToSubCursorHitColor();
    }
    
    private void UpdateMainCursor(bool isQte, bool isStunShot, bool isGuardBreak)
    {
        _binder.MainSuper.SetActive(false);
        _binder.MainKick.SetActive(false);
        _binder.MainBaisc.SetActive(false);

        if (isQte == true)
        {
            _binder.MainBaisc.SetActive(true);
            return;
        }

        if (isStunShot == true)
        {
            _binder.MainSuper.SetActive(true);
        }
        else if (isGuardBreak == true)
        {
            _binder.MainKick.SetActive(true);
        }
        else
        {
            _binder.MainBaisc.SetActive(true);
        }
    }

    private void UpdateSubCursor(Vector3 characterPosition, Quaternion rotation, float dashPoint, bool isQte, bool isStunShot, bool isGuardBreak)
    {
        var toVector = _binder.MainCursor.transform.position - characterPosition;
        characterPosition += toVector.normalized * _binder.PlayerRadius;

        _binder.SubMarker.transform.position = characterPosition;
        _binder.SubMarker.transform.rotation = rotation;

        var dashPointInt = (int)dashPoint;
        if (_currentDashPoint != dashPointInt)
        {
            for (int i = 0; i < _binder.SubCursorDashPointObjects.Length; i++)
            {
                _binder.SubCursorDashPointObjects[i].SetActive(i < dashPoint);
            }
        }
        _currentDashPoint = dashPointInt;

        _binder.SubCursorDashPointRoot.SetActive(true);
        _binder.SubCursorDashPointKick.SetActive(false);

        if (isQte == true)
        {
            return;
        }
        
        if (isStunShot == false)
        {
            _binder.SubCursorDashPointRoot.SetActive(isGuardBreak == false);
            _binder.SubCursorDashPointKick.SetActive(isGuardBreak == true);
        }
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

        UpdateMainCursorColor();
    }

    private void UpdateMainCursorColor()
    {
        if (_prevDetect == false && _enemyDetect == true)
        {
            _binder.MainCursor.color = _binder.DetectHighlightColor;
            _detectColorTimer.Play(_highlightDuration);
            return;
        }

        if (_enemyDetect == false)
        {
            _detectColorTimer.Stop();
            _binder.MainCursor.color = _binder.IdleColor;
            return;
        }
    }
    
    private struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }
}
