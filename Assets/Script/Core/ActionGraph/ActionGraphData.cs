public class ActionGraphBaseData
{
    public string                               _name;
    public ActionGraphNodeData[]                _actionNodeData;
    public ActionGraphBranchData[]              _branchData;
    public ActionGraphConditionCompareData[]    _conditionCompareData;
    public AnimationPlayDataInfo[]              _animationPlayData;

    public int                                  _defaultActionIndex = -1;

    public int                                  _actionNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
    public int                                  _animationPlayDataCount = -1;
}

public class ActionGraphNodeData
{
    public ActionGraphNodeData()
    {
        _animationInfoIndex = -1;
        _movementType = MovementBase.MovementType.Empty;
        _directionType = DirectionType.AlwaysRight;
        
        _index = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;
    public int                          _animationInfoIndex;
    public MovementBase.MovementType    _movementType;
    public DirectionType                _directionType;

    public int                          _index;
    public int                          _branchIndexStart;
    public int                          _branchCount;
}

public enum DirectionType
{
    AlwaysRight = 0,
    Keep,
    MoveInput,
    Count,
}

public class ActionGraphBranchData
{
    public int      _branchActionIndex;

    public int      _conditionCompareDataIndex;
}

public enum ConditionCompareType
{
    Equals = 0,
    Inverse,
    NotEquals,
    Or,
    And,
    Greater,
    Smaller,
    GreaterEqual,
    SmallerEqual,
    Count,
};

public enum ConditionNodeUpdateType
{
    Literal = 0,
    ConditionResult,

    Action_Test,
    Action_AnimationEnd,
    
    Count,
}

public enum ConditionNodeType
{
    Int = 0,
    Float,
    Bool,
    Count,
}

public class ConditionNodeInfo
{
    public ConditionNodeInfo(ConditionNodeUpdateType updateType, ConditionNodeType nodeTpye)
    {
        _updateType = updateType;
        _nodeType = nodeTpye;
    }

    public ConditionNodeUpdateType  _updateType;
    public ConditionNodeType        _nodeType;
}

public static class ConditionNodeInfoPreset
{
    public static System.Collections.Generic.Dictionary<string,ConditionNodeInfo> _nodePreset = new System.Collections.Generic.Dictionary<string, ConditionNodeInfo>
    {
        {"Literal_Bool",new ConditionNodeInfo(ConditionNodeUpdateType.Literal, ConditionNodeType.Bool)},
        {"Literal_Int",new ConditionNodeInfo(ConditionNodeUpdateType.Literal, ConditionNodeType.Int)},
        {"Literal_Float",new ConditionNodeInfo(ConditionNodeUpdateType.Literal, ConditionNodeType.Float)},
        {"RESULT",new ConditionNodeInfo(ConditionNodeUpdateType.ConditionResult, ConditionNodeType.Bool)},
        {"ActionTest",new ConditionNodeInfo(ConditionNodeUpdateType.Action_Test, ConditionNodeType.Bool)},
        {"End",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AnimationEnd, ConditionNodeType.Bool)},
        
    };

    public static int[] _dataSize =
    {
        4, // int
        4, // float
        1, // boolean
    };
}

public class ActionGraphConditionNodeData
{
    public string _symbolName;
}

public class ActionGraphConditionNodeData_Literal : ActionGraphConditionNodeData
{
    private byte[]       _data;

    public ActionGraphConditionNodeData_Literal()
    {
        _data = null;
    }
    
    public byte[] getLiteral()
    {
        DebugUtil.assert(_data != null, "literal data is null");
        return _data;
    }

    public void setLiteral(byte[] data)
    {
        DebugUtil.assert(data != null, "literal data is null to set");
        _data = data;
    }
    
}

public class ActionGraphConditionNodeData_ConditionResult : ActionGraphConditionNodeData
{
    public int          _resultIndex;

    public ActionGraphConditionNodeData_ConditionResult()
    {
        _symbolName = "RESULT_";
        _resultIndex = -1;
    }
    
    public int getResultIndex()
    {
        DebugUtil.assert(_resultIndex != -1, "resultIndex is not valid");
        return _resultIndex;
    }

    public void setResultIndex(int index)
    {
        _resultIndex = index;
    }
}


public class ActionGraphConditionCompareData
{
    public ActionGraphConditionNodeData[]   _conditionNodeDataArray;
    public ConditionCompareType[]           _compareTypeArray;

    public int                              _conditionNodeDataCount;
    public int                              _compareTypeCount;

}