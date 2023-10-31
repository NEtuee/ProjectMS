using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectInfoManager : Singleton<EffectInfoManager>
{
    public Dictionary<string, EffectInfoDataBase> _effectInfoData;

    public void setEffectInfoData(Dictionary<string, EffectInfoDataBase> effectInfoData)
    {
        _effectInfoData = effectInfoData;
    }

    public EffectInfoDataBase getEffectInfoData(string key)
    {
        if(_effectInfoData == null)
        {
            DebugUtil.assert(false,"Effect Info 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if(_effectInfoData.ContainsKey(key) == false)
            return null;

        return _effectInfoData[key];
    }

    public EffectRequestData createRequestData(string key, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        EffectInfoDataBase info = getEffectInfoData(key);
        if(info == null)
            return null;

        return info.createEffectRequestData(executeEntity,targetEntity);
    }


}
