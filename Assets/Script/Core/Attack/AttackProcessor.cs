using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackProcessorBase
{
    public abstract void initialize(ActionFrameEventBase attackFrameEvent);
    public abstract void executeAttack(ObjectBase executeEntity, ObjectBase targetEntity);

    public virtual void onExit(bool isForceEnd) {}
}

public class FollowAttackProcessor : AttackProcessorBase
{
    public ActionFrameEvent_FollowAttack _attackFrameEvent = null;
    public FrameEventMovement _frameEventMovement = new FrameEventMovement();

    public bool                        _isFirst = false;
    public bool                        _isCollision = false;
    public float                       _moveTimer = 0f;

    public UnityEngine.Vector3         _direction = UnityEngine.Vector3.zero;
    public UnityEngine.Vector3         _position = UnityEngine.Vector3.zero;
    public UnityEngine.Vector3         _offset = UnityEngine.Vector3.zero;

    public List<CollisionObjectData>   _collisionData = new List<CollisionObjectData>();
    public HashSet<object>             _collisionHash = new HashSet<object>();

    public GameEntityBase              _crossHairEntity = null;


    public override void initialize(ActionFrameEventBase attackFrameEvent)
    {
        _attackFrameEvent = attackFrameEvent as ActionFrameEvent_FollowAttack;

        _isFirst = true;
        _direction = UnityEngine.Vector3.zero;
        _position = UnityEngine.Vector3.zero;

        _collisionData.Clear();
        _collisionHash.Clear();

        _isCollision = false;
        _moveTimer = 0f;

        _crossHairEntity = null;
    }

    public override void executeAttack(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase gameEntityBase = executeEntity as GameEntityBase;
        if(gameEntityBase.getCurrentTargetEntity() == null)
            return;

        UnityEngine.Vector3 targetPosition = gameEntityBase.getCurrentTargetEntity().transform.position;
        if(_isFirst)
        {
            _frameEventMovement.initialize(gameEntityBase);
            if(_attackFrameEvent._toTarget)
                _position = targetPosition + _offset;
            else
                _position = gameEntityBase.transform.position + _offset;

            SpawnCharacterOptionDesc desc = new SpawnCharacterOptionDesc();
            desc._position = _position;
            desc._allyInfo = gameEntityBase.getAllyInfo();

            CharacterInfoData characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_attackFrameEvent._characterKey);
            _crossHairEntity = (SceneCharacterManager._managerInstance as SceneCharacterManager).createCharacterFromPool(characterInfoData,desc);
            _crossHairEntity._isDummyEntity = true;
        }

        if(_isFirst)
        {
            executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnBegin, _crossHairEntity, targetEntity);
            _isFirst = false;
        }

        _direction = targetPosition - _position;
        _direction.Normalize();

        if(_attackFrameEvent._moveTime != 0f)
            _moveTimer += GlobalTimer.Instance().getSclaedDeltaTime();

        if(_attackFrameEvent._moveTime == 0f || (_moveTimer < _attackFrameEvent._moveTime))
        {
            switch(_attackFrameEvent._followType)
            {
                case ActionFrameEvent_FollowAttack.FollowType.Attach:
                {
                    _position = targetPosition;
                }
                break;
                case ActionFrameEvent_FollowAttack.FollowType.Movement:
                {
                    _frameEventMovement.progress(GlobalTimer.Instance().getSclaedDeltaTime(),_direction);
                    _position += _frameEventMovement.getMovementOfFrame();
            
                    _frameEventMovement.resetMovementOfFrame();
                }
                break;
            }
        }

        _crossHairEntity.updatePosition(_position);

        gameEntityBase.setDirection((_position - gameEntityBase.transform.position).normalized);
        _crossHairEntity.setDirection(gameEntityBase.getDirection());

        _collisionData.Clear();
        bool collision = false;
        CollisionManager.Instance().queryRangeAll(CollisionType.Attack,_position,_attackFrameEvent._radius, ref _collisionData);
        foreach(var item in _collisionData)
        {
            if(AllyInfoManager.compareAllyTargetType(gameEntityBase, (item._collisionObject as GameEntityBase)) == AllyTargetType.Enemy)
            {
                collision = true;
                break;
            }
        }

        GizmoHelper.instance.drawCircle(_position,_attackFrameEvent._radius,6,collision ? UnityEngine.Color.green : UnityEngine.Color.red);
        if(collision)
        {
            if(_isCollision == false)
                executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnEnter,_crossHairEntity,targetEntity);
            
            _isCollision = true;
        }
        else if(_isCollision)
        {
            executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnExit,_crossHairEntity,targetEntity);
            _isCollision = false;
        }

        DebugUtil.assert(_crossHairEntity.getDirectionType() == DirectionType.Keep, "FollowAttack Entity의 DirectionType은 항상 Keep이어야 합니다.");
        DebugUtil.assert(_crossHairEntity.isDead() == false && _crossHairEntity.isActiveSelf(), "FollowAttack Entity는 죽으면 안됩니다.");
    }

    public override void onExit(bool isForceEnd)
    {
        if(isForceEnd == false)
            executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnEnd, _crossHairEntity, null);
        
        _crossHairEntity?.deactive();
        _crossHairEntity?.DeregisterRequest();
        _crossHairEntity = null;
    }

    public void executeChildFrameEvent(ChildFrameEventType eventType, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        WeightRandomManager.Instance().updateRandom();
        
        if(_attackFrameEvent._childFrameEventItems == null || _attackFrameEvent._childFrameEventItems.ContainsKey(eventType) == false)
            return;
        
        ChildFrameEventItem childFrameEventItem = _attackFrameEvent._childFrameEventItems[eventType];
        GameEntityBase executeGameEntity = null;
        if(executeEntity is GameEntityBase)
            executeGameEntity = executeEntity as GameEntityBase;

        for(int i = 0; i < childFrameEventItem._childFrameEventCount; ++i)
        {
            if(executeGameEntity != null && childFrameEventItem._childFrameEvents[i].checkCondition(executeGameEntity) == false)
                continue;

            if(childFrameEventItem._childFrameEvents[i].getFrameEventType() == FrameEventType.FrameEvent_Movement)
            {
                childFrameEventItem._childFrameEvents[i].initialize(executeEntity);
                (childFrameEventItem._childFrameEvents[i] as ActionFrameEvent_Movement).setMovementValue(_frameEventMovement,_direction);
                continue;
            }

            childFrameEventItem._childFrameEvents[i].initialize(executeEntity);
            childFrameEventItem._childFrameEvents[i].onExecute(executeEntity, targetEntity);
        }
    }
}

public class AttackProcessor : AttackProcessorBase
{
    private ActionFrameEvent_Attack _attackFrameEvent;
    private HashSet<ObjectBase>     _collisionList = new HashSet<ObjectBase>();
    private List<CollisionSuccessData> _collisionOrder = new List<CollisionSuccessData>();

    public CollisionDelegate       _collisionDelegate;
    public System.Action           _collisionEndEvent;

    public float                   _attackTermTime = 0f;

    public AttackProcessor()
    {
        _collisionDelegate = attackPrepare;
        _collisionEndEvent = attackProcess;
    }

    public override void initialize(ActionFrameEventBase attackFrameEvent)
    {
        _attackFrameEvent = attackFrameEvent as ActionFrameEvent_Attack;

        _collisionList.Clear();
        _collisionOrder.Clear();
    }

    public override void executeAttack(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        if(_attackFrameEvent._attackTerm > 0f)
        {
            _attackTermTime += GlobalTimer.Instance().getSclaedDeltaTime();
            if(_attackTermTime >= _attackFrameEvent._attackTerm)
            {
                _attackTermTime -= _attackFrameEvent._attackTerm;
                
                _collisionList.Clear();
                _collisionOrder.Clear();
            }
        }

        if(_attackFrameEvent._targetDirectionType != DirectionType.Count && executeEntity is GameEntityBase)
        {
            GameEntityBase gameEntityBase = executeEntity as GameEntityBase;
            _attackFrameEvent._collisionInfo.updateCollisionInfo(executeEntity.transform.position,gameEntityBase.getDirectionFromType(_attackFrameEvent._targetDirectionType));

            CollisionRequestData requestData;
            requestData._collision = _attackFrameEvent._collisionInfo;
            requestData._collisionDelegate = _collisionDelegate;
            requestData._collisionEndEvent = _collisionEndEvent;
            requestData._position = executeEntity.transform.position;
            requestData._direction = gameEntityBase.getDirectionFromType(_attackFrameEvent._targetDirectionType);
            requestData._requestObject = executeEntity;

            CollisionManager.Instance().collisionRequest(requestData);
        }
        else
        {
            _attackFrameEvent._collisionInfo.updateCollisionInfo(executeEntity.transform.position,executeEntity.getDirection());

            CollisionRequestData requestData;
            requestData._collision = _attackFrameEvent._collisionInfo;
            requestData._collisionDelegate = _collisionDelegate;
            requestData._collisionEndEvent = _collisionEndEvent;
            requestData._position = executeEntity.transform.position;
            requestData._direction = executeEntity.getDirection();
            requestData._requestObject = executeEntity;
            
            CollisionManager.Instance().collisionRequest(requestData);
        }

    }

    public void attackProcess()
    {
        int attackCount = _attackFrameEvent._collisionCount < 0 ? _collisionOrder.Count : _attackFrameEvent._collisionCount;
        if(attackCount > _collisionOrder.Count)
            attackCount = _collisionOrder.Count;

        for(int i = 0; i < attackCount; ++i)
        {
            if(attackTarget(_collisionOrder[i]) == false)
                break;
        }

        _collisionOrder.Clear();
    }

    public void attackPrepare(CollisionSuccessData successData)
    {
        if(successData._requester is ObjectBase == false || successData._target is GameEntityBase == false)
            return;

        ObjectBase requester = (ObjectBase)successData._requester;
        GameEntityBase targetEntity = (GameEntityBase)successData._target;

        if(AllyInfoManager.compareAllyTargetType(requester, targetEntity) != AllyTargetType.Enemy)
            return;
        
        if((targetEntity.getCurrentIgnoreAttackType() & _attackFrameEvent._attackType) != 0)
            return;

        float distanceSq = (((GameEntityBase)successData._target).transform.position - successData._startPoint).sqrMagnitude;
        for(int i = 0; i < _collisionOrder.Count; ++i)
        {
            GameEntityBase target = (GameEntityBase)_collisionOrder[i]._target;

            if((target.transform.position - successData._startPoint).sqrMagnitude > distanceSq)
            {
                _collisionOrder.Insert(i,successData);
                return;
            }
        }

        _collisionOrder.Add(successData);
    }

    private bool attackTarget(CollisionSuccessData successData)
    {
        ObjectBase requester = (ObjectBase)successData._requester;
        GameEntityBase target = (GameEntityBase)successData._target;

        if(_collisionList.Contains(target) == true)
            return true;
        else
            _collisionList.Add(target);

        _attackFrameEvent._collisionInfo.drawCollosionArea(UnityEngine.Color.green,1f);

        ChildFrameEventType eventType = ChildFrameEventType.Count;

        UnityEngine.Vector3 toTargetDirection = (requester.transform.position - target.transform.position).normalized;

        target.setAttackPoint(successData._startPoint);

        if(requester is GameEntityBase)
            ((GameEntityBase)requester).setAttackPoint(successData._startPoint);

        float attackInAngle = UnityEngine.Vector3.Angle(target.getCurrentDefenceDirection(), (successData._startPoint - target.transform.position).normalized);

        bool guardSuccess = (attackInAngle < target.getDefenceAngle() * 0.5f);
        bool canIgnore = canIgnoreDefenceType(target.getDefenceType());

        bool attackSuccess = false;

        if(_attackFrameEvent._pushVector.sqrMagnitude > float.Epsilon)
        {
            UnityEngine.Vector3 attackPointDirection;
            if(requester is GameEntityBase && ((GameEntityBase)requester)._isDummyEntity)
                attackPointDirection = requester.getDirection();
            else
                attackPointDirection = (target.transform.position - successData._startPoint).normalized;

            if(target.checkCurrentActionFlag(ActionFlags.IgnorePush) == false)
                target.setVelocity(UnityEngine.Quaternion.Euler(0f,0f,UnityEngine.Mathf.Atan2(attackPointDirection.y,attackPointDirection.x) * UnityEngine.Mathf.Rad2Deg) * _attackFrameEvent._pushVector);
        }

        if(((guardSuccess == false || target.getDefenceType() == DefenceType.Empty) && target.getDefenceType() != DefenceType.Evade) || canIgnore)
        {
            if(_attackFrameEvent._attackType == AttackType.Default)
            {
                requester.setAttackState(AttackState.AttackSuccess);
                if(_attackFrameEvent._notifyAttackSuccess)
                    target.setDefenceState(DefenceState.Hit);

                if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnAttack);
                
                if(_attackFrameEvent._notifyAttackSuccess)
                    target.executeAIEvent(AIChildEventType.AIChildEvent_OnAttacked);

                eventType = ChildFrameEventType.ChildFrameEvent_OnHit;
                attackSuccess = true;
            }
            else if(_attackFrameEvent._attackType == AttackType.GuardBreak)
            {
                if(target.getDefenceType() == DefenceType.Empty)
                {
                    requester.setAttackState(AttackState.AttackGuardBreakFail);
                    target.setDefenceState(DefenceState.GuardBreakFail);

                    if(requester is GameEntityBase)
                        ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnAttackGuardBreakFail);
                    target.executeAIEvent(AIChildEventType.AIChildEvent_OnGuardBreakFail);

                    eventType = ChildFrameEventType.ChildFrameEvent_OnGuardBreakFail;
                }
                else
                {
                    requester.setAttackState(AttackState.AttackGuardBreak);
                    target.setDefenceState(DefenceState.GuardBroken);

                    if(requester is GameEntityBase)
                        ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnGuardBreak);
                    target.executeAIEvent(AIChildEventType.AIChildEvent_OnGuardBroken);

                    eventType = ChildFrameEventType.ChildFrameEvent_OnGuardBreak;
                }
                
                attackSuccess = true;
            }
            else if(_attackFrameEvent._attackType == AttackType.Catch)
            {
                if(target.hasParentObject() == false)
                {
                    requester.setAttackState(AttackState.AttackCatch);
                    target.setDefenceState(DefenceState.Catched);
                
                    AttachChildDescription description;
                    description._childObject = target;
                    description._pivot = _attackFrameEvent._catchOffset;

                    requester.attachChildObject(description);

                    if(requester is GameEntityBase)
                        ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnCatchTarget);
                    target.executeAIEvent(AIChildEventType.AIChildEvent_OnCatched);
                }

                eventType = ChildFrameEventType.ChildFrameEvent_OnCatch;

                _attackFrameEvent.executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnHit, requester, target);
                attackSuccess = true;
            }
        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Guard)
        {
            requester.setAttackState(AttackState.AttackGuarded);
            target.setDefenceState(DefenceState.DefenceSuccess);

            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnGuarded);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnGuard);

            eventType = ChildFrameEventType.ChildFrameEvent_OnGuard;
        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Parry)
        {
            requester.setAttackState(AttackState.AttackParried);
            target.setDefenceState(DefenceState.ParrySuccess);
            
            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnParried);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnParry);

            eventType = ChildFrameEventType.ChildFrameEvent_OnParry;
        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Evade)
        {
            requester.setAttackState(AttackState.AttackEvade);
            target.setDefenceState(DefenceState.EvadeSuccess);

            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnEvaded);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnEvade);

            eventType = ChildFrameEventType.ChildFrameEvent_OnEvade;
        }
        
        _attackFrameEvent.executeChildFrameEvent(eventType, requester, target);

        if(target is GameEntityBase)
        {
            GameEntityBase targetGameEntity = (target as GameEntityBase);
            targetGameEntity.addDeadEvent((item)=>{
                if(item == null || requester == null || target == null)
                    return;

                _attackFrameEvent.executeChildFrameEvent(ChildFrameEventType.ChildFrameEvent_OnKill, requester, target);
            });
        }

        return attackSuccess;
    }

    private bool canIgnoreDefenceType(DefenceType defenceType)
    {
        if(_attackFrameEvent._ignoreDefenceType == null || _attackFrameEvent._ignoreDefenceType.Length == 0)
            return false;

        for(int i = 0; i < _attackFrameEvent._ignoreDefenceType.Length; ++i)
        {
            if(_attackFrameEvent._ignoreDefenceType[i] == defenceType)
                return true;
        }

        return false;
    }
}

public class AttackProcessorManager
{
    private Dictionary<ActionFrameEventBase, AttackProcessorBase> _attackProcessorMap = new Dictionary<ActionFrameEventBase, AttackProcessorBase>();

    public void initializeAttack(ActionFrameEventBase attackFrameEvent)
    {
        if(_attackProcessorMap.ContainsKey(attackFrameEvent) == false)
            _attackProcessorMap[attackFrameEvent] = createNewAttackProcessor(attackFrameEvent.getFrameEventType());

        AttackProcessorBase attackProcessor = _attackProcessorMap[attackFrameEvent];
        attackProcessor.initialize(attackFrameEvent);
    }

    public void executeAttack(ActionFrameEventBase attackFrameEvent, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        _attackProcessorMap[attackFrameEvent].executeAttack(executeEntity,targetEntity);
    }

    public void exitAttack(ActionFrameEventBase attackFrameEvent, bool isForceEnd)
    {
        _attackProcessorMap[attackFrameEvent].onExit(isForceEnd);
    }

    private AttackProcessorBase createNewAttackProcessor(FrameEventType frameEventType)
    {
        switch(frameEventType)
        {
            case FrameEventType.FrameEvent_Attack:
                return new AttackProcessor();
            case FrameEventType.FrameEvent_FollowAttack:
                return new FollowAttackProcessor();
        }

        DebugUtil.assert(false, "Attack FrameEvent가 아닌데 AttackProcessor로 들어왔습니다. 통보 요망 [Type: ]" + frameEventType);
        return null;
    }
}
