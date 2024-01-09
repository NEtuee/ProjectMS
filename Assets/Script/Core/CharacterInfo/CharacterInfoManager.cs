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
            DebugUtil.assert(false,"캐릭터 인포 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if(_characterInfoData.ContainsKey(key) == false)
        {
            DebugUtil.assert(false,"해당 캐릭터가 존재하지 않습니다. 오타는 아닌가요? [Key: {0}]", key);
            return null;
        }

        return _characterInfoData[key];

    }
}
