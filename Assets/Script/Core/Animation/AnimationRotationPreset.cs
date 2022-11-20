using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AnimationRotationPreset", menuName = "Scriptable Object/Animation Rotation Preset", order = int.MaxValue)]
public class AnimationRotationPreset : ScriptableObject
{
    [SerializeField]private List<AnimationRotationPresetData> _presetData = new List<AnimationRotationPresetData>();
    private Dictionary<string, AnimationRotationPresetData> _presetCache = new Dictionary<string, AnimationRotationPresetData>();
    private bool _isCacheConstructed = false;

    // private void Awake()
    // {
    //     constructPresetCache();
    // }

    public AnimationRotationPresetData getPresetData(string targetName)
    {
        AnimationRotationPresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(AnimationRotationPresetData item in _presetData)
            {
                if(item.getName() == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil.assert(target != null,"target animation rotation presetData is not exists : {0}",targetName);
        return target;
    }


    private void constructPresetCache()
    {
        if(_isCacheConstructed == true)
            return;

        foreach(AnimationRotationPresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}
