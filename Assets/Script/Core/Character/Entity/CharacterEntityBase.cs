using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEntityBase : GameEntityBase
{
    private string _qteEffectPath = "Sprites/UI/QTEHit/";
    private EffectItem _qteEffectItem = null;

    private bool _useCameraBoundLock = true;
    private bool _isInCameraBound = false;
    private int _stagePointIndex = 0;
    private float _targetSearchTime = 0f;

    public override void assign()
    {
        base.assign();

        _spriteRenderer.gameObject.layer = LayerMask.NameToLayer("Character");

        GameObject debugText = Instantiate(ResourceContainerEx.Instance().GetPrefab("Prefab/DebugTextManager"), Vector3.zero,Quaternion.identity);
        debugTextManager = debugText.GetComponent<DebugTextManager>();
        debugTextManager.padding = -0.1f;
        debugTextManager.stayTime = 0.3f;

        debugText.transform.SetParent(this.gameObject.transform);
        debugText.transform.localPosition = Vector3.zero;
    }

    public void clearCharacter()
    {
        _stagePointIndex = 0;
        MasterManager.instance._stageProcessor.updatePointIndex(transform.position, ref _stagePointIndex);
        _isInCameraBound = MasterManager.instance._stageProcessor.isInCameraBound(_stagePointIndex, transform.position, out Vector3 resultPosition);
    }

    public override void initializeCharacter(CharacterInfoData characterInfo, Vector3 direction)
    {
        base.initializeCharacter(characterInfo,direction);

        _useCameraBoundLock = characterInfo._useCameraBoundLock;

        RegisterRequest(QueryUniqueID("SceneCharacterManager"));
        targetSearchQuick();

        setSortingOrder(0);

        clearCharacter();

        _targetSearchTime = 1f;

    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        if(hasRotateSlot() == false)
            getMovementControl().addFrameToWorld(this);

        updateAttachedSlot(deltaTime);
        updateByActionFlag();

        if(isActiveSelf() == false)
            return;

        _targetSearchTime += deltaTime;
        if(_targetSearchTime < 0.5f)
            return;

        _targetSearchTime = 0f;
        if(isAIGraphValid() && getCurrentTargetSearchType() != TargetSearchType.None)
        {
            TargetSearchDescription desc = MessageDataPooling.GetMessageData<TargetSearchDescription>();
            desc._requester = this;
            desc._searchIdentifier = getCurrentAISearchIdentifier();
            desc._searchRange = getCurrentTargetSearchRange();
            desc._searchStartRange = getCurrentTargetSearchStartRange();
            desc._searchSphereRadius = getCurrentTargetSearchSphereRadius();
            desc._searchType = getCurrentTargetSearchType();

            SendMessageEx(MessageTitles.entity_searchNearest,QueryUniqueID("SceneCharacterManager"),desc);
        }
    }

    public void updateByActionFlag()
    {
        if(checkCurrentActionFlag(ActionFlags.QTE) && _qteEffectItem == null)
        {
            EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
            requestData.clearRequestData();
            requestData.createPresetAnimationRequestData(_qteEffectPath);
            requestData._parentTransform = this.transform;
            requestData._position = this.transform.position + Vector3.down;

            _qteEffectItem = EffectManager._instance.createEffect(requestData) as EffectItem;
            requestData.isUsing = true;
        }
        else if(checkCurrentActionFlag(ActionFlags.QTE) == false && _qteEffectItem != null)
        {
            _qteEffectItem.stopEffect();
            _qteEffectItem = null;
        }
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

        if(isActiveSelf() == false)
            return;

        if(_useCameraBoundLock)
        {
            int pointIndex = _stagePointIndex;
            MasterManager.instance._stageProcessor.updatePointIndex(transform.position, ref pointIndex);
            if(pointIndex != _stagePointIndex)
            {
                _stagePointIndex = pointIndex;
                _isInCameraBound = false;
            }

            if(_isInCameraBound == false)
                _isInCameraBound = MasterManager.instance._stageProcessor.isInCameraBound(_stagePointIndex, transform.position, out Vector3 resultPosition);

            if (_isInCameraBound)
            {
                bool inCameraBound = MasterManager.instance._stageProcessor.isInCameraBound(_stagePointIndex, transform.position, out Vector3 resultPosition);
                
                if (inCameraBound == false)
                {
                    Vector3 pushDirection = resultPosition - transform.position;
                    float length = pushDirection.magnitude;
                    pushDirection.Normalize();

                    setVelocity(pushDirection * length * 10f);
                }
            }
        }
    }

    public void setCameraBoundLock(bool value)
    {
        _useCameraBoundLock = value ? _characterInfo._useCameraBoundLock : false;

        if(_useCameraBoundLock)
            _isInCameraBound = MasterManager.instance._stageProcessor.isInCameraBound(_stagePointIndex, transform.position, out Vector3 resultPosition);
    }

    private void targetSearchQuick()
    {
        if(isAIGraphValid() && getCurrentTargetSearchType() != TargetSearchType.None)
        {
            TargetSearchDescription desc = MessageDataPooling.GetMessageData<TargetSearchDescription>();
            desc._requester = this;
            desc._searchIdentifier = getCurrentAISearchIdentifier();
            desc._searchRange = getCurrentTargetSearchRange();
            desc._searchStartRange = getCurrentTargetSearchStartRange();
            desc._searchSphereRadius = getCurrentTargetSearchSphereRadius();
            desc._searchType = getCurrentTargetSearchType();

            SendMessageQuick(MessageTitles.entity_searchNearestQuick,QueryUniqueID("SceneCharacterManager"),desc);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CharacterEntityBase))]
public class CharacterEntityBaseEditor : GameEntityBaseEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif