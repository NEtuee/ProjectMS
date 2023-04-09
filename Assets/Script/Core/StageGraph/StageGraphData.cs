using UnityEngine;
using System.Xml;

public abstract class StageGraphEventBase
{
    public abstract StageGraphEventType getStageGraphEventType();
    public abstract void Initialize();
    public abstract bool Execute(StageGraphManager graphManager, float deltaTime);
    public abstract void loadXml(XmlNode node);
}

public class StageGraphEvent_SpawnCharacter : StageGraphEventBase
{
    private string                      _characterKey;

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private string                      _uniqueEntityKey = "";

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.SpawnCharacter;
    
    public override void Initialize()
    {
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
    }

    public override bool Execute(StageGraphManager graphManager,float deltaTime)
    {
        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(_characterInfoData,_spawnDesc);

        if(createdCharacter == null)
            return true;

        if(_uniqueEntityKey != "")
            graphManager.addUniqueEntity(_uniqueEntityKey, createdCharacter);
        
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
            else if(attrName == "UniqueKey")
            {
                _uniqueEntityKey = attrValue;
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

    public override bool Execute(StageGraphManager graphManager,float deltaTime)
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

public class StageGraphEvent_SetCrossHair : StageGraphEventBase
{
    private string _uniqueKey = "";

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.SetCrossHair;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(StageGraphManager graphManager,float deltaTime)
    {
        GameEntityBase unqueEntity = graphManager.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
            return true;

        CrossHairUI._instance.setTarget(unqueEntity);
        CrossHairUI._instance.setActive(true);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class StageGraphEvent_SetAudioListner : StageGraphEventBase
{
    private string _uniqueKey = "";

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.SetAudioListner;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(StageGraphManager graphManager,float deltaTime)
    {
        GameEntityBase unqueEntity = graphManager.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
            return true;

        FMODAudioManager.Instance().setListener(unqueEntity.transform);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class StageGraphEvent_SetCameraTarget : StageGraphEventBase
{
    private string _uniqueKey = "";
    private CameraModeType _cameraMode = CameraModeType.Count;

    public override StageGraphEventType getStageGraphEventType() => StageGraphEventType.SetCameraTarget;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(StageGraphManager graphManager,float deltaTime)
    {
        CameraControlEx.Instance().setCameraTarget(graphManager.getUniqueEntity(_uniqueKey));
        CameraControlEx.Instance().initialize();

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "CameraMode")
                _cameraMode = (CameraModeType)System.Enum.Parse(typeof(CameraModeType), attrValue);
        }
    }
}

public enum StageGraphEventType
{
    SpawnCharacter,
    WaitSecond,
    SetCameraTarget,
    SetAudioListner,
    SetCrossHair,
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
