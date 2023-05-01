using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEntityBase : GameEntityBase
{
    public override void assign()
    {
        base.assign();

        _spriteRenderer.gameObject.layer = LayerMask.NameToLayer("Character");

        GameObject debugText = Instantiate(ResourceContainerEx.Instance().GetPrefab("Prefab/DebugTextManager"),Vector3.zero,Quaternion.identity);
        debugTextManager = debugText.GetComponent<DebugTextManager>();
        debugTextManager.padding = -0.1f;
        debugTextManager.stayTime = 0.3f;

        debugText.transform.SetParent(this.gameObject.transform);
        debugText.transform.localPosition = Vector3.zero;
    }

    public override void initializeCharacter(CharacterInfoData characterInfo, Vector3 direction)
    {
        base.initializeCharacter(characterInfo,direction);

        RegisterRequest(QueryUniqueID("SceneCharacterManager"));
        targetSearchQuick();
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        getMovementControl().addFrameToWorld(this);

        if(isAIGraphValid() && getCurrentTargetSearchType() != TargetSearchType.None)
        {
            TargetSearchDescription desc = MessageDataPooling.GetMessageData<TargetSearchDescription>();
            desc._requester = this;
            desc._searchIdentifier = getCurrentSearchIdentifier();
            desc._searchRange = getCurrentTargetSearchRange();
            desc._searchStartRange = getCurrentTargetSearchStartRange();
            desc._searchSphereRadius = getCurrentTargetSearchSphereRadius();
            desc._searchType = getCurrentTargetSearchType();

            SendMessageEx(MessageTitles.entity_searchNearest,QueryUniqueID("SceneCharacterManager"),desc);
        }
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

    }


    private void targetSearchQuick()
    {
        if(isAIGraphValid() && getCurrentTargetSearchType() != TargetSearchType.None)
        {
            TargetSearchDescription desc = MessageDataPooling.GetMessageData<TargetSearchDescription>();
            desc._requester = this;
            desc._searchIdentifier = getCurrentSearchIdentifier();
            desc._searchRange = getCurrentTargetSearchRange();
            desc._searchStartRange = getCurrentTargetSearchStartRange();
            desc._searchSphereRadius = getCurrentTargetSearchSphereRadius();
            desc._searchType = getCurrentTargetSearchType();

            SendMessageQuick(MessageTitles.entity_searchNearestQuick,QueryUniqueID("SceneCharacterManager"),desc);
        }
    }
}
