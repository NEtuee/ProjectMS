using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AttackPreset", menuName = "Scriptable Object/Attack Preset", order = int.MaxValue)]
public class AttackPreset : ScriptableObject
{
    [SerializeField]private List<AttackPresetData> _presetData = new List<AttackPresetData>();
    private Dictionary<string, AttackPresetData> _presetCache = new Dictionary<string, AttackPresetData>();
    private bool _isCacheConstructed = false;


    public AttackPresetData getPresetData(string targetName)
    {
        AttackPresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(AttackPresetData item in _presetData)
            {
                if(item._name == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil.assert(target != null,"target presetData is not exists : {0}",targetName);
        return target;
    }


    private void constructPresetCache()
    {
        if(_isCacheConstructed == true)
            return;

        foreach(AttackPresetData item in _presetData)
        {
            _presetCache.Add(item._name, item);
        }

        _isCacheConstructed = true;
    }

}
