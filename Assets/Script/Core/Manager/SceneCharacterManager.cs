using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSearchDescription : MessageData
{
    public ObjectBase              _requester;
    public TargetSearchType        _searchType;
    public SearchIdentifier        _searchIdentifier;
    public float                   _searchRange;
}

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
    private List<TargetSearchDescription> _targetSearchRequestList = new List<TargetSearchDescription>();

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

        AddAction(MessageTitles.entity_searchNearest,(msg)=>{
            TargetSearchDescription desc = msg.data as TargetSearchDescription;
            _targetSearchRequestList.Add(desc);
        });
    }
    public override void initialize()
    {
        base.initialize();
    }

    public override void progress(float deltaTime)
    {
        processTargetSearch();
        base.progress(deltaTime);
    }


    public void processTargetSearch()
    {
        int requestCount = _targetSearchRequestList.Count;
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
                continue;

            for(int i = 0; i < requestCount; ++i)
            {
                float range = _targetSearchRequestList[i]._searchRange * _targetSearchRequestList[i]._searchRange;

                if(_targetSearchRequestList[i]._requester is CharacterEntityBase == false || receiver is CharacterEntityBase == false)
                {
                    DebugUtil.assert(false,"must be character entity, code error");
                    return;
                }
                else if(_targetSearchRequestList[i]._searchIdentifier == SearchIdentifier.Count)
                {
                    DebugUtil.assert(false,"invalid search identifier: Count");
                    return;
                }

                CharacterEntityBase requester = _targetSearchRequestList[i]._requester as CharacterEntityBase;
                CharacterEntityBase receiverCharacter = receiver as CharacterEntityBase;

                if(requester == receiverCharacter || _targetSearchRequestList[i]._searchIdentifier != receiverCharacter._searchIdentifier || receiverCharacter.isDead())
                    continue;

                GameEntityBase currentTarget = requester.getCurrentTargetEntity();
                if(currentTarget == null)
                {
                    requester.setTargetEntity(receiverCharacter);
                    continue;
                }
                
                float toCurrent = currentTarget.getDistanceSq(requester);
                float toNew = receiverCharacter.getDistanceSq(requester);
                if(toNew < range && toCurrent > toNew)
                    requester.setTargetEntity(receiverCharacter);
                    
            }
        }

        for(int i = 0; i < requestCount; ++i)
        {
            MessageDataPooling.ReturnData(_targetSearchRequestList[i]);
        }
        _targetSearchRequestList.Clear();
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
