using UnityEngine;
using System.Xml;

public enum SequencerGraphEventType
{
    SpawnCharacter,
    WaitSecond,
    SetCameraTarget,
    SetAudioListner,
    SetCrossHair,
    SetHPSphere,
    WaitTargetDead,
    TeleportTargetTo,
    ApplyPostProcessProfile,
    SaveEventExecuteIndex,
    CallAIEvent,
    WaitSignal,
    SetCameraZoom,
    FadeIn,
    FadeOut,
    ForceQuit,
    BlockInput,

    Count,
}

public abstract class SequencerGraphEventBase
{
    public abstract SequencerGraphEventType getSequencerGraphEventType();
    public abstract void Initialize();
    public abstract bool Execute(SequencerGraphProcessor processor, float deltaTime);
    public virtual void Exit(SequencerGraphProcessor processor) {}
    public abstract void loadXml(XmlNode node);
}

public class SequencerGraphEvent_WaitSignal : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSignal;

    public string _targetSignal = "";

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return processor.checkSignal(_targetSignal);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Signal")
                _targetSignal = attrValue;
        }
    }
}


public class SequencerGraphEvent_CallAIEvent : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.CallAIEvent;

    public string _customAiEventName = "";
    public string _uniqueKey = "";

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ObjectBase executeTargetEntity = processor.getUniqueEntity(_uniqueKey);
        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return true;

        (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "EventName")
                _customAiEventName = attributes[i].Value;
            else if(attrName == "UniqueKey")
                _uniqueKey = attributes[i].Value;

        }
    }
}

public class SequencerGraphEvent_FadeIn : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeIn;

    private float _lambda = -1f;

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeIn(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = float.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_BlockInput : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockInput;

    private bool _value = false;

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase playerEntity = StageProcessor.Instance().getPlayerEntity();
        if(playerEntity == null)
        {
            DebugUtil.assert(false,"플레이어가 존재하지 않습니다.");
            return true;
        }

        playerEntity.blockInput(_value);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Value")
                _value = bool.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_ForceQuit : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ForceQuit;

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {

    }
}

public class SequencerGraphEvent_FadeOut : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeOut;

    private float _lambda = -1f;

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeOut(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = float.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_SetCameraZoom : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraZoom;

    private float _zoom = -1f;

    public override void Initialize()
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_zoom <= 0f)
            CameraControlEx.Instance().setDefaultZoomSize();
        else
            CameraControlEx.Instance().setZoomSize(_zoom);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Size")
                _zoom = float.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_SpawnCharacter : SequencerGraphEventBase
{
    private string                      _characterKey;

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private string                      _uniqueEntityKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SpawnCharacter;
    
    public override void Initialize()
    {
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(_characterInfoData,_spawnDesc);

        if(createdCharacter == null)
            return true;

        if(_uniqueEntityKey != "")
            processor.addUniqueEntity(_uniqueEntityKey, createdCharacter);
        
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

public class SequencerGraphEvent_WaitSecond : SequencerGraphEventBase
{
    private float   _waitTime = 0f;
    private float   _timer = 0f;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSecond;
    
    public override void Initialize()
    {
        _timer = 0f;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
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

public class SequencerGraphEvent_SetHPSphere : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHPSphere;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"unique entity key is not Exists : {0}",_uniqueKey);
            return true;
        }

        HPSphereUIManager.Instance().release();
        HPSphereUIManager.Instance().initialize(uniqueEntity);
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

public class SequencerGraphEvent_SetCrossHair : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCrossHair;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"unique entity key is not Exists : {0}",_uniqueKey);
            return true;
        }

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

public class SequencerGraphEvent_WaitTargetDead : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitTargetDead;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
            return true;
        
        return unqueEntity.isDead();
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

public class SequencerGraphEvent_SaveEventExecuteIndex : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SaveEventExecuteIndex;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
}

public class SequencerGraphEvent_ApplyPostProcessProfile : SequencerGraphEventBase
{
    private string _path = "";
    private float _blendTime = 0f;
    private PostProcessProfileApplyType _applyType = PostProcessProfileApplyType.BaseBlend;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ApplyPostProcessProfile;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject(_path);
        if(profile == null || (profile is PostProcessProfile) == false)
            return true;

        switch(_applyType)
        {
            case PostProcessProfileApplyType.BaseBlend:
                CameraControlEx.Instance().getPostProcessProfileControl().addBaseBlendProfile(profile as PostProcessProfile,_blendTime);
            break;
            case PostProcessProfileApplyType.Additional:
                CameraControlEx.Instance().getPostProcessProfileControl().setAdditionalEffectProfile(profile as PostProcessProfile,_blendTime);
            break;
        }
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
                _path = attrValue;
            else if(attrName == "BlendTime")
                _blendTime = float.Parse(attrValue);
            else if(attrName == "ApplyType")
                _applyType = (PostProcessProfileApplyType)System.Enum.Parse(typeof(PostProcessProfileApplyType), attrValue);
        }
    }
}

public class SequencerGraphEvent_TeleportTargetTo : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private Vector3 _targetPosition;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TeleportTargetTo;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"unique entity key is not Exists : {0}",_uniqueKey);
            return true;
        }
        
        unqueEntity.updatePosition(_targetPosition);
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
            else if(attrName == "Position")
                _targetPosition = XMLScriptConverter.valueToVector3(attrValue);
        }
    }
}

public class SequencerGraphEvent_SetAudioListner : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetAudioListner;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"unique entity key is not Exists : {0}",_uniqueKey);
            return true;
        }

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

public class SequencerGraphEvent_SetCameraTarget : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private CameraModeType _cameraMode = CameraModeType.Count;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraTarget;
    
    public override void Initialize()
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setCameraTarget(processor.getUniqueEntity(_uniqueKey));
        if(_cameraMode != CameraModeType.Count)
            CameraControlEx.Instance().setCameraMode(_cameraMode);

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

public enum SequencerGraphPhaseType
{
    Initialize = 0,
    Update,
    End,
    Count,
}

public class SequencerGraphPhaseData
{
    public SequencerGraphEventBase[]    _sequencerGraphEventList;
    public int                          _sequencerGraphEventCount;
}

public class SequencerGraphBaseData
{
    public string                       _sequencerName;

    public SequencerGraphPhaseData[]    _sequencerGraphPhase = new SequencerGraphPhaseData[(int)SequencerGraphPhaseType.Count];
    
}
