using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class APUI : MonoBehaviour
{
    public static APUI _instance;

    struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }

    public Image[] _apImages;

    private bool[] _apGen;
    private AnimationPlayer[] _animationPlayer = null;
    private AnimationPresetInfo[] _apAnimations = new AnimationPresetInfo[4];
    private GameEntityBase _targetEntity;

    public void Awake()
    {
        _instance = this;

        _animationPlayer = new AnimationPlayer[_apImages.Length];
        _apGen = new bool[_apImages.Length];

        for(int index = 0; index < _apImages.Length; ++index)
        {
            _animationPlayer[index] = new AnimationPlayer();
            _animationPlayer[index].initialize();

            _apGen[index] = false;
        }

        _apAnimations[0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Empty/"), "Sprites/UI/AP/Empty");
        _apAnimations[1] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Full/"),"Sprites/UI/AP/Full");
        _apAnimations[2] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Gen/"),"Sprites/UI/AP/Gen");
        _apAnimations[3] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AP/Lost/"),"Sprites/UI/AP/Lost");
    }

    public void Update()
    {
        updateState();
        updateAnimation(GlobalTimer.Instance().getSclaedDeltaTime());
    }

    public void initialize()
    {
        if(_targetEntity == null)
            return;
        
        float status = _targetEntity.getStatus("DashPoint");
        for(int index = 0; index < _apImages.Length; ++index)
        {
            float current = (float)index + 1;

            _apGen[index] = status >= current;
            if(_apGen[index])
                _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[1]._path,_apAnimations[1]._customPreset);
            else
                _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[0]._path,_apAnimations[0]._customPreset);
        }
    }

    public void updateState()
    {
        if(_targetEntity == null)
            return;
        
        float status = _targetEntity.getStatus("DashPoint");
        for(int index = 0; index < _apImages.Length; ++index)
        {
            float current = (float)index + 1;

            bool currentState = status >= current;
            if(currentState != _apGen[index])
            {
                if(currentState)
                    _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[2]._path,_apAnimations[2]._customPreset);
                else
                    _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[3]._path,_apAnimations[3]._customPreset);
            }
            
            _apGen[index] = currentState;
        }
    }

    public void updateAnimation(float deltaTime)
    {
        if(_targetEntity == null)
            return;

        for(int index = 0; index < _apImages.Length; ++index)
        {
            if(_animationPlayer[index].progress(deltaTime, null))
            {
                if(_animationPlayer[index].getCurrentAnimationName() == _apAnimations[2]._path)
                    _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[1]._path,_apAnimations[1]._customPreset);
                else if(_animationPlayer[index].getCurrentAnimationName() == _apAnimations[3]._path)
                    _animationPlayer[index].changeAnimationByCustomPreset(_apAnimations[0]._path,_apAnimations[0]._customPreset);
            }

            _apImages[index].sprite = _animationPlayer[index].getCurrentSprite();
            _apImages[index].SetNativeSize();
        }
    }

    public void setTarget(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        initialize();
    }
}
