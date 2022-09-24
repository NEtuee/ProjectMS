using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityBase : SequencerObjectBase
{

    public string actionGraphPath = "Assets\\Data\\Example\\ActionGraphTest.xml";
    public string statusInfoName = "CommonPlayerStatus";

    private SpriteRenderer      _spriteRenderer;
    private GameObject          _spriteObject;
    private ActionGraph         _actionGraph;
    private StatusInfo          _statusInfo;


    private MovementControl     _movementControl = new MovementControl();

    private FlipState           _flipState = new FlipState();
    private Quaternion          _spriteRotation = Quaternion.identity;

    private AttackState         _attackState = AttackState.Default;
    private DefenceState        _defenceState = DefenceState.Default;

    public override void assign()
    {
        base.assign();

        AddAction(MessageTitles.game_teleportTarget,(msg)=>{
            Transform targetTransform = (Transform)msg.data;
            transform.position = targetTransform.position;
        });
    }

    public override void initialize()
    {
        base.initialize();
        _actionGraph = new ActionGraph(ActionGraphLoader.readFromXML(IOControl.PathForDocumentsFile(actionGraphPath)));
        _actionGraph.assign();
        _actionGraph.initialize();

        _statusInfo = new StatusInfo(statusInfoName);

        createSpriteRenderObject();
    }
    
    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        if(_actionGraph != null)
        {
            string prevActionName = _actionGraph.getCurrentActionName();
            
            updateConditionData();

            //action,movementGraph 바뀌는 시점
            if(_actionGraph.progress() == true)
            {
                //movement 바뀌는 시점
                _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
                _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());
                //Debug.Log("execute : " + prevActionName + " -> " + _actionGraph.getCurrentActionName());
            }

            updateDirection();
            _movementControl?.progress(deltaTime, _direction);

            //animation 바뀌는 시점
            _actionGraph.updateAnimation(Time.deltaTime, this);

            _spriteRenderer.sprite = _actionGraph.getCurrentSprite();
            _flipState = getCurrentFlipState();

            _spriteRenderer.flipX = _flipState.xFlip;
            _spriteRenderer.flipY = _flipState.yFlip;
        }

        rotationUpdate();
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);
        resetState();

        _statusInfo.updateStatus(deltaTime);

    }

    public void resetState()
    {
        _attackState = AttackState.Default;
        _defenceState = DefenceState.Default;
    }

    public void updateConditionData()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0f).normalized;
        Vector3 mousePosition = (MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position).normalized;

        float angleBetweenStick = MathEx.clampDegree(Vector3.SignedAngle(input, mousePosition,Vector3.forward));
        float angleDirection = MathEx.clampDegree(Vector3.SignedAngle(Vector3.right, _direction, Vector3.forward));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Dash, Input.GetKey(KeyCode.Space));
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleBetweenStick, angleBetweenStick);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleDirection, angleDirection);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsXFlip, _flipState.xFlip);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsYFlip, _flipState.yFlip);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_AttackCharge, Input.GetMouseButton(0));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_Guard, Input.GetMouseButton(1));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Blocked, _attackState == AttackState.AttackBlocked);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Success, _attackState == AttackState.AttackSuccess);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Crash, _defenceState == DefenceState.DefenceCrash);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Success, _defenceState == DefenceState.DefenceSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Hit, _defenceState == DefenceState.Hit);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Entity_Dead, _statusInfo.isDead());

    }

    public override void release(bool disposeFromMaster)
    {
        base.release(disposeFromMaster);

        if(disposeFromMaster == false)
            _movementControl?.release();
            
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
                Vector3 direction = MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position;
                if(MathEx.abs(direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = direction.x < 0;
                if(MathEx.abs(direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = direction.y < 0;
                break;
            case FlipType.Keep:
                flipState.xFlip = _spriteRenderer.flipX;
                flipState.yFlip = _spriteRenderer.flipY;
                break;
        }

        DebugUtil.assert((int)FlipType.Count == 4, "flip type count error");

        return flipState;
    }

    //todo : input manager 만들어서 거기서 moveiNput 가져오게 만들기
    private void updateDirection()
    {
        DirectionType directionType = DirectionType.AlwaysRight;
        if(_actionGraph != null)
            directionType = _actionGraph.getDirectionType();

        switch(directionType)
        {
            case DirectionType.AlwaysRight:
                _direction = Vector3.right;
                break;
            case DirectionType.Keep:
                break;
            case DirectionType.MoveInput:
                Vector3 input = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0f);
                if(MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false )
                {
                    _direction = input;
                    _direction.Normalize();
                }
                else
                {
                    _direction = Vector3.zero;
                }

                break;
            case DirectionType.MousePoint:
                _direction = MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position;
                _direction.Normalize();
                break;
            case DirectionType.Count:
                DebugUtil.assert(false, "invalid direction type : {0}",_actionGraph.getDirectionType());
                break;
        }
    }

    private void rotationUpdate()
    {
        RotationType rotationType = RotationType.AlwaysRight;
        if(_actionGraph != null)
            rotationType = _actionGraph.getCurrentRotationType();

        switch(rotationType)
        {
            case RotationType.AlwaysRight:
                _spriteRotation = Quaternion.identity;
                break;
            case RotationType.Direction:
                _spriteRotation = Quaternion.FromToRotation(Vector3.right,_direction);
                break;
            case RotationType.MousePoint:
                _spriteRotation = Quaternion.FromToRotation(Vector3.right,(MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position).normalized);
                break;
            case RotationType.Keep:
                break;
    
        }

        DebugUtil.assert((int)RotationType.Count == 4, "invalid rotation type");
        _spriteObject.transform.localRotation = _spriteRotation;
    }

    public bool isMoving()
    {
        return _movementControl.isMoving();
    }
    public bool isValid() 
    {
        return _movementControl != null && _actionGraph != null && _spriteRenderer != null;
    }

    public void setSpriteRotation(Quaternion rotation)
    {
        _spriteObject.transform.rotation = rotation;
    }

    public void createSpriteRenderObject()
    {
        _spriteObject = new GameObject("SpriteObject");
        _spriteObject.transform.SetParent(this.transform);
        _spriteObject.transform.localPosition = Vector3.zero;

        _spriteRenderer = _spriteObject.AddComponent<SpriteRenderer>();
    }

    public void setAttackState(AttackState state) {_attackState = state;}
    public void setDefenceState(DefenceState state) {_defenceState = state;}

    public string getCurrentActionName() {return _actionGraph == null ? "" : _actionGraph.getCurrentActionName();}
    public DefenceType getDefenceType() {return _actionGraph.getCurrentDefenceType();}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _actionGraph.getMoveValuePerFrameFromTimeDesc();}
    public MovementGraph getCurrentMovementGraph(){return _actionGraph.getCurrentMovementGraph();}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return _actionGraph.getCurrentMovementGraphPreset();}
    public MovementControl getMovementControl(){return _movementControl;}
}
