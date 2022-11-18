using System.Collections.Generic;

[System.Serializable]
public class AIGraphBaseData
{
    public string                               _name;
    public AIGraphNodeData[]                    _aiGraphNodeData = null;
    public AIPackageBaseData[]                  _aiPackageData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _aiPackageCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
}

[System.Serializable]
public class AIGraphNodeData
{
    public AIGraphNodeData()
    {
        _packageIndex = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();

    public int                          _packageIndex;
    public int                          _branchIndexStart;
    public int                          _branchCount;
}





[System.Serializable]
public class AIPackageBaseData
{
    public string                               _name;
    public AIPackageNodeData[]                  _aiPackageNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<AIPackageEventType, AIChildFrameEventItem> _aiPackageEvents = new Dictionary<AIPackageEventType, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
}

[System.Serializable]
public class AIPackageNodeData
{
    public AIPackageNodeData()
    {
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;

    public float                        _updateTime = 1f;
    public TargetSearchType             _targetSearchType = TargetSearchType.None;
    public SearchIdentifier             _searchIdentifier = SearchIdentifier.Count;
    public float                        _targetSearchRange = 0f;

    public UnityEngine.Vector3          _targetPosition;
    public float                        _arriveThreshold = 0.1f;
    public bool                         _hasTargetPosition = false;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();

    public int                          _branchIndexStart;
    public int                          _branchCount;
}
