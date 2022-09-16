using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEntityBase : GameEntityBase
{
    private CollisionInfo _collisionInfo;
    
    private Color _debugColor = Color.red;

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("SceneCharacterManager"));

        CollisionInfoData data = new CollisionInfoData(.1f,0f);
        _collisionInfo = new CollisionInfo(data);

        CollisionManager.Instance().registerObject(_collisionInfo, this, CollisionLayer.Default);
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        getMovementControl().addFrameToWorld(this.transform);

        _collisionInfo.updateCollisionInfo(transform.position,Vector3.right);
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

    private void OnDrawGizmos()
    {
        if(_collisionInfo == null)
            return;
        
        Color color = Gizmos.color;

        Gizmos.color = _debugColor;
        for(int i = 0; i < 36; ++i)
        {
            float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
            float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

            float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
            float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

            Gizmos.DrawLine(new Vector3(x,y) * _collisionInfo.getRadius() + transform.position,new Vector3(x2,y2) * _collisionInfo.getRadius() + transform.position);
        }
        
        _debugColor = Color.red;
        Gizmos.color = color;
    }
}
