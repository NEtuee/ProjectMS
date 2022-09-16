using System.Collections.Generic;
using UnityEngine;


public class AnimationPlayDataInfo
{
    public AnimationPlayDataInfo(){}

    //따로 때내어야 함
    public ActionFrameEventBase[]       _frameEventData;

    public string                       _path = "";
    public float                        _framePerSec = -1f;
    public float                        _startFrame = -1f;
    public float                        _endFrame = -1f;
    public int                          _frameEventDataCount = -1;

    public bool                         _isLoop = false;
    public bool                         _hasMovementGraph = false;


    public FlipState                    _flipState;
}

public struct FlipState
{
    public bool xFlip;
    public bool yFlip;
}

public class AnimationPlayer
{
    private AnimationTimeProcessor _animationTimeProcessor;
    private AnimationPlayDataInfo _currentAnimationPlayData;

    private string _currentAnimationName;
    private Sprite[] _currentAnimationSprites;
    private MovementGraph _currentMovementGraph;

    private int _currentFrameEventIndex;

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
        processFrameEvent(_currentAnimationPlayData, targetEntity);

        return _animationTimeProcessor.isEnd();
    }

    public void Release()
    {
        
    }
    
    public void processFrameEvent(AnimationPlayDataInfo playData, ObjectBase targetEntity)
    {
        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        for(int i = _currentFrameEventIndex; i < playData._frameEventDataCount; ++i)
        {
            if(MathEx.equals(playData._frameEventData[i]._startFrame, currentFrame,float.Epsilon) == true || playData._frameEventData[i]._startFrame < currentFrame)
            {
                playData._frameEventData[i].onExecute(targetEntity);
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

        setCurrentFrameEventIndex(playData);
    }

    public int getCurrentIndex() {return _animationTimeProcessor.getCurrentIndex();}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc() {return _animationTimeProcessor.getMoveValuePerFrameFromTimeDesc();}
    public AnimationTimeProcessor getTimeProcessor(){return _animationTimeProcessor;}
    public MovementGraph getCurrentMovementGraph() {return _currentMovementGraph;}

    public FlipState getCurrentFlipState() 
    {
        return _currentAnimationPlayData._flipState;
    }

    public Sprite getCurrentSprite()
    {
        return _currentAnimationSprites[_animationTimeProcessor.getCurrentIndex()];
    }
}
