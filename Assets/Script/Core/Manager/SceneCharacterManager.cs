using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCharacterManager : ManagerBase
{
    [System.Serializable]
    private struct CharacterKeyValue
    {
        public string name;
        public int uniqueID;
        public bool IsValid() {return name != null && name.Equals("") == false && uniqueID != 0;}
    }
    //[SerializeField] private List<CharacterKeyValue> sceneCharacterList = new List<CharacterKeyValue>();
    private Dictionary<int, string> _characterIDCacheMap = new Dictionary<int, string>();
    public override void RegisterReceiver(ObjectBase receiver)
    {
        base.RegisterReceiver(receiver);
        _characterIDCacheMap.Add(receiver.GetUniqueID(),receiver.gameObject.name);
    }
    public override void DeregisteReceiver(int target)
    {
        base.DeregisteReceiver(target);
        _characterIDCacheMap.Remove(target);
    }
    public override void assign()
    {
        base.assign();
        CacheUniqueID("SceneCharacterManager");
        RegisterRequest();
        SceneMaster.Instance().SetCharacterManager(this);
        //CreateCacheMap();
    }
    public override void initialize()
    {
        base.initialize();
    }
    public GameEntityBase GetCharacter(string targetName)
    {
        int key = FindCharacterKey(targetName);
        if(IsInReceivers(key) == false)
        {
            DebugUtil.assert(false,"target not found : {0}, {1}",targetName, key);
            return null;
        }
        return GetReciever(key) as GameEntityBase;
    }
    // private void CreateCacheMap()
    // {
    //     _characterIDCacheMap = new Dictionary<string, int>();
    //     foreach(var character in sceneCharacterList)
    //     {
    //         if(_characterIDCacheMap.ContainsKey(character.name) == true)
    //         {
    //             DebugUtil.assert(false,"duplicate characters found, name : {0}",character.name);
    //             return;
    //         }
            
    //         _characterIDCacheMap.Add(character.name,character.uniqueID);
    //     }
    // }
    private int FindCharacterKey(string targetName)
    {
        //CharacterKeyValue findKey = new CharacterKeyValue();
        // if(_characterIDCacheMap == null)
        // {
        //     findKey = sceneCharacterList.Find((x)=>{return x.name.Equals(targetName);});
        //     DebugUtil.assert(findKey.IsValid(),"attempt to find an invalid target [{0}]",targetName);
        //     return findKey.uniqueID;
        // }
        // else
        {
            foreach(var character in _characterIDCacheMap)
            {
                if(character.Value.CompareTo(targetName) == 0)
                {
                    return character.Key;
                }
            }
            DebugUtil.assert(false,"attempt to find an invalid target: {0}",targetName);
            return -1;
        }
        
    }
}
