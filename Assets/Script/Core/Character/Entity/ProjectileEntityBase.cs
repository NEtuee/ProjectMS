using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProjectileEntityBase : ObjectBase
{
    private ProjectileGraph _projectileGraph;

    private CollisionInfo _collisionInfo;

    private Color _debugColor = Color.red;
    private CollisionDelegate _collisionDelegate;

    public override void assign()
    {
        base.assign();
        _projectileGraph = new ProjectileGraph(ProjectileGraphLoader.readFromXML(IOControl.PathForDocumentsFile("Assets\\Data\\ProjectileGraph\\ProjectileGraphExample.xml"))[0]);
        CollisionInfoData data = new CollisionInfoData(.1f,0f, CollisionType.Projectile);
        _collisionInfo = new CollisionInfo(data);

        _collisionDelegate = onProjectileHit;
        
        createSpriteRenderObject();
    }

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("ProjectileManager"));

        _projectileGraph.initialize();

        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }

    public void shot(ProjectileGraphShotInfoData shotInfoData, Vector3 startPosition)
    {
        transform.position = startPosition;
        _projectileGraph.initialize(shotInfoData);
    }

    public override void progress(float deltaTime)
    {
        if(_projectileGraph.isEnd() == true)
        {
            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnEnd,this,null);

            gameObject.SetActive(false);
            return;
        }

        base.progress(deltaTime);

        bool isEnd = _projectileGraph.progress(deltaTime, this);
        _projectileGraph.updateLifeTime(deltaTime);

        transform.position += _projectileGraph.getMovementOfFrame();

        _spriteRenderer.sprite = _projectileGraph.getCurrentSprite();
        
        _collisionInfo.updateCollisionInfo(transform.position,Vector3.right);
        CollisionManager.Instance().collisionRequest(_collisionInfo,this,_collisionDelegate,null);
        
        GizmoHelper.instance.drawCircle(transform.position,_collisionInfo.getRadius(),36,_debugColor);
    }

    private void onProjectileHit(CollisionSuccessData successData)
    {
        if(successData._requester is ProjectileEntityBase == false || successData._target is GameEntityBase == false || _projectileGraph.isEnd())
            return;

        ProjectileEntityBase requester = successData._requester as ProjectileEntityBase;
        GameEntityBase target = successData._target as GameEntityBase;

        _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHit,requester,target);

        _projectileGraph.decreasePenetrateCount();
        if(_projectileGraph.isPenetrateEnd() == true)
            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHitEnd,requester,target);    
    }

    
}
