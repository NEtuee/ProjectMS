
using System.Collections.Generic;

[System.Serializable]
public class ActionGraph
{
    private int _currentActionNodeIndex = -1;
    private int _prevActionNodeIndex = -1;

    private Dictionary<ConditionNodeUpdateType, byte[]> _actionConditionNodeData = new Dictionary<ConditionNodeUpdateType, byte[]>();

    private Dictionary<string, byte[]>          _statusConditionData = new Dictionary<string, byte[]>();
    private HashSet<string>                     _targetFrameTagData = new HashSet<string>();

    private ActionGraphBaseData                 _actionGraphBaseData;
    private AnimationPlayer                     _animationPlayer = new AnimationPlayer();

    private HashSet<string>                     _currentFrameTag = new HashSet<string>();
    private List<byte[]>                        _conditionResultList = new List<byte[]>();

    public ActionGraph(){}
    public ActionGraph(ActionGraphBaseData baseData){_actionGraphBaseData = baseData;}

    public bool _actionChangedByOther = false;

    public void assign()
    {
        createCoditionNodeDataAll();
    }

    public void initialize(ActionGraphBaseData baseData)
    {
        _actionGraphBaseData = baseData;

        initialize();
    }

    public void initialize()
    {
        _animationPlayer.initialize();
        if(_actionGraphBaseData._defaultActionIndex != -1)
            changeAction(_actionGraphBaseData._defaultActionIndex);
    }

    public bool progress()
    {
        if(_actionChangedByOther == true)
        {
            _actionChangedByOther = false;
            return true;
        }

        return processAction(getCurrentAction());
    }

    public void updateAnimation(float deltaTime, GameEntityBase targetEntity)
    {
        bool isEnd = _animationPlayer.progress(deltaTime, targetEntity);
        _animationPlayer.processMultiSelectAnimation(this);
        setActionConditionData_Bool(ConditionNodeUpdateType.Action_AnimationEnd,isEnd);
    }

    public void release()
    {

    }

    

    private void createCoditionNodeDataAll()
    {
        DebugUtil.assert((int)ConditionNodeUpdateType.Count == 36, "check this");

        foreach(var item in ConditionNodeInfoPreset._nodePreset.Values)
        {
            if(item._updateType == ConditionNodeUpdateType.Literal || 
                item._updateType == ConditionNodeUpdateType.ConditionResult || 
                item._updateType == ConditionNodeUpdateType.Status ||
                item._updateType == ConditionNodeUpdateType.Key || 
                item._updateType == ConditionNodeUpdateType.FrameTag || 
                item._updateType == ConditionNodeUpdateType.TargetFrameTag ||
                item._updateType == ConditionNodeUpdateType.Weight)
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

    public void changeActionOther(int actionIndex)
    {
        _actionChangedByOther = changeAction(actionIndex);
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
        bool weightCondition = true;
        if(branchData._weightConditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData compareData = _actionGraphBaseData._conditionCompareData[branchData._weightConditionCompareDataIndex];
            weightCondition = processActionCondition(compareData);
        }
        
        bool keyCondition = true;
        if(branchData._keyConditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData keyCompareData = _actionGraphBaseData._conditionCompareData[branchData._keyConditionCompareDataIndex];
            keyCondition = processActionCondition(keyCompareData);
        }

        bool condition = true;
        if(branchData._conditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData compareData = _actionGraphBaseData._conditionCompareData[branchData._conditionCompareDataIndex];
            condition = processActionCondition(compareData);
        }

        return weightCondition && keyCondition && condition;

    }

    public bool processActionCondition(ActionGraphConditionCompareData compareData)
    {
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

    public void setActionConditionData_TargetFrameTag(HashSet<string> targetFrameTagHashset)
    {
        _targetFrameTagData = targetFrameTagHashset;
    }

    public bool setActionConditionData_Status(string statusName, float value)
    {
        if(_statusConditionData.ContainsKey(statusName) == false)
            _statusConditionData.Add(statusName, new byte[4]);

        return copyBytes_Float(value,_statusConditionData[statusName]);
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
        {
            return ((ActionGraphConditionNodeData_Literal)nodeData).getLiteral();
        }
        else if(updateType == ConditionNodeUpdateType.ConditionResult)
        {
            return _conditionResultList[((ActionGraphConditionNodeData_ConditionResult)nodeData).getResultIndex()];
        }
        else if(updateType == ConditionNodeUpdateType.Status)
        {
            return _statusConditionData[((ActionGraphConditionNodeData_Status)nodeData)._targetStatus];
        }
        else if(updateType == ConditionNodeUpdateType.FrameTag)
        {
            return _currentFrameTag.Contains(((ActionGraphConditionNodeData_FrameTag)nodeData)._targetFrameTag) ? CommonConditionNodeData.trueByte : CommonConditionNodeData.falseByte;
        }
        else if(updateType == ConditionNodeUpdateType.TargetFrameTag)
        {
            if(_targetFrameTagData == null)
                return CommonConditionNodeData.falseByte;

            return _targetFrameTagData.Contains(((ActionGraphConditionNodeData_FrameTag)nodeData)._targetFrameTag) ? CommonConditionNodeData.trueByte : CommonConditionNodeData.falseByte;
        }
        else if(updateType == ConditionNodeUpdateType.Weight)
        {
            ActionGraphConditionNodeData_Weight data = (ActionGraphConditionNodeData_Weight)nodeData;
            return WeightRandomManager.Instance().getRandom(data._weightGroupKey, data._weightName) ? CommonConditionNodeData.trueByte : CommonConditionNodeData.falseByte;

        }
        else if(updateType == ConditionNodeUpdateType.Key)
        {
            return ActionKeyInputManager.Instance().actionKeyCheck(((ActionGraphConditionNodeData_Key)nodeData)._targetKeyName);
        }

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

    public int getActionIndex(string nodeName) 
    {
        if(_actionGraphBaseData._actionIndexMap.ContainsKey(nodeName) == false)
        {
            DebugUtil.assert(false, "target action is not exists: {0}",nodeName);
            return -1;
        }

        return _actionGraphBaseData._actionIndexMap[nodeName];
    }

    public bool applyFrameTag(string tag)
    {
        if(_currentFrameTag.Contains(tag) == true)
            return false;

        _currentFrameTag.Add(tag);
        return true;
    }

    public void deleteFrameTag(string tag)
    {
        if(_currentFrameTag.Contains(tag))
            _currentFrameTag.Remove(tag);
    }

    public HashSet<string> getCurrentFrameTagList() {return _currentFrameTag;}

    public void setAnimationSpeed(float speed) {_animationPlayer.setAnimationSpeed(speed);}

    public bool checkFrameTag(string tag) {return _currentFrameTag.Contains(tag);}

    public bool isActionLoop() {return _currentActionNodeIndex == _prevActionNodeIndex;}
    public int[] getDefaultBuffList() {return _actionGraphBaseData._defaultBuffList;}
    public int[] getCurrentBuffList() {return getCurrentAction()._applyBuffList;}
    public float getCurrentMoveScale() 
    {
        if(getCurrentAction()._normalizedSpeed == false)
            return getCurrentAction()._moveScale;
        else
            return getCurrentAction()._moveScale * _animationPlayer.getCurrentAnimationDuration();
    }

    public float getCurrentFrame() {return _animationPlayer.getCurrentFrame();}
    public float getCurrentDefenceAngle() {return getCurrentAction()._defenceAngle;}
    public DefenceType getCurrentDefenceType() {return getCurrentAction()._defenceType;}
    public RotationType getCurrentRotationType() {return getCurrentAction()._rotationType;}
    public DirectionType getDirectionType() {return getCurrentAction()._directionType;}
    public bool getCurrentDirectionUpdateOnce() {return getCurrentAction()._directionUpdateOnce;}

    public DefenceDirectionType getDefenceDirectionType() {return getCurrentAction()._defenceDirectionType;}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _animationPlayer.getMoveValuePerFrameFromTimeDesc();}
    public string getCurrentActionName() {return getCurrentAction()._nodeName; }
    public UnityEngine.Sprite getCurrentSprite() {return _animationPlayer.getCurrentSprite();}
    public UnityEngine.Quaternion getAnimationRotationPerFrame() {return _animationPlayer.getAnimationRotationPerFrame();}
    public UnityEngine.Quaternion getCurrentAnimationRotation() {return _animationPlayer.getCurrentAnimationRotation();}
    public UnityEngine.Vector3 getAnimationScalePerFrame() {return _animationPlayer.getAnimationScalePerFrame();}
    public UnityEngine.Vector3 getCurrentAnimationScale() {return _animationPlayer.getCurrentAnimationScale();}
    public FlipState getCurrentFlipState() {return _animationPlayer.getCurrentFlipState();}
    public FlipType getCurrentFlipType() {return getCurrentAction()._flipType;}
    public MovementGraph getCurrentMovementGraph() {return _animationPlayer.getCurrentMovementGraph();}
    public MovementBase.MovementType getCurrentMovement() {return getCurrentAction()._movementType;}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return getCurrentAction()._movementGraphPresetData;}
    private ActionGraphNodeData getCurrentAction() {return _actionGraphBaseData._actionNodeData[_currentActionNodeIndex];}
}
