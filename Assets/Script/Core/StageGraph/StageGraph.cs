using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGraphManager : Singleton<StageGraphManager>
{
    private StageGraphBaseData _currentStage = null;
    private int _currentIndex = 0;
    private bool _isStageEventEnd = false;

    public void initialize()
    {
        _currentIndex = 0;
        _isStageEventEnd = false;
    }

    public void progress(float deltaTime)
    {
        if(_currentStage == null || _isStageEventEnd)
            return;

        _isStageEventEnd = true;
        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[1]._stageGraphEventCount;)
        {
            if(_currentStage._stageGraphPhase[1]._stageGraphEventList[index].Execute(deltaTime) == false)
            {
                _isStageEventEnd = false;
                break;
            }

            ++index;
        }


    }

    public void startStage(string stageGraphPath)
    {
        _currentStage = ResourceContainerEx.Instance().GetStageGraph(stageGraphPath);
        if(_currentStage == null)
            return;

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[0]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Initialize();
            _currentStage._stageGraphPhase[0]._stageGraphEventList[index].Execute(0f);
        }

        for(int index = _currentIndex; index < _currentStage._stageGraphPhase[1]._stageGraphEventCount; ++index)
        {
            _currentStage._stageGraphPhase[1]._stageGraphEventList[index].Initialize();
        }

        initialize();
    }

}
