using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkBalloon : MonoBehaviour
{
    public TextMesh _textMesh;

    private float _currentTime = 0f;

    public void setText(string text, float time)
    {
        _textMesh.text = text;
        _currentTime = time;
    }

    public bool updateTalkBalloon(float deltaTime)
    {
        _currentTime -= deltaTime;
        return isEnd();
    }

    public bool isEnd()
    {
        return _currentTime <= 0f;
    }

    public void setActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
