using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProjectileEntityBase : ObjectBase
{
    protected ProjectileGraph _projectileGraph;

    private CollisionInfo _collisionInfo;

    private Color _debugColor = Color.red;
    private CollisionDelegate _collisionDelegate;

    private HashSet<int> _collisionUniqueIDList = new HashSet<int>();

    private bool _spriteRotation = false;
    private float _gravity = 0f;
    private float _gravityAccumulate = 0f;
    private bool _cameraBound = false;

    private Vector3 _lastHitNormal = Vector3.zero;

    public override void assign()
    {
        base.assign();
        _projectileGraph = new ProjectileGraph();
        CollisionInfoData data = new CollisionInfoData(0f,0f,0f,0f,CollisionType.Projectile);
        _collisionInfo = new CollisionInfo(data);
        _collisionDelegate = onProjectileHit;
        
        createSpriteRenderObject();
    }

    public void initializeProjectile()
    {
        _cameraBound = false;
        _spriteRotation = false;
        _gravity = 0f;
        _gravityAccumulate = 0f;
        _lastHitNormal = Vector3.zero;

        _collisionUniqueIDList.Clear();
    }

    public void setData(ProjectileGraphBaseData baseData)
    {
        _spriteRotation = baseData._useSpriteRotation;
        _gravity = baseData._gravity;
        _gravityAccumulate = 0f;
        _cameraBound = baseData._cameraBound;

        _projectileGraph.setData(baseData);
        _collisionInfo.setCollisionInfo(baseData._collisionRadius, baseData._collisionAngle, baseData._collisionStartDistance);

        _spriteRenderer.gameObject.layer = baseData._castShadow ? LayerMask.NameToLayer("Character") : LayerMask.NameToLayer("EffectEtc");
    }

    public void shot(Vector3 startPosition)
    {
        updatePosition(startPosition);
        _projectileGraph.initialize();
        setDirection(_projectileGraph.getCurrentDirection());
        _spriteRenderer.sprite = _projectileGraph.getCurrentSprite();

        RegisterRequest(QueryUniqueID("ProjectileManager"));
        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }

    public void shot(ProjectileGraphShotInfoData shotInfoData, Vector3 startPosition)
    {
        updatePosition(startPosition);

        if(_spriteRotation)
            _spriteObject.transform.localRotation = Quaternion.Euler(0f,0f,shotInfoData._defaultAngle);
        else
            _spriteObject.transform.localRotation = Quaternion.identity;

        _spriteObject.transform.localScale = Vector3.one;
        _spriteRenderer.transform.localPosition = Vector3.zero;

        _projectileGraph.initialize(shotInfoData);
        setDirection(_projectileGraph.getCurrentDirection());
        _spriteRenderer.sprite = _projectileGraph.getCurrentSprite();

        RegisterRequest(QueryUniqueID("ProjectileManager"));
        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }

    public override void progress(float deltaTime)
    {
        bool isProjectileEnd = _projectileGraph.isEnd();
        bool isOutOfBound = false;
        Vector3 boundNormal = _lastHitNormal;

        if( isProjectileEnd == false && _cameraBound)
        {
            if(hasChildObject() && getChildObject() is CharacterEntityBase)
            {
                CharacterEntityBase character = getChildObject() as CharacterEntityBase;
                isOutOfBound = character.isInCameraBound(out boundNormal) == false;

            }
            else
            {
                isOutOfBound = CameraControlEx.Instance().IsInCameraBound(transform.position, out boundNormal) == false;
            }
        }

        if( isProjectileEnd == true || isOutOfBound == true )
        {
            ObjectBase executeTargetEntity = this;
            if(_projectileGraph.isEventExecuteBySummoner())
                executeTargetEntity = getSummonObject() == null ? this : getSummonObject();

            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnEnd,executeTargetEntity,null);

            if(hasChildObject() && (isOutOfBound || _projectileGraph.isPenetrateEnd()))
            {
                ObjectBase childObject = getChildObject();
                if(childObject is GameEntityBase)
                {
                    Vector3 velocity = Vector3.Reflect(_projectileGraph.getCurrentDirection(),boundNormal) * 2.5f;
                    (childObject as GameEntityBase).setVelocity(velocity);
                }
            }

            detachChildObject();
            DeregisterRequest();
            CollisionManager.Instance().deregisterObject(_collisionInfo.getCollisionInfoData(),this);
            return;
        }

        base.progress(deltaTime);

        _projectileGraph.progress(deltaTime, this);
        _projectileGraph.updateLifeTime(deltaTime);

        Vector3 movementOfFrame = _projectileGraph.getMovementOfFrame();
        Vector3 position = transform.position;
        position += movementOfFrame;

        _gravityAccumulate += _gravity * deltaTime;
        position += Vector3.up * _gravityAccumulate * deltaTime;

        _spriteRenderer.sprite = _projectileGraph.getCurrentSprite();
        _spriteRenderer.transform.localRotation = _projectileGraph.getCurrentAnimationRotation();

        Vector3 outScale = Vector3.one;
        _projectileGraph.getCurrentAnimationScale(out outScale);
        _spriteRenderer.transform.localScale = outScale;

        if(_spriteRotation && movementOfFrame.sqrMagnitude != 0f)
            _spriteObject.transform.localRotation *= Quaternion.Euler(0f,0f,Mathf.Atan2(movementOfFrame.y,movementOfFrame.x) * Mathf.Rad2Deg);

        Vector3 outTranslation = Vector3.zero;
        _projectileGraph.getCurrentAnimationTranslation(out outTranslation);
        _spriteRenderer.transform.localPosition = outTranslation;

        Vector3 direction = movementOfFrame.normalized;
        if(direction.sqrMagnitude != 0f)
            setDirection(direction);
        
        updatePosition(position);

        CollisionRequestData requestData;
        requestData._collision = _collisionInfo;
        requestData._collisionDelegate = _collisionDelegate;
        requestData._collisionEndEvent = null;
        requestData._position = position;
        requestData._direction = getDirection();
        requestData._requestObject = this;
        CollisionManager.Instance().collisionRequest(requestData);

        
        GizmoHelper.instance.drawCircle(transform.position,_collisionInfo.getRadius(),36,_debugColor);
    }

    private void onProjectileHit(CollisionSuccessData successData)
    {
        if(successData._requester is ProjectileEntityBase == false || successData._target is GameEntityBase == false || _projectileGraph.isEnd())
            return;
        
        ProjectileEntityBase requester = successData._requester as ProjectileEntityBase;
        GameEntityBase target = successData._target as GameEntityBase;

        if(getChildObject() == target)
            return;

        if(AllyInfoManager.compareAllyTargetType(requester, target) == AllyTargetType.Ally)
            return;

        if(_collisionUniqueIDList.Contains(target.GetUniqueID()))
            return;

        if(_projectileGraph.getPenetrateCount() == -1)
            return;
        
        _collisionUniqueIDList.Add(target.GetUniqueID());

        ObjectBase executeTargetEntity = requester;
        if(_projectileGraph.isEventExecuteBySummoner())
            executeTargetEntity = requester.getSummonObject() == null ? requester : requester.getSummonObject();

        if(target.getDefenceType() == DefenceType.Evade)
        {
            target.setDefenceState(DefenceState.EvadeSuccess);
            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnEvaded,executeTargetEntity,target);
        }
        else
        {
            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHit,executeTargetEntity,target);

            _lastHitNormal = transform.position - target.transform.position;
            _lastHitNormal.Normalize();

            _projectileGraph.decreasePenetrateCount();
            if(_projectileGraph.isPenetrateEnd() == true)
                _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHitEnd,executeTargetEntity,target);    
        }

        
    }

    
}
