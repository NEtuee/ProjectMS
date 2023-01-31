using UnityEngine;
using System.Xml;

public abstract class StageGraphEventBase
{
    public abstract StageGraphEventType getStageGraphEventType();
    public abstract void Initialize();
    public abstract bool Execute(float deltaTime);
    public abstract void loadXml(XmlNode node);
}

public class StageGraphEvent_SpawnCharacter : StageGraphEventBase
{
    private string                      _characterKey;

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.SpawnCharacter;
    
    public override void Initialize()
    {
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
    }

    public override bool Execute(float deltaTime)
    {
        Message spawnMessage = MessagePool.GetMessage();
        SpawnCharacterOptionDescData messageData = MessageDataPooling.GetMessageData<SpawnCharacterOptionDescData>();

        messageData._characterInfoData = _characterInfoData;
        messageData._spawnCharacterOptionDesc = _spawnDesc;

        spawnMessage.Set(MessageTitles.entity_spawnCharacter,MessageReceiver.QueryUniqueID("SceneCharacterManager"),messageData,null);
        MasterManager.instance.HandleMessage(spawnMessage);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "CharacterKey")
            {
                _characterKey = attrValue;
            }
            else if(attrName == "Position")
            {
                _spawnDesc._position = XMLScriptConverter.valueToVector3(attrValue);
            }
            else if(attrName == "SearchIdentifier")
            {
                _spawnDesc._searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }

        }
    }
}

public class StageGraphEvent_WaitSecond : StageGraphEventBase
{
    private float   _waitTime = 0f;
    private float   _timer = 0f;

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.WaitSecond;
    
    public override void Initialize()
    {
        _timer = 0f;
    }

    public override bool Execute(float deltaTime)
    {
        _timer += deltaTime;
        return _waitTime <= _timer;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Time")
                _waitTime = float.Parse(attrValue);
        }
    }
}

public enum StageGraphEventType
{
    SpawnCharacter,
    WaitSecond,
    Count,
}

public enum StageGraphPhaseType
{
    Initialize = 0,
    Update,
    Count,
}

public class StageGraphPhaseData
{
    public StageGraphEventBase[]    _stageGraphEventList;
    public int                      _stageGraphEventCount;
}

public class StageGraphBaseData
{
    public string                   _stageName;

    public StageGraphPhaseData[]    _stageGraphPhase = new StageGraphPhaseData[(int)StageGraphPhaseType.Count];
    
}
