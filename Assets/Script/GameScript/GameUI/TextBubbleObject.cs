using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
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
    
        _iconWaitIcon = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/talkballoon/dialogsticker/opening/"), "Sprites/UI/talkballoon/dialogsticker/opening");
        _animationPlayer.initialize();
        _animationPlayer.changeAnimationByCustomPreset(_iconWaitIcon._path, _iconWaitIcon._customPreset);

        _scalePreset = (ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationScalePreset") as AnimationScalePreset).getPresetData("PortraitAppear");

        _timeProcessItem = _timeProcessor.addTimer("PortraitAppear", 0.2f);
    }
    
    public void SetActive(bool active)
    {
        if (active == false)
        {
            gameObject.SetActive(false);
            _isPlay = false;
            IconWaitInput.gameObject.SetActive(false);
        }
        else
        {
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
    }
    
    public void Update()
    {
        if (_isPlay == false)
        {
            return;
        }
        
        _timeProcessor.updateProcessor(Time.deltaTime);

        UpdateCommand();
        UpdateFollowPosition();
        updatePortrait();
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

    public void showPortrait(Sprite sprite)
    {
        _portrait.sprite = sprite;
        _timeProcessItem.initialize();

        updatePortrait();

        _portrait.gameObject.SetActive(true);
    }

    public void hidePortrait()
    {
        _portrait.gameObject.SetActive(false);
    }

    private void updatePortrait()
    {
        float yScale = _scalePreset.evaulate(_timeProcessItem.getRate()).y;
        _portrait.rectTransform.localScale = new Vector3(FollowTarget.getFlipState().xFlip ? -1f : 1f, yScale, 1f);
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
