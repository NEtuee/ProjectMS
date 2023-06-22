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
    private Dictionary<string, GameEntityBase>  _uniqueEntityDictionary = new Dictionary<string, GameEntityBase>();
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

        _deleteUniqueTargetList.Clear();

        if(_currentSequencer == null || _isSequencerEventEnd)
            return;

        _isSequencerEventEnd = true;
        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventCount;)
        {
            _currentIndex = index;
            if(_currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Execute(this, deltaTime) == false)
            {
                _isSequencerEventEnd = false;
                break;
            }

            _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Exit(this);

            if(_currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].getSequencerGraphEventType() == SequencerGraphEventType.SaveEventExecuteIndex)
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

    public bool isValid()
    {
        return _currentSequencer != null;
    }

    public void stopSequencer()
    {
        for(int index = 0; index < _currentSequencer._sequencerGraphPhase[2]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[2]._sequencerGraphEventList[index].Initialize();
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
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Initialize();
            _currentSequencer._sequencerGraphPhase[0]._sequencerGraphEventList[index].Execute(this, 0f);
        }

        for(int index = _currentIndex; index < _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventCount; ++index)
        {
            _currentSequencer._sequencerGraphPhase[1]._sequencerGraphEventList[index].Initialize();
        }

        if(_currentSequencer == _savedEventItem._targetSequencerGraph)
        {
            _currentIndex = _savedEventItem._savedIndex;
        }
    }

}
