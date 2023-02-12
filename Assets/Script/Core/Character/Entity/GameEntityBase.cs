using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityBase : SequencerObjectBase
{

    public static float         _defaultFriction = 2f;


    public string               actionGraphPath = "Assets\\Data\\ActionGraph\\ActionGraphTest.xml";
    public string               aiGraphPath = "Assets\\Data\\AIGraph\\CommonEnemyAI.xml";
    public string               statusInfoName = "CommonPlayerStatus";


    public DebugTextManager     debugTextManager;
    public bool                 _actionDebug = false;
    public bool                 _statusDebug = false;
    public bool                 _aiDebug = false;

    
    private ActionGraph         _actionGraph;
    private AIGraph             _aiGraph;
    private DanmakuGraph        _danmakuGraph;
    private StatusInfo          _statusInfo;

    private CollisionInfo       _collisionInfo;


    private MovementControl     _movementControl = new MovementControl();

    private FlipState           _flipState = new FlipState();
    private Quaternion          _spriteRotation = Quaternion.identity;

    private DefenceType         _currentDefenceType = DefenceType.Empty;


    private Vector3             _currentVelocity = Vector3.zero;
    private Vector3             _recentlyAttackPoint = Vector3.zero;
    private Vector3             _defenceDirection = Vector3.zero;


    private int[]               _currentActionBuffList = null;

    private Color               _debugColor = Color.red;

    private GameEntityBase      _currentTarget;

    private bool                _updateDirection = true;
    private bool                _updateFlipState = true;

    private bool                _initializeFromCharacter = false;


    private Quaternion          _actionStartRotation = Quaternion.identity;
    private Quaternion          _angleBaseRotation = Quaternion.identity;

    public override void assign()
    {
        base.assign();

        AddAction(MessageTitles.entity_setTarget,(msg)=>{
            _currentTarget = msg.data as GameEntityBase;
        });

        AddAction(MessageTitles.game_teleportTarget,(msg)=>{
            Transform targetTransform = (Transform)msg.data;
            transform.position = targetTransform.position;
        });

        
        _actionGraph = new ActionGraph();
        _actionGraph.assign();

        _aiGraph = new AIGraph();
        _aiGraph.assign();

        _danmakuGraph = new DanmakuGraph();

        _statusInfo = new StatusInfo();

        createSpriteRenderObject();
    }

    public virtual void initializeCharacter(CharacterInfoData characterInfo)
    {
        base.initialize();
        
        gameObject.name = characterInfo._displayName;
        actionGraphPath = characterInfo._actionGraphPath;
        aiGraphPath = characterInfo._aiGraphPath;
        statusInfoName = characterInfo._statusName;

        _actionGraph.initialize(ResourceContainerEx.Instance().GetActionGraph(characterInfo._actionGraphPath));
        _aiGraph.initialize(this, _actionGraph, ResourceContainerEx.Instance().GetAIGraph(characterInfo._aiGraphPath));
        _danmakuGraph.initialize(this);

        _statusInfo.initialize(characterInfo._statusName);

        applyActionBuffList(_actionGraph.getDefaultBuffList());

        CollisionInfoData data = new CollisionInfoData(characterInfo._characterRadius,0f,0f, CollisionType.Character);
        _collisionInfo = new CollisionInfo(data);

        CollisionManager.Instance().registerObject(_collisionInfo, this);

        _initializeFromCharacter = true;
    }

    public override void initialize()
    {
        if(_initializeFromCharacter)
            return;
        base.initialize();
        
        _actionGraph.initialize(ResourceContainerEx.Instance().GetActionGraph(actionGraphPath));
        _aiGraph.initialize(this, _actionGraph, ResourceContainerEx.Instance().GetAIGraph(aiGraphPath));
        _danmakuGraph.initialize(this);

        _statusInfo.initialize(statusInfoName);

        applyActionBuffList(_actionGraph.getDefaultBuffList());

        CollisionInfoData data = new CollisionInfoData(0.2f,0f,0f, CollisionType.Character);
        _collisionInfo = new CollisionInfo(data);

        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }
    
    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        _statusInfo.updateStatus(deltaTime);
        _statusInfo.updateActionConditionData(this);

        _danmakuGraph.process(deltaTime);

        if(_aiGraph != null)
        {
            _aiGraph.updateConditionData();
            
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_TargetDistance, getDistance(_currentTarget));
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_TargetExists, _currentTarget != null);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_ArrivedTarget, _aiGraph.isAIArrivedTarget());
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_CurrentPackageEnd, _aiGraph.isCurrentPackageEnd());
            _actionGraph.setActionConditionData_TargetFrameTag(_currentTarget == null ? null : _currentTarget.getCurrentFrameTagList());
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_PackageStateExecutedTime, _aiGraph.getCurrentPackageExecutedTime());
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_GraphStateExecutedTime, _aiGraph.getCurrentGraphExecutedTime());

            _aiGraph.progress(deltaTime,this);
        }
        else
        {
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_TargetDistance, 0f);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_TargetExists, false);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_ArrivedTarget, false);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_CurrentPackageEnd, false);
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_PackageStateExecutedTime, 0f);
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_GraphStateExecutedTime, 0f);
        }

        if(_actionGraph != null)
        {
            string prevActionName = _actionGraph.getCurrentActionName();
            
            updateConditionData();

            //action,movementGraph 바뀌는 시점
            if(_actionGraph.progress() == true)
            {
                _currentDefenceType = _actionGraph.getCurrentDefenceType();
                
                if(_actionGraph.isActionLoop() == false)
                {
                    applyActionBuffList();
                    _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
                    _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());

                    _updateDirection = true;
                    _updateFlipState = true;

                    updateDirection();
                    updateRotation();

                    _angleBaseRotation = _spriteRotation;
                    _actionStartRotation = _spriteRotation;
                    _actionStartRotation = Quaternion.Inverse(_actionStartRotation);
                }
            }

            updateDirection();
            
            if(_updateFlipState)
            {
                _flipState = getCurrentFlipState();
                _updateFlipState = _actionGraph.getCurrentFlipTypeUpdateOnce() == false;
            }

            //animation 바뀌는 시점
            _actionGraph.updateAnimation(deltaTime, this);
            _movementControl?.progress(deltaTime, _direction);
            
            updatePhysics(deltaTime);

            _spriteRenderer.transform.localRotation = _actionGraph.getCurrentAnimationRotation();
            _spriteRenderer.transform.localScale = _actionGraph.getCurrentAnimationScale();

            updateRotation();

            _spriteRenderer.sprite = _actionGraph.getCurrentSprite((_spriteRotation * _actionStartRotation).eulerAngles.z);

            _spriteRenderer.flipX = _flipState.xFlip;
            _spriteRenderer.flipY = _flipState.yFlip;
        }

        if(_actionDebug == true)
        {
            debugTextManager.updateDebugText("Action","Action: " + getCurrentActionName());
            debugTextManager.updateDebugText("Defence","Defence: " + getDefenceType());

            if(_aiGraph != null && _aiGraph.isValid())
                debugTextManager.updateDebugText("AI","AIState: " + getCurrentAIName());

            string frameTag = "";
            HashSet<string> frameTagList = _actionGraph.getCurrentFrameTagList();
            foreach(var item in frameTagList)
            {
                frameTag += item + ", ";
            }

            debugTextManager.updateDebugText("FrameTag","FrameTag: " + frameTag);
        }
    


        if(getDefenceAngle() != 0f)
        {
            GizmoHelper.instance.drawArc(transform.position,0.8f,getDefenceAngle(),_defenceDirection,Color.cyan,0f);
        }

        _collisionInfo.updateCollisionInfo(transform.position,getDirection());

        if(_actionDebug == true)
        {
            GizmoHelper.instance.drawLine(transform.position, transform.position + _direction * 0.5f,Color.magenta);
            GizmoHelper.instance.drawLine(transform.position, transform.position + ControllerEx.Instance().getJoystickAxisR(transform.position) * 0.5f,Color.cyan);

            _collisionInfo.drawCollosionArea(_debugColor);

            debugTextManager.updatePosition(new Vector3(0f, _collisionInfo.getBoundBox().getBottom() - transform.position.y, 0f));
        }

        if(_aiDebug == true)
        {
            if(_aiGraph.hasTargetPosition() == true)
            {
                Color targetColor = _aiGraph.isAIArrivedTarget() ? Color.green : Color.red;
                GizmoHelper.instance.drawCircle(_aiGraph.getCurrentTargetPosition(),0.1f,18,targetColor);
                GizmoHelper.instance.drawLine(_aiGraph.getCurrentTargetPosition(), transform.position, targetColor);
            }

            bool hasTarget = _currentTarget != null;
            switch(_aiGraph.getCurrentTargetSearchType())
            {
                case TargetSearchType.Near:
                    GizmoHelper.instance.drawCircle(transform.position,_aiGraph.getCurrentTargetSearchRange(),18,hasTarget ? Color.green : Color.red);
                break;
                case TargetSearchType.NearDirection:
                case TargetSearchType.NearMousePointDirection:
                {
                    Vector3 direction = Vector3.right;
                    if(_aiGraph.getCurrentTargetSearchType() == TargetSearchType.NearDirection)
                        direction = getDirection();
                    else if(_aiGraph.getCurrentTargetSearchType() == TargetSearchType.NearMousePointDirection)
                        direction = getDirectionFromType(DirectionType.MousePoint);

                    GizmoHelper.instance.drawCircle(transform.position,_aiGraph.getCurrentTargetSearchRange(),18,hasTarget ? Color.green : Color.red);
                    GizmoHelper.instance.drawCircle(transform.position + direction * _aiGraph.getCurrentTargetSearchStartRange(),_aiGraph.getCurrentTargetSearchSphereRadius(),18,hasTarget ? Color.green : Color.red);

                    GizmoHelper.instance.drawLine(transform.position + direction * _aiGraph.getCurrentTargetSearchStartRange(), transform.position + direction * _aiGraph.getCurrentTargetSearchRange(), hasTarget ? Color.green : Color.red);
                    GizmoHelper.instance.drawCircle(transform.position + direction * _aiGraph.getCurrentTargetSearchRange(),_aiGraph.getCurrentTargetSearchSphereRadius(),18,hasTarget ? Color.green : Color.red);
                }
                break;
            }

            if(_currentTarget != null)
            {
                GizmoHelper.instance.drawLine(_currentTarget.transform.position, transform.position, Color.cyan);
            }
        }

        _debugColor = Color.red;
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);
        resetState();

        if(_statusDebug == true)
        {
            _statusInfo.updateDebugTextXXX(debugTextManager);
        }
        
        CollisionManager.Instance().collisionRequest(_collisionInfo,this,collisionTest,collisionEndEvent);
    }

    public override void dispose(bool disposeFromMaster)
    {
        CollisionManager.Instance().deregisterObject(_collisionInfo.getCollisionInfoData(),this);
        _aiGraph.release();
        _actionGraph.release();
        _danmakuGraph.release();

        base.dispose(disposeFromMaster);
    }

    private void collisionTest(CollisionSuccessData data)
    {
        _debugColor = Color.green;
    }

    private void collisionEndEvent()
    {

    }

    public void resetState()
    {
        _attackState = AttackState.Default;
        _defenceState = DefenceState.Default;
    }

    public void updateStatusConditionData(string targetName, float value)
    {
        _actionGraph.setActionConditionData_Status(targetName,value);
    }

    public void updateConditionData()
    {
        Vector3 input = ControllerEx.Instance().GetJoystickAxis();
        Vector3 inputDirection = ControllerEx.Instance().getJoystickAxisR(transform.position);

        float angleBetweenStick = MathEx.clampDegree(Vector3.SignedAngle(input, inputDirection,Vector3.forward));
        float angleDirection = MathEx.clampDegree(Vector3.SignedAngle(Vector3.right, _direction, Vector3.forward));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Dash, Input.GetKey(KeyCode.Space));
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleBetweenStick, angleBetweenStick);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleDirection, angleDirection);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsXFlip, _flipState.xFlip);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsYFlip, _flipState.yFlip);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_CurrentFrame, _actionGraph.getCurrentFrame());

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_AttackCharge, Input.GetMouseButton(0));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_AttackBlood, Input.GetKey(KeyCode.R));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_Guard, Input.GetMouseButton(1));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Guarded, _attackState == AttackState.AttackGuarded);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Success, _attackState == AttackState.AttackSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Parried, _attackState == AttackState.AttackParried);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Evaded, _attackState == AttackState.AttackEvade);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_GuardBreak, _attackState == AttackState.AttackGuardBreak);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Crash, _defenceState == DefenceState.DefenceCrash);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Success, _defenceState == DefenceState.DefenceSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Parry, _defenceState == DefenceState.ParrySuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Hit, _defenceState == DefenceState.Hit);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Evade, _defenceState == DefenceState.EvadeSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_GuardBroken, _defenceState == DefenceState.GuardBroken);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Entity_Dead, _statusInfo.isDead());

    }

    public override void release(bool disposeFromMaster)
    {
        base.release(disposeFromMaster);

        if(disposeFromMaster == false)
            _movementControl?.release();
        
        _aiGraph.release();
        _actionGraph.release();
        _danmakuGraph.release();
    }

    private FlipState getCurrentFlipState()
    {
        FlipState currentFlipState = _actionGraph.getCurrentFlipState();
        FlipState flipState = new FlipState();

        FlipType flipType = _actionGraph.getCurrentFlipType();

        switch(flipType)
        {
            case FlipType.Direction:
                if(MathEx.abs(_direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = _direction.x < 0;
                if(MathEx.abs(_direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = _direction.y < 0;

                break;
            case FlipType.MousePoint:
            {
                Vector3 direction = ControllerEx.Instance().getJoystickAxisR(transform.position);
                if(MathEx.abs(direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = direction.x < 0;
                if(MathEx.abs(direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = direction.y < 0;
            }
            break;
            case FlipType.MoveDirection:
            {
                Vector3 direction = getMovementControl().getMoveDirection();
                if(MathEx.abs(direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = direction.x < 0;
                if(MathEx.abs(direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = direction.y < 0;
            }
            break;
            case FlipType.Keep:
                flipState.xFlip = _spriteRenderer.flipX;
                flipState.yFlip = _spriteRenderer.flipY;
                break;
        }

        DebugUtil.assert((int)FlipType.Count == 5, "flip type count error");

        return flipState;
    }

    public void clearActionBuffList()
    {
        _statusInfo.clearBuff();
    }

    public void deleteActionBuffList(int[] buffList)
    {
        if(buffList == null)
            return;

        for(int i = 0; i < buffList.Length; ++i)
        {
            _statusInfo.deleteBuff(buffList[i]);
        }
    }

    public void applyActionBuffList(int[] buffList)
    {
        if(buffList == null)
            return;

        for(int i = 0; i < buffList.Length; ++i)
        {
            _statusInfo.applyBuff(buffList[i]);
        }
    }

    private void applyActionBuffList()
    {
        if(_currentActionBuffList != null)
        {
            for(int i = 0; i < _currentActionBuffList.Length; ++i)
            {
                _statusInfo.deleteBuff(_currentActionBuffList[i]);
            }
        }

        _currentActionBuffList = _actionGraph.getCurrentBuffList();

        applyActionBuffList(_currentActionBuffList);
    }

    private void updatePhysics(float deltaTime)
    {
        transform.position += _currentVelocity * deltaTime;
        _currentVelocity = MathEx.convergence0(_currentVelocity, _defaultFriction * deltaTime);
    }

    //todo : input manager 만들어서 거기서 moveiNput 가져오게 만들기
    private void updateDirection()
    {
        if(_updateDirection == false)
            return;

        DirectionType directionType = DirectionType.AlwaysRight;
        DefenceDirectionType defenceDirectionType = DefenceDirectionType.Direction;

        if(_actionGraph != null)
        {
            directionType = _actionGraph.getDirectionType();
            defenceDirectionType = _actionGraph.getDefenceDirectionType();

            Debug.Log(directionType);
        }

        _direction = getDirectionFromType(directionType);

        switch(defenceDirectionType)
        {
            case DefenceDirectionType.Direction:
                _defenceDirection = _direction;
                break;
            case DefenceDirectionType.MousePoint:
                _defenceDirection = ControllerEx.Instance().getJoystickAxisR(transform.position);
                break;
        }

        _updateDirection = _actionGraph.getCurrentDirectionUpdateOnce() == false;
    }

    public Vector3 getDirectionFromType(DirectionType directionType)
    {
        Vector3 direction = _direction;
        switch(directionType)
        {
            case DirectionType.AlwaysRight:
                direction = Vector3.right;
                break;
            case DirectionType.Keep:
                break;
            case DirectionType.MoveInput:
                Vector3 input = ControllerEx.Instance().GetJoystickAxis();
                if(MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false )
                {
                    direction = input;
                    direction.Normalize();
                }
                else
                {
                    direction = Vector3.zero;
                }

                break;
            case DirectionType.MousePoint:
                direction = ControllerEx.Instance().getJoystickAxisR(transform.position);
                break;
            case DirectionType.AttackedPoint:
                direction = (_recentlyAttackPoint - transform.position).normalized;
                break;
            case DirectionType.AI:
                direction = _aiGraph.getRecentlyAIDirection();
                break;
            case DirectionType.AITarget:
                if(_currentTarget != null && _currentTarget.isValid())
                    direction = (_currentTarget.transform.position - transform.position).normalized;
                break;
            case DirectionType.MoveDirection:
                direction = getMovementControl().getMoveDirection();
                break;
            case DirectionType.Count:
                DebugUtil.assert(false, "invalid direction type : {0}",_actionGraph.getDirectionType());
                break;
        }

        return direction;
    }

    private void updateRotation()
    {
        RotationType rotationType = RotationType.AlwaysRight;
        if(_actionGraph != null)
            rotationType = _actionGraph.getCurrentRotationType();

        float targetRotation = 0f;
        switch(rotationType)
        {
            case RotationType.AlwaysRight:
                targetRotation = 0f;
                break;
            case RotationType.Direction:
                targetRotation = MathEx.directionToAngle(_direction);
                break;
            case RotationType.MousePoint:
                targetRotation = MathEx.directionToAngle( ControllerEx.Instance().getJoystickAxisR(transform.position));
                break;
            case RotationType.MoveDirection:
                targetRotation = MathEx.directionToAngle(getMovementControl().getMoveDirection());
                break;
            case RotationType.Keep:
                break;
    
        }
        DebugUtil.assert((int)RotationType.Count == 5, "check this");

        if(_actionGraph != null && _actionGraph.isRotateBySpeed())
        {
            if (MathEx.equals(_spriteRotation.eulerAngles.z, targetRotation, float.Epsilon) == false)
            {
                float angle = Mathf.MoveTowardsAngle(_spriteRotation.eulerAngles.z, targetRotation, _actionGraph.getCurrentRotateSpeed() * Time.deltaTime);
                _spriteRotation = Quaternion.Euler(0f,0f,angle);
            }
        }
        else
        {
            _spriteRotation = Quaternion.Euler(0f,0f, targetRotation);
        }

        float zRotation = _spriteRotation.eulerAngles.z;
        if(rotationType != RotationType.AlwaysRight)
            zRotation -= (getFlipState().xFlip ? -180f : 0f);

        _spriteObject.transform.localRotation *= Quaternion.Euler(0f,0f,zRotation);
    }

    public bool isMoving()
    {
        return _movementControl.isMoving();
    }
    public bool isValid() 
    {
        return _movementControl != null && _actionGraph != null && _spriteRenderer != null && gameObject.activeInHierarchy;
    }

    public void setSpriteRotation(Quaternion rotation)
    {
        _spriteObject.transform.rotation = rotation;
    }

    public void addDanmaku(string path)
    {
        _danmakuGraph.addDanmakuGraph(path);
    }
    
    public bool isDead() {return _statusInfo.isDead();}

    public void executeAIEvent(AIChildEventType eventType) {_aiGraph.executeAIEvent(eventType);}

    public HashSet<string> getCurrentFrameTagList() {return _actionGraph.getCurrentFrameTagList();}

    public void addVelocity(Vector3 velocity) {_currentVelocity += velocity;}

    public bool applyFrameTag(string tag) {return _actionGraph.applyFrameTag(tag);}
    public void deleteFrameTag(string tag) {_actionGraph.deleteFrameTag(tag);}
    public bool checkFrameTag(string tag) {return _actionGraph.checkFrameTag(tag);}

    public bool isAIGraphValid() {return _aiGraph != null && _aiGraph.isValid();}
    public TargetSearchType getCurrentTargetSearchType() {return _aiGraph.getCurrentTargetSearchType();}
    public SearchIdentifier getCurrentSearchIdentifier() {return _aiGraph.getCurrentSearchIdentifier();}
    public float getCurrentTargetSearchRange() {return _aiGraph.getCurrentTargetSearchRange();}
    public float getCurrentTargetSearchStartRange() {return _aiGraph.getCurrentTargetSearchStartRange();}
    public float getCurrentTargetSearchSphereRadius() {return _aiGraph.getCurrentTargetSearchSphereRadius();}

    public Vector3 getCurrentDefenceDirection() {return _defenceDirection;}

    public void terminateAIPackage() {_aiGraph.terminatePackage();}
    public void setAIState(int index) {_aiGraph.changeAIPackageStateOther(index);}

    public void setAiDirection(float angle) {_aiGraph.setAIDirection(angle);}
    public void setAiDirection(Vector3 direction) {_aiGraph.setAIDirection(direction);}

    public void setAnimationSpeed(float speed) {_actionGraph.setAnimationSpeed(speed);}

    public void setAttackPoint(Vector3 attackPoint) {_recentlyAttackPoint = attackPoint;}

    public void setDefenceType(DefenceType defenceType) {_currentDefenceType = defenceType;}

    public void setAction(int index) {_actionGraph.changeActionOther(index);}
    public void setAction(string nodeName) {_actionGraph.changeActionOther(_actionGraph.getActionIndex(nodeName));}

    public void setTargetEntity(GameEntityBase target) {_currentTarget = target;}
    public GameEntityBase getCurrentTargetEntity() {return _currentTarget;}

    public FlipState getFlipState() {return _flipState;}

    public CollisionInfo getCollisionInfo() {return _collisionInfo;}
    public string getCurrentAIName() {return _aiGraph.getCurrentAIStateName();}
    public string getCurrentActionName() {return _actionGraph == null ? "" : _actionGraph.getCurrentActionName();}
    public float getDefenceAngle() {return _actionGraph.getCurrentDefenceAngle();}
    public DefenceType getDefenceType() {return _currentDefenceType;}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _actionGraph.getMoveValuePerFrameFromTimeDesc();}
    public MovementGraph getCurrentMovementGraph(){return _actionGraph.getCurrentMovementGraph();}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return _actionGraph.getCurrentMovementGraphPreset();}
    public MovementBase getCurrentMovement() {return _movementControl.getCurrentMovement();}
    public MovementControl getMovementControl(){return _movementControl;}
}
