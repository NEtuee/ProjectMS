using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityBase : SequencerObjectBase
{
    private SpriteRenderer _spriteRenderer;
    private ActionGraph _actionGraph;
    private CollisionInfo _collisionInfo;

    private MovementControl _movementControl = new MovementControl();
    

    private Vector3 _direction = Vector3.right;

    public override void assign()
    {
        base.assign();

        AddAction(MessageTitles.game_teleportTarget,(msg)=>{
            Transform targetTransform = (Transform)msg.data;
            transform.position = targetTransform.position;
        });

        CollisionInfoData data = new CollisionInfoData(0.1f,0f);
        _collisionInfo = new CollisionInfo(data);
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
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0f);

            string prevActionName = _actionGraph.getCurrentActionName();
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Dash, Input.GetKey(KeyCode.Space));

            //action ,animation, movementGraph 바뀌는 시점
            if(_actionGraph.progress(Time.deltaTime, this) == true)
            {
                //movement 바뀌는 시점
                _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
//                Debug.Log("execute : " + prevActionName + " -> " + _actionGraph.getCurrentActionName());
            }

            _spriteRenderer.sprite = _actionGraph.getCurrentSprite();
            FlipState flipState = _actionGraph.getCurrentFlipState(_direction);
            _spriteRenderer.flipX = flipState.xFlip;
            _spriteRenderer.flipY = flipState.yFlip;
        }
    }

    public override void release(bool disposeFromMaster)
    {
        base.release(disposeFromMaster);

        if(disposeFromMaster == false)
            _movementControl?.release();
            
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
        return _movementControl != null && _actionGraph != null && _spriteRenderer != null && _collisionInfo != null;
    }

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _actionGraph.getMoveValuePerFrameFromTimeDesc();}
    public MovementGraph getCurrentMovementGraph(){return _actionGraph.getCurrentMovementGraph();}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return _actionGraph.getCurrentMovementGraphPreset();}
    public MovementControl getMovementControl(){return _movementControl;}
}
