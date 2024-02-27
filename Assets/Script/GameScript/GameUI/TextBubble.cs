using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class TextBubble : IUIElement
{
    private TextBubblePoolBinder _binder;
    // private readonly Queue<BubbleCommend> _commendQueue = new Queue<BubbleCommend>();
    // private BubbleCommend _currentCommand;
    //
    // private Transform _followTargetTransform;
    // private bool _isPlay = false;
    // private Action _onEnd;
    private Queue<TextBubbleObject> _pool = new Queue<TextBubbleObject>();

    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "말풍선 풀 바인더가 없습니다.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as TextBubblePoolBinder;
    }

    public void Initialize()
    {
        // _textPresenter = new TextPresenter(_binder);
        // _binder.gameObject.SetActive(false);
        
        _binder.TextBubblePrefab.SetActive(false);
    }

    public void ReturnPool(TextBubbleObject disableObject)
    {
        _binder.StartCoroutine(DeferredReturnPool(disableObject));
    }

    private IEnumerator DeferredReturnPool(TextBubbleObject disableObject)
    {
        yield return null;
        _pool.Enqueue(disableObject);
    }
    
    public void SetActive(bool active)
    {
        if (active == false)
        {
            _binder.gameObject.SetActive(false);
        }
        else
        {
            _binder.gameObject.SetActive(true);
        }
    }

    public void PlayCommand(List<BubbleCommend> commandList, GameEntityBase followTarget, Action onEnd)
    {
        var instance = GetInstance();
        if (instance != null)
        {
            instance.PlayCommand(commandList, followTarget, _binder.RandomRange, onEnd);
        }
    }

    private TextBubbleObject GetInstance()
    {
        if (_pool.Count <= 0)
        {
            return CreateInstance();
        }
        
        return _pool.Dequeue();
    }
    
    private TextBubbleObject CreateInstance()
    {
        var newInstance = Object.Instantiate(_binder.TextBubblePrefab, _binder.transform);
        newInstance.Init(this);
        return newInstance;
    }
    
    public void UpdateByManager()
    {
    }
}

public class TextPresenter
{
    private TextBubbleObject _binder;
    private RectTransform _textCompRect;
    private RectTransform _bubbleRect;
    private RectTransform _backRect;
    private StringBuilder _stringBuilder;

    private bool _chagneColor = false;
    private string _changeColorHex;

    private bool _bold = false;
    
    public TextPresenter(TextBubbleObject binder)
    {
        _binder = binder;
        _textCompRect = _binder.TextComp.transform as RectTransform;
        _bubbleRect = binder.BubblePolygonMain.rectTransform;
        _backRect = binder.BubblePolygonBack.rectTransform;
        _stringBuilder = new StringBuilder();
    }

    public void Clear()
    {
        _stringBuilder.Clear();
        UpdateText(_stringBuilder.ToString());
        _binder.BubblePolygonBack.ForceUpdate();
        _binder.BubblePolygonMain.ForceUpdate();
    }

    public void InitTextLength(int length)
    {
        _stringBuilder.Capacity = length + 100;
    }

    public void Active(bool active)
    {
        _binder.SetActive(active);
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

    public void ActiveInputWaitIcon()
    {
        _binder.PlayIconAnimation();
    }
    
    private void UpdateText(string text)
    {
        _binder.TextComp.text = text;
        _textCompRect.sizeDelta = new Vector2(_binder.TextComp.preferredWidth, _binder.TextComp.preferredHeight);
        _bubbleRect.sizeDelta = new Vector2(_binder.TextComp.preferredWidth + _binder.WidthPadding * 2.0f, _binder.TextComp.preferredHeight + _binder.HeightPadding * 2.0f);
        _backRect.sizeDelta = _bubbleRect.sizeDelta;
    }
}

public interface BubbleCommend
{
    public void Start(TextPresenter presenter, float startTime);
    public bool Update(TextPresenter presenter, float deltaTime);
    public void End();
}

public class DeferredActive : BubbleCommend
{
    public void Start(TextPresenter presenter, float startTime)
    {
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        presenter.Active(true);
        return true;
    }

    public void End()
    {
    }
}

public class ShowText : BubbleCommend
{
    private float _interval;
    private float _prevShowTime = 0;
    private float _currentTime;
    private int _current = 0;
    private char[] _chArray;
    private bool _activeInputIcon = false;
    
    private const int _activeInputIconRemainLength = 3;

    public ShowText(float interval, string text, bool activeInputIcon = false)
    {
        _interval = interval;
        _chArray = text.ToCharArray();
    }

    public void SetActiveInputIcon()
    {
        _activeInputIcon = true;
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
        _current = 0;
        _currentTime = startTime;
        _prevShowTime = _currentTime;
        presenter.AddCharacter(_chArray[_current]);
        _current++;

        if (_activeInputIcon == true &&
            _chArray.Length <= _activeInputIconRemainLength)
        {
            presenter.ActiveInputWaitIcon();
        }
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

        if (_activeInputIcon == true &&
            _chArray.Length - _current == _activeInputIconRemainLength)
        {
            presenter.ActiveInputWaitIcon();
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
        _currentTime = 0;
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

public class WaitInput : BubbleCommend
{
    private string _input;
    
    public WaitInput(string input)
    {
        _input = input;
    }
    
    public void Start(TextPresenter presenter, float startTime)
    {
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        if (ActionKeyInputManager.Instance().keyCheck(_input))
        {
            return false;
        }
        
        return true;
    }

    public void End()
    {
    }
}

public class Clear : BubbleCommend
{
    public Clear()
    {
    }


    public void Start(TextPresenter presenter, float startTime)
    {
        presenter.Clear();
    }

    public bool Update(TextPresenter presenter, float deltaTime)
    {
        return false;
    }

    public void End()
    {
    }
}

