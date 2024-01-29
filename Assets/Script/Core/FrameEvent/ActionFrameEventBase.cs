using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Drawing;
using UnityEngine.UIElements;
using System;

public enum FrameEventType
{
    FrameEvent_Test = 0,
    FrameEvent_Attack,
    FrameEvent_ApplyBuff,
    FrameEvent_ApplyBuffTarget,
    FrameEvent_DeleteBuff,
    FrameEvent_TeleportToTarget,
    FrameEvent_TeleportToTargetBack,
    FrameEvent_SetDefenceType,
    FrameEvent_Effect,
    FrameEvent_TimelineEffect,
    FrameEvent_ParticleEffect,
    FrameEvent_AnimationEffect,
    FrameEvent_SetFrameTag,
    FrameEvent_Projectile,
    FrameEvent_Danmaku,
    FrameEvent_SetAnimationSpeed,
    FrameEvent_SetCameraZoom,
    FrameEvent_SetCameraDelay,
    FrameEvent_KillEntity,
    FrameEvent_Movement,
    FrameEvent_ZoomEffect,
    FrameEvent_ShakeEffect,
    FrameEvent_StopUpdate,
    FrameEvent_SetTimeScale,
    FrameEvent_SpawnCharacter,
    FrameEvent_ReleaseCatch,
    FrameEvent_TalkBalloon,
    FrameEvent_DeactiveTalkBalloon,
    FrameEvent_SetAction,
    FrameEvent_CallAIEvent,
    FrameEvent_AudioPlay,
    FrameEvent_PlaySequencer,
    FrameEvent_SequencerSignal,
    FrameEvent_ApplyPostProcessProfile,
    FrameEvent_SetDirectionType,
    FrameEvent_Torque,
    FrameEvent_EffectPreset,
    FrameEvent_SetRotateSlotValue,
    FrameEvent_FollowAttack,

    Count,
}

public enum ChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnGuard,
    ChildFrameEvent_OnParry,
    ChildFrameEvent_OnEvade,
    ChildFrameEvent_OnGuardBreak,
    ChildFrameEvent_OnGuardBreakFail,
    ChildFrameEvent_OnCatch,
    ChildFrameEvent_OnKill,

    ChildFrameEvent_OnBegin,
    ChildFrameEvent_OnEnter,
    ChildFrameEvent_OnExit,
    ChildFrameEvent_OnEnd,

    Count,
}

public enum SetTargetType
{
    SetTargetType_Self,
    SetTargetType_Target,
    SetTargetType_AITarget,
}

public class ChildFrameEventItem
{
    public ActionFrameEventBase[] _childFrameEvents;
    public int _childFrameEventCount;

}

public abstract class ActionFrameEventBase
{
    public float                                _startFrame;
    public float                                _endFrame;

    public bool                                 _isTimeBase = true;

    public ActionGraphConditionCompareData      _conditionCompareData = null;

    public Dictionary<ChildFrameEventType, ChildFrameEventItem> _childFrameEventItems = null;
    
    public abstract FrameEventType getFrameEventType();
    public virtual void initialize(ObjectBase executeEntity){}
    public abstract bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public virtual void onExit(ObjectBase executeEntity, bool isForceEnd){}
    public abstract void loadFromXML(XmlNode node);

    public virtual void initializeFromAttack(CommonMaterial attackMaterial) {}

    public bool checkCondition(GameEntityBase targetEntity)
    {
        if(_conditionCompareData == null)
            return true;

        return targetEntity.processActionCondition(_conditionCompareData);
    }
}

public class ActionFrameEvent_CallAIEvent : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_CallAIEvent;}

    public string _customAiEventName = "";
    public CallAIEventTargetType _eventTargetType = CallAIEventTargetType.Self;

    public List<CharacterEntityBase> _rangeSearchEntityList = new List<CharacterEntityBase>();
    public SearchIdentifier _searchIdentifier = SearchIdentifier.Count;

    public float _range = 0f;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        ObjectBase executeTargetEntity = null;
        switch(_eventTargetType)
        {
            case CallAIEventTargetType.Self:
            {
                executeTargetEntity = executeEntity;
            }
            break;
            case CallAIEventTargetType.FrameEventTarget:
            {
                executeTargetEntity = targetEntity;
            }
            break;
            case CallAIEventTargetType.Summoner:
            {
                executeTargetEntity = executeEntity.getSummonObject();
            }
            break;
            case CallAIEventTargetType.Range:
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                _rangeSearchEntityList.Clear();
                sceneCharacterManager.targetSearchRange(executeEntity.transform.position,_range,_searchIdentifier,ref _rangeSearchEntityList);

                foreach(var item in _rangeSearchEntityList)
                {
                    if(item == null || item is GameEntityBase == false)
                        continue;
                        
                    (item as GameEntityBase).executeCustomAIEvent(_customAiEventName);
                }
            }
            return true;
            case CallAIEventTargetType.SummonTargetRange:
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                _rangeSearchEntityList.Clear();
                sceneCharacterManager.targetSearchRange(executeEntity.transform.position,_range,_searchIdentifier,ref _rangeSearchEntityList);

                foreach(var item in _rangeSearchEntityList)
                {
                    if(item == null || item is GameEntityBase == false)
                        continue;
                    
                    if(item.getSummonObject() != executeEntity)
                        continue;

                    (item as GameEntityBase).executeCustomAIEvent(_customAiEventName);
                }
            }
            return true;
        }

        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return true;

        (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "EventName")
            {
                _customAiEventName = attributes[i].Value;
            }
            else if(attrName == "EventTargetType")
            {
                _eventTargetType = (CallAIEventTargetType)System.Enum.Parse(typeof(CallAIEventTargetType), attrValue);
            }
            else if(attrName == "Range")
            {
                _range = float.Parse(attrValue);
            }
            else if(attrName == "SearchIdentifier")
            {
                _searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }
        }

        if(_eventTargetType == CallAIEventTargetType.Range || _eventTargetType == CallAIEventTargetType.SummonTargetRange)
        {
            DebugUtil.assert_fileOpen(_range != 0f, "EventTargetType이 [{0}] 인데 Range가 0 입니다. Range를 입력해 주세요.",node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node), _eventTargetType.ToString());
        }
    }
}

public class ActionFrameEvent_SetDirectionType : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetDirectionType;}
    public DirectionType _directionType = DirectionType.Count;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;
        
        (executeEntity as GameEntityBase).setDirectionType(_directionType);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "DirectionType")
                _directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attrValue);
        }
    }
}

public class ActionFrameEvent_ApplyPostProcessProfile : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyPostProcessProfile;}

    private string _path = "";
    private float _blendTime = 0f;
    private PostProcessProfileApplyType _applyType = PostProcessProfileApplyType.BaseBlend;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        UnityEngine.ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject(_path);
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

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
                _path = attrValue;
            else if(attrName == "BlendTime")
                _blendTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "ApplyType")
                _applyType = (PostProcessProfileApplyType)System.Enum.Parse(typeof(PostProcessProfileApplyType), attrValue);
        }
    }
}

public class ActionFrameEvent_SequencerSignal : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SequencerSignal;}

    public string _signal = "";
    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        GameEntityBase targetGameEntity = targetEntity as GameEntityBase;
        (executeEntity as GameEntityBase).addSequencerSignal(_signal);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Signal")
                _signal = attrValue;
        }
    }
}

public class ActionFrameEvent_PlaySequencer : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_PlaySequencer;}

    public string _sequencerPath = "";
    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        GameEntityBase targetGameEntity = targetEntity as GameEntityBase;
        (executeEntity as GameEntityBase).startSequencer(_sequencerPath, targetGameEntity,true);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Path")
                _sequencerPath = attributes[i].Value;
        }
    }
}

public class ActionFrameEvent_AudioPlay : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_AudioPlay;}

    public int _audioID = 0;
    public bool _toTarget = false;
    public bool _attach = false;
    public bool _useFlip = false;

    public int[] _parameterID = null;
    public float[] _parameterValue = null;

    public UnityEngine.Vector3 _spawnOffset = UnityEngine.Vector3.zero;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        UnityEngine.Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        UnityEngine.Vector3 offset = _spawnOffset;
        if(_useFlip && executeEntity is CharacterEntityBase)
        {
            float angle = MathEx.directionToAngle(executeEntity.getDirection());
            if(_useFlip && (executeEntity as CharacterEntityBase).getFlipState().xFlip)
                offset.y *= -1f;

            offset = UnityEngine.Quaternion.Euler(0f,0f,angle) * offset;
        }

        FMODUnity.StudioEventEmitter eventEmitter = null;

        if(_attach)
            eventEmitter = FMODAudioManager.Instance().Play(_audioID,offset,_toTarget ? targetEntity.transform : executeEntity.transform);
        else
            eventEmitter = FMODAudioManager.Instance().Play(_audioID, centerPosition + offset);

        if(eventEmitter != null && isValidParameter())
            FMODAudioManager.Instance().setParam(ref eventEmitter,_audioID,_parameterID,_parameterValue);

        if(executeEntity is GameEntityBase)
        {
            GameEntityBase entityBase = (executeEntity as GameEntityBase);
            if(entityBase._soundDebug || GameEditorMaster._instance._soundDebugAll)
                entityBase.debugTextManager.updateDebugText("Sound: " + _audioID,"AudioEvent: " + _audioID.ToString(),1f,UnityEngine.Color.white);
        }

        return true;
    }

    public bool isValidParameter()
    {
        return _parameterID != null && _parameterValue != null && _parameterID.Length == _parameterValue.Length;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "ID")
            {
                _audioID = int.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UseFlip")
            {
                _useFlip = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0}",attributes[i].Value);
                    return;
                }

                _spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                _spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                _spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attributes[i].Name == "ParameterID")
            {
                string[] paramIDArray = attributes[i].Value.Split(' ');
                List<int> idList = new List<int>();
                foreach(var item in paramIDArray)
                {
                    idList.Add(int.Parse(item));
                }

                _parameterID = idList.ToArray();
            }
            else if(attributes[i].Name == "ParameterValue")
            {
                string[] paramValueArray = attributes[i].Value.Split(' ');
                List<float> valueList = new List<float>();
                foreach(var item in paramValueArray)
                {
                    valueList.Add(float.Parse(item));
                }

                _parameterValue = valueList.ToArray();
            }
        }
    }
}

public class ActionFrameEvent_SetAction : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetAction;}

    private string _actionName = "";

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        ((GameEntityBase)executeEntity).setAction(_actionName);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Action")
            {
                _actionName = attributes[i].Value;
            }

        }
    }
}


public class ActionFrameEvent_TalkBalloon : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TalkBalloon;}

    private string _key = "";

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        SubtitleData_SimpleTalk simpleTalkData = SubtitleManager.Instance().getSimpleTalkData(_key);
        if(simpleTalkData == null)
        {
            DebugUtil.assert(false,"존재하지 않는 SimpleTalk Key 입니다. 데이터를 확인 해 주세요. [Key: {0}]",_key);
            return true;
        }

        TalkBalloonManager.Instance().activeTalkBalloon(executeEntity.transform,new UnityEngine.Vector3(0f, (executeEntity as GameEntityBase).getHeadUpOffset(),0f),simpleTalkData._text,simpleTalkData._time);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "SimpleTalkKey")
                _key = attributes[i].Value;
        }

        if(_key == "")
            DebugUtil.assert(false,"SimpleTalk Key가 존재하지 않습니다. 이거 필수임");
    }
}

public class ActionFrameEvent_DeactiveTalkBalloon : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_DeactiveTalkBalloon;}

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        TalkBalloonManager.Instance().deactiveTalkBalloon(executeEntity.transform);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
    }
}

public class ActionFrameEvent_ReleaseCatch : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ReleaseCatch;}

    private UnityEngine.Vector3         _pushVector;
    private string                      _catchTargetAction = "";

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        ObjectBase parentObject = executeEntity.hasChildObject() ? executeEntity : (executeEntity.hasParentObject() ? executeEntity.getParentObject() : null);
        if(parentObject == null)
            return true;

        ObjectBase childObject = parentObject.getChildObject();
        ObjectBase pushTarget = executeEntity == parentObject ? childObject : parentObject;

        if(pushTarget is GameEntityBase)
        {
            GameEntityBase target = (pushTarget as GameEntityBase);

            if(_catchTargetAction != "")
                target.setAction(_catchTargetAction);
            
            if(_pushVector.sqrMagnitude > float.Epsilon)
            {
                UnityEngine.Vector3 attackPointDirection = executeEntity.getDirection();
                target.setVelocity(UnityEngine.Quaternion.Euler(0f,0f,UnityEngine.Mathf.Atan2(attackPointDirection.y,attackPointDirection.x) * UnityEngine.Mathf.Rad2Deg) * _pushVector);
            }
        }

        parentObject.detachChildObject();

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Push")
            {
                _pushVector = XMLScriptConverter.valueToVector3(attributes[i].Value);
            }
            else if(attrName == "TargetAction")
            {
                _catchTargetAction = attrValue;
            }
        }
    }
}

public class ActionFrameEvent_SpawnCharacter : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SpawnCharacter;}

    private string                      _characterKey;
    private string                      _startAINode = "";

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private UnityEngine.Vector3         _spawnOffset;

    private bool                        _inherit = false;
    private bool                        _inheritDirection = false;
    private bool                        _useFlip = false;

    public override void initialize(ObjectBase executeEntity)
    {
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;

        UnityEngine.Vector3 offset = _spawnOffset;
        if(_useFlip && executeEntity is CharacterEntityBase)
        {
            float angle = MathEx.directionToAngle(executeEntity.getDirection());
            if(_useFlip && (executeEntity as CharacterEntityBase).getFlipState().xFlip)
                offset.y *= -1f;

            offset = UnityEngine.Quaternion.Euler(0f,0f,angle) * offset;
        }
        
        _spawnDesc._position = executeEntity.transform.position + offset;
        if(_inheritDirection)
            _spawnDesc._direction = executeEntity.getDirection();
            
        CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(_characterInfoData,_spawnDesc);
        createdCharacter.setSummonObject(_inherit ? executeEntity.getSummonObject() : executeEntity);
        if(_startAINode != "")
            createdCharacter.setAINode(_startAINode);

        return true;
    }

    public override void loadFromXML(XmlNode node)
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
            else if(attrName == "Offset")
            {
                _spawnOffset = XMLScriptConverter.valueToVector3(attrValue);
            }
            else if(attrName == "SearchIdentifier")
            {
                _spawnDesc._searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }
            else if(attrName == "Inherit")
            {
                _inherit = bool.Parse(attrValue);
            }
            else if(attrName == "InheritDirection")
            {
                _inheritDirection = bool.Parse(attrValue);
            }
            else if(attrName == "UseFlip")
            {
                _useFlip = bool.Parse(attrValue);
            }
            else if(attrName == "AINode")
            {
                _startAINode = attrValue;
            }
        }
    }
}

public class ActionFrameEvent_StopUpdate : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_StopUpdate;}

    public float _stopTime = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        FloatData data = MessageDataPooling.GetMessageData<FloatData>();
        data.value = _stopTime;

        Message msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.entity_stopUpdate,0,data,null);
        MasterManager.instance.HandleMessage(msg);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Time")
                _stopTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_SetTimeScale : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetTimeScale;}

    public float _targetTimeScale = 0f;
    public float _timeScalingTime = 0f;
    public float _timeScaleBlendTime = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3Data data = MessageDataPooling.GetMessageData<Vector3Data>();
        data.value = new UnityEngine.Vector3(_targetTimeScale, _timeScalingTime, _timeScaleBlendTime);

        Message msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.entity_setTimeScale,0,data,null);
        MasterManager.instance.HandleMessage(msg);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _targetTimeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _timeScalingTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "BlendTime")
                _timeScaleBlendTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_ZoomEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ZoomEffect;}

    public float _zoomScale = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        CameraControlEx.Instance().Zoom(_zoomScale);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _zoomScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_ShakeEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ShakeEffect;}

    public float _shakeTime = 0f;
    public float _shakeScale = 0f;
    public float _shakeSpeed = 1f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        CameraControlEx.Instance().setShake(_shakeScale, _shakeSpeed, _shakeTime);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _shakeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _shakeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _shakeSpeed = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_FollowAttack : ActionFrameEventBase
{
    public enum FollowType
    {
        Attach,
        Movement,
        Count,
    }

    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_FollowAttack;}

    public FollowType      _followType = FollowType.Count;
    
    public string          _characterKey = "";

    public bool            _toTarget = false;

    public float           _radius = 0f;
    public float           _moveTime = 0f;

    public SearchIdentifier _searchIdentifier = SearchIdentifier.Enemy;

    public override void initialize(ObjectBase executeEntity)
    {
        base.initialize(executeEntity);
        executeEntity._attackProcessorManager.initializeAttack(this);
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity._attackProcessorManager.executeAttack(this,executeEntity,targetEntity);
        return true;
    }

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        base.onExit(executeEntity,isForceEnd);
        executeEntity._attackProcessorManager.exitAttack(this,isForceEnd);
    }
    

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "FollowType")
            {
                _followType = (FollowType)System.Enum.Parse(typeof(FollowType), attributes[i].Value);
            }
            else if(attrName == "Radius")
            {
                _radius = float.Parse(attrValue);
            }
            else if(attrName == "MoveTime")
            {
                _moveTime = float.Parse(attrValue);
            }
            else if(attrName == "CharacterKey")
            {
                _characterKey = attrValue;
            }
            else if(attrName == "ToTarget")
            {
                _toTarget = bool.Parse(attrValue);
            }
        }
    }

}

public class ActionFrameEvent_SetRotateSlotValue : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetRotateSlotValue;}

    float _radius = 0f;
    float _speed = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase)
            (executeEntity as GameEntityBase).setRotateSlotValue(_speed, _radius);
            
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Radius")
                _radius = float.Parse(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _speed = float.Parse(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_EffectPreset : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_EffectPreset;}

    private string _effectInfoKey = "";
    private CommonMaterial _attackMaterial;
    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        EffectInfoManager.Instance().requestEffect(_effectInfoKey,executeEntity, targetEntity,_attackMaterial);
        return true;
    }

    public override void initializeFromAttack(CommonMaterial attackMaterial)
    {
        base.initializeFromAttack(attackMaterial);
        _attackMaterial = attackMaterial;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Key")
                _effectInfoKey = attributes[i].Value;
        }
    }
}

public class ActionFrameEvent_Torque : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Torque;}

    private enum TorqueModifyType
    {
        Set,
        Add,
        Count,
    };

    private TorqueModifyType _torqueModifyType = TorqueModifyType.Count;
    private FloatEx _value = new FloatEx();
    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;

        GameEntityBase entity = (GameEntityBase)executeEntity;
        switch(_torqueModifyType)
        {
            case TorqueModifyType.Set:
            entity.setTorque(_value);
            break;
            case TorqueModifyType.Add:
            entity.addTorque(_value);
            break;
            case TorqueModifyType.Count:
            DebugUtil.assert(false, "잘못된 토크 모디파이 타입");
            break;
        }
        
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Add")
            {
                _value.loadFromXML(attributes[i].Value);
                _torqueModifyType = TorqueModifyType.Add;
            }
            if(attributes[i].Name == "Set")
            {
                _value.loadFromXML(attributes[i].Value);
                _torqueModifyType = TorqueModifyType.Set;
            }
        }
    }
}

public class ActionFrameEvent_Movement : ActionFrameEventBase
{
    struct MovementSetValueType
    {
        public FloatEx _value;
        public int _targetValue;

        public MovementSetValueType(string valueString, int targetValue)
        {
            _value = new FloatEx();
            _value.loadFromXML(valueString);

            _targetValue = targetValue;
        }

        public MovementSetValueType(float value, int targetValue)
        {
            _value = new FloatEx();
            _value.setValue(value);

            _targetValue = targetValue;
        }
    };

    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Movement;}

    private MovementSetValueType[] _setValueList = null;
    private int _valueListCount = 0;
    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;

        MovementBase currentMovement = ((GameEntityBase)executeEntity).getCurrentMovement();
        if(currentMovement == null)
            return false;
        
        if(currentMovement.getMovementType() != MovementBase.MovementType.FrameEvent)
        {
            DebugUtil.assert(false,"movement frame event is only can use, when movement type is frameEvent movement : currentType[{0}] Action[{1}] FullPath[{2}]", currentMovement.getMovementType().ToString(), ((GameEntityBase)executeEntity).getCurrentActionName(),((GameEntityBase)executeEntity).actionGraphPath);
            return false;
        }

        UnityEngine.Vector3 direction = executeEntity.getDirection();
        setMovementValue((FrameEventMovement)currentMovement, direction);
        
        return true;
    }

    public void setMovementValue(FrameEventMovement movement, UnityEngine.Vector3 direction)
    {
        for(int i = 0; i < _valueListCount; ++i)
        {
            movement.setMovementValue(direction,_setValueList[i]._value,_setValueList[i]._targetValue);
        }
    }

    public override void loadFromXML(XmlNode node)
    {
        List<MovementSetValueType> movementSetValueList = new List<MovementSetValueType>();
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName != "Speed" && attrName != "Velocity" && attrName != "MaxVelocity" && attrName != "Friction")
                continue;

            int targetValue = (int)((FrameEventMovement.FrameEventMovementValueType)System.Enum.Parse(typeof(FrameEventMovement.FrameEventMovementValueType), attrName));
            MovementSetValueType movementSetValueType = new MovementSetValueType(attrValue, targetValue);
            movementSetValueList.Add(movementSetValueType);
        }

        _setValueList = movementSetValueList.ToArray();
        _valueListCount = movementSetValueList.Count;
    }
}

public class ActionFrameEvent_KillEntity : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_KillEntity;}

    private string _path;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity.deactive();
        executeEntity.DeregisterRequest();

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
    }
}


public class ActionFrameEvent_Danmaku : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Danmaku;}

    private string _path;
    private UnityEngine.Vector3 _offsetPosition = UnityEngine.Vector3.zero;
    private bool _useFlip = false;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase )
        {
            GameEntityBase requester = (GameEntityBase)executeEntity;
            requester.addDanmaku(_path,_offsetPosition,_useFlip);
        }
        else if(executeEntity is ProjectileEntityBase)
        {
            DanmakuManager.Instance().addDanmaku(_path,executeEntity.transform.position,_offsetPosition,_useFlip,executeEntity._searchIdentifier);
        }
        

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
                _path = attributes[i].Value;
            else if(attributes[i].Name == "Offset")
                _offsetPosition = XMLScriptConverter.valueToVector3(attributes[i].Value);
            else if(attributes[i].Name == "UseFlip")
                _useFlip = bool.Parse(attributes[i].Value);
        }
    }
}


public class ActionFrameEvent_SetFrameTag : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetFrameTag;}

    private string _frameTag;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        
        return requester.applyFrameTag(_frameTag);
    }

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        
        requester.deleteFrameTag(_frameTag);

    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Tag")
                _frameTag = attributes[i].Value;
        }
    }
}

public class ActionFrameEvent_SetCameraZoom : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetCameraZoom;}

    private float _zoom = -1f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(_zoom <= 0f)
            CameraControlEx.Instance().setDefaultZoomSize();
        else
            CameraControlEx.Instance().setZoomSize(_zoom,4f);
        return true;
    }

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        CameraControl.Instance().setDelay(false);

    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Size")
                _zoom = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_SetCameraDelay : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetCameraDelay;}

    private float _time;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        CameraControl.Instance().setDelay(true);
        return true;
    }

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        CameraControl.Instance().setDelay(false);

    }

    public override void loadFromXML(XmlNode node)
    {

    }
}



public class ActionFrameEvent_SetDefenceType : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetDefenceType;}

    private DefenceType _defenceType;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        
        requester.setDefenceType(_defenceType);
        
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "DefenceType")
                _defenceType = (DefenceType)System.Enum.Parse(typeof(DefenceType), attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_SetAnimationSpeed : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetAnimationSpeed;}

    private float _speed = 1f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        requester.setAnimationSpeed(_speed);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Speed")
                _speed = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_TeleportToTarget : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TeleportToTarget;}

    private TeleportTarget _teleportTarget = TeleportTarget.ActionTarget;

    private float _distanceOffset = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        GameEntityBase target = null;
        
        switch(_teleportTarget)
        {
            case TeleportTarget.ActionTarget:
                target = targetEntity is GameEntityBase ? (GameEntityBase)targetEntity : null;
            break;
            case TeleportTarget.AITarget:
                target = requester.getCurrentTargetEntity();
            break;
            case TeleportTarget.Summoner:
                target = (GameEntityBase)requester.getSummonObject();
            break;
        }
        
        if(target == null)
            return true;

        UnityEngine.Vector3 direction = (requester.transform.position - target.transform.position).normalized;

        requester.updatePosition(target.transform.position + direction * (requester.getCollisionInfo().getRadius() + target.getCollisionInfo().getRadius() + _distanceOffset));
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "DistanceOffset")
                _distanceOffset = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "TeleportTarget")
                _teleportTarget = (TeleportTarget)Enum.Parse(typeof(TeleportTarget), attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_TeleportToTargetBack : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TeleportToTargetBack;}

    private TeleportTarget _teleportTarget = TeleportTarget.ActionTarget;

    private float _distanceOffset = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
         GameEntityBase target = null;
        
        switch(_teleportTarget)
        {
            case TeleportTarget.ActionTarget:
                target = targetEntity is GameEntityBase ? (GameEntityBase)targetEntity : null;
            break;
            case TeleportTarget.AITarget:
                target = requester.getCurrentTargetEntity();
            break;
            case TeleportTarget.Summoner:
                target = (GameEntityBase)requester.getSummonObject();
            break;
        }

        if(target == null)
            return true;

        UnityEngine.Vector3 direction = requester.getDirection();
        UnityEngine.Vector3 perpendicular = MathEx.getPerpendicularPointOnLine(executeEntity.transform.position,executeEntity.transform.position + direction * 9999f, target.transform.position);
        
        float length = (requester.getCollisionInfo().getRadius() + target.getCollisionInfo().getRadius() + _distanceOffset);
        requester.updatePosition(perpendicular + direction * length);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "DistanceOffset")
                _distanceOffset = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "TeleportTarget")
                _teleportTarget = (TeleportTarget)Enum.Parse(typeof(TeleportTarget), attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_DeleteBuff : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_DeleteBuff;}

    private int[] buffKeyList = null;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        ((GameEntityBase)executeEntity).deleteActionBuffList(buffKeyList);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "BuffList")
            {
                string[] buffList = attributes[i].Value.Split(' ');

                buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0}", buffList[j]);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

            }
        }
    }
}

public class ActionFrameEvent_ApplyBuff : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyBuff;}

    private int[] buffKeyList = null;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return false;
        
        ((GameEntityBase)executeEntity).applyActionBuffList(buffKeyList);
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "ApplyBuff")
            {
                string[] buffList = attributes[i].Value.Split(' ');

                buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0}", buffList[j]);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

            }
        }
    }
}

public class ActionFrameEvent_ApplyBuffTarget : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyBuffTarget;}

    private int[] buffKeyList = null;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(targetEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase target = (GameEntityBase)targetEntity;

        target.applyActionBuffList(buffKeyList);
        if(executeEntity is GameEntityBase && target.predictionDead())
            (executeEntity as GameEntityBase).setActionCondition_Bool(ConditionNodeUpdateType.Entity_Kill, true);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "ApplyBuff")
            {
                string[] buffList = attributes[i].Value.Split(' ');

                buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0}", buffList[j]);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

            }
        }
    }
}

public class ActionFrameEvent_Attack : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Attack;}

    public CollisionInfo           _collisionInfo;
    
    
    public DefenceType[]           _ignoreDefenceType = null;
    public DirectionType           _targetDirectionType = DirectionType.Count;

    public AttackType              _attackType;
    public UnityEngine.Vector3     _pushVector = UnityEngine.Vector3.zero;
    public UnityEngine.Vector3     _catchOffset = UnityEngine.Vector3.zero;

    public CommonMaterial          _attackMaterial = CommonMaterial.Empty;

    public HashSet<ObjectBase>     _collisionList = new HashSet<ObjectBase>();
    public List<CollisionSuccessData> _collisionOrder = new List<CollisionSuccessData>();

    public float                   _attackTerm = -1f;
    public bool                    _notifyAttackSuccess = true;
    public int                     _collisionCount = -1;


    public override void initialize(ObjectBase executeEntity)
    {
        executeEntity._attackProcessorManager.initializeAttack(this);
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity._attackProcessorManager.executeAttack(this,executeEntity,targetEntity);
        _collisionInfo.drawCollosionArea(UnityEngine.Color.red, _startFrame != _endFrame ? 0f : 1f);

        return true;
    }   

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        base.onExit(executeEntity,isForceEnd);
        executeEntity._attackProcessorManager.exitAttack(this,isForceEnd);
    }

    public void executeChildFrameEvent(ChildFrameEventType eventType, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        if(_childFrameEventItems == null || _childFrameEventItems.ContainsKey(eventType) == false)
            return;
        
        ChildFrameEventItem childFrameEventItem = _childFrameEventItems[eventType];
        GameEntityBase executeGameEntity = null;
        if(executeEntity is GameEntityBase)
            executeGameEntity = executeEntity as GameEntityBase;

        for(int i = 0; i < childFrameEventItem._childFrameEventCount; ++i)
        {
            if(executeGameEntity != null && childFrameEventItem._childFrameEvents[i].checkCondition(executeGameEntity) == false)
                continue;

            childFrameEventItem._childFrameEvents[i].initializeFromAttack(_attackMaterial);
            childFrameEventItem._childFrameEvents[i].initialize(executeEntity);
            childFrameEventItem._childFrameEvents[i].onExecute(executeEntity, targetEntity);
        }
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        float radius = 0f;
        float angle = 0f;
        float startDistance = 0f;
        float rayRadius = 0f;
        _attackType = AttackType.Default;


        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Radius")
            {
                radius = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "Angle")
            {
                angle = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "StartDistance")
            {
                startDistance = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "AttackType")
            {
                _attackType = (AttackType)System.Enum.Parse(typeof(AttackType), attributes[i].Value);
            }
            else if(attributes[i].Name == "AttackPreset")
            {
                AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AttackPreset") as AttackPreset;
                AttackPresetData presetData = preset.getPresetData(attributes[i].Value);
                if(presetData == null)
                {
                    DebugUtil.assert(false, "failed to load attack preset: {0}",attributes[i].Value);
                    return;
                }

                radius = presetData._attackRadius;
                angle = presetData._attackAngle;
                startDistance = presetData._attackStartDistance;
                rayRadius = presetData._attackRayRadius;
                _pushVector = presetData._pushVector;
                _attackMaterial = presetData._attackMaterial;
            }
            else if(attributes[i].Name == "IgnoreDefenceType")
            {
                string value = attributes[i].Value;
                
                if(value == null || value == "")
                {
                    DebugUtil.assert(false,"ignoreDefenceType must need type");
                    return;
                }

                string[] defencies = value.Split(' ');
                _ignoreDefenceType = new DefenceType[defencies.Length];

                for(int index = 0; index < defencies.Length; ++index)
                {
                    _ignoreDefenceType[index] = (DefenceType)System.Enum.Parse(typeof(DefenceType), defencies[index]);
                }
            }
            else if(attributes[i].Name == "Push")
            {
                string[] value = attributes[i].Value.Split(' ');
                if(value == null || value.Length > 3)
                {
                    DebugUtil.assert(false, "invalid Action Frame Event Data: Push, {0}",attributes[i].Value);
                    return;
                }

                _pushVector = new UnityEngine.Vector3(XMLScriptConverter.valueToFloatExtend(value[0]),XMLScriptConverter.valueToFloatExtend(value[1]),XMLScriptConverter.valueToFloatExtend(value[2]));
            }
            else if(attributes[i].Name == "CatchOffset")
            {
                _catchOffset = XMLScriptConverter.valueToVector3(attributes[i].Value);
            }
            else if(attributes[i].Name == "AttackCount")
            {
                _collisionCount = int.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "NotifyAttackSuccess")
            {
                _notifyAttackSuccess = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "DirectionType")
            {
                _targetDirectionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attributes[i].Value);
            }
            else if(attributes[i].Name == "AttackTerm")
            {
                _attackTerm = float.Parse(attributes[i].Value);
            }
        }

        CollisionInfoData data = new CollisionInfoData(radius,angle,startDistance,rayRadius, CollisionType.Attack);
        _collisionInfo = new CollisionInfo(data);

        
    }
}

public class ActionFrameEvent_Test : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Test;}

    private string _debugLog = "";

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        UnityEngine.Debug.Log("Test Frame Event : " + _debugLog);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Log")
                _debugLog = attributes[i].Value;
        }
    }
}

