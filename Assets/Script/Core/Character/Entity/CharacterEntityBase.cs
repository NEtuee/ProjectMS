using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEntityBase : GameEntityBase
{
    public TextMesh _actionText;

    private CollisionInfo _collisionInfo;
    
    private Color _debugColor = Color.red;

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("SceneCharacterManager"));

        CollisionInfoData data = new CollisionInfoData(1f,10f);
        _collisionInfo = new CollisionInfo(data);

        CollisionManager.Instance().registerObject(_collisionInfo, this, CollisionLayer.Default);
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        getMovementControl().addFrameToWorld(this.transform);

        _actionText.text = getCurrentActionName();

        _collisionInfo.updateCollisionInfo(transform.position,getDirection());

        _collisionInfo.drawCollosionArea(_debugColor);
        _collisionInfo.drawBoundBox(_debugColor);

        _debugColor = Color.red;
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

        CollisionManager.Instance().collisionRequest(_collisionInfo,collisionTest,CollisionLayer.Default);
    }


    private void collisionTest(object data)
    {
        _debugColor = Color.green;
    }
}
