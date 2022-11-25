using System.Collections.Generic;
using UnityEngine;


public class AnimationPlayDataInfo
{
    public AnimationPlayDataInfo(){}

    //따로 때내어야 함
    public ActionFrameEventBase[]       _frameEventData = null;
    public MultiSelectAnimationData[]   _multiSelectAnimationData = null;
    
    public AnimationRotationPresetData  _rotationPresetData = null;
    public AnimationScalePresetData     _scalePresetData = null;

    public int                          _multiSelectAnimationDataCount = 0;

    public string                       _path = "";
    public float                        _framePerSec = -1f;
    public float                        _startFrame = -1f;
    public float                        _endFrame = -1f;
    public int                          _frameEventDataCount = -1;

    public bool                         _isLoop = false;
    public bool                         _hasMovementGraph = false;


    public FlipState                    _flipState;
}

public class MultiSelectAnimationData
{
    public string                           _path = "";
    public ActionGraphConditionCompareData  _actionConditionData = null;
}

public struct FlipState
{
    public bool xFlip;
    public bool yFlip;
}

public struct FrameEventProcessDescription
{
    public ActionFrameEventBase     _targetFrameEvent;
    public ObjectBase               _executeObject;
    public float                    _endTime;

    public void processFrameEvent()
    {
        _targetFrameEvent.onExecute(_executeObject);
    }

    public void exitFrameEvent()
    {
        _targetFrameEvent.onExit(_executeObject);
    }
}

public class AnimationPlayer
{
    private AnimationTimeProcessor _animationTimeProcessor;
    private AnimationPlayDataInfo _currentAnimationPlayData;

    private string _currentAnimationName;
    private Sprite[] _currentAnimationSprites;
    private MovementGraph _currentMovementGraph;

    private int _currentFrameEventIndex;

    private List<FrameEventProcessDescription> _frameEventProcessList = new List<FrameEventProcessDescription>();

    public AnimationPlayer()
    {
        _animationTimeProcessor = new AnimationTimeProcessor();
    }

    public bool isValid()
    {
        return _currentAnimationPlayData != null;
    }

    public void initialize()
    {
        _animationTimeProcessor.initialize();
    }

    public bool progress(float deltaTime, ObjectBase targetEntity)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"invalid playdata");
            return false;
        }

        _animationTimeProcessor.updateTime(deltaTime);
        processFrameEventContinue();
        processFrameEvent(_currentAnimationPlayData, targetEntity);

        return _animationTimeProcessor.isEnd();
    } 

    public void Release()
    {
        
    }

    public void processMultiSelectAnimation(ActionGraph actionGraph)
    {
        if(_currentAnimationPlayData._multiSelectAnimationDataCount == 0)
            return;

        for(int i = 0; i < _currentAnimationPlayData._multiSelectAnimationDataCount; ++i)
        {
            if(actionGraph.processActionCondition(_currentAnimationPlayData._multiSelectAnimationData[i]._actionConditionData) == true)
            {
                _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._multiSelectAnimationData[i]._path);
                return;
            }
        }

        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._path);
    }
    

    public void processFrameEventContinue()
    {
        for(int i = 0; i < _frameEventProcessList.Count;)
        {
            _frameEventProcessList[i].processFrameEvent();

            if(_frameEventProcessList[i]._endTime <= _animationTimeProcessor.getAnimationTotalPlayTime() || 
               MathEx.equals(_frameEventProcessList[i]._endTime, _animationTimeProcessor.getAnimationTotalPlayTime(), float.Epsilon))
            {
                _frameEventProcessList[i].exitFrameEvent();
                _frameEventProcessList.RemoveAt(i);
            }
            else
                ++i;
        }
    }
    
    public void processFrameEvent(AnimationPlayDataInfo playData, ObjectBase targetEntity)
    {
        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        for(int i = _currentFrameEventIndex; i < playData._frameEventDataCount; ++i)
        {
            ActionFrameEventBase frameEvent = playData._frameEventData[i];
            if(MathEx.equals(frameEvent._startFrame, currentFrame,float.Epsilon) == true || frameEvent._startFrame < currentFrame)
            {
                frameEvent.initialize();

                if(frameEvent.onExecute(targetEntity) == true && frameEvent._endFrame > frameEvent._startFrame)
                {
                    FrameEventProcessDescription desc;
                    desc._executeObject = targetEntity;
                    desc._endTime = _animationTimeProcessor.frameToTime(frameEvent._endFrame);
                    desc._targetFrameEvent = frameEvent;

                    _frameEventProcessList.Add(desc);
                }
                else
                {
                    frameEvent.onExit(targetEntity);
                }

                _currentFrameEventIndex++;
            }
            else
            {
                return;
            }
        }
    }

    private void setCurrentFrameEventIndex(AnimationPlayDataInfo playData)
    {
        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        for(int i = 0; i < playData._frameEventDataCount; ++i)
        {
            _currentFrameEventIndex = i;
            if(playData._frameEventData[i]._startFrame >= currentFrame)
                return;
        }
    }

    public void changeAnimation(AnimationPlayDataInfo playData)
    {
        _currentAnimationPlayData = playData;
        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(playData._path);
        if(playData._hasMovementGraph == true)
        {
            _currentMovementGraph = ResourceContainerEx.Instance().getMovementgraph(playData._path);
            DebugUtil.assert(_currentMovementGraph != null, "movementGraph Not Exists");        
        }

        DebugUtil.assert(_currentAnimationSprites != null, "animation sprite array is null");

        float startFrame = playData._startFrame;
        startFrame = startFrame == -1f ? 0f : startFrame;

        float endFrame = playData._endFrame;
        endFrame = endFrame == -1f ? (float)_currentAnimationSprites.Length : endFrame;

        _animationTimeProcessor.initialize();
        _animationTimeProcessor.setFrame(startFrame,endFrame, playData._framePerSec);
        _animationTimeProcessor.setLoop(playData._isLoop);
        _animationTimeProcessor.setFrameToTime(playData._startFrame);
        _animationTimeProcessor.setAnimationSpeed(1f);

        for(int i = 0; i < _frameEventProcessList.Count; ++i)
        {
            _frameEventProcessList[i].exitFrameEvent();
        }
        _frameEventProcessList.Clear();

        setCurrentFrameEventIndex(playData);
    }

    public void setAnimationSpeed(float speed) {_animationTimeProcessor.setAnimationSpeed(speed);}

    public float getCurrentFrame() {return _animationTimeProcessor.getCurrentFrame();}
    public float getCurrentAnimationDuration() {return _animationTimeProcessor.getAnimationDuration();}

    public int getCurrentIndex() {return _animationTimeProcessor.getCurrentIndex();}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc() {return _animationTimeProcessor.getMoveValuePerFrameFromTimeDesc();}
    public AnimationTimeProcessor getTimeProcessor(){return _animationTimeProcessor;}
    public MovementGraph getCurrentMovementGraph() {return _currentMovementGraph;}


    public Vector3 getCurrentAnimationScale()
    {
        if(_currentAnimationPlayData._scalePresetData == null)
            return Vector3.one;
        
        Vector2 currentScale = _currentAnimationPlayData._scalePresetData.evaulate(_animationTimeProcessor.getCurrentNormalizedTime());
        return new Vector3(currentScale.x,currentScale.y,1f);
    }

    public Vector3 getAnimationScalePerFrame()
    {
        if(_currentAnimationPlayData._scalePresetData == null)
            return Vector3.one;
        

        Vector2 scale = _currentAnimationPlayData._scalePresetData.getScaleValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc());
        return new Vector3(scale.x,scale.y,1f);
    }


    public Quaternion getCurrentAnimationRotation()
    {
        if(_currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;

        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.evaulate(_animationTimeProcessor.getCurrentNormalizedTime()));
    }

    public Quaternion getAnimationRotationPerFrame()
    {
        if(_currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;
        
        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.getRotateValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc()));
    }

    public FlipState getCurrentFlipState() 
    {
        return _currentAnimationPlayData._flipState;
    }

    public Sprite getCurrentSprite()
    {
        return _currentAnimationSprites[_animationTimeProcessor.getCurrentIndex()];
    }
}
