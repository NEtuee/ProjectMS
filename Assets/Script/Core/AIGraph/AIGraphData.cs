using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        binaryWriter.Write(_defaultAIIndex);
        binaryWriter.Write(_aiNodeCount);
        binaryWriter.Write(_aiPackageCount);
        binaryWriter.Write(_branchCount);
        binaryWriter.Write(_conditionCompareDataCount);
        binaryWriter.Write(_customValueDataCount);

        for(int i = 0; i < _aiNodeCount; ++i)
        {
            _aiGraphNodeData[i].serialize(ref binaryWriter);
        }

        for(int i = 0; i < _aiPackageCount; ++i)
        {
            _aiPackageData[i].serialize(ref binaryWriter);
        }

        for(int i = 0; i < _branchCount; ++i)
        {
            _branchData[i].serialize(ref binaryWriter);
        }

        for(int i = 0; i < _conditionCompareDataCount; ++i)
        {
            _conditionCompareData[i].serialize(ref binaryWriter);
        }

        for(int i = 0; i < _customValueDataCount; ++i)
        {
            _customValueData[i].serialize(ref binaryWriter);
        }

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
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _defaultAIIndex = binaryReader.ReadInt32();
        _aiNodeCount = binaryReader.ReadInt32();
        _aiPackageCount = binaryReader.ReadInt32();
        _branchCount = binaryReader.ReadInt32();
        _conditionCompareDataCount = binaryReader.ReadInt32();
        _customValueDataCount = binaryReader.ReadInt32();

        if(_aiNodeCount != 0)
        {
            _aiGraphNodeData = new AIGraphNodeData[_aiNodeCount];
            for(int i = 0; i < _aiNodeCount; ++i)
            {
                _aiGraphNodeData[i] = new AIGraphNodeData();
                _aiGraphNodeData[i].deserialize(ref binaryReader);
            }
        }

        if(_aiPackageCount != 0)
        {
            _aiPackageData = new AIPackageBaseData[_aiPackageCount];
            for(int i = 0; i < _aiPackageCount; ++i)
            {
                _aiPackageData[i] = new AIPackageBaseData();
                _aiPackageData[i].deserialize(ref binaryReader);
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

        if(_customValueDataCount != 0)
        {
            _customValueData = new AIGraphCustomValue[_customValueDataCount];
            for(int i = 0; i < _customValueDataCount; ++i)
            {
                _customValueData[i] = new AIGraphCustomValue();
                _customValueData[i].deserialize(ref binaryReader);
            }
        }

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
    }
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

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        binaryWriter.Write(_aiNodeCount);
        if(_aiPackageNodeData != null)
        {
            for(int i = 0; i < _aiNodeCount; ++i)
            {
                _aiPackageNodeData[i].serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_branchCount);
        if(_branchData != null)
        {
            for(int i = 0; i < _branchCount; ++i)
            {
                _branchData[i].serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_conditionCompareDataCount);
        if(_conditionCompareData != null)
        {
            for(int i = 0; i < _conditionCompareDataCount; ++i)
            {
                _conditionCompareData[i].serialize(ref binaryWriter);
            }
        }

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

        binaryWriter.Write(_aiPackageEvents == null ? 0 : _aiPackageEvents.Count);
        if(_aiPackageEvents != null)
        {
            foreach(var item in _aiPackageEvents)
            {
                binaryWriter.Write((int)item.Key);
                item.Value.serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_defaultAIIndex);
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _aiNodeCount = binaryReader.ReadInt32();
        if(_aiNodeCount != 0)
        {
            _aiPackageNodeData = new AIPackageNodeData[_aiNodeCount];
            for(int i = 0; i < _aiNodeCount; ++i)
            {
                _aiPackageNodeData[i] = new AIPackageNodeData();
                _aiPackageNodeData[i].deserialize(ref binaryReader);
            }
        }

        _branchCount = binaryReader.ReadInt32();
        if(_branchCount != 0)
        {
            _branchData = new ActionGraphBranchData[_branchCount];
            for(int i = 0; i < _branchCount; ++i)
            {
                _branchData[i] = new ActionGraphBranchData();
                _branchData[i].deserialize(ref binaryReader);
            }
        }

        _conditionCompareDataCount = binaryReader.ReadInt32();
        if(_conditionCompareDataCount != 0)
        {
            _conditionCompareData = new ActionGraphConditionCompareData[_conditionCompareDataCount];
            for(int i = 0; i < _conditionCompareDataCount; ++i)
            {
                _conditionCompareData[i] = new ActionGraphConditionCompareData();
                _conditionCompareData[i].deserialize(ref binaryReader);
            }
        }

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

        int aiPackageEventCount = binaryReader.ReadInt32();
        if(aiPackageEventCount != 0)
        {
            foreach(var item in _aiPackageEvents)
            {
                AIPackageEventType packageEventType = (AIPackageEventType)binaryReader.ReadInt32();
                AIChildFrameEventItem childEventItem = new AIChildFrameEventItem();
                childEventItem.deserialize(ref binaryReader);

                _aiPackageEvents.Add(packageEventType, childEventItem);
            }
        }

        _defaultAIIndex = binaryReader.ReadInt32();
    }
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

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_nodeName);
        binaryWriter.Write(_updateTime);
        binaryWriter.Write((int)_targetSearchType);
        binaryWriter.Write((int)_searchAllyTargetType);
        binaryWriter.Write(_targetSearchRange);
        binaryWriter.Write(_targetSearchStartRange);
        binaryWriter.Write(_targetSearchSphereRadius);
        BinaryHelper.writeVector3(ref binaryWriter, _targetPosition);
        binaryWriter.Write(_arriveThreshold);
        binaryWriter.Write(_hasTargetPosition);

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

        binaryWriter.Write(_branchIndexStart);
        binaryWriter.Write(_branchCount);
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _nodeName = binaryReader.ReadString();
        _updateTime = binaryReader.ReadSingle();
        _targetSearchType = (TargetSearchType)binaryReader.ReadInt32();
        _searchAllyTargetType = (AllyTargetType)binaryReader.ReadInt32();
        _targetSearchRange = binaryReader.ReadSingle();
        _targetSearchStartRange = binaryReader.ReadSingle();
        _targetSearchSphereRadius = binaryReader.ReadSingle();
        _targetPosition = BinaryHelper.readVector3(ref binaryReader);
        _arriveThreshold = binaryReader.ReadSingle();
        _hasTargetPosition = binaryReader.ReadBoolean();

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

        _branchIndexStart = binaryReader.ReadInt32();
        _branchCount = binaryReader.ReadInt32();
    }
}

[System.Serializable]
public class AIGraphCustomValue
{
    public string _name = "";
    public float _customValue = 0f;

    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        binaryWriter.Write(_customValue);
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _customValue = binaryReader.ReadSingle();
    }
}