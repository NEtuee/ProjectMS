using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightRandomManager : Singleton<WeightRandomManager>
{
    public WeightGroupDataList _weightGroupData;

    public float _randomWeightThisFrame = 0f;

    public void updateRandom()
    {
        _randomWeightThisFrame = Random.Range(0f,1f);
    }

    public bool getRandom(string group, string key)
    {
        if(_weightGroupData._weightGroupDataList.ContainsKey(group) == false)
        {
            DebugUtil.assert(false,"target weight group is not exists: {0}",group);
            return false;
        }
        
        return isWeightValid(_weightGroupData._weightGroupDataList[group],key);
    }

    private bool isWeightValid(WeightGroupData groupData, string key)
    {
        float weight = 0f;
        for(int i = 0; i < groupData._weightCount; ++i)
        {
            weight += groupData._weights[i]._weight;
            if(weight >= _randomWeightThisFrame)
                return groupData._weights[i]._key == key;
        }

        return false;
    }

    public void setWeightGroupData(WeightGroupDataList data)
    {
        _weightGroupData = data;
    }
}
