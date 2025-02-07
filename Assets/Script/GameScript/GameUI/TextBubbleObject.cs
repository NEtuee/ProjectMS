using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextBubbleObject : TextBubbleBinder
{
    private TextPresenter _textPresenter;
    private readonly Queue<BubbleCommend> _commendQueue = new Queue<BubbleCommend>();
    private BubbleCommend _currentCommand;

    public GameEntityBase FollowTarget { get; private set; }
    private bool _isPlay = false;
    private Action _onEnd;

    private TextBubble _owner;

    private AnimationPlayer _animationPlayer = new AnimationPlayer();
    private AnimationPresetInfo _iconWaitIcon;
    private AnimationScalePresetData _scalePreset;

    private TimeProcessor _timeProcessor = new TimeProcessor();
    private TimeProcessor.TimeProcessItem _timeProcessItem = null;
    private TimeProcessor.TimeProcessItem _eyeRandomTimeProcessItem = null;
    
    private AnimationTimeProcessor _portraitEyeTimeProcessor = new AnimationTimeProcessor();
    private AnimationTimeProcessor _portraitMouthTimeProcessor = new AnimationTimeProcessor();

    private int _currentMouthIndex = 0;
    private Vector3 _portraitScaleOrigin;
    private Vector3 _portraitPositionOrigin;

    private ExpressionData _portraitExpressionData = null;

    private struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }

    public void Init(TextBubble owner)
    {
        _textPresenter = new TextPresenter(this);
        gameObject.SetActive(false);
        _owner = owner;

        _portraitScaleOrigin = _portrait.rectTransform.localScale;
        _portraitPositionOrigin = _portrait.rectTransform.anchoredPosition;
    
        _iconWaitIcon = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/talkballoon/dialogsticker/opening/"), "Sprites/UI/talkballoon/dialogsticker/opening");
        _animationPlayer.initialize();
        _animationPlayer.changeAnimationByCustomPreset(_iconWaitIcon._path, _iconWaitIcon._customPreset);

        _scalePreset = (ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationScalePreset") as AnimationScalePreset).getPresetData("PortraitAppear");

        _timeProcessItem = _timeProcessor.addTimer("PortraitAppear", 0.2f);
        _eyeRandomTimeProcessItem = _timeProcessor.addTimer("EyeRandomTime", 1f);
    }
    
    public void SetActive(bool active)
    {
        _textPresenter?.setTalking(false);
        if (active == false)
        {
            gameObject.SetActive(false);
            _isPlay = false;
            IconWaitInput.gameObject.SetActive(false);
            _textPresenter?.Clear();

        }
        else
        {
            if(gameObject.activeSelf == false)
            {
                _textPresenter.InitializeTextBubbleSize();
                _textPresenter.Clear();
            }
                
            gameObject.SetActive(true);
        }
    }
    
    public void PlayCommand(List<BubbleCommend> commandList, GameEntityBase followTarget, Vector2 randomRange, Action onEnd)
    {
        if (commandList == null || commandList.Count <= 0)
        {
            _onEnd?.Invoke();
            _owner.ReturnPool(this);
            return;
        }
        
        _textPresenter.Clear();
        hidePortrait();

        var add1 = GetRandomAdd(randomRange);
        var add2 = GetRandomAdd(randomRange);
        var add3 = GetRandomAdd(randomRange);
        var add4 = GetRandomAdd(randomRange);
        BubblePolygonMain.InitRandomAdd(add1, add2, add3, add4);
        
         add1 = GetRandomAdd(randomRange);
         add2 = GetRandomAdd(randomRange);
         add3 = GetRandomAdd(randomRange);
         add4 = GetRandomAdd(randomRange);
        BubblePolygonBack.InitRandomAdd(add1, add2, add3, add4);


        BubblePolygonArrow.InitRandomAdd(
            new Vector2(Random.Range(5f,15f),0f), 
            new Vector2(Random.Range(-15f,5f),0f), 
            new Vector2(BubblePolygonArrow.rectTransform.rect.width * 0.5f,0f), 
            new Vector2(-BubblePolygonArrow.rectTransform.rect.width * 0.5f,0f));
        
        FollowTarget = followTarget;
        UpdateFollowPosition();

        _commendQueue.Clear();

        foreach (var commend in commandList)
        {
            _commendQueue.Enqueue(commend);
        }

        SetActive(true);
        _isPlay = true;
        _onEnd = onEnd;

        Update();
    }
    
    public void Update()
    {
        updatePortrait();

        if (_isPlay == false)
        {
            return;
        }
        
        _timeProcessor.updateProcessor(Time.deltaTime);

        UpdateCommand();
        UpdateFollowPosition();
        CheckDead();
        UpdateInputWaitIcon();
    }

    public void FixedUpdate()
    {
        if (_isPlay == false)
        {
            return;
        }
        
        //UpdateFollowPosition();
    }

    public void PlayIconAnimation()
    {
        IconWaitInput.gameObject.SetActive(true);
        _animationPlayer.changeAnimationByCustomPreset(_iconWaitIcon._path, _iconWaitIcon._customPreset);
    }

    public void HideIconAnimation()
    {
        IconWaitInput.gameObject.SetActive(false);
        _animationPlayer.initialize();
    }

    public void showPortrait(ExpressionData expressionData)
    {
        _portraitExpressionData = expressionData;
        _portrait.sprite = _portraitExpressionData._baseSprite;
        _timeProcessItem.initialize();

        _portraitEye.rectTransform.anchoredPosition = _portraitExpressionData._eyeOffset;
        _portraitMouth.rectTransform.anchoredPosition = _portraitExpressionData._mouthOffset;

        _portraitEyeTimeProcessor.initialize();
        _portraitMouthTimeProcessor.initialize();

        _portraitEyeTimeProcessor.setFrame(0f, (float)_portraitExpressionData._eyeAnimationOrder.Length, _portraitExpressionData._eyeFPS);
        _portraitMouthTimeProcessor.setFrame(0f, 1f, _portraitExpressionData._mouthFPS);

        _portraitEyeTimeProcessor.setLoop(_portraitExpressionData._eyeRandomPause == false);
        _portraitMouthTimeProcessor.setLoop(true);

        _portraitEyeTimeProcessor.setLoopCount(_portraitExpressionData._eyeRandomPause ? 1 : 0);
        _portraitMouthTimeProcessor.setLoopCount(0);

        _portraitEye.sprite = _portraitExpressionData._eyeAnimation[_portraitExpressionData._eyeAnimationOrder[0]];
        _portraitMouth.sprite = _portraitExpressionData._talkAnimation[0];

        _currentMouthIndex = 0;

        if(_portraitExpressionData._eyeRandomPause)
        {
            Vector2 pauseMinMax = _portraitExpressionData._eyeRandomPauseMinMax;
            _eyeRandomTimeProcessItem.set(Random.Range(pauseMinMax.x,pauseMinMax.y));
        }

        updatePortrait();

        _portrait.gameObject.SetActive(true);
    }

    public void hidePortrait()
    {
        _portrait.gameObject.SetActive(false);
    }

    private void updatePortrait()
    {
        if(_portraitExpressionData == null)
            return;

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        float yScale = _scalePreset.evaulate(_timeProcessItem.getRate()).y;
        bool xFlip = FollowTarget.getFlipState().xFlip;
        Vector3 resultScale = new Vector3(xFlip ? 1f : -1f, yScale, 1f);
        resultScale.x *= _portraitScaleOrigin.x;
        resultScale.y *= _portraitScaleOrigin.y;
        resultScale.z *= _portraitScaleOrigin.z;

        Vector3 resultPosition = _portraitPositionOrigin;
        resultPosition.x *= xFlip ? 1f : -1f;

        _portrait.rectTransform.localScale = resultScale;
        _portrait.rectTransform.anchoredPosition = resultPosition;

        if(_textPresenter.isTalking())
        {
            _portraitMouthTimeProcessor.updateTime(deltaTime);
            if(_portraitMouthTimeProcessor.isLoopedThisFrame())
            {
                if(_portraitExpressionData._talkAnimation.Length > 2)
                {
                    while(true)
                    {
                        int newIndex = Random.Range(1,_portraitExpressionData._talkAnimation.Length);
                        if(_currentMouthIndex != newIndex)
                        {
                            _currentMouthIndex = newIndex;
                            break;
                        }
                    }
                }
                else
                {
                    _currentMouthIndex = 1;
                }

            }
        }
        else
        {
            _currentMouthIndex = 0;
        }

        _portraitMouth.sprite = _portraitExpressionData._talkAnimation[_currentMouthIndex];

        bool pauseEye = false;
        if(_portraitExpressionData._eyeRandomPause)
        {
            _eyeRandomTimeProcessItem.update(deltaTime);
            pauseEye = _eyeRandomTimeProcessItem.isTriggered() == false;
        }

        if(pauseEye == false)
        {
            _portraitEyeTimeProcessor.updateTime(deltaTime);
            int order = _portraitExpressionData._eyeAnimationOrder[_portraitEyeTimeProcessor.getCurrentIndex()];

            _portraitEye.sprite = _portraitExpressionData._eyeAnimation[order];
        }

        if(_portraitExpressionData._eyeRandomPause && _portraitEyeTimeProcessor.isEnd())
        {
            _portraitEyeTimeProcessor.resetTimeProcessor();

            Vector2 pauseMinMax = _portraitExpressionData._eyeRandomPauseMinMax;
            _eyeRandomTimeProcessItem.set(Random.Range(pauseMinMax.x,pauseMinMax.y));
        }

    }

    private void UpdateInputWaitIcon()
    {
        if (_animationPlayer.isEnd() == true)
        {
            return;
        }

        _animationPlayer.progress(GlobalTimer.Instance().getSclaedDeltaTime(), null);
        IconWaitInput.sprite = _animationPlayer.getCurrentSprite();
    }

    private void UpdateCommand()
    {
        if (_currentCommand == null)
        {
            if (_commendQueue.Count <= 0)
            {
                return;
            }
            
            _currentCommand = _commendQueue.Dequeue();
            _currentCommand.Start(_textPresenter, FollowTarget, GlobalTimer.Instance().getScaledGlobalTime());
        }

        if (_currentCommand.Update(_textPresenter, FollowTarget, GlobalTimer.Instance().getSclaedDeltaTime()) == false)
        {
            _currentCommand.End();
            
            if (_commendQueue.Count <= 0)
            {
                _currentCommand = null;
                _isPlay = false;
                SetActive(false);
                _onEnd?.Invoke();
                _owner.ReturnPool(this);
            }
            else
            {
                _currentCommand = _commendQueue.Dequeue();
                _currentCommand.Start(_textPresenter, FollowTarget, GlobalTimer.Instance().getScaledGlobalTime());
            }
        }

        _textPresenter.UpdateTextBubbleSize(GlobalTimer.Instance().getSclaedDeltaTime());
    }

    public void ForceEnd()
    {
        _commendQueue.Clear();
        _currentCommand?.End();
        _currentCommand = null;

        _isPlay = false;
        SetActive(false);
        _onEnd?.Invoke();
        _owner.ReturnPool(this);
    }

    public bool isFollowTargetInsideCamera()
    {
        if (FollowTarget == null)
            return false;

        Vector3 cameraInPosition;
        return CameraControlEx.Instance().IsInCameraBound(FollowTarget.transform.position, Camera.main.transform.position, out cameraInPosition, out Vector3 normal);
    }
    
    private void UpdateFollowPosition()
    {
        if (FollowTarget == null)
        {
            return;
        }

        var followScreenPos = Camera.main.WorldToScreenPoint(FollowTarget.transform.position + FollowOffset);
        followScreenPos.z = 0;
        transform.position = followScreenPos;
    }

    private void CheckDead()
    {
        if (FollowTarget == null)
        {
            return;
        }

        if (FollowTarget.isDead() == true)
        {
            SetActive(false);
        }
    }

    private Vector2 GetRandomAdd(Vector2 randomRange)
    {
        return new Vector2(Random.Range(-randomRange.x, randomRange.x + 1), Random.Range(-randomRange.y, randomRange.y + 1));
    }
}
