using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;

[System.Serializable]
public class AIGraphBaseData
{
    public string                               _name;
    public AIGraphNodeData[]                    _aiGraphNodeData = null;
    public AIPackageBaseData[]                  _aiPackageData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;
    public AIGraphCustomValue[]                 _customValueData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string, AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _aiPackageCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
    public int                                  _customValueDataCount = -1;

// #if UNITY_EDITOR
    public string _fullPath = "";
// #endif
}

[System.Serializable]
public class AIGraphNodeData
{
    public AIGraphNodeData()
    {
        _packageIndex = -1;
        _packageEntryNodeIndex = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
        _coolDownTime = 0f;
    }

    public string                       _nodeName;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                          _packageIndex;
    public int                          _packageEntryNodeIndex;
    public int                          _branchIndexStart;
    public int                          _branchCount;
    public float                        _coolDownTime;

// #if UNITY_EDITOR
    public int _lineNumber = 0;
// #endif

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_nodeName);

        binaryWriter.Write(_aiEvents == null ? 0 : _aiEvents.Count);
        if(_aiEvents != null)
        {
            foreach(var item in _aiEvents)
            {
                binaryWriter.Write((int)item.Key);
                item.Value.serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_customAIEvents == null ? 0 : _customAIEvents.Count);
        if(_customAIEvents != null)
        {
            foreach(var item in _customAIEvents)
            {
                binaryWriter.Write(item.Key);
                item.Value.serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_packageIndex);
        binaryWriter.Write(_packageEntryNodeIndex);
        binaryWriter.Write(_branchIndexStart);
        binaryWriter.Write(_branchCount);
        binaryWriter.Write(_coolDownTime);
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _nodeName = binaryReader.ReadString();
        int aiEventCount = binaryReader.ReadInt32();
        for(int i = 0; i < aiEventCount; ++i)
        {
            AIChildEventType childEventType = (AIChildEventType)binaryReader.ReadInt32();
            AIChildFrameEventItem childEventItem = new AIChildFrameEventItem();
            childEventItem.deserialize(ref binaryReader);

            _aiEvents.Add(childEventType, childEventItem);
        }

        int customAiEventCount = binaryReader.ReadInt32();
        for(int i = 0; i < customAiEventCount; ++i)
        {
            string key = binaryReader.ReadString();
            AIChildFrameEventItem childEventItem = new AIChildFrameEventItem();
            childEventItem.deserialize(ref binaryReader);

            _customAIEvents.Add(key, childEventItem);
        }

        _packageIndex = binaryReader.ReadInt32();
        _packageEntryNodeIndex = binaryReader.ReadInt32();
        _branchIndexStart = binaryReader.ReadInt32();
        _branchCount = binaryReader.ReadInt32();
        _coolDownTime = binaryReader.ReadSingle();
    }
}





[System.Serializable]
public class AIPackageBaseData
{
    public string                               _name;
    public AIPackageNodeData[]                  _aiPackageNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public Dictionary<AIPackageEventType, AIChildFrameEventItem> _aiPackageEvents = new Dictionary<AIPackageEventType, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;

// #if UNITY_EDITOR
    public string _fullPath = "";
// #endif
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
    public AllyTargetType               _searchAllyTargetType = AllyTargetType.Count;
    public float                        _targetSearchRange = 999f;
    public float                        _targetSearchStartRange = 0f;
    public float                        _targetSearchSphereRadius = 0f;

    public UnityEngine.Vector3          _targetPosition;
    public float                        _arriveThreshold = 0.1f;
    public bool                         _hasTargetPosition = false;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                          _branchIndexStart;
    public int                          _branchCount;

// #if UNITY_EDITOR
    public int _lineNumber = 0;
// #endif
}

[System.Serializable]
public class AIGraphCustomValue
{
    public string _name = "";
    public float _customValue = 0f;
}