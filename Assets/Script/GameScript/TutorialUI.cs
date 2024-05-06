using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public SpriteRenderer _tutorialMouseLeft;
    public SpriteRenderer[] _tutorialMouseLeftDrag = null;
    public SpriteRenderer[] _tutorialMouseRightDrag = null;
    public SpriteRenderer[] _tutorialW = null;
    
    private AnimationPlayer _leftClickPlayer = new AnimationPlayer();
    private AnimationPlayer _leftPlayer = new AnimationPlayer();
    private AnimationPlayer _rightPlayer = new AnimationPlayer();
    private AnimationPlayer _wPlayer = new AnimationPlayer();

    private bool _leftInput = false;
    private bool _rightInput = false;
    private bool _wInput = false;

    void Start()
    {
        _leftPlayer.initialize();
        _leftPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");

        _rightPlayer.initialize();
        _rightPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");

        _leftClickPlayer.initialize();
        _leftClickPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMB/");

        _wPlayer.initialize();
        _wPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingW/");
    }

    void Update()
    {
        if(ActionKeyInputManager.Instance().keyCheck("AttackCharge"))
        {
            if(_leftInput == false)
            {
                _leftPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/InputLMBDRAG/");
                _leftInput = true;
            }
        }
        else
        {
            if(_leftInput)
            {
                _leftPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");
                _leftInput = false;
            }
        }

        if(ActionKeyInputManager.Instance().keyCheck("NormalMove"))
        {
            if(_rightInput == false)
            {
                _rightPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/InputLMBDRAG/");
                _rightInput = true;
            }
        }
        else
        {
            if(_rightInput)
            {
                _rightPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");
                _rightInput = false;
            }
        }

        if(ActionKeyInputManager.Instance().keyCheck("GuardBreak2"))
        {
            if(_wInput == false)
            {
                _wPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/InputW/");
                _wInput = true;
            }
        }
        else
        {
            if(_wInput)
            {
                _wPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingW/");
                _wInput = false;
            }
        }

        _leftPlayer.progress(Time.deltaTime,null);
        _rightPlayer.progress(Time.deltaTime,null);
        _leftClickPlayer.progress(Time.deltaTime,null);

        if(_tutorialMouseLeftDrag != null)
        {
            foreach(var item in _tutorialMouseLeftDrag)
                item.sprite = _leftPlayer.getCurrentSprite();
        }

        if(_tutorialMouseRightDrag != null)
        {
            foreach(var item in _tutorialMouseRightDrag)
                item.sprite = _rightPlayer.getCurrentSprite();
        }

        if(_tutorialW != null)
        {
            foreach(var item in _tutorialW)
                item.sprite = _wPlayer.getCurrentSprite();
        }

        _tutorialMouseLeft.sprite = _leftClickPlayer.getCurrentSprite();
    }
}
