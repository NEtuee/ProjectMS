using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGraphManager : Singleton<StageGraphManager>
{
    private StageGraphBaseData                  _currentStage = null;
    private Dictionary<string, GameEntityBase>  _uniqueEntityDictionary = new Dictionary<string, GameEntityBase>();
    private int                                 _currentIndex = 0;
    private bool                                _isStageEventEnd = false;

    public void initialize()
    {
        _currentIndex = 0;
        _isStageEventEnd = false;

        _uniqueEntityDictionary.Clear();
    }

    public void progress(float deltaTime)
    {
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
        {
            DebugUtil.assert(false,"unique entity key is not Exists : {0}",uniqueKey);
            return null;
        }

        return _uniqueEntityDictionary[uniqueKey];
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

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[0]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Initialize();
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Execute(this, 0f);
        }

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[1]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[1]._stageGraphEventList[index].Initialize();
        }

        initialize();
    }

}
