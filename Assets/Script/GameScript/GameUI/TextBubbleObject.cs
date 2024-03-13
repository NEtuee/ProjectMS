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

    private GameEntityBase _followTarget;
    private bool _isPlay = false;
    private Action _onEnd;

    private TextBubble _owner;

    private AnimationPlayer _animationPlayer = new AnimationPlayer();
    private AnimationPresetInfo _iconWaitIcon;
    
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
        
        _followTarget = followTarget;
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
            _currentCommand.Start(_textPresenter, GlobalTimer.Instance().getScaledGlobalTime());
        }

        if (_currentCommand.Update(_textPresenter, GlobalTimer.Instance().getSclaedDeltaTime()) == false)
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
                _currentCommand.Start(_textPresenter, GlobalTimer.Instance().getScaledGlobalTime());
            }
        }
    }

    public bool isFollowTargetInsideCamera()
    {
        if (_followTarget == null)
            return false;

        Vector3 cameraInPosition;
        return CameraControlEx.Instance().IsInCameraBound(_followTarget.transform.position, Camera.main.transform.position, out cameraInPosition);
    }
    
    private void UpdateFollowPosition()
    {
        if (_followTarget == null)
        {
            return;
        }

        var followScreenPos = Camera.main.WorldToScreenPoint(_followTarget.transform.position + FollowOffset);
        followScreenPos.z = 0;
        transform.position = followScreenPos;
    }

    private void CheckDead()
    {
        if (_followTarget == null)
        {
            return;
        }

        if (_followTarget.isDead() == true)
        {
            SetActive(false);
        }
    }

    private Vector2 GetRandomAdd(Vector2 randomRange)
    {
        return new Vector2(Random.Range(-randomRange.x, randomRange.x + 1), Random.Range(-randomRange.y, randomRange.y + 1));
    }
}
