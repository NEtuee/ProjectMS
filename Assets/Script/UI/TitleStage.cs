using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleStage : MonoBehaviour
{
    public Animator _titleStageAnimator;
    private bool _active = true;
    private bool _keyCheck = true;

    private float _timer = 0f;

    void Start()
    {
        GameUI.Instance.activeOptionUI(true);
        _active = true;

        FMODAudioManager.Instance().Play(5000, Vector3.zero);
        FMODAudioManager.Instance().Play(5001, Vector3.zero);
        FMODAudioManager.Instance().Play(5002, Vector3.zero);
        FMODAudioManager.Instance().Play(5003, Vector3.zero);
    }

    public void Update()
    {
        if(_active == false)
            return;

        if(Input.anyKeyDown && Input.GetMouseButton(0) == false && _keyCheck)
        {
            _titleStageAnimator.SetTrigger("Press");
            _keyCheck = false;

            _timer = 0f;
        }
        else if(Input.anyKey == false && _keyCheck == false)
        {
            _titleStageAnimator.SetTrigger("Idle");
            _keyCheck = true;
        }

        if(_keyCheck == false)
        {
            _timer += Time.deltaTime;
            if(_timer >= 1.0f)
            {
                _active = false;
                _titleStageAnimator.gameObject.SetActive(false);

                FMODAudioManager.Instance().Play(4908, Vector3.zero);

                MasterManager.instance._stageProcessor.addSequencerSignal("NextStage");
            }
        }
    }
}
