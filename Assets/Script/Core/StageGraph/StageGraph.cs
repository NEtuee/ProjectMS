using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGraphManager : Singleton<StageGraphManager>
{
    private struct eventIndexItem
    {
        public StageGraphBaseData _targetStageGraph;
        public int _savedIndex;
    }

    private StageGraphBaseData                  _currentStage = null;
    private Dictionary<string, GameEntityBase>  _uniqueEntityDictionary = new Dictionary<string, GameEntityBase>();
    private int                                 _currentIndex = 0;
    private bool                                _isStageEventEnd = false;

    private eventIndexItem                      _savedEventItem = new eventIndexItem{_savedIndex = 0, _targetStageGraph = null};

    private List<string>                        _deleteUniqueTargetList = new List<string>();

    public void initialize()
    {
        _currentIndex = 0;
        _isStageEventEnd = false;

        _uniqueEntityDictionary.Clear();
        _deleteUniqueTargetList.Clear();
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

        if(_currentStage == null || _isStageEventEnd)
            return;

        _isStageEventEnd = true;
        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[1]._stageGraphEventCount;)
        {
            _currentIndex = index;
            if(_currentStage._stageGraphPhase[1]._stageGraphEventList[index].Execute(this, deltaTime) == false)
            {
                _isStageEventEnd = false;
                break;
            }

            if(_currentStage._stageGraphPhase[1]._stageGraphEventList[index].getStageGraphEventType() == StageGraphEventType.SaveEventExecuteIndex)
            {
                _savedEventItem._savedIndex = index + 1;
                _savedEventItem._targetStageGraph = _currentStage;
            }

            ++index;
        }

    }

    public void addUniqueEntity(string uniqueKey, GameEntityBase uniqueEntity)
    {
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
        return _currentStage != null;
    }

    public void stopStage()
    {
        initialize();
        _currentStage = null;

        Message msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.game_stageEnd,MessageReceiver._boradcastNumber,null,null);

        MasterManager.instance.HandleBroadcastMessage(msg);
    }

    public void startStage(string stageGraphPath)
    {
        _currentStage = ResourceContainerEx.Instance().GetStageGraph(stageGraphPath);
        if(_currentStage == null)
            return;

        initialize();

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[0]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Initialize();
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Execute(this, 0f);
        }

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[1]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[1]._stageGraphEventList[index].Initialize();
        }

        if(_currentStage == _savedEventItem._targetStageGraph)
        {
            _currentIndex = _savedEventItem._savedIndex;
        }
    }

}
