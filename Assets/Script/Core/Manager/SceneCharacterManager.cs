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

public struct SpawnCharacterOptionDesc
{
    public Vector3          _position;
    public Quaternion       _rotation;
    public SearchIdentifier _searchIdentifier;

    public static SpawnCharacterOptionDesc defaultValue = new SpawnCharacterOptionDesc{ _position = Vector3.zero, _rotation = Quaternion.identity, _searchIdentifier = SearchIdentifier.Count};
}

public class SpawnCharacterOptionDescData : MessageData
{
    public SpawnCharacterOptionDesc _spawnCharacterOptionDesc;
    public CharacterInfoData _characterInfoData;
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

    private Dictionary<int, string> _characterIDCacheMap = new Dictionary<int, string>();

    private Dictionary<int, CharacterEntityBase> _enableCharacterPoolCacheMap = new Dictionary<int, CharacterEntityBase>();
    private Queue<CharacterEntityBase> _disableCharacterPoolCacheMap = new Queue<CharacterEntityBase>();

    private List<TargetSearchDescription> _targetSearchRequestList = new List<TargetSearchDescription>();

    public override void RegisterReceiver(ObjectBase receiver)
    {
        base.RegisterReceiver(receiver);
        _characterIDCacheMap.Add(receiver.GetUniqueID(),receiver.gameObject.name);
    }

    public override void DeregisteReceiver(int target)
    {
        ObjectBase targetReciver = GetReciever(target);
        if(targetReciver != null && targetReciver is CharacterEntityBase)
        {
            CharacterEntityBase characterEntity = targetReciver as CharacterEntityBase;
            CollisionManager.Instance().deregisterObject(characterEntity.getCollisionInfo().getCollisionInfoData(),characterEntity);

            if(_enableCharacterPoolCacheMap.ContainsKey(target))
            {
                _disableCharacterPoolCacheMap.Enqueue(_enableCharacterPoolCacheMap[target]);
                _enableCharacterPoolCacheMap.Remove(target);

            }
        }

        _characterIDCacheMap.Remove(target);
        base.DeregisteReceiver(target);
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

        AddAction(MessageTitles.entity_spawnCharacter, (msg)=>{
            SpawnCharacterOptionDescData desc = MessageDataPooling.CastData<SpawnCharacterOptionDescData>(msg.data);
            createCharacterFromPool(desc._characterInfoData, desc._spawnCharacterOptionDesc);
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

    public CharacterEntityBase createCharacterFromPool(CharacterInfoData characterData, SpawnCharacterOptionDesc spawnDesc)
    {
        CharacterEntityBase characterEntity = null;
        if(_disableCharacterPoolCacheMap.Count != 0)
        {
            characterEntity = _disableCharacterPoolCacheMap.Dequeue();
            characterEntity.gameObject.SetActive(true);
        }
        else
        {
            GameObject characterObject = new GameObject(characterData._displayName);
            characterEntity = characterObject.AddComponent<CharacterEntityBase>();
        }

        _enableCharacterPoolCacheMap.Add(characterEntity.GetUniqueID(), characterEntity);

        characterEntity._searchIdentifier = spawnDesc._searchIdentifier;
        characterEntity.transform.position = spawnDesc._position;
        characterEntity.transform.rotation = spawnDesc._rotation;

        characterEntity.initializeCharacter(characterData);
        
        return characterEntity;
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

    private int FindCharacterKey(string targetName)
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
