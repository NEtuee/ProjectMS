using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfoManager : Singleton<CharacterInfoManager>
{
    public Dictionary<string, CharacterInfoData> _characterInfoData;

    public void SetCharacterInfo(Dictionary<string, CharacterInfoData> characterInfo)
    {
        _characterInfoData = characterInfo;
    }

    public CharacterInfoData GetCharacterInfoData(string key)
    {
        if(_characterInfoData == null)
        {
            DebugUtil.assert(false,"character info container is null");
            return null;
        }

        if(_characterInfoData.ContainsKey(key) == false)
            return null;

        return _characterInfoData[key];

    }
}
