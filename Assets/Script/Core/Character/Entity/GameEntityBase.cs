using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityBase : SequencerObjectBase
{
    private SpriteRenderer _spriteRenderer;
    private ActionGraph _actionGraph;

    private MovementControl _movementControl = new MovementControl();
    

    private Vector3 _direction = Vector3.right;
    private FlipState _flipState = new FlipState();

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
        _actionGraph = new ActionGraph(ActionGraphLoader.readFromXML(IOControl.PathForDocumentsFile("Assets\\Data\\Example\\ActionGraphTest.xml")));
        _actionGraph.assign();
        _actionGraph.initialize();

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        directionUpdate();

        _movementControl?.progress(deltaTime, _direction);

        if(_actionGraph != null)
        {
            string prevActionName = _actionGraph.getCurrentActionName();
            
            updateConditionData();

            //action ,animation, movementGraph 바뀌는 시점
            if(_actionGraph.progress(Time.deltaTime, this) == true)
            {
                //movement 바뀌는 시점
                _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
                _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());
                Debug.Log("execute : " + prevActionName + " -> " + _actionGraph.getCurrentActionName());
            }

            _spriteRenderer.sprite = _actionGraph.getCurrentSprite();
            _flipState = getCurrentFlipState();

            _spriteRenderer.flipX = _flipState.xFlip;
            _spriteRenderer.flipY = _flipState.yFlip;
        }
    }

    public void updateConditionData()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0f).normalized;
        Vector3 mousePosition = (MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position).normalized;

        float angleBetweenStick = Vector3.SignedAngle(input, mousePosition,Vector3.forward);
        angleBetweenStick += angleBetweenStick < 0f ? 360f : 0f;

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Dash, Input.GetKey(KeyCode.Space));
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleBetweenStick, angleBetweenStick);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleDirection, Vector3.Angle(Vector3.right, _direction));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsXFlip, _flipState.xFlip);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsYFlip, _flipState.yFlip);
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

        DebugUtil.assert((int)FlipType.Count == 3, "flip type count error");

        return flipState;
    }

    //todo : input manager 만들어서 거기서 moveiNput 가져오게 만들기
    private void directionUpdate()
    {
        switch(_actionGraph.getDirectionType())
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

    public bool isMoving()
    {
        return _movementControl.isMoving();
    }
    public bool isValid() 
    {
        return _movementControl != null && _actionGraph != null && _spriteRenderer != null;
    }

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _actionGraph.getMoveValuePerFrameFromTimeDesc();}
    public MovementGraph getCurrentMovementGraph(){return _actionGraph.getCurrentMovementGraph();}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return _actionGraph.getCurrentMovementGraphPreset();}
    public MovementControl getMovementControl(){return _movementControl;}
}
