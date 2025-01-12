
using System.Collections.Generic;


[System.Serializable]
public class ActionGraph
{
    public struct InputBufferItem
    {
        public static double kBufferedInputThreshold = 0.13;

        public double _inputStart;
        public string _inputKey;
    }

    private int _currentActionNodeIndex = -1;
    private int _prevActionNodeIndex = -1;

    private int _currentAnimationIndex = 0;

    private List<byte[]> _actionConditionNodeData = null;
    private Dictionary<string, float>           _customValueDictionary = new Dictionary<string, float>();


    private Dictionary<string, byte[]>          _statusConditionData = new Dictionary<string, byte[]>();
    private HashSet<string>                     _targetFrameTagData = new HashSet<string>();

    private ActionGraphBaseData                 _actionGraphBaseData;
    private AnimationPlayer                     _animationPlayer = new AnimationPlayer();

    private HashSet<string>                     _currentFrameTag = new HashSet<string>();
    private List<byte[]>                        _conditionResultList = new List<byte[]>();

    private Dictionary<string, bool>            _graphStateCoolTimeMap = new Dictionary<string, bool>();

    private List<InputBufferItem>               _inputBuffer = new List<InputBufferItem>();

    public ActionGraph(){}
    public ActionGraph(ActionGraphBaseData baseData){_actionGraphBaseData = baseData;}

    private bool _actionChangedByOther = false;
    private bool _blockInput = false;

    private float _actionExecutedTime = 0f;

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
        initializeConditionData();
        _animationPlayer.initialize();
        _actionExecutedTime = 0f;

        _currentActionNodeIndex = -1;
        _prevActionNodeIndex = -1;
        _currentAnimationIndex = 0;

        _blockInput = false;

        _graphStateCoolTimeMap.Clear();
        _inputBuffer.Clear();

        setDefaultAction();
    }

    private void setDefaultAction()
    {
        if(_actionGraphBaseData != null && _actionGraphBaseData._defaultActionIndex != -1)
            changeAction(_actionGraphBaseData._defaultActionIndex);
    }

    public void setDefaultActionOther()
    {
        if(_actionGraphBaseData != null && _actionGraphBaseData._defaultActionIndex != -1)
            changeActionOther(_actionGraphBaseData._defaultActionIndex);
    }

    public bool progress(float deltaTime)
    {
        if(_actionChangedByOther == true)
        {
            _actionChangedByOther = false;
            return true;
        }

        _actionExecutedTime += deltaTime;
        return processAction(getCurrentAction());
    }

    public void clearInputBuffer()
    {
        _inputBuffer.Clear();
    }

    public void updateInputBuffer(string key, ActionKeyInputData inputData)
    {
        for(int i = 0; i < _inputBuffer.Count; ++i)
        {
            if(_inputBuffer[i]._inputKey == key)
            {
                _inputBuffer[i] = new InputBufferItem{_inputKey = key, _inputStart = GlobalTimer.Instance().getScaledGlobalTime()};
                return;
            }
        }

        _inputBuffer.Add(new InputBufferItem{_inputKey = key, _inputStart = GlobalTimer.Instance().getScaledGlobalTime()});
    }

    public bool checkInputBuffer(string key)
    {
        for(int i = 0; i < _inputBuffer.Count; ++i)
        {
            if(_inputBuffer[i]._inputKey == key)
            {
                return (GlobalTimer.Instance().getScaledGlobalTime() - _inputBuffer[i]._inputStart) <= InputBufferItem.kBufferedInputThreshold;
            }
        }

        return false;
    }

    public void updateAnimation(float deltaTime, GameEntityBase targetEntity)
    {
        bool isEnd = _animationPlayer.progress(deltaTime, targetEntity);
        _animationPlayer.processMultiSelectAnimation(this);

        if(isEnd && getCurrentAction()._animationInfoCount > ++_currentAnimationIndex)
        {
            isEnd = false;
            changeAnimation(getCurrentAction()._animationInfoIndex, _currentAnimationIndex);
        }

        if(isEnd)
            _currentAnimationIndex = getCurrentAction()._animationInfoCount - 1;

        setActionConditionData_Bool(ConditionNodeUpdateType.Action_AnimationEnd,isEnd);
    }

    public void updateTriggerEvent(GameEntityBase targetEntity)
    {
        ActionGraphNodeData currentAction = getCurrentAction();
        if(currentAction._triggerCount <= 0)
            return;

        for(int i = 0; i < currentAction._triggerCount; ++i)
        {
            processTriggerEvent(_actionGraphBaseData._triggerEventData[currentAction._triggerIndexStart + i], targetEntity);
        }
    }

    public void processTriggerEvent(ActionGraphTriggerEventData triggerEventData, GameEntityBase targetEntity)
    {
        if(triggerEventData == null)
            return;

        if(triggerEventData._conditionCompareDataIndex >= 0)
        {
            if(processActionCondition(_actionGraphBaseData._conditionCompareData[triggerEventData._conditionCompareDataIndex]) == false)
                return;
        }

        for(int index = 0; index < triggerEventData._frameEventData.Length; ++index)
        {
            ActionFrameEventBase frameEvent = triggerEventData._frameEventData[index];
            frameEvent.initialize(targetEntity);
            frameEvent.onExecute(targetEntity);
            frameEvent.onExit(targetEntity, false);
        }

    }

    public void release()
    {

    }

    private void createCoditionNodeDataAll()
    {
        int conditionCount = (int)ConditionNodeUpdateType.Count;
        _actionConditionNodeData = new List<byte[]>(conditionCount);
        for(int i = 0; i < conditionCount; ++i)
        {
            _actionConditionNodeData.Add(null);
        }

        foreach(var item in ConditionNodeInfoPreset._nodePreset.Values)
        {
            if(item._updateType == ConditionNodeUpdateType.Literal || 
                item._updateType == ConditionNodeUpdateType.ConditionResult || 
                item._updateType == ConditionNodeUpdateType.Status ||
                item._updateType == ConditionNodeUpdateType.Key || 
                item._updateType == ConditionNodeUpdateType.FrameTag || 
                item._updateType == ConditionNodeUpdateType.TargetFrameTag ||
                item._updateType == ConditionNodeUpdateType.Weight ||
                item._updateType == ConditionNodeUpdateType.AngleSector || 
                item._updateType == ConditionNodeUpdateType.AICustomValue ||
                item._updateType == ConditionNodeUpdateType.AI_GraphCoolTime)
                continue;

            _actionConditionNodeData[(int)item._updateType] = new byte[ConditionNodeInfoPreset._dataSize[(int)item._nodeType]];
        }

    }

    private void createConditionNodeData(ConditionNodeInfo info)
    {
        
    }

    private void initializeConditionData()
    {
        foreach(var item in _actionConditionNodeData)
        {
            if(item == null)
                continue;

            for(int i = 0; i < item.Length; ++i)
                item[i] = 0;
        }
    }

    public void initializeCustomValue(AIGraphCustomValue[] customValueData)
    {
        _customValueDictionary.Clear();

        if(customValueData == null)
            return;

        for(int index = 0; index < customValueData.Length; ++index)
        {
            _customValueDictionary.Add(customValueData[index]._name,customValueData[index]._customValue);
        }
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

    public void changeAnimationByPlayInfo(AnimationPlayDataInfo animationPLayDataInfo)
    {
        _animationPlayer.changeAnimation(animationPLayDataInfo);
    }

    public void changeAnimationByCustomPreset(string path)
    {
        _animationPlayer.changeAnimationByCustomPreset(path);
    }

    private bool changeAction(int actionIndex)
    {
        if(actionIndex < 0 || actionIndex >= _actionGraphBaseData._actionNodeCount)
        {
            DebugUtil.assert(false,"잘못된 액션 인덱스 입니다. [{0}]", actionIndex);
            return false;
        }

        int prevIndex = _prevActionNodeIndex;
        int currIndex = _currentActionNodeIndex;

        _prevActionNodeIndex = _currentActionNodeIndex;
        _currentActionNodeIndex = actionIndex;
        _currentAnimationIndex = 0;

        int animationInfoIndex = getCurrentAction()._animationInfoIndex;
        if(animationInfoIndex != -1)
            changeAnimation(animationInfoIndex,_currentAnimationIndex);
        // else
        //     DebugUtil.assert(false, "something is wrong : {0}",getCurrentActionName());

        if(getCurrentAction()._isActionSelection == true)
        {
            if(processAction(getCurrentAction()) == false)
            {
                _prevActionNodeIndex = prevIndex;
                _currentActionNodeIndex = currIndex;

                return false;
            }
        }

        _actionExecutedTime = 0f;
        setActionConditionData_Bool(ConditionNodeUpdateType.Action_AnimationEnd, false);
        setActionConditionData_Float(ConditionNodeUpdateType.Action_ActionExecutedTime, 0f);
        
        return true;
    }

    private void changeAnimation(int animationSetIndex, int animationIndex)
    {
        if(animationIndex < 0 || animationIndex >= _actionGraphBaseData._animationPlayDataCount)
        {
            DebugUtil.assert(false,"잘못된 애니메이션 인덱스 입니다. 통보 요망 [AnimationIndex: {0}]", animationIndex);
            return;
        }

        _animationPlayer.changeAnimation(_actionGraphBaseData._animationPlayData[animationSetIndex][animationIndex]);
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
            DebugUtil.assert(isNodeType(compareData._conditionNodeDataArray[0],ConditionNodeType.Bool) == true, "잘못된 노드 타입 입니다. 통보 요망 : {0}",compareData._conditionNodeDataArray[0]._symbolName);

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
        int updateTypeIndex = (int)updateType;
        if(updateTypeIndex <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"잘못된 타입 입니다. 통보 요망 : {0}",updateType);
            return false;
        }

        return copyBytes_Int(value,_actionConditionNodeData[updateTypeIndex]);
    }

    public bool setActionConditionData_Float(ConditionNodeUpdateType updateType, float value)
    {
        int updateTypeIndex = (int)updateType;
        if(updateTypeIndex <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"잘못된 타입 입니다. 통보 요망 : {0}",updateType);
            return false;
        }

        return copyBytes_Float(value,_actionConditionNodeData[updateTypeIndex]);
    }

    public bool getActionConditionData_Bool(ConditionNodeUpdateType updateType)
    {
        int updateTypeIndex = (int)updateType;
        if(updateTypeIndex <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"잘못된 타입 입니다. 통보 요망 : {0}",updateType);
            return false;
        }

        return _actionConditionNodeData[updateTypeIndex][0] == 1;
    }

    public float getActionConditionData_Float(ConditionNodeUpdateType updateType)
    {
        int updateTypeIndex = (int)updateType;
        if(updateTypeIndex <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"잘못된 타입 입니다. 통보 요망 : {0}",updateType);
            return 0f;
        }

        return System.BitConverter.ToSingle(_actionConditionNodeData[updateTypeIndex],0);
    }

    public bool setActionConditionData_Bool(ConditionNodeUpdateType updateType, bool value)
    {
        int updateTypeIndex = (int)updateType;
        if( updateTypeIndex <= (int)ConditionNodeUpdateType.ConditionResult )
        {
            DebugUtil.assert(false,"잘못된 타입 입니다. 통보 요망 : {0}",updateType);
            return false;
        }

        return copyBytes_Bool(value,_actionConditionNodeData[updateTypeIndex]);
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
            DebugUtil.assert(false,"컨디션 심볼 타입이 서로 비교할 수 없는 타입입니다. : {0}[{1}] {2}[{3}]",lvalueNodeInfo._nodeType,lvalue._symbolName, rvalueNodeInfo._nodeType,rvalue._symbolName);
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
                DebugUtil.assert(false,"해당 타입은 And 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            return lvalueData[0] > 0 && rvalueData[0] > 0;
        case ConditionCompareType.Greater:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"해당 타입은 크기 비교 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
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
                DebugUtil.assert(false,"해당 타입은 크기 비교 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
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
                DebugUtil.assert(false,"해당 타입은 Or 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
                return false;
            }
            return lvalueData[0] > 0 || rvalueData[0] > 0;
        case ConditionCompareType.Smaller:
            if(lvalueNodeInfo._nodeType == ConditionNodeType.Bool)
            {
                DebugUtil.assert(false,"해당 타입은 크기 비교 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
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
                DebugUtil.assert(false,"해당 타입은 크기 비교 연산을 지원하지 않습니다. : {0}",lvalueNodeInfo._nodeType);
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

            DebugUtil.assert(false,"잘못된 타입 입니다. : {0}",lvalueNodeInfo._nodeType);

        return false;
    }

    public static bool isNodeType(ActionGraphConditionNodeData nodeData, ConditionNodeType nodeType)
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
            if(_blockInput)
                return CommonConditionNodeData.falseByte;

            ActionGraphConditionNodeData_Key keyCondition = (ActionGraphConditionNodeData_Key)nodeData;
            if(checkInputBuffer(keyCondition._targetKeyName))
                return CommonConditionNodeData.trueByte;
            
            return ActionKeyInputManager.Instance().actionKeyCheck((keyCondition)._targetKeyName);
        }
        else if(updateType == ConditionNodeUpdateType.AngleSector)
        {
            return ActionKeyInputManager.Instance().actionKeyCheck(((ActionGraphConditionNodeData_Key)nodeData)._targetKeyName);
        }
        else if(updateType == ConditionNodeUpdateType.AICustomValue)
        {
            ActionGraphConditionNodeData_AICustomValue data = (ActionGraphConditionNodeData_AICustomValue)nodeData;
            float customValue = getCustomValue(data._customValueName);
            copyBytes_Float(customValue,data.getLiteral());

            return data.getLiteral();
        }
        else if(updateType == ConditionNodeUpdateType.AI_GraphCoolTime)
        {
            ActionGraphConditionNodeData_AIGraphCoolTime data = (ActionGraphConditionNodeData_AIGraphCoolTime)nodeData;
            return checkAIGraphCoolTimeValue(data._graphNodeName) ? CommonConditionNodeData.trueByte : CommonConditionNodeData.falseByte;
        }

        int updateTypeIndex = (int)updateType;
        if(_actionConditionNodeData[updateTypeIndex] == null)
        {
            DebugUtil.assert(false,"해당 업데이트 타입은 존재하지 않습니다. : {0}",updateType);
            return null;
        }

        return _actionConditionNodeData[updateTypeIndex];
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
            DebugUtil.assert(false, "인덱스를 가져 오려는 액션이 존재하지 않습니다. 통보 요망 : {0}",nodeName);
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

    public void setAIGraphCoolTimeValue(string graphNodeName, bool result)
    {
        _graphStateCoolTimeMap[graphNodeName] = result;
    }

    public bool checkAIGraphCoolTimeValue(string graphNodeName)
    {
        if(_graphStateCoolTimeMap.ContainsKey(graphNodeName) == false)
            return true;

        return _graphStateCoolTimeMap[graphNodeName];
    }

    public void setCustomValue(string customValueName, float value)
    {
        if(_customValueDictionary.ContainsKey(customValueName) == false)
        {
            DebugUtil.assert(false, "대상 CustomValue가 존재하지 않습니다. [Name: {0}]", customValueName);
            return;
        }

        _customValueDictionary[customValueName] = value;
    }

    public void addCustomValue(string customValueName, float value)
    {
        if(_customValueDictionary.ContainsKey(customValueName) == false)
        {
            DebugUtil.assert(false, "대상 CustomValue가 존재하지 않습니다. [Name: {0}]", customValueName);
            return;
        }

        _customValueDictionary[customValueName] += value;
    }

    public float getCustomValue(string customValueName)
    {
        if(_customValueDictionary.ContainsKey(customValueName) == false)
        {
            DebugUtil.assert(false, "대상 CustomValue가 존재하지 않습니다. [Name: {0}]", customValueName);
            return 0f;
        }

        return _customValueDictionary[customValueName];
    }
    
    public void blockInput(bool value)
    {
        _blockInput = value;
    }

    public Dictionary<string,float> getCustomValueDictionary() {return _customValueDictionary;}

    public HashSet<string> getCurrentFrameTagList() {return _currentFrameTag;}

    public float getActionExecutedTime() {return _actionExecutedTime;}

    public bool isActiveCollision() {return getCurrentAction()._activeCollision;}
    public void setAnimationSpeed(float speed) {_animationPlayer.setAnimationSpeed(speed);}

    public bool checkFrameTag(string tag) {return _currentFrameTag.Contains(tag);}

    public bool isActionLoop() {return _currentActionNodeIndex == _prevActionNodeIndex;}
    public int[] getDefaultBuffList() {return _actionGraphBaseData._defaultBuffList;}
    public int[] getCurrentBuffList() {return getCurrentAction()._applyBuffList;}
    public AttackType getCurrentIgnoreAttackType() {return getCurrentAction()._ignoreAttackType;}
    public float getCurrentMoveScale() 
    {
        if(getCurrentAction()._normalizedSpeed == false)
            return getCurrentAction()._moveScale;
        else
            return getCurrentAction()._moveScale * _animationPlayer.getCurrentAnimationDuration();
    }

    public float getAnimationPlayTimeByIndex(int actionIndex)
    {
        int animationInfoIndex = _actionGraphBaseData._actionNodeData[actionIndex]._animationInfoIndex;
        int animationInfoCount = _actionGraphBaseData._actionNodeData[actionIndex]._animationInfoCount;

        float totalDuration = 0f;
        for(int index = 0; index < animationInfoCount; ++index)
        {
            float duration = _actionGraphBaseData._animationPlayData[animationInfoIndex][index].getTotalDuration();
            totalDuration += duration;
        }

        return totalDuration;
    }

    public bool checkCurrentActionFlag(ActionFlags flag)
    {
        return (getCurrentAction()._actionFlag & (ulong)flag) != 0;
    }

    public bool checkPrevActionFlag(ActionFlags flag)
    {
        ActionGraphNodeData prevAction = getPrevAction();
        if(prevAction == null)
            return false;

        return (prevAction._actionFlag & (ulong)flag) != 0;
    }

    public bool isCurrentOutlineAction()
    {
        return checkCurrentActionFlag(ActionFlags.OutlineGuard) || checkCurrentActionFlag(ActionFlags.OutlineSuperArmor);
    }

    public bool isPrevOutlineAction()
    {
        return checkPrevActionFlag(ActionFlags.OutlineGuard) || checkPrevActionFlag(ActionFlags.OutlineSuperArmor);
    }

    public UnityEngine.Color getOutlineColor()
    {
        if(checkCurrentActionFlag(ActionFlags.OutlineGuard))
            return new UnityEngine.Color(0.223f, 0.6f, 0.862f, 1.0f);
        else if(checkCurrentActionFlag(ActionFlags.OutlineSuperArmor))
            return new UnityEngine.Color(1.0f, 0.47f, 0.0f, 1.0f);
        else
            return UnityEngine.Color.white;
    }


    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc()
    {
        if(getCurrentAction()._animationInfoCount == 1)
            return _animationPlayer.getMoveValuePerFrameFromTimeDesc();

        float totalDuration = getAnimationPlayTimeByIndex(_currentActionNodeIndex);
        int currentAnimationInfoIndex = getCurrentAction()._animationInfoIndex;

        float passedDurationRate = 0f;
        for(int index = 0; index < _currentAnimationIndex; ++index)
        {
            float duration = _actionGraphBaseData._animationPlayData[currentAnimationInfoIndex][index].getTotalDuration();
            passedDurationRate += duration * (1f / totalDuration);
        }

        float currentDuration = _actionGraphBaseData._animationPlayData[currentAnimationInfoIndex][_currentAnimationIndex].getTotalDuration();
        MoveValuePerFrameFromTimeDesc timeDesc = _animationPlayer.getMoveValuePerFrameFromTimeDesc();
        timeDesc.currentNormalizedTime = passedDurationRate + (timeDesc.currentTime * (1f / totalDuration));
        timeDesc.prevNormalizedTime = passedDurationRate + (timeDesc.prevTime * (1f / totalDuration));

        return timeDesc;
    }

    public string getCurrentAnimationName() {return _animationPlayer.getCurrentAnimationName();}

    public int getDummyActionIndex() {return _actionGraphBaseData._dummyActionIndex;}
    public bool isRotateBySpeed() {return getCurrentAction()._rotateBySpeed;}
    public float getCurrentRotateSpeed() {return getCurrentAction()._rotateSpeed;}
    public float getCurrentFrame() {return _animationPlayer.getCurrentFrame();}
    public float getCurrentDefenceAngle() {return getCurrentAction()._defenceAngle;}
    public float getCurrentDirectionAngle() {return getCurrentAction()._additionalDirectionAngle;}
    public float getCurrentHeadUpOffset() {return getCurrentAction()._headUpOffset;}
    public DefenceType getCurrentDefenceType() {return getCurrentAction()._defenceType;}
    public RotationType getCurrentRotationType() {return getCurrentAction()._rotationType;}
    public DirectionType getDirectionType() {return getCurrentAction()._directionType;}
    public bool getCurrentDirectionUpdateOnce() {return getCurrentAction()._directionUpdateOnce;}
    public bool getCurrentFlipTypeUpdateOnce() {return getCurrentAction()._flipTypeUpdateOnce;}

    public CommonMaterial getCurrentMaterial() {return getCurrentAction()._characterMaterial;}
    public DefenceDirectionType getDefenceDirectionType() {return getCurrentAction()._defenceDirectionType;}
    public string getCurrentActionName() {return getCurrentAction()._nodeName; }
    public UnityEngine.Sprite getCurrentSprite(float currentAngleDegree) {return _animationPlayer.getCurrentSprite(currentAngleDegree);}
    public UnityEngine.Quaternion getAnimationRotationPerFrame() {return _animationPlayer.getAnimationRotationPerFrame();}
    public UnityEngine.Quaternion getCurrentAnimationRotation() {return _animationPlayer.getCurrentAnimationRotation();}
    public UnityEngine.Vector3 getAnimationScalePerFrame() {return _animationPlayer.getAnimationScalePerFrame();}
    public bool getCurrentAnimationScale(out UnityEngine.Vector3 outScale) {return _animationPlayer.getCurrentAnimationScale(out outScale);}
    public bool getCurrentAnimationTranslation(out UnityEngine.Vector3 outTranslation) { return _animationPlayer.getCurrentAnimationTranslation(out outTranslation); }
    public FlipState getCurrentFlipState() {return _animationPlayer.getCurrentFlipState();}
    public FlipType getCurrentFlipType() {return getCurrentAction()._flipType;}
    public MovementBase.MovementType getCurrentMovement() {return getCurrentAction()._movementType;}

    public float getMoveScaleByIndex(int index) {return _actionGraphBaseData._actionNodeData[index]._moveScale;}
    public MovementGraphPresetData getMovementGraphPresetByIndex(int index) {return _actionGraphBaseData._actionNodeData[index]._movementGraphPresetData;}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return getCurrentAction()._movementGraphPresetData;}
    private ActionGraphNodeData getCurrentAction() {return _actionGraphBaseData._actionNodeData[_currentActionNodeIndex];}
    public ActionGraphNodeData getPrevAction() {return _prevActionNodeIndex == -1 ? null : _actionGraphBaseData._actionNodeData[_prevActionNodeIndex];}

#if UNITY_EDITOR
    public ActionGraphNodeData getCurrentAction_Debug() {return _actionGraphBaseData._actionNodeData[_currentActionNodeIndex];}
    public ActionGraphBaseData getActionGraphBaseData_Debug() {return _actionGraphBaseData;}
#endif
}
