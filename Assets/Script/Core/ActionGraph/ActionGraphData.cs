public class ActionGraphBaseData
{
    public string                               _name;
    public ActionGraphNodeData[]                _actionNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;
    public AnimationPlayDataInfo[]              _animationPlayData = null;

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
    public MovementBase.MovementType    _movementType = MovementBase.MovementType.Empty;
    public MovementGraphPresetData      _movementGraphPresetData = null;
    public DirectionType                _directionType = DirectionType.AlwaysRight;
    public RotationType                 _rotationType = RotationType.AlwaysRight;
    public FlipType                     _flipType = FlipType.AlwaysTurnOff;

    public float                        _moveScale = 1f;

    public bool                         _isActionSelection = false;

    public int                          _index;
    public int                          _branchIndexStart;
    public int                          _branchCount;
}

public enum DirectionType
{
    AlwaysRight = 0,
    Keep,
    MoveInput,
    MousePoint,
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

public enum FlipType
{
    AlwaysTurnOff = 0,
    Direction,
    MousePoint,
    Keep,
    Count,
};

public enum RotationType
{
    AlwaysRight = 0,
    Direction,
    MousePoint,
    Keep,
    Count,
}

public enum ConditionNodeUpdateType
{
    Literal = 0,
    ConditionResult,

    Action_Test,
    Action_Dash,
    Action_AnimationEnd,
    Action_AngleBetweenStick,
    Action_AngleDirection,
    Action_IsXFlip,
    Action_IsYFlip,

    Input_AttackCharge,
    Input_Guard,

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
        {"ActionDash",new ConditionNodeInfo(ConditionNodeUpdateType.Action_Dash, ConditionNodeType.Bool)},
        {"End",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AnimationEnd, ConditionNodeType.Bool)},
        {"AngleBetweenStick",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AngleBetweenStick, ConditionNodeType.Float)},
        {"AngleDirection",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AngleDirection, ConditionNodeType.Float)},

        {"IsXFlip",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsXFlip, ConditionNodeType.Bool)},
        {"IsYFlip",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsYFlip, ConditionNodeType.Bool)},

        {"InputAttackCharge",new ConditionNodeInfo(ConditionNodeUpdateType.Input_AttackCharge, ConditionNodeType.Bool)},
        {"InputGuard",new ConditionNodeInfo(ConditionNodeUpdateType.Input_Guard, ConditionNodeType.Bool)},
        
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