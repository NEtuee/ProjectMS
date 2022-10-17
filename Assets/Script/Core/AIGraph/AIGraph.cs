using System.Collections.Generic;
using UnityEngine;

public class AIGraph
{
    private int _currentAINodeIndex = -1;
    private int _prevAINodeIndex = -1;


    private int _currentPackageStateIndex = -1;
    private int _prevPackageStateIndex = -1;

    private float _updateTimer = 0f;

    private Vector3 _recentlyAiDirection = Vector3.zero;

    private ActionGraph _actionGraph;

    private AIGraphBaseData _aiGraphBaseData;
    private List<byte[]> _conditionResultList = new List<byte[]>();
    private List<AIChildEventType> _reservedEvents = new List<AIChildEventType>();

    public AIGraph(){}
    public AIGraph(ActionGraph actionGraph, AIGraphBaseData baseData){_actionGraph = actionGraph; _aiGraphBaseData = baseData;}

    public bool isValid()
    {
        return _aiGraphBaseData != null && _actionGraph != null && _aiGraphBaseData._aiPackageCount != 0;
    }

    public void assign()
    {
    }

    public void initialize(GameEntityBase targetEntity)
    {
        if(_aiGraphBaseData._defaultAIIndex != -1)
            changeAINode(_aiGraphBaseData._defaultAIIndex, targetEntity);
    }

    public bool progress(float deltaTime, GameEntityBase targetEntity)
    {
        if(isValid() == false)
            return false;

        for(int i =0; i < _reservedEvents.Count; ++i)
        {
            processAIEvent(_reservedEvents[i],targetEntity);
        }

        _reservedEvents.Clear();
        
        processAINode(getCurrentAINode(), targetEntity);
        processAIPackage(deltaTime, getCurrentAIPackageNode(), targetEntity);
        return true;
    }

    public void updateConditionData()
    {
        
    }

    public void release()
    {

    }

    public void executeAIEvent(AIChildEventType eventType)
    {
        _reservedEvents.Add(eventType);
    }

    private bool processAINode(AIGraphNodeData aiGraphNode, GameEntityBase targetEntity)
    {
        bool nodeChanged = false;
        int startIndex = aiGraphNode._branchIndexStart;
        for(int i = startIndex; i < startIndex + aiGraphNode._branchCount; ++i)
        {
            if(processActionBranch(_aiGraphBaseData._branchData[i],_aiGraphBaseData._conditionCompareData) == true)
            {
                nodeChanged = changeAINode(_aiGraphBaseData._branchData[i]._branchActionIndex, targetEntity);
                break;
            }
        }

        if(nodeChanged == true)
        {
            processAIEvent(AIChildEventType.AIChildEvent_OnExit, targetEntity, ref getPrevAINode()._aiEvents);
            processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, ref getCurrentAINode()._aiEvents);
        }

        return nodeChanged;
    }

    private bool processAIPackage(float deltaTime, AIPackageNodeData aiPackageNode, GameEntityBase targetEntity)
    {
        AIPackageNodeData currentPackageNode = getCurrentAIPackageNode();
        _updateTimer -= deltaTime;

        processAIEvent(AIChildEventType.AIChildEvent_OnFrame, targetEntity, ref currentPackageNode._aiEvents);


        if(_updateTimer <= 0f)
            _updateTimer = currentPackageNode._updateTime;
        else
            return false;

        processAIEvent(AIChildEventType.AIChildEvent_OnUpdate, targetEntity, ref currentPackageNode._aiEvents);

        bool stateChanged = false;
        int startIndex = aiPackageNode._branchIndexStart;
        for(int i = startIndex; i < startIndex + aiPackageNode._branchCount; ++i)
        {
            if(processActionBranch(getCurrentAIPackage()._branchData[i],getCurrentAIPackage()._conditionCompareData) == true)
            {
                stateChanged = changeAIPackageState(getCurrentAIPackage()._branchData[i]._branchActionIndex);
                break;
            }
        }

        if(stateChanged == true)
        {
            processAIEvent(AIChildEventType.AIChildEvent_OnExit, targetEntity, ref getPrevAIPackageNode()._aiEvents);
            processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, ref getCurrentAIPackageNode()._aiEvents);

            _updateTimer = getCurrentAIPackageNode()._updateTime;
        }

        return stateChanged;
    }

    private bool changeAINode(int aiPackageIndex, GameEntityBase targetEntity)
    {
        if(aiPackageIndex < 0 || aiPackageIndex >= _aiGraphBaseData._aiNodeCount)
        {
            DebugUtil.assert(false,"invalid ai package index");
            return false;
        }

        int prevIndex = _prevAINodeIndex;
        int currIndex = _currentAINodeIndex;

        _prevAINodeIndex = _currentAINodeIndex;
        _currentAINodeIndex = aiPackageIndex;

        changeAIPackageState(getCurrentAIPackage()._defaultAIIndex);
        _prevPackageStateIndex = -1;

        processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, ref getCurrentAIPackageNode()._aiEvents);
        _updateTimer = getCurrentAIPackageNode()._updateTime;

        return true;
    }


    private bool changeAIPackageState(int aiPackageStateIndex)
    {
        if(aiPackageStateIndex < 0 || aiPackageStateIndex >= getCurrentAIPackage()._aiNodeCount)
        {
            DebugUtil.assert(false,"invalid ai package index");
            return false;
        }

        int prevIndex = _prevPackageStateIndex;
        int currIndex = _currentPackageStateIndex;

        _prevPackageStateIndex = _currentPackageStateIndex;
        _currentPackageStateIndex = aiPackageStateIndex;

        return true;
    }

    private void processAIEvent(AIChildEventType aiEventType, GameEntityBase targetEntity)
    {
        if(processAIEvent(aiEventType,targetEntity, ref _aiGraphBaseData._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, ref getCurrentAINode()._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, ref getCurrentAIPackage()._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, ref getCurrentAIPackageNode()._aiEvents))
            return;

    }

    private bool processAIEvent(AIChildEventType aiEventType, GameEntityBase targetEntity, ref Dictionary<AIChildEventType,AIChildFrameEventItem> aiEventDic)
    {
        if(aiEventDic.ContainsKey(aiEventType) == true)
        {
            AIChildFrameEventItem item = aiEventDic[aiEventType];
            for(int i = 0; i < item._childFrameEventCount; ++i)
            {
                item._childFrameEvents[i].onExecute(targetEntity);
            }

            if(item._consume)
                return true;
        }

        return false;
    }

    public bool processActionBranch(ActionGraphBranchData branchData, ActionGraphConditionCompareData[] compareDatas)
    {
        bool keyCondition = true;
        if(branchData._keyConditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData keyCompareData = compareDatas[branchData._keyConditionCompareDataIndex];
            keyCondition = processActionCondition(keyCompareData);
        }

        bool condition = true;
        if(branchData._conditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData compareData = compareDatas[branchData._conditionCompareDataIndex];
            condition = processActionCondition(compareData);
        }

        return keyCondition && condition;

    }

    public bool processActionCondition(ActionGraphConditionCompareData compareData)
    {
        if(compareData._conditionNodeDataCount == 0)
            return true;

        if(compareData._conditionNodeDataCount == 1)
        {
            DebugUtil.assert(_actionGraph.isNodeType(compareData._conditionNodeDataArray[0],ConditionNodeType.Bool) == true, "invalid node data type!!! : {0}",compareData._conditionNodeDataArray[0]._symbolName);

            return _actionGraph.getDataFromConditionNode(compareData._conditionNodeDataArray[0])[0] == 1;
        }

        for(int i = 0; i < compareData._compareTypeCount; ++i)
        {
            ActionGraphConditionNodeData lvalue = compareData._conditionNodeDataArray[i * 2];
            ActionGraphConditionNodeData rvalue = compareData._conditionNodeDataArray[i * 2 + 1];

            addResultData(_actionGraph.compareTwoCondition(lvalue,rvalue,compareData._compareTypeArray[i]), i);
        }

        return _conditionResultList[compareData._compareTypeCount - 1][0] == 1;
    }

    public void addResultData(bool value, int index)
    {
        if(_conditionResultList.Count <= index)
            _conditionResultList.Add(System.BitConverter.GetBytes(value));
        else
            _conditionResultList[index][0] = value == true ? (byte)1 : (byte)0;
    }

    public Vector3 getRecentlyAIDirection() {return _recentlyAiDirection;}
    public void setAIDirection(Vector3 direction){_recentlyAiDirection = direction;}
    public void setAIDirection(float angle)
    {
        angle = angle * Mathf.Deg2Rad;
        _recentlyAiDirection.x = Mathf.Cos(angle);
        _recentlyAiDirection.y = Mathf.Sin(angle);
        _recentlyAiDirection.z = 0f;
    }

    public TargetSearchType getCurrentTargetSearchType() {return getCurrentAIPackageNode()._targetSearchType;}
    public SearchIdentifier getCurrentSearchIdentifier() {return getCurrentAIPackageNode()._searchIdentifier;}

    public float getCurrentTargetSearchRange() {return getCurrentAIPackageNode()._targetSearchRange;}

    private AIPackageNodeData getCurrentAIPackageNode() {return getCurrentAIPackage()._aiPackageNodeData[_currentPackageStateIndex];}
    private AIPackageNodeData getPrevAIPackageNode() {return getCurrentAIPackage()._aiPackageNodeData[_prevPackageStateIndex];}
    private AIPackageBaseData getCurrentAIPackage() {return _aiGraphBaseData._aiPackageData[getCurrentAINode()._packageIndex];}
    private AIPackageBaseData getPrevAIPackage() {return _aiGraphBaseData._aiPackageData[getPrevAINode()._packageIndex];}
    private AIGraphNodeData getCurrentAINode() {return _aiGraphBaseData._aiGraphNodeData[_currentAINodeIndex];}
    private AIGraphNodeData getPrevAINode() {return _aiGraphBaseData._aiGraphNodeData[_prevAINodeIndex];}
}
