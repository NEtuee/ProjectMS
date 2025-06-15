#define INCLUDE_FULLPATH

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEditor;


[System.Serializable]
public class ActionGraphBaseData : SerializableDataType
{
    public string                               _name;
    public ActionGraphNodeData[]                _actionNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;
    public AnimationPlayDataInfo[][]            _animationPlayData = null;
    public ActionGraphTriggerEventData[]        _triggerEventData = null;

    public Dictionary<string, int>             _actionIndexMap = new Dictionary<string, int>();

    public int[]                                _defaultBuffList = null;

    public int                                  _defaultActionIndex = -1;

    public int                                  _actionNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
    public int                                  _animationPlayDataCount = -1;

    public int                                  _dummyActionIndex = -1;

//#if UNITY_EDITOR
    public string _fullPath;
//#endif

    public void buildActionIndexMap()
    {
        for(int i = 0; i < _actionNodeCount; ++i)
        {
            _actionIndexMap.Add(_actionNodeData[i]._nodeName,i);
        }
    }

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        binaryWriter.Write(_actionNodeCount);
        binaryWriter.Write(_defaultBuffList == null ? 0 : _defaultBuffList.Length);
        if(_defaultBuffList != null)
        {
            for(int i = 0; i < _defaultBuffList.Length; ++i)
            {
                binaryWriter.Write(_defaultBuffList[i]);
            }
        }

        binaryWriter.Write(_defaultActionIndex);
        binaryWriter.Write(_branchCount);
        binaryWriter.Write(_conditionCompareDataCount);
        binaryWriter.Write(_animationPlayDataCount);
        binaryWriter.Write(_dummyActionIndex);

        if(_actionNodeCount != 0)
        {
            for(int i = 0; i < _actionNodeCount; ++i)
            {
                _actionNodeData[i].serialize(ref binaryWriter);
            }
        }

        if(_branchCount != 0)
        {
            for(int i = 0; i < _branchCount; ++i)
            {
                _branchData[i].serialize(ref binaryWriter);
            }
        }

        if(_conditionCompareDataCount != 0)
        {
            for(int i = 0; i < _conditionCompareDataCount; ++i)
            {
                _conditionCompareData[i].serialize(ref binaryWriter);
            }
        }

        if(_animationPlayDataCount != 0)
        {
            for(int i = 0; i < _animationPlayDataCount; ++i)
            {
                binaryWriter.Write(_animationPlayData[i].Length);
                for(int j =0; j < _animationPlayData[i].Length; ++j)
                {
                    _animationPlayData[i][j].serialize(ref binaryWriter);
                }
            }
        }

        binaryWriter.Write(_triggerEventData == null ? 0 : _triggerEventData.Length);
        if(_triggerEventData != null)
        {
            for(int i = 0; i < _triggerEventData.Length; ++i)
            {
                _triggerEventData[i].serialize(ref binaryWriter);
            }
        }

#if INCLUDE_FULLPATH
        binaryWriter.Write(_fullPath);
#endif
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _actionNodeCount = binaryReader.ReadInt32();
        int buffListLength = binaryReader.ReadInt32();
        if(buffListLength != 0)
        {
            _defaultBuffList = new int[buffListLength];
        }

        for(int i = 0; i < buffListLength; ++i)
        {
            _defaultBuffList[i] = binaryReader.ReadInt32();
        }

        _defaultActionIndex = binaryReader.ReadInt32();
        _branchCount = binaryReader.ReadInt32();
        _conditionCompareDataCount = binaryReader.ReadInt32();
        _animationPlayDataCount = binaryReader.ReadInt32();
        _dummyActionIndex = binaryReader.ReadInt32();

        if(_actionNodeCount != 0)
        {
            _actionNodeData = new ActionGraphNodeData[_actionNodeCount];
            for(int i = 0; i < _actionNodeCount; ++i)
            {
                _actionNodeData[i] = new ActionGraphNodeData();
                _actionNodeData[i].deserialize(ref binaryReader);
            }
        }

        if(_branchCount != 0)
        {
            _branchData = new ActionGraphBranchData[_branchCount];
            for(int i = 0; i < _branchCount; ++i)
            {
                _branchData[i] = new ActionGraphBranchData();
                _branchData[i].deserialize(ref binaryReader);
            }
        }

        if(_conditionCompareDataCount != 0)
        {
            _conditionCompareData = new ActionGraphConditionCompareData[_conditionCompareDataCount];
            for(int i = 0; i < _conditionCompareDataCount; ++i)
            {
                _conditionCompareData[i] = new ActionGraphConditionCompareData();
                _conditionCompareData[i].deserialize(ref binaryReader);
            }
        }

        if(_animationPlayDataCount != 0)
        {
            _animationPlayData = new AnimationPlayDataInfo[_animationPlayDataCount][];
            for(int i = 0; i < _animationPlayDataCount; ++i)
            {
                int length = binaryReader.ReadInt32();
                _animationPlayData[i] = new AnimationPlayDataInfo[length];

                for(int j =0; j < length; ++j)
                {
                    _animationPlayData[i][j] = new AnimationPlayDataInfo();
                    _animationPlayData[i][j].deserialize(ref binaryReader);
                }
            }
        }

        int triggerEventCount = binaryReader.ReadInt32();
        if(triggerEventCount != 0)
        {
            _triggerEventData = new ActionGraphTriggerEventData[triggerEventCount];
            for(int i = 0; i < triggerEventCount; ++i)
            {
                _triggerEventData[i] = new ActionGraphTriggerEventData();
                _triggerEventData[i].deserialize(ref binaryReader);
            }
        }

#if INCLUDE_FULLPATH
        _fullPath = binaryReader.ReadString();
#endif

        buildActionIndexMap();
    }
}

[System.Serializable]
public class ActionGraphTriggerEventData
{
    public ActionFrameEventBase[]   _frameEventData = null;
    public int                      _conditionCompareDataIndex = -1;

#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_frameEventData == null ? 0 : _frameEventData.Length);
        if(_frameEventData != null)
        {
            for(int i = 0; i < _frameEventData.Length; ++i)
            {
                _frameEventData[i].serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_conditionCompareDataIndex);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        int frameEventCount = binaryReader.ReadInt32();
        if(frameEventCount != 0)
        {
            _frameEventData = new ActionFrameEventBase[frameEventCount];
            for(int i = 0; i < frameEventCount; ++i)
            {
                _frameEventData[i] = ActionFrameEventBase.buildFrameEvent(ref binaryReader);
            }
        }

        _conditionCompareDataIndex = binaryReader.ReadInt32();

    }
}

[System.Serializable]
public class ActionGraphNodeData
{
    public ActionGraphNodeData()
    {
        _animationInfoIndex = -1;
        _animationInfoCount = 0;
        _movementType = MovementBase.MovementType.Empty;
        _directionType = DirectionType.AlwaysRight;
        _defenceDirectionType = DefenceDirectionType.Direction;
        
        _index = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
        _triggerIndexStart = 0;
        _triggerCount = 0;
    }

    public string                       _nodeName;
    public int                          _animationInfoIndex;
    public int                          _animationInfoCount;

    public MovementBase.MovementType    _movementType = MovementBase.MovementType.Empty;
    public MovementGraphPresetData      _movementGraphPresetData = null;
    public DirectionType                _directionType = DirectionType.AlwaysRight;
    public DefenceDirectionType         _defenceDirectionType = DefenceDirectionType.Direction;
    public RotationType                 _rotationType = RotationType.AlwaysRight;
    public FlipType                     _flipType = FlipType.AlwaysTurnOff;
    public DefenceType                  _defenceType = DefenceType.Empty;
    public CommonMaterial               _characterMaterial = CommonMaterial.Count;

    public int[]                        _applyBuffList = null;
    public AttackType                   _ignoreAttackType = AttackType.None;

    public float                        _additionalDirectionAngle = 0f;
    public float                        _defenceAngle = 360f;
    public float                        _moveScale = 1f;
    public float                        _rotateSpeed = -1f;
    public float                        _headUpOffset = -1f;

    public bool                         _normalizedSpeed = false;
    public bool                         _rotateBySpeed = false;

    public bool                         _isActionSelection = false;
    public bool                         _directionUpdateOnce = false;
    public bool                         _flipTypeUpdateOnce = false;

    public bool                         _activeCollision = true;
    public bool                         _isDummyAction = false;

    public bool                         _hasAngleSector = false;
    public int                          _angleSectorCount = -1;

    public ulong                        _actionFlag = 0;

    public int                          _index;
    public int                          _branchIndexStart;
    public int                          _branchCount;

    public int                          _triggerIndexStart;
    public int                          _triggerCount;

    public int _lineNumber;

#if UNITY_EDITOR

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_nodeName);
        binaryWriter.Write(_animationInfoIndex);
        binaryWriter.Write(_animationInfoCount);
        binaryWriter.Write((int)_movementType);
        binaryWriter.Write(_movementGraphPresetData == null ? "NULL" : _movementGraphPresetData.getName());
        binaryWriter.Write((int)_directionType);
        binaryWriter.Write((int)_defenceDirectionType);
        binaryWriter.Write((int)_rotationType);
        binaryWriter.Write((int)_flipType);
        binaryWriter.Write((int)_defenceType);
        binaryWriter.Write((int)_characterMaterial);
        BinaryHelper.writeArray(ref binaryWriter, _applyBuffList);
        binaryWriter.Write((int)_ignoreAttackType);
        binaryWriter.Write(_additionalDirectionAngle);
        binaryWriter.Write(_defenceAngle);
        binaryWriter.Write(_moveScale);
        binaryWriter.Write(_rotateSpeed);
        binaryWriter.Write(_headUpOffset);
        binaryWriter.Write(_normalizedSpeed);
        binaryWriter.Write(_rotateBySpeed);
        binaryWriter.Write(_isActionSelection);
        binaryWriter.Write(_directionUpdateOnce);
        binaryWriter.Write(_flipTypeUpdateOnce);
        binaryWriter.Write(_activeCollision);
        binaryWriter.Write(_isDummyAction);
        binaryWriter.Write(_hasAngleSector);
        binaryWriter.Write(_angleSectorCount);
        binaryWriter.Write(_actionFlag);
        binaryWriter.Write(_index);
        binaryWriter.Write(_branchIndexStart);
        binaryWriter.Write(_branchCount);
        binaryWriter.Write(_triggerIndexStart);
        binaryWriter.Write(_triggerCount);
#if INCLUDE_FULLPATH
        binaryWriter.Write(_lineNumber);
#endif
    }
#endif

    public void deserialize(ref BinaryReader binaryReader)
    {
        _nodeName = binaryReader.ReadString();
        _animationInfoIndex = binaryReader.ReadInt32();
        _animationInfoCount = binaryReader.ReadInt32();
        _movementType = (MovementBase.MovementType)binaryReader.ReadInt32();
        string graphPreset = binaryReader.ReadString();
        if(graphPreset != "NULL")
        {
            MovementGraphPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset/MovementGraphPreset") as MovementGraphPreset;
            _movementGraphPresetData = preset.getPresetData(graphPreset);
        }
        else
        {
            _movementGraphPresetData = null;
        }

        _directionType = (DirectionType)binaryReader.ReadInt32();
        _defenceDirectionType = (DefenceDirectionType)binaryReader.ReadInt32();
        _rotationType = (RotationType)binaryReader.ReadInt32();
        _flipType = (FlipType)binaryReader.ReadInt32();
        _defenceType = (DefenceType)binaryReader.ReadInt32();
        _characterMaterial = (CommonMaterial)binaryReader.ReadInt32();
        _applyBuffList = BinaryHelper.readArrayInt(ref binaryReader);
        _ignoreAttackType = (AttackType)binaryReader.ReadInt32();
        _additionalDirectionAngle = binaryReader.ReadSingle();
        _defenceAngle = binaryReader.ReadSingle();
        _moveScale = binaryReader.ReadSingle();
        _rotateSpeed = binaryReader.ReadSingle();
        _headUpOffset = binaryReader.ReadSingle();
        _normalizedSpeed = binaryReader.ReadBoolean();
        _rotateBySpeed = binaryReader.ReadBoolean();
        _isActionSelection = binaryReader.ReadBoolean();
        _directionUpdateOnce = binaryReader.ReadBoolean();
        _flipTypeUpdateOnce = binaryReader.ReadBoolean();
        _activeCollision = binaryReader.ReadBoolean();
        _isDummyAction = binaryReader.ReadBoolean();
        _hasAngleSector = binaryReader.ReadBoolean();
        _angleSectorCount = binaryReader.ReadInt32();
        _actionFlag = binaryReader.ReadUInt64();
        _index = binaryReader.ReadInt32();
        _branchIndexStart = binaryReader.ReadInt32();
        _branchCount = binaryReader.ReadInt32();
        _triggerIndexStart = binaryReader.ReadInt32();
        _triggerCount = binaryReader.ReadInt32();

#if INCLUDE_FULLPATH
        _lineNumber = binaryReader.ReadInt32();
#endif

    }
}

public enum DirectionType
{
    AlwaysRight = 0,
    AlwaysUp,
    AlwaysLeft,
    AlwaysDown,
    Keep,
    MoveInput,
    MousePoint,
    AttackedPoint,
    MoveDirection,
    AI,
    AITarget,
    CatchTargetFace,
    Summoner,
    ToSummoner,
    Count,
}

public enum DefenceDirectionType
{
    Direction = 0,
    MousePoint,
    Count,
}

[System.Serializable]
public class ActionGraphBranchData
{
    public int      _branchActionIndex;

    public int      _conditionCompareDataIndex = -1;
    public int      _keyConditionCompareDataIndex = -1;
    public int      _weightConditionCompareDataIndex = -1;

    public bool     _isConditionalState = false;
    public int      _conditionFailNextIndex = -1;

    public int _lineNumber;
#if UNITY_EDITOR

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_branchActionIndex);
        binaryWriter.Write(_conditionCompareDataIndex);
        binaryWriter.Write(_keyConditionCompareDataIndex);
        binaryWriter.Write(_weightConditionCompareDataIndex);
        binaryWriter.Write(_isConditionalState);
        binaryWriter.Write(_conditionFailNextIndex);

#if INCLUDE_FULLPATH
        binaryWriter.Write(_lineNumber);
#endif
    }
#endif

    public void deserialize(ref BinaryReader binaryReader)
    {
        _branchActionIndex = binaryReader.ReadInt32();
        _conditionCompareDataIndex = binaryReader.ReadInt32();
        _keyConditionCompareDataIndex = binaryReader.ReadInt32();
        _weightConditionCompareDataIndex = binaryReader.ReadInt32();
        _isConditionalState = binaryReader.ReadBoolean();
        _conditionFailNextIndex = binaryReader.ReadInt32();


#if INCLUDE_FULLPATH
        _lineNumber = binaryReader.ReadInt32();
#endif

    }
}

public enum ActionFlags : ulong
{
    ClearPush = 1,
    LaserEffect = 1 << 1,
    QTE = 1 << 2,
    Hide = 1 << 3,
    IgnorePush = 1 << 4,
    ClearDanmaku = 1 << 5,
    OutlineGuard = 1 << 6,
    OutlineSuperArmor = 1 << 7,
    OutlineNormal = 1 << 8,
};

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

public enum DefenceType
{
    Empty,
    Guard,
    Parry,
    Evade,
    Count,
}

public enum FlipType
{
    AlwaysTurnOff = 0,
    Direction,
    MousePoint,
    MoveDirection,
    Keep,
    Count,
};

public enum RotationType
{
    AlwaysRight = 0,
    Direction,
    MousePoint,
    MoveDirection,
    Keep,
    Torque,
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
    Action_AngleFlipDirectionToStick,
    Action_AngleDirectionToTarget,
    Action_IsXFlip,
    Action_IsYFlip,
    Action_CurrentFrame,
    Action_IsCatcher,
    Action_IsCatchTarget,
    Action_ActionExecutedTime,
    Action_AngleToTarget,

    Input_AttackCharge,
    Input_AttackBlood,
    Input_Guard,
    Input_CanInput,

    Attack_Guarded,
    Attack_Success,
    Attack_Parried,
    Attack_Evaded,
    Attack_GuardBreak,
    Attack_GuardBreakFail,
    Attack_CatchTarget,

    Defence_Success,
    Defence_Crash,
    Defence_Parry,
    Defence_Evade,
    Defence_GuardBroken,
    Defence_GuardBreakFail,
    Defence_Catched,

    Defence_Hit,

    Entity_Dead,
    Entity_Kill,
    Entity_LifeTime,

    AI_TargetDistance,
    AI_TargetExists,
    AI_ArrivedTarget,
    AI_CurrentPackageEnd,
    AI_PackageStateExecutedTime,
    AI_GraphStateExecutedTime,
    AI_GraphCoolTime,

    Status,
    Key,
    FrameTag,
    TargetFrameTag,
    Weight,
    AngleSector,
    AICustomValue,
    AttackTag,

    Count,
}

public enum ConditionNodeType
{
    Int = 0,
    Float,
    Bool,
    Count,
}

[System.Serializable]
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

static public class CommonConditionNodeData
{
    static public byte[] trueByte = new byte[1]{1};
    static public byte[] falseByte = new byte[1]{0};
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
        {"AngleFlipDirectionToStick",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AngleFlipDirectionToStick, ConditionNodeType.Float)},
        {"AngleDirectionToTarget",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AngleDirectionToTarget, ConditionNodeType.Float)},

        {"IsXFlip",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsXFlip, ConditionNodeType.Bool)},
        {"IsYFlip",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsYFlip, ConditionNodeType.Bool)},
        {"CurrentFrame",new ConditionNodeInfo(ConditionNodeUpdateType.Action_CurrentFrame, ConditionNodeType.Float)},
        {"IsCatcher",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsCatcher, ConditionNodeType.Bool)},
        {"IsCatchTarget",new ConditionNodeInfo(ConditionNodeUpdateType.Action_IsCatchTarget, ConditionNodeType.Bool)},
        {"ActionExecutedTime",new ConditionNodeInfo(ConditionNodeUpdateType.Action_ActionExecutedTime, ConditionNodeType.Float)},
        {"AngleToTarget",new ConditionNodeInfo(ConditionNodeUpdateType.Action_AngleToTarget, ConditionNodeType.Float)},

        {"InputAttackCharge",new ConditionNodeInfo(ConditionNodeUpdateType.Input_AttackCharge, ConditionNodeType.Bool)},
        {"InputAttackBlood",new ConditionNodeInfo(ConditionNodeUpdateType.Input_AttackBlood, ConditionNodeType.Bool)},
        {"InputGuard",new ConditionNodeInfo(ConditionNodeUpdateType.Input_Guard, ConditionNodeType.Bool)},
        {"CanInput",new ConditionNodeInfo(ConditionNodeUpdateType.Input_CanInput, ConditionNodeType.Bool)},

        {"AttackGuarded",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_Guarded, ConditionNodeType.Bool)},
        {"AttackSuccess",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_Success, ConditionNodeType.Bool)},
        {"AttackParried",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_Parried, ConditionNodeType.Bool)},
        {"AttackEvaded",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_Evaded, ConditionNodeType.Bool)},
        {"AttackGuardBreak",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_GuardBreak, ConditionNodeType.Bool)},
        {"AttackGuardBreakFail",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_GuardBreakFail, ConditionNodeType.Bool)},
        {"AttackCatchTarget",new ConditionNodeInfo(ConditionNodeUpdateType.Attack_CatchTarget, ConditionNodeType.Bool)},

        {"DefenceSuccess",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Success, ConditionNodeType.Bool)},
        {"DefenceCrash",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Crash, ConditionNodeType.Bool)},
        {"ParrySuccess",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Parry, ConditionNodeType.Bool)},
        {"Hit",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Hit, ConditionNodeType.Bool)},
        {"EvadeSuccess",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Evade, ConditionNodeType.Bool)},
        {"GuardBroken",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_GuardBroken, ConditionNodeType.Bool)},
        {"GuardBreakFail",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_GuardBreakFail, ConditionNodeType.Bool)},
        {"Catched",new ConditionNodeInfo(ConditionNodeUpdateType.Defence_Catched, ConditionNodeType.Bool)},

        {"Dead",new ConditionNodeInfo(ConditionNodeUpdateType.Entity_Dead, ConditionNodeType.Bool)},
        {"Kill",new ConditionNodeInfo(ConditionNodeUpdateType.Entity_Kill, ConditionNodeType.Bool)},
        {"LifeTime",new ConditionNodeInfo(ConditionNodeUpdateType.Entity_LifeTime, ConditionNodeType.Float)},

        {"TargetExists",new ConditionNodeInfo(ConditionNodeUpdateType.AI_TargetExists, ConditionNodeType.Bool)},
        {"TargetDistance",new ConditionNodeInfo(ConditionNodeUpdateType.AI_TargetDistance, ConditionNodeType.Float)},
        {"ArrivedTarget",new ConditionNodeInfo(ConditionNodeUpdateType.AI_ArrivedTarget, ConditionNodeType.Bool)},
        {"CurrentPackageEnd",new ConditionNodeInfo(ConditionNodeUpdateType.AI_CurrentPackageEnd, ConditionNodeType.Bool)},
        {"PackageExecutedTime",new ConditionNodeInfo(ConditionNodeUpdateType.AI_PackageStateExecutedTime, ConditionNodeType.Float)},
        {"GraphExecutedTime",new ConditionNodeInfo(ConditionNodeUpdateType.AI_GraphStateExecutedTime, ConditionNodeType.Float)},
        {"AIGraphCoolTime",new ConditionNodeInfo(ConditionNodeUpdateType.AI_GraphCoolTime, ConditionNodeType.Bool)},

        {"Status",new ConditionNodeInfo(ConditionNodeUpdateType.Status, ConditionNodeType.Float)},
        {"Key",new ConditionNodeInfo(ConditionNodeUpdateType.Key, ConditionNodeType.Bool)},
        {"FrameTag",new ConditionNodeInfo(ConditionNodeUpdateType.FrameTag, ConditionNodeType.Bool)},
        {"TargetFrameTag",new ConditionNodeInfo(ConditionNodeUpdateType.TargetFrameTag, ConditionNodeType.Bool)},
        {"Weight",new ConditionNodeInfo(ConditionNodeUpdateType.Weight, ConditionNodeType.Bool)},
        {"AngleSector",new ConditionNodeInfo(ConditionNodeUpdateType.AngleSector, ConditionNodeType.Int)},
        {"CustomValue",new ConditionNodeInfo(ConditionNodeUpdateType.AICustomValue, ConditionNodeType.Float)},
        {"AttackTag",new ConditionNodeInfo(ConditionNodeUpdateType.AttackTag, ConditionNodeType.Bool)},
        
    };

    public static int[] _dataSize =
    {
        4, // int
        4, // float
        1, // boolean
    };
}

public enum ConditionNodeDataType
{
    Normal,
    Literal,
    ConditionResult,
    AICustomValue,
    FrameTag,
    Weight,
    Key,
    Status,
    AIGraphCoolTime,
    AttackTag,
}

[System.Serializable]
public class ActionGraphConditionNodeData
{
    public string _symbolName;

    public virtual ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.Normal;

#if UNITY_EDITOR
    public virtual void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_symbolName);
    }
#endif

    public virtual void deserialize(ref BinaryReader binaryReader)
    {
        _symbolName = binaryReader.ReadString();
    }
}


[System.Serializable]
public class ActionGraphConditionNodeData_AttackTag : ActionGraphConditionNodeData
{
    public int _attackTagHash = 0;

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.AttackTag;

    public ActionGraphConditionNodeData_AttackTag()
    {
        
    }
    
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_attackTagHash);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _attackTagHash = binaryReader.ReadInt32();
    }
    
}

[System.Serializable]
public class ActionGraphConditionNodeData_AICustomValue : ActionGraphConditionNodeData_Literal
{
    public string _customValueName = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.AICustomValue;

    public ActionGraphConditionNodeData_AICustomValue()
    {
        setLiteral(System.BitConverter.GetBytes(0f));
    }
    
    public string getCustomValueName() 
    {
        return _customValueName;
    }

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customValueName);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _customValueName = binaryReader.ReadString();
    }
}

[System.Serializable]
public class ActionGraphConditionNodeData_FrameTag : ActionGraphConditionNodeData
{
    public string _targetFrameTag = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.FrameTag;

    public ActionGraphConditionNodeData_FrameTag()
    {
        
    }
    
    public string getFrameTag() {return _targetFrameTag;}

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetFrameTag);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _targetFrameTag = binaryReader.ReadString();
    }
    
}
[System.Serializable]
public class ActionGraphConditionNodeData_Weight : ActionGraphConditionNodeData
{
    public string _weightGroupKey = "";
    public string _weightName = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.Weight;
    public ActionGraphConditionNodeData_Weight()
    {
        
    }

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_weightGroupKey);
        binaryWriter.Write(_weightName);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _weightGroupKey = binaryReader.ReadString();
        _weightName = binaryReader.ReadString();
    }
}
[System.Serializable]
public class ActionGraphConditionNodeData_Key : ActionGraphConditionNodeData
{
    public string _targetKeyName = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.Key;
    public ActionGraphConditionNodeData_Key()
    {
        
    }
    
    public string getKeyName() {return _targetKeyName;}

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetKeyName);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _targetKeyName = binaryReader.ReadString();
    }
}
[System.Serializable]
public class ActionGraphConditionNodeData_Status : ActionGraphConditionNodeData
{
    public string _targetStatus = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.Status;
    public ActionGraphConditionNodeData_Status()
    {
        
    }
    
    public string getStatus() {return _targetStatus;}

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetStatus);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _targetStatus = binaryReader.ReadString();
    }
}
[System.Serializable]
public class ActionGraphConditionNodeData_AIGraphCoolTime : ActionGraphConditionNodeData
{
    public string _graphNodeName = "";

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.AIGraphCoolTime;
    public ActionGraphConditionNodeData_AIGraphCoolTime()
    {
    }
    
    public string getNodeName() {return _graphNodeName;}

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_graphNodeName);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _graphNodeName = binaryReader.ReadString();
    }
    
}
[System.Serializable]
public class ActionGraphConditionNodeData_Literal : ActionGraphConditionNodeData
{
    private byte[]       _data;

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.Literal;
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

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_data.Length);
        binaryWriter.Write(_data);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        int length = binaryReader.ReadInt32();
        _data = binaryReader.ReadBytes(length);
    }
    
}
[System.Serializable]
public class ActionGraphConditionNodeData_ConditionResult : ActionGraphConditionNodeData
{
    public int          _resultIndex;

    public override ConditionNodeDataType getConditionNodeDataType => ConditionNodeDataType.ConditionResult;
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

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_resultIndex);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _resultIndex = binaryReader.ReadInt32();
    }
}

[System.Serializable]
public class ActionGraphConditionCompareData
{
    public ActionGraphConditionNodeData[]   _conditionNodeDataArray;
    public ConditionCompareType[]           _compareTypeArray;

    public int                              _conditionNodeDataCount;
    public int                              _compareTypeCount;

#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_conditionNodeDataCount);
        for(int i = 0; i < _conditionNodeDataCount; ++i)
        {
            binaryWriter.Write((int)_conditionNodeDataArray[i].getConditionNodeDataType);
            _conditionNodeDataArray[i].serialize(ref binaryWriter);
        }

        binaryWriter.Write(_compareTypeCount);
        for(int i = 0; i < _compareTypeCount; ++i)
        {
            binaryWriter.Write((int)_compareTypeArray[i]);
        }
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _conditionNodeDataCount = binaryReader.ReadInt32();
        _conditionNodeDataArray = _conditionNodeDataCount == 0 ? null : new ActionGraphConditionNodeData[_conditionNodeDataCount];
        for(int i = 0; i < _conditionNodeDataCount; ++i)
        {
            ConditionNodeDataType dataType = (ConditionNodeDataType)binaryReader.ReadInt32();
            switch(dataType)
            {
                case ConditionNodeDataType.Normal:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData();
                break;
                case ConditionNodeDataType.AICustomValue:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_AICustomValue();
                break;
                case ConditionNodeDataType.AIGraphCoolTime:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_AIGraphCoolTime();
                break;
                case ConditionNodeDataType.ConditionResult:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_ConditionResult();
                break;
                case ConditionNodeDataType.FrameTag:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_FrameTag();
                break;
                case ConditionNodeDataType.Key:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_Key();
                break;
                case ConditionNodeDataType.Literal:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_Literal();
                break;
                case ConditionNodeDataType.Status:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_Status();
                break;
                case ConditionNodeDataType.Weight:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_Weight();
                break;
                case ConditionNodeDataType.AttackTag:
                    _conditionNodeDataArray[i] = new ActionGraphConditionNodeData_AttackTag();
                break;
                default:
                {
                    DebugUtil.assert(false, "fatal error!!");
                    return;
                }
            }

            _conditionNodeDataArray[i].deserialize(ref binaryReader);
        }

        _compareTypeCount = binaryReader.ReadInt32();
        _compareTypeArray = _compareTypeCount <= 0 ? null : new ConditionCompareType[_compareTypeCount];
        for(int i = 0; i < _compareTypeCount; ++i)
        {
            _compareTypeArray[i] = (ConditionCompareType)binaryReader.ReadInt32();
        }
    }

}