using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public SpriteRenderer _tutorialMouseLeft;
    public SpriteRenderer _tutorialMouseLeftDrag;
    public SpriteRenderer _tutorialMouseRightDrag;
    
    private AnimationPlayer _leftClickPlayer = new AnimationPlayer();
    private AnimationPlayer _leftPlayer = new AnimationPlayer();
    private AnimationPlayer _rightPlayer = new AnimationPlayer();

    private bool _leftInput = false;
    private bool _rightInput = false;

    void Start()
    {
        _leftPlayer.initialize();
        _leftPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");

        _rightPlayer.initialize();
        _rightPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMBDRAG/");

        _leftClickPlayer.initialize();
        _leftClickPlayer.changeAnimationByCustomPreset("Sprites/UI/KeyInput/WaitingLMB/");
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

        _leftPlayer.progress(Time.deltaTime,null);
        _rightPlayer.progress(Time.deltaTime,null);
        _leftClickPlayer.progress(Time.deltaTime,null);

        _tutorialMouseLeftDrag.sprite = _leftPlayer.getCurrentSprite();
        _tutorialMouseRightDrag.sprite = _rightPlayer.getCurrentSprite();

        _tutorialMouseLeft.sprite = _leftClickPlayer.getCurrentSprite();
    }
}
