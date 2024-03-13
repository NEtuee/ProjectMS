using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;

public class EffectInfoManager : Singleton<EffectInfoManager>
{
    public Dictionary<string, EffectInfoDataBase[]> _effectInfoData;

    public void setEffectInfoData(Dictionary<string, EffectInfoDataBase[]> effectInfoData)
    {
        _effectInfoData = effectInfoData;
    }

    public EffectInfoDataBase[] getEffectInfoData(string key)
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

    public EffectItemBase requestEffect(string effectInfoKey, Vector3 position)
    {
        EffectRequestData requestData = createRequestData(effectInfoKey,null,null,CommonMaterial.Empty, CommonMaterial.Empty);
        if(requestData == null)
            return null;

        requestData._position += position;

        EffectItemBase itemBase = EffectManager._instance.createEffect(requestData);
        requestData.isUsing = true;

        return itemBase;
    }

    public EffectItemBase requestEffect(string effectInfoKey, ObjectBase executeEntity, ObjectBase targetEntity, CommonMaterial attackMaterial)
    {
        CommonMaterial defenceMaterial = CommonMaterial.Empty;
        if(targetEntity != null && targetEntity is GameEntityBase)
            defenceMaterial = (targetEntity as GameEntityBase).getCharacterMaterial();
        
        EffectRequestData requestData = createRequestData(effectInfoKey,executeEntity,targetEntity,attackMaterial, defenceMaterial);
        if(requestData == null)
            return null;

        EffectItemBase itemBase = EffectManager._instance.createEffect(requestData);
        requestData.isUsing = true;

        return itemBase;
    }

    public EffectRequestData createRequestData(string key, ObjectBase executeEntity, ObjectBase targetEntity, CommonMaterial attackMaterial, CommonMaterial defenceMaterial)
    {
        EffectInfoDataBase[] infoArray = getEffectInfoData(key);
        if(infoArray == null)
            return null;

        foreach(var item in infoArray)
        {
            if(item.compareMaterial(attackMaterial, defenceMaterial))
                return item.createEffectRequestData(executeEntity,targetEntity);
        }

        return null;
    }


}
