using System.Collections.Generic;

public class AIGraphBaseData
{
    public string                               _name;
    public AIGraphNodeData[]                    _aiGraphNodeData = null;
    public AIPackageBaseData[]                  _aiPackageData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents;

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _aiPackageCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
}

public class AIGraphNodeData
{
    public AIGraphNodeData()
    {
        _packageIndex = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents;

    public int                          _packageIndex;
    public int                          _branchIndexStart;
    public int                          _branchCount;
}






public class AIPackageBaseData
{
    public string                               _name;
    public AIPackageNodeData[]                  _aiPackageNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents;

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
}

public class AIPackageNodeData
{
    public AIPackageNodeData()
    {
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;

    public float                        _updateTime;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents;

    public int                          _branchIndexStart;
    public int                          _branchCount;
}
