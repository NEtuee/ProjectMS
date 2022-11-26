using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AnimationScalePreset", menuName = "Scriptable Object/Animation Scale Preset", order = int.MaxValue)]
public class AnimationScalePreset : ScriptableObject
{
    [SerializeField]private List<AnimationScalePresetData> _presetData = new List<AnimationScalePresetData>();
    private Dictionary<string, AnimationScalePresetData> _presetCache = new Dictionary<string, AnimationScalePresetData>();
    private bool _isCacheConstructed = false;

    // private void Awake()
    // {
    //     constructPresetCache();
    // }

    public AnimationScalePresetData getPresetData(string targetName)
    {
        AnimationScalePresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(AnimationScalePresetData item in _presetData)
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

        foreach(AnimationScalePresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}
