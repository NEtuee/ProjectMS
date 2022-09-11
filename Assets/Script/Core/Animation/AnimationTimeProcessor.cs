public class AnimationTimeProcessor
{
    private float       _framePerSecond = 0f;
    private float       _frameToTime = 0f;
    private float       _animationTime = 0f;

    private float       _animationStartTime = 0f;
    private float       _animationEndTime = 0f;
    
    private bool        _isLoop = false;
    private bool        _isEnd = false;

    private int         _totalLoopCountPerFrame;

    private float       _currentAnimationTime = 0f;
    private int         _currentIndex = 0;

    private float       _prevAnimationTime = 0f;
    private int         _prevIndex = 0;

    public bool isValid()
    {
        //return _frameCount != 0;
        return true;
    }

    public void initialize()
    {
        _isEnd = false;
        _isLoop = false;

        _currentAnimationTime = 0f;
        _currentIndex = 0;

        _prevAnimationTime = 0f;
        _prevIndex = 0;
    }

    public bool updateTime(float deltaTime)
    {
        DebugUtil.assert(isValid(),"frame count is zero");
        
        _totalLoopCountPerFrame = 0;

        if(_isEnd == true)
            return true;

        _prevAnimationTime = _currentAnimationTime;
        _prevIndex = _currentIndex;

        _currentAnimationTime += deltaTime;
        _isEnd = CurrentAnimationIsEndInner();

        if(_isLoop == true && _isEnd == true)
        {
            while(_prevAnimationTime >= _animationEndTime)
            {
                _prevAnimationTime -= _animationTime;
            }

            while(_currentAnimationTime >= _animationEndTime)
            {
                ++_totalLoopCountPerFrame;
                _currentAnimationTime -= _animationTime;
            }

            _isEnd = false;
        }
        else if(_isEnd == true)
        {
            _currentAnimationTime = _animationEndTime;
        }
            
        _currentIndex = getIndexInner();

        return _isEnd;
    }

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc()
    {
        MoveValuePerFrameFromTimeDesc desc;
        desc.currentNormalizedTime = getCurrentNormalizedTime();
        desc.prevNormalizedTime = getPrevNormalizedTime();
        desc.loopCount = getTotalLoopCount();

        return desc;
    }

    public int getCurrentIndex()
    {
        return _currentIndex;
    }

    private int getIndexInner()
    {
        int currentIndex = (int)(_currentAnimationTime / _frameToTime);
        return _currentAnimationTime >= _animationEndTime ? currentIndex - 1 : currentIndex;
    }

    public int getTotalLoopCount()
    {
        return _totalLoopCountPerFrame;
    }

    public float getCurrentNormalizedTime()
    {
        return (_currentAnimationTime - _animationStartTime) / _animationTime;
    }

    public float getPrevNormalizedTime()
    {
        return (_prevAnimationTime - _animationStartTime) / _animationTime;
    }

    public float getCurrentFrame()
    {
        return _currentAnimationTime / _frameToTime;
    }

    public void setFrameToTime(float frame)
    {
        _currentAnimationTime = frame * _frameToTime;
        updateTime(0f);
    }

    public void setLoop(bool isLoop) 
    {
        _isLoop = isLoop;
    }

    public void setFrame(float startFrame, float endFrame, float fps)
    {
        _framePerSecond = fps;
        _frameToTime = 1f / _framePerSecond;

        _animationStartTime = startFrame * _frameToTime;
        _animationEndTime = endFrame * _frameToTime;
        _animationTime = _animationEndTime - _animationStartTime;
    }

    public bool isEnd()
    {
        return _isEnd;
    }

    private bool CurrentAnimationIsEndInner()
    {
        return _animationTime <= _currentAnimationTime;
    }

}

