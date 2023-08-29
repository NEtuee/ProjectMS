using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequencerGraphProcessor
{
    private struct eventIndexItem
    {
        public SequencerGraphBaseData _targetSequencerGraph;
        public int _savedIndex;
    }

    private SequencerGraphBaseData              _currentSequencer = null;
    public Dictionary<string, GameEntityBase>   _uniqueEntityDictionary = new Dictionary<string, GameEntityBase>();
    public Dictionary<string, List<GameEntityBase>> _uniqueGroupEntityDictionary = new Dictionary<string, List<GameEntityBase>>();

    private int                                 _currentIndex = 0;
    private bool                                _isSequencerEventEnd = false;

    private eventIndexItem                      _savedEventItem = new eventIndexItem{_savedIndex = 0, _targetSequencerGraph = null};

    private List<string>                        _deleteUniqueTargetList = new List<string>();

    private List<string>                        _signalList = new List<string>();

    public void initialize()
    {
        _currentIndex = 0;
        _isSequencerEventEnd = false;

        _uniqueEntityDictionary.Clear();
        _uniqueGroupEntityDictionary.Clear();
        _deleteUniqueTargetList.Clear();
        _signalList.Clear();
    }

    public void progress(float deltaTime)
    {
        foreach(var item in _uniqueEntityDictionary)
        {
            if(item.Value.isDead())
                _deleteUniqueTargetList.Add(item.Key);
        }

        foreach(var item in _deleteUniqueTargetList)
            _uniqueEntityDictionary.Remove(item);
        
        foreach(var item in _uniqueGroupEntityDictionary.Values)
        {
            for(int index = 0; index < item.Count;)
            {
                if(item[index].isDead())
                    item.RemoveAt(index);
                else
                    ++index;
            }
        }

        _deleteUniqueTargetList.Clear();

        if(_currentSequencer == null || _isSequencerEventEnd)
            return;

        _isSequencerEventEnd = true;
        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventCount;)
        {
            SequencerGraphEventType eventType = _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].getSequencerGraphEventType();
            if(eventType == SequencerGraphEventType.ForceQuit)
            {
                _isSequencerEventEnd = true;
                break;
            }

            _currentIndex = index;
            if(_currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Execute(this, deltaTime) == false)
            {
                _isSequencerEventEnd = false;
                break;
            }

            _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Exit(this);

            if(eventType == SequencerGraphEventType.SaveEventExecuteIndex)
            {
                _savedEventItem._savedIndex = index + 1;
                _savedEventItem._targetSequencerGraph = _currentSequencer;
            }

            ++index;
        }
        
        if(_isSequencerEventEnd)
            stopSequencer();
        
        _signalList.Clear();
    }

    public void addSignal(string signal)
    {
        _signalList.Add(signal);
    }

    public bool checkSignal(string signal)
    {
        return _signalList.Contains(signal);
    }

    public void addUniqueEntity(string uniqueKey, GameEntityBase uniqueEntity)
    {
        if(uniqueEntity == null)
            return;

        if(_uniqueEntityDictionary.ContainsKey(uniqueKey))
        {
            DebugUtil.assert(false,"unique entity key is already use : {0}",uniqueKey);
            return;
        }
        _uniqueEntityDictionary.Add(uniqueKey,uniqueEntity);
    }

    public GameEntityBase getUniqueEntity(string uniqueKey)
    {
        if(_uniqueEntityDictionary.ContainsKey(uniqueKey) == false)
            return null;

        return _uniqueEntityDictionary[uniqueKey];
    }

    public void addUniqueGroupEntity(string uniqueGroupKey, GameEntityBase uniqueEntity)
    {
        if(uniqueEntity == null)
            return;

        if(_uniqueGroupEntityDictionary.ContainsKey(uniqueGroupKey) == false)
            _uniqueGroupEntityDictionary.Add(uniqueGroupKey, new List<GameEntityBase>());

        _uniqueGroupEntityDictionary[uniqueGroupKey].Add(uniqueEntity);
    }

    public List<GameEntityBase> getUniqueGroup(string uniqueGroup)
    {
        if(_uniqueGroupEntityDictionary.ContainsKey(uniqueGroup) == false)
        {
            DebugUtil.assert(false,"존재하지 않는 유니크 그룹 입니다. 오타는 아닌가요? [Group: {0}]",uniqueGroup);
            return null;
        }

        return _uniqueGroupEntityDictionary[uniqueGroup];
    }

    public bool isValid()
    {
        return _currentSequencer != null;
    }

    public void stop()
    {
        clearSequencerGraphProcessor();
    }

    public void stopSequencer()
    {
        for(int index = 0; index < _currentSequencer._sequencerGraphPhase[2]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[2]._sequencerGraphEventList[index].Initialize(this);
            _currentSequencer._sequencerGraphPhase[2]._sequencerGraphEventList[index].Execute(this, 0f);
        }
    }

    public bool isSequencerEnd()
    {
        return _isSequencerEventEnd;
    }

    public void clearSequencerGraphProcessor()
    {
        initialize();
        _currentSequencer = null;
        _savedEventItem._savedIndex = -1;
        _savedEventItem._targetSequencerGraph = null;
    }

    public void startSequencerFromStage(string sequencerGraphPath, StagePointData currentPoint, List<CharacterEntityBase> pointCharacters, GameEntityBase ownerEntity, GameEntityBase targetEntity, bool includePlayer = false)
    {
        _currentSequencer = ResourceContainerEx.Instance().GetSequencerGraph(sequencerGraphPath);
        if(_currentSequencer == null)
            return;

        initialize();

        if(currentPoint._characterSpawnData.Length != pointCharacters.Count)
        {
            DebugUtil.assert(false, "발생시 통보 요망");
            return;
        }

        for(int index = 0; index < pointCharacters.Count; ++index)
        {
            if(currentPoint._characterSpawnData[index]._uniqueKey != "")
                addUniqueEntity(currentPoint._characterSpawnData[index]._uniqueKey, pointCharacters[index]);
            
            if(currentPoint._characterSpawnData[index]._uniqueGroupKey != "")
                addUniqueGroupEntity(currentPoint._characterSpawnData[index]._uniqueGroupKey, pointCharacters[index]);

        }

        if(ownerEntity != null)
            addUniqueEntity("Owner",ownerEntity);
        if(targetEntity != null)
            addUniqueEntity("Target",targetEntity);
        if(includePlayer)
            addUniqueEntity("Player",StageProcessor.Instance().getPlayerEntity());

        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Initialize(this);
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Execute(this, 0f);
        }

        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Initialize(this);
        }

        if(_currentSequencer == _savedEventItem._targetSequencerGraph)
        {
            _currentIndex = _savedEventItem._savedIndex;
        }
    }


    public void startSequencer(string sequencerGraphPath, GameEntityBase ownerEntity, GameEntityBase targetEntity, bool includePlayer = false)
    {
        _currentSequencer = ResourceContainerEx.Instance().GetSequencerGraph(sequencerGraphPath);
        if(_currentSequencer == null)
            return;

        initialize();

        if(ownerEntity != null)
            addUniqueEntity("Owner",ownerEntity);
        if(targetEntity != null)
            addUniqueEntity("Target",targetEntity);
        if(includePlayer)
            addUniqueEntity("Player",StageProcessor.Instance().getPlayerEntity());

        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Initialize(this);
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Execute(this, 0f);
        }

        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Initialize(this);
        }

        if(_currentSequencer == _savedEventItem._targetSequencerGraph)
        {
            _currentIndex = _savedEventItem._savedIndex;
        }
    }

}
