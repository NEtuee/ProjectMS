using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllyInfoManager : Singleton<AllyInfoManager>
{
    public List<AllyInfoData> _allyInfoData;

    public void SetAllyInfo(Dictionary<string, AllyInfoData> allyInfo)
    {
        _allyInfoData = new List<AllyInfoData>();
        foreach(var item in allyInfo.Values)
        {
            _allyInfoData.Add(item);
        }
    }

    public static AllyTargetType compareAllyTargetType(ObjectBase entityOne, ObjectBase entityTwo)
    {
        return compareAllyTargetType(entityOne.getAllyInfo(), entityTwo.getAllyInfo());         
    }

    public AllyTargetType compareAllyTargetType(int keyOne, int keyTwo)
    {
        AllyInfoData one = GetAllyInfoData(keyOne);
        AllyInfoData two = GetAllyInfoData(keyTwo);

        return compareAllyTargetType(one, two);         
    }

    public static AllyTargetType compareAllyTargetType(AllyInfoData one, AllyInfoData two)
    {
        if(one == null || two == null)
            return AllyTargetType.Neutral;

        if(one._enemyGroup != null && one._enemyGroup.Contains(two._index))
            return AllyTargetType.Enemy;
        else if(one._neutralGroup != null && one._neutralGroup.Contains(two._index))
            return AllyTargetType.Neutral;
        else if(one == two || ( one._allyGroup != null && one._allyGroup.Contains(two._index)))
            return AllyTargetType.Ally;

        return AllyTargetType.Neutral;            
    }

    public AllyInfoData GetAllyInfoData(int key)
    {
        if(_allyInfoData == null)
        {
            DebugUtil.assert(false,"AllyInfo 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if(_allyInfoData.Count >= key || key < 0)
            return null;

        return _allyInfoData[key];
    }

    public AllyInfoData GetAllyInfoData(string key)
    {
        if(_allyInfoData == null)
        {
            DebugUtil.assert(false,"AllyInfo 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        foreach(var item in _allyInfoData)
        {
            if(item._key == key)
                return item;
        }

        DebugUtil.assert(false,"해당 Ally 정보가 존재하지 않습니다. 오타는 아닌가요? [Key: {0}]", key);
        return null;
    }
}