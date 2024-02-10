using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBubbleObject : TextBubbleBinder
{
    private TextPresenter _textPresenter;
    private readonly Queue<BubbleCommend> _commendQueue = new Queue<BubbleCommend>();
    private BubbleCommend _currentCommand;

    private GameEntityBase _followTarget;
    private bool _isPlay = false;
    private Action _onEnd;

    private TextBubble _owner;

    public void Init(TextBubble owner)
    {
        _textPresenter = new TextPresenter(this);
        gameObject.SetActive(false);
        _owner = owner;
    }
    
    public void SetActive(bool active)
    {
        if (active == false)
        {
            gameObject.SetActive(false);
            _isPlay = false;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    
    public void PlayCommand(List<BubbleCommend> commandList, GameEntityBase followTarget, Action onEnd)
    {
        if (commandList == null || commandList.Count <= 0)
        {
            _onEnd?.Invoke();
            _owner.ReturnPool(this);
            return;
        }

        _textPresenter.Clear();

        _followTarget = followTarget;
        UpdateFollowPosition();
        _textPresenter.Clear();
        
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
    }

    public void FixedUpdate()
    {
        if (_isPlay == false)
        {
            return;
        }
        
        //UpdateFollowPosition();
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
}
