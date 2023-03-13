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

#if UNITY_EDITOR
        GameObject debugText = Instantiate(ResourceContainerEx.Instance().GetPrefab("Prefab/DebugTextManager"),Vector3.zero,Quaternion.identity);
        debugTextManager = debugText.GetComponent<DebugTextManager>();
        debugTextManager.padding = -0.1f;
        debugTextManager.stayTime = 0.3f;

        debugText.transform.SetParent(this.gameObject.transform);
        debugText.transform.localPosition = Vector3.zero;
#endif
    }

    public override void initializeCharacter(CharacterInfoData characterInfo)
    {
        base.initializeCharacter(characterInfo);

        RegisterRequest(QueryUniqueID("SceneCharacterManager"));
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


    
}
