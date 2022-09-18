
using System.Collections.Generic;

//todo : animation start, ed frame, 
public class ActionGraph
{
    private int _currentActionNodeIndex = -1;
    private int _prevActionNodeIndex = -1;

    private Dictionary<ConditionNodeUpdateType, byte[]> _actionConditionNodeData = new Dictionary<ConditionNodeUpdateType, byte[]>();

    private ActionGraphBaseData _actionGraphBaseData;
    private AnimationPlayer _animationPlayer = new AnimationPlayer();
    private List<byte[]> _conditionResultList = new List<byte[]>();

    public ActionGraph(){}
    public ActionGraph(ActionGraphBaseData baseData){_actionGraphBaseData = baseData;}

    public void assign()
    {
        createCoditionNodeDataAll();
    }

    public void initialize()
    {
        _animationPlayer.initialize();
        if(_actionGraphBaseData._defaultActionIndex != -1)
            changeAction(_actionGraphBaseData._defaultActionIndex);
    }

    public bool progress()
    {
        return processAction(getCurrentAction());
    }

    public void updateAnimation(float deltaTime, GameEntityBase targetEntity)
    {
        bool isEnd = _animationPlayer.progress(deltaTime, targetEntity);
        setActionConditionData_Bool(ConditionNodeUpdateType.Action_AnimationEnd,isEnd);
    }

    public void release()
    {

    }

    private void createCoditionNodeDataAll()
    {
        foreach(var item in ConditionNodeInfoPreset._nodePreset.Values)
        {
            if(item._updateType == ConditionNodeUpdateType.Literal || item._updateType == ConditionNodeUpdateType.ConditionResult)
                continue;

            createConditionNodeData(item);
        }

    }

    private void createConditionNodeData(ConditionNodeInfo info)
    {
        if(_actionConditionNodeData.ContainsKey(info._updateType) == true)
        {
            DebugUtil.assert(false, "key already exists");
            return;
        }
        else if(info._updateType == ConditionNodeUpdateType.Count || info._nodeType == ConditionNodeType.Count)
        {
            DebugUtil.assert(false, "wrong type");
            return;
        }


        _actionConditionNodeData.Add(info._updateType, new byte[ConditionNodeInfoPreset._dataSize[(int)info._nodeType]]);
    }

    private bool processAction(ActionGraphNodeData actionData)
    {
        bool actionChanged = false;
        int startIndex = actionData._branchIndexStart;
        for(int i = startIndex; i < startIndex + actionData._branchCount; ++i)
        {
            if(processActionBranch(_actionGraphBaseData._branchData[i]) == true)
            {
                actionChanged = changeAction(_actionGraphBaseData._branchData[i]._branchActionIndex);
                break;
            }
        }

        return actionChanged;
    }

    private bool changeAction(int actionIndex)
    {
        if(actionIndex < 0 || actionIndex >= _actionGraphBaseData._actionNodeCount)
        {
            DebugUtil.assert(false,"invalid action index");
            return false;
        }

        int prevIndex = _prevActionNodeIndex;
        int currIndex = _currentActionNodeIndex;

        _prevActionNodeIndex = _currentActionNodeIndex;
        _currentActionNodeIndex = actionIndex;

        int animationInfoIndex = getCurrentAction()._animationInfoIndex;
        if(animationInfoIndex != -1)
            changeAnimation(animationInfoIndex);

        if(getCurrentAction()._isActionSelection == true)
        {
            if(processAction(getCurrentAction()) == false)
            {
                _prevActionNodeIndex = prevIndex;
                _currentActionNodeIndex = currIndex;

                return false;
            }
        }

        return true;
    }

    private void changeAnimation(int animationIndex)
    {
        if(animationIndex < 0 || animationIndex >= _actionGraphBaseData._animationPlayDataCount)
        {
            DebugUtil.assert(false,"invalid animation index");
            return;
        }

        _animationPlayer.changeAnimation(_actionGraphBaseData._animationPlayData[animationIndex]);
    }

    public bool processActionBranch(ActionGraphBranchData branchData)
    {
        ActionGraphConditionCompareData compareData = _actionGraphBaseData._conditionCompareData[branchData._conditionCompareDataIndex];

        if(compareData._conditionNodeDataCount == 0)
            return true;

        if(compareData._conditionNodeDataCount == 1)
        {
            DebugUtil.assert(isNodeType(compareData._conditionNodeDataArray[0],ConditionNodeType.Bool) == true, "invalid node data type!!! : {0}",compareData._conditionNodeDataArray[0]._symbolName);

            return getDataFromConditionNode(compareData._conditionNodeDataArray[0])[0] == 1;
        }

        for(int i = 0; i < compareData._compareTypeCount; ++i)
        {
            ActionGraphConditionNodeData lvalue = compareData._conditionNodeDataArray[i * 2];
            ActionGraphConditionNodeData rvalue = compareData._conditionNodeDataArray[i * 2 + 1];

            addResultData(compareTwoCondition(lvalue,rvalue,compareData._compareTypeArray[i]), i);
        }

        return _conditionResultList[compareData._compareTypeCount - 1][0] == 1;
    }

    public bool setActionConditionData_Int(ConditionNodeUpdateType updateType, int value)
    {
        if((int)updateType <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"invalud type : {0}",updateType);
            return false;
        }

        return copyBytes_Int(value,_actionConditionNodeData[updateType]);
    }

    public bool setActionConditionData_Float(ConditionNodeUpdateType updateType, float value)
    {
        if((int)updateType <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"invalud type : {0}",updateType);
            return false;
        }

        return copyBytes_Float(value,_actionConditionNodeData[updateType]);
    }

    public bool setActionConditionData_Bool(ConditionNodeUpdateType updateType, bool value)
    {
        if((int)updateType <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"invalud type : {0}",updateType);
            return false;
        }

        return copyBytes_Bool(value,_actionConditionNodeData[updateType]);
    }

    private unsafe bool copyBytes_Int(int value, byte[] destination, int offset = 0)
    {
        if (destination == null)
        {
            DebugUtil.assert(false,"byte destination is nullptr");
            return false;
        }

        if (offset < 0 || (offset + sizeof(int) > destination.Length))
        {
            DebugUtil.assert(false,"byte offset out of index");
            return false;
        }

        fixed (byte* ptrToStart = destination)
        {
            *(int*)(ptrToStart + offset) = value;
        }

        return true;
    }

    private unsafe bool copyBytes_Float(float value, byte[] destination, int offset = 0)
    {
        if (destination == null)
        {
            DebugUtil.assert(false,"byte destination is nullptr");
            return false;
        }

        if (offset < 0 || (offset + sizeof(float) > destination.Length))
        {
            DebugUtil.assert(false,"byte offset out of index");
            return false;
        }

        fixed (byte* ptrToStart = destination)
        {
            *(float*)(ptrToStart + offset) = value;
        }

        return true;
    }

    private unsafe bool copyBytes_Bool(bool value, byte[] destination, int offset = 0)
    {
        if (destination == null)
        {
            DebugUtil.assert(false,"byte destination is nullptr");
            return false;
        }

        if (offset < 0 || (offset + 1 > destination.Length))
        {
            DebugUtil.assert(false,"byte offset out of index");
            return false;
        }

        destination[offset] = value ? (byte)1 : (byte)0;

        return true;
    }


    //todo : 이제 비교는 되니까 식 만들어서 최종 결과 계산할 수 있게 만들어야 함. 단순 boolean, inverse boolean 처리 할 수 있도록 해야 함
    public bool compareTwoCondition(ActionGraphConditionNodeData lvalue, ActionGraphConditionNodeData rvalue, ConditionCompareType compareType)
    {
        ConditionNodeInfo lvalueNodeInfo = ConditionNodeInfoPreset._nodePreset[lvalue._symbolName];
        ConditionNodeInfo rvalueNodeInfo = ConditionNodeInfoPreset._nodePreset[rvalue._symbolName];

        if(lvalueNodeInfo._nodeType != rvalueNodeInfo._nodeType)
        {
            DebugUtil.assert(false,"value type is not match : {0}[{1}] {2}[{3}]",lvalueNodeInfo._nodeType,lvalue._symbolName, rvalueNodeInfo._nodeType,rvalue._symbolName);
            return false;
        }

        byte[] lvalueData = getDataFromConditionNode(lvalue);
        byte[] rvalueData = getDataFromConditionNode(rvalue);

        int dataSize = ConditionNodeInfoPreset._dataSize[(int)lvalueNodeInfo._nodeType];

        switch(compareType)
        {
        case ConditionCompareType.Equals:
            for(int i = 0; i < dataSize; ++i)
            {
                if(lvalueData[i] != rvalueData[i])
                    return false;
            }
            return true;
        case ConditionCompareType.Inverse:
            return !System.BitConverter.ToBoolean(lvalueData,0);
        case ConditionCompareType.NotEquals:
            for(int i = 0; i < dataSize; ++i)
            {
                if(lvalueData[i] != rvalueData[i])
                    return true;
            }
            return false;
        case ConditionCompareType.And:
            if(lvalueNodeInfo._nodeType != ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            return lvalueData[0] > 0 && rvalueData[0] > 0;
        case ConditionCompareType.Greater:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Int)
            {
                int l = System.BitConverter.ToInt32(lvalueData,0);
                int r = System.BitConverter.ToInt32(rvalueData,0);
                return l > r;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Float)
            {
                float l = System.BitConverter.ToSingle(lvalueData,0);
                float r = System.BitConverter.ToSingle(rvalueData,0);
                return l > r;
            }
            break;
        case ConditionCompareType.GreaterEqual:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Int)
            {
                int l = System.BitConverter.ToInt32(lvalueData,0);
                int r = System.BitConverter.ToInt32(rvalueData,0);
                return l >= r;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Float)
            {
                float l = System.BitConverter.ToSingle(lvalueData,0);
                float r = System.BitConverter.ToSingle(rvalueData,0);

                return l >= r;
            }
            break;
        case ConditionCompareType.Or:
            if(lvalueNodeInfo._nodeType != ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            return lvalueData[0] > 0 || rvalueData[0] > 0;
        case ConditionCompareType.Smaller:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Int)
            {
                int l = System.BitConverter.ToInt32(lvalueData,0);
                int r = System.BitConverter.ToInt32(rvalueData,0);
                return l < r;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Float)
            {
                float l = System.BitConverter.ToSingle(lvalueData,0);
                float r = System.BitConverter.ToSingle(rvalueData,0);
                return l < r;
            }
            break;
        case ConditionCompareType.SmallerEqual:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"this type is not supported : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Int)
            {
                int l = System.BitConverter.ToInt32(lvalueData,0);
                int r = System.BitConverter.ToInt32(rvalueData,0);
                return l <= r;
            }
            else if(lvalueNodeInfo._nodeType == ConditionNodeType.Float)
            {
                float l = System.BitConverter.ToSingle(lvalueData,0);
                float r = System.BitConverter.ToSingle(rvalueData,0);
                return l <= r;
            }
            break;
        }

            DebugUtil.assert(false,"invalid type : {0}",lvalueNodeInfo._nodeType);

        return false;
    }

    public bool isNodeType(ActionGraphConditionNodeData nodeData, ConditionNodeType nodeType)
    {
        return nodeType == ConditionNodeInfoPreset._nodePreset[nodeData._symbolName]._nodeType;
    }

    public byte[] getDataFromConditionNode(ActionGraphConditionNodeData nodeData)
    {
        ConditionNodeType nodeType = ConditionNodeInfoPreset._nodePreset[nodeData._symbolName]._nodeType;
        ConditionNodeUpdateType updateType = ConditionNodeInfoPreset._nodePreset[nodeData._symbolName]._updateType;

        if(updateType == ConditionNodeUpdateType.Literal)
            return ((ActionGraphConditionNodeData_Literal)nodeData).getLiteral();
        else if(updateType == ConditionNodeUpdateType.ConditionResult)
            return _conditionResultList[((ActionGraphConditionNodeData_ConditionResult)nodeData).getResultIndex()];

        if(_actionConditionNodeData.ContainsKey(updateType) == false)
        {
            DebugUtil.assert(false,"target update type does not exists : {0}",updateType);
            return null;
        }

        return _actionConditionNodeData[updateType];
    }

    public void addResultData(bool value, int index)
    {
        if(_conditionResultList.Count <= index)
            _conditionResultList.Add(System.BitConverter.GetBytes(value));
        else
            _conditionResultList[index][0] = value == true ? (byte)1 : (byte)0;
    }


    public float getCurrentMoveScale() {return getCurrentAction()._moveScale;}
    public DefenceType getCurrentDefenceType() {return getCurrentAction()._defenceType;}
    public RotationType getCurrentRotationType() {return getCurrentAction()._rotationType;}
    public DirectionType getDirectionType() {return getCurrentAction()._directionType;}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _animationPlayer.getMoveValuePerFrameFromTimeDesc();}
    public string getCurrentActionName() {return getCurrentAction()._nodeName; }
    public UnityEngine.Sprite getCurrentSprite() {return _animationPlayer.getCurrentSprite();}
    public FlipState getCurrentFlipState() {return _animationPlayer.getCurrentFlipState();}
    public FlipType getCurrentFlipType() {return getCurrentAction()._flipType;}
    public MovementGraph getCurrentMovementGraph() {return _animationPlayer.getCurrentMovementGraph();}
    public MovementBase.MovementType getCurrentMovement() {return getCurrentAction()._movementType;}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return getCurrentAction()._movementGraphPresetData;}
    private ActionGraphNodeData getCurrentAction() {return _actionGraphBaseData._actionNodeData[_currentActionNodeIndex];}
}
