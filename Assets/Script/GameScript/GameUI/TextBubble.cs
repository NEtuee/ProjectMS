using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextBubble : IUIElement
{
    private TextBubbleBinder _binder;
    private TextPresenter _textPresenter;
    private readonly Queue<BubbleCommend> _commendQueue = new Queue<BubbleCommend>();
    private BubbleCommend _currentCommand;

    private bool _isPlay = false;
    private Action _onEnd;

    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "말풍선 바인더가 없습니다.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as TextBubbleBinder;
    }

    public void Initialize()
    {
        _textPresenter = new TextPresenter(_binder);
        _binder.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (active == false)
        {
            _binder.gameObject.SetActive(false);
            _isPlay = false;
        }
        else
        {
            _binder.gameObject.SetActive(true);
        }
    }

    public void PlayCommand(List<BubbleCommend> commandList, Action onEnd)
    {
        if (commandList == null || commandList.Count <= 0)
        {
            _onEnd?.Invoke();
            return;
        }

        _commendQueue.Clear();

        foreach (var commend in commandList)
        {
            _commendQueue.Enqueue(commend);
        }

        SetActive(true);
        _isPlay = true;
        _onEnd = onEnd;
    }
    
    public void UpdateByManager()
    {
        if (_isPlay == false)
        {
            return;
        }
        
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
            }
            else
            {
                _currentCommand = _commendQueue.Dequeue();
                _currentCommand.Start(_textPresenter, GlobalTimer.Instance().getScaledGlobalTime());
            }
        }
    }
}

public class TextPresenter
{
    private TextBubbleBinder _binder;
    private RectTransform _textCompRect;
    private RectTransform _backgroundRect;
    private StringBuilder _stringBuilder;

    private bool _chagneColor = false;
    private string _changeColorHex;

    private bool _bold = false;
    
    public TextPresenter(TextBubbleBinder binder)
    {
        _binder = binder;
        _textCompRect = _binder.TextComp.transform as RectTransform;
        _backgroundRect = binder.BackGroundImage.rectTransform;
        _stringBuilder = new StringBuilder();
    }

    public void InitTextLength(int length)
    {
        _stringBuilder.Capacity = length + 100;
    }

    public void AddCharacter(char ch)
    {
        if (_chagneColor == true && _bold == true)
        {
            _stringBuilder.Append($"<b><color=#{_changeColorHex}>{ch}</color></b>");
        }
        else if (_chagneColor == true)
        {
            _stringBuilder.Append($"<color=#{_changeColorHex}>{ch}</color>");
        }
        else if (_bold == true)
        {
            _stringBuilder.Append($"<b>{ch}</b>");
        }
        else
        {
            _stringBuilder.Append(ch);
        }
        
        UpdateText(_stringBuilder.ToString());
    }

    public void SetColor(string colorHex)
    {
        _changeColorHex = colorHex;
        _chagneColor = true;
    }

    public void EndColor()
    {
        _chagneColor = false;
    }

    public void LineAlignment()
    {
        _stringBuilder.Append("\n");
        //UpdateText(_stringBuilder.ToString());
    }

    public void Bold()
    {
        _bold = true;
    }

    public void EndBold()
    {
        _bold = false;
    }
    
    private void UpdateText(string text)
    {
        _binder.TextComp.text = text;
        _textCompRect.sizeDelta = new Vector2(_binder.TextComp.preferredWidth, _binder.TextComp.preferredHeight);
        _backgroundRect.sizeDelta = new Vector2(_binder.TextComp.preferredWidth + _binder.WidthPadding * 2.0f, _binder.TextComp.preferredHeight + _binder.HeightPadding * 2.0f);
    }
}

public interface BubbleCommend
{
    public void Start(TextPresenter presenter, float startTime);
    public bool Update(TextPresenter presenter, float deltaTime);
    public void End();
}

public class ShowText : BubbleCommend
{
    private float _interval;
    private float _prevShowTime = 0;
    private float _currentTime;
    private int _current = 0;
    private char[] _chArray;

    public ShowText(float interval, string text)
    {
        _interval = interval;
        _chArray = text.ToCharArray();
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        _currentTime = startTime;
        _prevShowTime = _currentTime;
        presenter.AddCharacter(_chArray[_current]);
        _current++;
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        if (_current >= _chArray.Length)
        {
            return false;
        }
        
        _currentTime += deltaTime;

        if (_currentTime - _prevShowTime >= _interval)
        {
            _prevShowTime = _currentTime;
            presenter.AddCharacter(_chArray[_current]);
            _current++;
        }
        
        return true;
    }

    public void End()
    {
    }
}

public class SetTextColor : BubbleCommend
{
    private string _colorHex;
    
    public SetTextColor(string colorHex)
    {
        _colorHex = colorHex;
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.SetColor(_colorHex);
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

public class EndTextColor : BubbleCommend
{
    public EndTextColor()
    {
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.EndColor();
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

public class AddLineAlignment : BubbleCommend
{
    public AddLineAlignment()
    {
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.LineAlignment();
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

public class SetBold : BubbleCommend
{
    public SetBold()
    {
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.Bold();
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

public class EndBold : BubbleCommend
{
    public EndBold()
    {
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.EndBold();
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

public class Wait : BubbleCommend
{
    private float _time;
    private float _currentTime;
    
    public Wait(float time)
    {
        _time = time;
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        if (_currentTime >= _time)
        {
            return false;
        }
        
        _currentTime += deltaTime;
        return true;
    }

    public void End()
    {
    }
}

