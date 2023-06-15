using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequencerGraphProcessManager
{
    private SimplePool<SequencerGraphProcessor> _sequencerGrpahProcessorPool = new SimplePool<SequencerGraphProcessor>();
    private List<SequencerGraphProcessor> _activeProcessorList = new List<SequencerGraphProcessor>();

    private GameEntityBase _ownerEntity = null;

    public SequencerGraphProcessManager(GameEntityBase ownerEntity)
    {
        _ownerEntity = ownerEntity;
    }

    public void initialize()
    {
        clearSequencerGraphProcessManager();
    }

    public void progress(float deltaTime)
    {
        for(int index = 0; index < _activeProcessorList.Count;)
        {
            SequencerGraphProcessor processor = _activeProcessorList[index];            
#if UNITY_EDITOR
            if(processor.isValid())
            {
                DebugUtil.assert(false,"프로세서가 있는데 데이터가 없다. 통보 요망");
                continue;
            }
#endif
            processor.progress(deltaTime);
            if(processor.isSequencerEnd())
            {
                _activeProcessorList.RemoveAt(index);
                _sequencerGrpahProcessorPool.enqueue(processor);
            }
            else
            {
                ++index;
            }
        }
    }

    public void startSequencerClean(string sequencerKey, GameEntityBase targetEntity)
    {
        SequencerGraphProcessor processor = _sequencerGrpahProcessorPool.dequeue();
        processor.clearSequencerGraphProcessor();
        processor.startStage(sequencerKey,_ownerEntity,targetEntity);

        _activeProcessorList.Add(processor);
    }

    public void clearSequencerGraphProcessManager()
    {
        foreach(var processor in _activeProcessorList)
        {
            processor.stopStage();
            processor.clearSequencerGraphProcessor();
            _sequencerGrpahProcessorPool.enqueue(processor);
        }

        _activeProcessorList.Clear();
    }
}
