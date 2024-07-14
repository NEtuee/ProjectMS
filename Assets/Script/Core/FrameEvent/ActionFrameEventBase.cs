using System.Xml;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

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
    FrameEvent_FrameTag,
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
    FrameEvent_SetHideUIAll,
    FrameEvent_StopSwitch,
    FrameEvent_UIEvent,
    FrameEvent_SpawnPrefab,
    FrameEvent_DeletePrefab,
    FrameEvent_ClearStatus,

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

#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_childFrameEventCount);
        for(int i = 0; i < _childFrameEventCount; ++i)
        {
            _childFrameEvents[i].serialize(ref binaryWriter);
        }
    }
#endif

    public void deserialize(ref BinaryReader binaryReader)
    {
        _childFrameEventCount = binaryReader.ReadInt32();
        _childFrameEvents = _childFrameEventCount == 0 ? null : new ActionFrameEventBase[_childFrameEventCount];
        for(int i = 0; i < _childFrameEventCount; ++i)
        {
            _childFrameEvents[i] = ActionFrameEventBase.buildFrameEvent(ref binaryReader);
        }
    }
}

public enum FrameEventExecuteTimingType
{
    None,
    TimeBase,
    FrameBase,
}

public abstract class ActionFrameEventBase : SerializableDataType
{
    public float                                _startFrame = 0f;
    public float                                _endFrame = 0f;

    public FrameEventExecuteTimingType          _executeTimingType = FrameEventExecuteTimingType.None;

    public ActionGraphConditionCompareData      _conditionCompareData = null;

    public Dictionary<ChildFrameEventType, ChildFrameEventItem> _childFrameEventItems = new Dictionary<ChildFrameEventType, ChildFrameEventItem>();
    
    public abstract FrameEventType getFrameEventType();
    public virtual void initialize(ObjectBase executeEntity){}
    public abstract bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public virtual void onExit(ObjectBase executeEntity, bool isForceEnd){}

    public virtual void initializeFromAttack(CommonMaterial attackMaterial) {}

    public bool checkCondition(GameEntityBase targetEntity)
    {
        if(_conditionCompareData == null)
            return true;

        return targetEntity.processActionCondition(_conditionCompareData);
    }

#if UNITY_EDITOR
    public abstract void loadFromXML(XmlNode node);

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write((int)getFrameEventType());
        binaryWriter.Write(_startFrame);
        binaryWriter.Write(_endFrame);
        binaryWriter.Write((int)_executeTimingType);
        binaryWriter.Write(_conditionCompareData != null);
        if(_conditionCompareData != null)
            _conditionCompareData.serialize(ref binaryWriter);
        
        if(_childFrameEventItems == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(_childFrameEventItems.Count);
            foreach(var item in _childFrameEventItems)
            {
                binaryWriter.Write((int)item.Key);
                item.Value.serialize(ref binaryWriter);
            }
        }
    }

#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _startFrame = binaryReader.ReadSingle();
        _endFrame = binaryReader.ReadSingle();
        _executeTimingType = (FrameEventExecuteTimingType)binaryReader.ReadInt32();
        bool hasCondition = binaryReader.ReadBoolean();
        if(hasCondition)
        {
            _conditionCompareData = new ActionGraphConditionCompareData();
            _conditionCompareData.deserialize(ref binaryReader);
        }

        int childFrameEventCount = binaryReader.ReadInt32();
        for(int i = 0; i < childFrameEventCount; ++i)
        {
            ChildFrameEventType frameEventType = (ChildFrameEventType)binaryReader.ReadInt32();
            ChildFrameEventItem item = new ChildFrameEventItem();
            item.deserialize(ref binaryReader);

            _childFrameEventItems.Add(frameEventType, item);
        }
    }

    public static ActionFrameEventBase buildFrameEvent(ref BinaryReader binaryReader)
    {
        FrameEventType frameEventType = (FrameEventType)binaryReader.ReadInt32();
        ActionFrameEventBase actionFrameEvent = getFrameEvent(frameEventType);
        actionFrameEvent.deserialize(ref binaryReader);

        return actionFrameEvent;
    }

    public static ActionFrameEventBase getFrameEvent(FrameEventType frameEventType)
    {
        ActionFrameEventBase outFrameEvent = null;
        if(frameEventType == FrameEventType.FrameEvent_Test)
            outFrameEvent = new ActionFrameEvent_Test();
        else if(frameEventType == FrameEventType.FrameEvent_Attack)
            outFrameEvent = new ActionFrameEvent_Attack();
        else if(frameEventType == FrameEventType.FrameEvent_ApplyBuff)
            outFrameEvent = new ActionFrameEvent_ApplyBuff();
        else if(frameEventType == FrameEventType.FrameEvent_ApplyBuffTarget)
            outFrameEvent = new ActionFrameEvent_ApplyBuffTarget();
        else if(frameEventType == FrameEventType.FrameEvent_DeleteBuff)
            outFrameEvent = new ActionFrameEvent_DeleteBuff();
        else if(frameEventType == FrameEventType.FrameEvent_TeleportToTarget)
            outFrameEvent = new ActionFrameEvent_TeleportToTarget();
        else if(frameEventType == FrameEventType.FrameEvent_TeleportToTargetBack)
            outFrameEvent = new ActionFrameEvent_TeleportToTargetBack();
        else if(frameEventType == FrameEventType.FrameEvent_SetDefenceType)
            outFrameEvent = new ActionFrameEvent_SetDefenceType();
        else if(frameEventType == FrameEventType.FrameEvent_Effect)
            outFrameEvent = new ActionFrameEvent_Effect();
        else if(frameEventType == FrameEventType.FrameEvent_TimelineEffect)
            outFrameEvent = new ActionFrameEvent_TimelineEffect();
        else if(frameEventType == FrameEventType.FrameEvent_ParticleEffect)
            outFrameEvent = new ActionFrameEvent_ParticleEffect();
        else if(frameEventType == FrameEventType.FrameEvent_AnimationEffect)
            outFrameEvent = new ActionFrameEvent_AnimationEffect();
        else if(frameEventType == FrameEventType.FrameEvent_FrameTag)
            outFrameEvent = new ActionFrameEvent_FrameTag();
        else if(frameEventType == FrameEventType.FrameEvent_Projectile)
            outFrameEvent = new ActionFrameEvent_Projectile();
        else if(frameEventType == FrameEventType.FrameEvent_Danmaku)
            outFrameEvent = new ActionFrameEvent_Danmaku();
        else if(frameEventType == FrameEventType.FrameEvent_SetAnimationSpeed)
            outFrameEvent = new ActionFrameEvent_SetAnimationSpeed();
        else if(frameEventType == FrameEventType.FrameEvent_SetCameraZoom)
            outFrameEvent = new ActionFrameEvent_SetCameraZoom();
        else if(frameEventType == FrameEventType.FrameEvent_SetCameraDelay)
            outFrameEvent = new ActionFrameEvent_SetCameraDelay();
        else if(frameEventType == FrameEventType.FrameEvent_KillEntity)
            outFrameEvent = new ActionFrameEvent_KillEntity();
        else if(frameEventType == FrameEventType.FrameEvent_Movement)
            outFrameEvent = new ActionFrameEvent_Movement();
        else if(frameEventType == FrameEventType.FrameEvent_ZoomEffect)
            outFrameEvent = new ActionFrameEvent_ZoomEffect();
        else if(frameEventType == FrameEventType.FrameEvent_ShakeEffect)
            outFrameEvent = new ActionFrameEvent_ShakeEffect();
        else if(frameEventType == FrameEventType.FrameEvent_StopUpdate)
            outFrameEvent = new ActionFrameEvent_StopUpdate();
        else if(frameEventType == FrameEventType.FrameEvent_SetTimeScale)
            outFrameEvent = new ActionFrameEvent_SetTimeScale();
        else if(frameEventType == FrameEventType.FrameEvent_SpawnCharacter)
            outFrameEvent = new ActionFrameEvent_SpawnCharacter();
        else if(frameEventType == FrameEventType.FrameEvent_ReleaseCatch)
            outFrameEvent = new ActionFrameEvent_ReleaseCatch();
        else if(frameEventType == FrameEventType.FrameEvent_TalkBalloon)
            outFrameEvent = new ActionFrameEvent_TalkBalloon();
        else if(frameEventType == FrameEventType.FrameEvent_DeactiveTalkBalloon)
            outFrameEvent = new ActionFrameEvent_DeactiveTalkBalloon();
        else if(frameEventType == FrameEventType.FrameEvent_SetAction)
            outFrameEvent = new ActionFrameEvent_SetAction();
        else if(frameEventType == FrameEventType.FrameEvent_CallAIEvent)
            outFrameEvent = new ActionFrameEvent_CallAIEvent();
        else if(frameEventType == FrameEventType.FrameEvent_AudioPlay)
            outFrameEvent = new ActionFrameEvent_AudioPlay();
        else if(frameEventType == FrameEventType.FrameEvent_PlaySequencer)
            outFrameEvent = new ActionFrameEvent_PlaySequencer();
        else if(frameEventType == FrameEventType.FrameEvent_SequencerSignal)
            outFrameEvent = new ActionFrameEvent_SequencerSignal();
        else if(frameEventType == FrameEventType.FrameEvent_ApplyPostProcessProfile)
            outFrameEvent = new ActionFrameEvent_ApplyPostProcessProfile();
        else if(frameEventType == FrameEventType.FrameEvent_SetDirectionType)
            outFrameEvent = new ActionFrameEvent_SetDirectionType();
        else if(frameEventType == FrameEventType.FrameEvent_Torque)
            outFrameEvent = new ActionFrameEvent_Torque();
        else if(frameEventType == FrameEventType.FrameEvent_EffectPreset)
            outFrameEvent = new ActionFrameEvent_EffectPreset();
        else if(frameEventType == FrameEventType.FrameEvent_SetRotateSlotValue)
            outFrameEvent = new ActionFrameEvent_SetRotateSlotValue();
        else if(frameEventType == FrameEventType.FrameEvent_FollowAttack)
            outFrameEvent = new ActionFrameEvent_FollowAttack();
        else if(frameEventType == FrameEventType.FrameEvent_SetHideUIAll)
            outFrameEvent = new ActionFrameEvent_SetHideUIAll();
        else if(frameEventType == FrameEventType.FrameEvent_StopSwitch)
            outFrameEvent = new ActionFrameEvent_StopSwitch();
        else if(frameEventType == FrameEventType.FrameEvent_UIEvent)
            outFrameEvent = new ActionFrameEvent_UIEvent();
        else if(frameEventType == FrameEventType.FrameEvent_SpawnPrefab)
            outFrameEvent = new ActionFrameEvent_SpawnPrefab();
        else if(frameEventType == FrameEventType.FrameEvent_DeletePrefab)
            outFrameEvent = new ActionFrameEvent_DeletePrefab();
        else if(frameEventType == FrameEventType.FrameEvent_ClearStatus)
            outFrameEvent = new ActionFrameEvent_ClearStatus();
        else
        {
            DebugUtil.assert(false, "invalid frameEvent type: {0}",frameEventType.ToString());
            return null;
        }

        return outFrameEvent;
    }
}

public class ActionFrameEvent_CallAIEvent : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_CallAIEvent;}

    public string _customAiEventName = "";
    public CallAIEventTargetType _eventTargetType = CallAIEventTargetType.Self;

    public List<CharacterEntityBase> _rangeSearchEntityList = new List<CharacterEntityBase>();
    public AllyTargetType _allyTarget = AllyTargetType.Count;

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
                sceneCharacterManager.targetSearchRange(executeEntity.transform.position,_range,_allyTarget,executeEntity.getAllyInfo(),ref _rangeSearchEntityList);
                foreach(var item in _rangeSearchEntityList)
                {
                    if(item == null || item is GameEntityBase == false || item == executeEntity)
                        continue;
                        
                    (item as GameEntityBase).executeCustomAIEvent(_customAiEventName);
                }
            }
            return true;
            case CallAIEventTargetType.SummonTargetRange:
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                _rangeSearchEntityList.Clear();
                sceneCharacterManager.targetSearchRange(executeEntity.transform.position,_range,_allyTarget,executeEntity.getAllyInfo(),ref _rangeSearchEntityList);

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

#if UNITY_EDITOR
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
            else if(attrName == "AllyTarget")
            {
                _allyTarget = (AllyTargetType)System.Enum.Parse(typeof(AllyTargetType), attrValue);
            }
        }

        if(_eventTargetType == CallAIEventTargetType.Range || _eventTargetType == CallAIEventTargetType.SummonTargetRange)
        {
            DebugUtil.assert_fileOpen(_range != 0f, "EventTargetType이 [{0}] 인데 Range가 0 입니다. Range를 입력해 주세요.",node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node), _eventTargetType.ToString());
        }
    }
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customAiEventName);
        BinaryHelper.writeEnum<CallAIEventTargetType>(ref binaryWriter, _eventTargetType);
        binaryWriter.Write((int)_allyTarget);
        binaryWriter.Write(_range);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _customAiEventName = binaryReader.ReadString();
        _eventTargetType = BinaryHelper.readEnum<CallAIEventTargetType>(ref binaryReader);
        _allyTarget = (AllyTargetType)binaryReader.ReadInt32();
        _range = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_directionType);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _directionType = (DirectionType)binaryReader.ReadInt32();
    }
}

public class ActionFrameEvent_ApplyPostProcessProfile : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyPostProcessProfile;}

    private string _path = "";
    private float _blendTime = 0f;
    private int _blendOrder = 0;
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
                CameraControlEx.Instance().getPostProcessProfileControl().setAdditionalEffectProfile(profile as PostProcessProfile,_blendOrder,_blendTime);
            break;
        }
        
        return true;
    }

#if UNITY_EDITOR
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
            else if(attrName == "Order")
                _blendOrder = int.Parse(attrValue);
        }

        if(_blendOrder >= 999)
        {
            DebugUtil.assert_fileOpen(false,"Order는 999미만이어야 합니다.",node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node));
            _blendOrder = 0;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_path);
        binaryWriter.Write(_blendTime);
        binaryWriter.Write(_blendOrder);
        binaryWriter.Write((int)_applyType);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _path = binaryReader.ReadString();
        _blendTime = binaryReader.ReadSingle();
        _blendOrder = binaryReader.ReadInt32();
        _applyType = (PostProcessProfileApplyType)binaryReader.ReadInt32();
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
        MasterManager.instance._stageProcessor.addSequencerSignal(_signal);

        if(executeEntity is GameEntityBase == false)
            return true;

        (executeEntity as GameEntityBase).addSequencerSignal(_signal);
        return true;
    }

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_signal);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _signal = binaryReader.ReadString();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_sequencerPath);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _sequencerPath = binaryReader.ReadString();
    }
}

public class ActionFrameEvent_AudioPlay : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_AudioPlay;}

    public int _audioID = 0;
    public bool _toTarget = false;
    public bool _attach = false;
    public bool _useFlip = false;
    public bool _isSwitch = false;

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
        {
            if(_isSwitch)
                eventEmitter = FMODAudioManager.Instance().playSwitch(executeEntity,_audioID,offset,_toTarget ? targetEntity.transform : executeEntity.transform);
            else
                eventEmitter = FMODAudioManager.Instance().Play(_audioID,offset,_toTarget ? targetEntity.transform : executeEntity.transform);
        }
        else
        {
            if(_isSwitch)
                eventEmitter = FMODAudioManager.Instance().playSwitch(executeEntity, _audioID, centerPosition + offset, null);
            else
                eventEmitter = FMODAudioManager.Instance().Play(_audioID, centerPosition + offset);
        }

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

#if UNITY_EDITOR
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
            else if(attributes[i].Name == "Switch")
            {
                _isSwitch = bool.Parse(attributes[i].Value);
            }
        }
    }


    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_audioID);
        binaryWriter.Write(_toTarget);
        binaryWriter.Write(_attach);
        binaryWriter.Write(_useFlip);
        binaryWriter.Write(_isSwitch);

        if(_parameterID == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(_parameterID.Length);
            for(int i = 0; i < _parameterID.Length; ++i)
            {
                binaryWriter.Write(_parameterID[i]);
            }
        }

        if(_parameterValue == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(_parameterValue.Length);
            for(int i = 0; i < _parameterValue.Length; ++i)
            {
                binaryWriter.Write(_parameterValue[i]);
            }
        }

        BinaryHelper.writeVector3(ref binaryWriter, _spawnOffset);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);

        _audioID = binaryReader.ReadInt32();
        _toTarget = binaryReader.ReadBoolean();
        _attach = binaryReader.ReadBoolean();
        _useFlip = binaryReader.ReadBoolean();
        _isSwitch = binaryReader.ReadBoolean();

        int parameterIDLength = binaryReader.ReadInt32();
        if(parameterIDLength == 0)
        {
            _parameterID = null;
        }
        else
        {
            _parameterID = new int[parameterIDLength];
            for(int i = 0; i < parameterIDLength; ++i)
            {
                _parameterID[i] = binaryReader.ReadInt32();
            }
        }

        int parameterValueLength = binaryReader.ReadInt32();
        if(parameterValueLength == 0)
        {
            _parameterValue = null;
        }
        else
        {
            _parameterValue = new float[parameterValueLength];
            for(int i = 0; i < parameterValueLength; ++i)
            {
                _parameterValue[i] = binaryReader.ReadSingle();
            }
        }

        _spawnOffset = BinaryHelper.readVector3(ref binaryReader);
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_actionName);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _actionName = binaryReader.ReadString();
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
        var entity = executeEntity as GameEntityBase;
        if (entity == null)
        {
            return true;
        }
        
        var commandList = DialogTextManager.Instance().GetDialogCommand(_key);
        if (commandList == null)
        {
            DebugUtil.assert(false, "존재하지 않는 Dialog 커맨드 입니다. 데이터를 확인 해 주세요. [Key: {0}]", _key);
            return true;
        }
        
        GameUI.Instance.TextBubble.PlayCommand(commandList, entity, null);
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "SimpleTalkKey")
                _key = attributes[i].Value;
        }

        if (_key == "")
        {
            DebugUtil.assert(false, "SimpleTalk Key가 존재하지 않습니다. 이거 필수임");
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_key);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _key = binaryReader.ReadString();
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

        GameUI.Instance.TextBubble.ForceEnd(executeEntity as GameEntityBase);
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
    }
}

public class ActionFrameEvent_ReleaseCatch : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ReleaseCatch;}

    private UnityEngine.Vector3         _pushVector = UnityEngine.Vector3.zero;
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
            
            if(target.checkCurrentActionFlag(ActionFlags.IgnorePush) == false && _pushVector.sqrMagnitude > float.Epsilon)
            {
                UnityEngine.Vector3 attackPointDirection = executeEntity.getDirection();
                target.setVelocity(UnityEngine.Quaternion.Euler(0f,0f,UnityEngine.Mathf.Atan2(attackPointDirection.y,attackPointDirection.x) * UnityEngine.Mathf.Rad2Deg) * _pushVector);
            }
        }

        parentObject.detachChildObject();

        return true;
    }

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        BinaryHelper.writeVector3(ref binaryWriter, _pushVector);
        binaryWriter.Write(_catchTargetAction);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _pushVector = BinaryHelper.readVector3(ref binaryReader);
        _catchTargetAction = binaryReader.ReadString();
    }
}

public class ActionFrameEvent_SpawnCharacter : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SpawnCharacter;}

    private string                      _characterKey = "";
    private string                      _startAINode = "";

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private UnityEngine.Vector3         _spawnOffset = UnityEngine.Vector3.zero;

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

#if UNITY_EDITOR
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
            else if(attrName == "AllyInfo")
            {
                _spawnDesc._allyInfo = AllyInfoManager.Instance().GetAllyInfoData(attrValue);
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_characterKey);
        binaryWriter.Write(_startAINode);
        binaryWriter.Write(_spawnDesc._allyInfo == null ? "" : _spawnDesc._allyInfo._key);
        BinaryHelper.writeVector3(ref binaryWriter, _spawnOffset);
        binaryWriter.Write(_inherit);
        binaryWriter.Write(_inheritDirection);
        binaryWriter.Write(_useFlip);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _characterKey = binaryReader.ReadString();
        _startAINode = binaryReader.ReadString();
        string allyInfoKey = binaryReader.ReadString();
        if(allyInfoKey != "")
            _spawnDesc._allyInfo = AllyInfoManager.Instance().GetAllyInfoData(allyInfoKey);

        _spawnOffset = BinaryHelper.readVector3(ref binaryReader);
        _inherit = binaryReader.ReadBoolean();
        _inheritDirection = binaryReader.ReadBoolean();
        _useFlip = binaryReader.ReadBoolean();

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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Time")
                _stopTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_stopTime);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _stopTime = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetTimeScale);
        binaryWriter.Write(_timeScalingTime);
        binaryWriter.Write(_timeScaleBlendTime);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _targetTimeScale = binaryReader.ReadSingle();
        _timeScalingTime = binaryReader.ReadSingle();
        _timeScaleBlendTime = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _zoomScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_zoomScale);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _zoomScale = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_shakeTime);
        binaryWriter.Write(_shakeScale);
        binaryWriter.Write(_shakeSpeed);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _shakeTime = binaryReader.ReadSingle();
        _shakeScale = binaryReader.ReadSingle();
        _shakeSpeed = binaryReader.ReadSingle();
    }
}

public class ActionFrameEvent_ClearStatus : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ClearStatus;}

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return true;

        (executeEntity as GameEntityBase).getStatusInfo().initialize();
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
    }
}

public class ActionFrameEvent_DeletePrefab : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_DeletePrefab;}

    public string _key = "";

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        MasterManager.instance._stageProcessor.removeSpawnPrefab(_key);
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Key")
                _key = attrValue;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_key);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _key = binaryReader.ReadString();
    }
}

public class ActionFrameEvent_SpawnPrefab : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SpawnPrefab;}

    public float _lifeTime = 0f;
    public string _key = "";

    private GameObject _prefab;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(_prefab == null)
            return true;
            
        GameObject prefab = GameObject.Instantiate(_prefab, executeEntity.transform.position,UnityEngine.Quaternion.identity);
        if(prefab == null)
            return true;
        
        if(_lifeTime > 0f)
            GameObject.Destroy(prefab, _lifeTime);
        if(_key != "")
            MasterManager.instance._stageProcessor.addSpawnPrefab(_key,prefab);

        prefab.transform.SetParent( MasterManager.instance._stageProcessor.getBackgroundObject()?.transform );
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
                _prefab = ResourceContainerEx.Instance().GetPrefab(attrValue);
            else if(attrName == "LifeTime")
                _lifeTime = float.Parse(attrValue);
            else if(attrName == "Key")
                _key = attrValue;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_lifeTime);
        binaryWriter.Write(_key);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _lifeTime = binaryReader.ReadSingle();
        _key = binaryReader.ReadString();
    }
}

public class ActionFrameEvent_UIEvent : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_UIEvent;}
    public string _key = "";

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        GameUI.Instance.NotifyToUI(_key);
        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Key")
                _key = attrValue;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_key);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _key = binaryReader.ReadString();
    }
}

public class ActionFrameEvent_StopSwitch : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_StopSwitch;}

    public enum StopSwitchType
    {
        None,
        Audio,
        Effect
    }

    public int _audioKey = 0;
    public string _effectKey = "";
    public StopSwitchType _stopSwitchType = StopSwitchType.None;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        switch(_stopSwitchType)
        {
            case StopSwitchType.Audio:
                FMODAudioManager.Instance().stopSwitch(executeEntity, _audioKey);
            break;
            case StopSwitchType.Effect:
                EffectManager._instance.stopEffectswitch(executeEntity,_effectKey);
            break;
        }

        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Switch")
            {
                _stopSwitchType = (StopSwitchType)System.Enum.Parse(typeof(StopSwitchType), attrValue);
            }
            else if(attrName == "Key")
            {
                switch(_stopSwitchType)
                {
                    case StopSwitchType.Audio:
                        _audioKey = int.Parse(attrValue);
                    break;
                    case StopSwitchType.Effect:
                        _effectKey = attrValue;
                    break;
                    default:
                        DebugUtil.assert_fileOpen(false, "Switch Type이 먼저 결정되어야 합니다.", node.BaseURI, XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return;
                }
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_audioKey);
        binaryWriter.Write(_effectKey);
        binaryWriter.Write((int)_stopSwitchType);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _audioKey = binaryReader.ReadInt32();
        _effectKey = binaryReader.ReadString();
        _stopSwitchType = (StopSwitchType)binaryReader.ReadInt32();
    }
}

public class ActionFrameEvent_SetHideUIAll : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetHideUIAll;}

    public bool _value;

    public override void initialize(ObjectBase executeEntity)
    {
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        HPSphereUIManager.Instance().setActive(_value == false);
        GameUI.Instance.SetActiveCrossHair(_value == false);
        ScreenDirector._instance.setActiveMainHud(_value == false);

        if(executeEntity is GameEntityBase)
            (executeEntity as GameEntityBase).setGraphicInterfaceActive(_value == false);

        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Hide")
                _value = bool.Parse(attrValue);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_value);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _value = binaryReader.ReadBoolean();
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
    
#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_followType);
        binaryWriter.Write(_characterKey);
        binaryWriter.Write(_toTarget);
        binaryWriter.Write(_radius);
        binaryWriter.Write(_moveTime);
        binaryWriter.Write((int)_searchIdentifier);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _followType = (FollowType)binaryReader.ReadInt32();
        _characterKey = binaryReader.ReadString();
        _toTarget = binaryReader.ReadBoolean();
        _radius = binaryReader.ReadSingle();
        _moveTime = binaryReader.ReadSingle();
        _searchIdentifier = (SearchIdentifier)binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_radius);
        binaryWriter.Write(_speed);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _radius = binaryReader.ReadSingle();
        _speed = binaryReader.ReadSingle();
    }
}

public class ActionFrameEvent_EffectPreset : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_EffectPreset;}

    private string _effectInfoKey = "";
    private bool _isSwitch = false;
    private CommonMaterial _attackMaterial;
    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(_isSwitch)
        {
            EffectManager._instance.playEffectSwitch(executeEntity,targetEntity,_effectInfoKey);
        }
        else
        {
            EffectInfoManager.Instance().requestEffect(_effectInfoKey,executeEntity, targetEntity,_attackMaterial);
        }
        
        return true;
    }

    public override void initializeFromAttack(CommonMaterial attackMaterial)
    {
        base.initializeFromAttack(attackMaterial);
        _attackMaterial = attackMaterial;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Key")
                _effectInfoKey = attributes[i].Value;
            else if(attributes[i].Name == "Switch")
                _isSwitch = bool.Parse(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_effectInfoKey);
        binaryWriter.Write(_isSwitch);
        binaryWriter.Write((int)_attackMaterial);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _effectInfoKey = binaryReader.ReadString();
        _isSwitch = binaryReader.ReadBoolean();
        _attackMaterial = (CommonMaterial)binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_torqueModifyType);
        _value.serialize(ref binaryWriter);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _torqueModifyType = (TorqueModifyType)binaryReader.ReadInt32();
        _value.deserialize(ref binaryReader);
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
#if UNITY_EDITOR
        public void serialize(ref BinaryWriter binaryWriter)
        {
            _value.serialize(ref binaryWriter);
            binaryWriter.Write(_targetValue);
        }
#endif
        public void deserialize(ref BinaryReader binaryReader)
        {
            _value.deserialize(ref binaryReader);
            _targetValue = binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_valueListCount);
        for(int i = 0; i < _setValueList.Length; ++i)
        {
            _setValueList[i].serialize(ref binaryWriter);
        }
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _valueListCount = binaryReader.ReadInt32();
        if(_valueListCount != 0)
            _setValueList = new MovementSetValueType[_valueListCount];
        for(int i = 0; i < _valueListCount; ++i)
        {
            _setValueList[i] = new MovementSetValueType();
            _setValueList[i]._value = new FloatEx();
            _setValueList[i].deserialize(ref binaryReader);
        }
    }
}

public class ActionFrameEvent_KillEntity : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_KillEntity;}


    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity.deactive();
        executeEntity.DeregisterRequest();

        return true;
    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
    }
}

public class ActionFrameEvent_Danmaku : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Danmaku;}

    private string _path;
    private UnityEngine.Vector3 _offsetPosition = UnityEngine.Vector3.zero;
    private bool _useFlip = false;
    private bool _useDirection = false;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        float offsetAngle = _useDirection ? MathEx.directionToAngle(executeEntity.getDirection()) : 0f;
        UnityEngine.Quaternion eulerAngle = UnityEngine.Quaternion.Euler(0f,0f,offsetAngle);
        UnityEngine.Vector3 offsetPosition = eulerAngle * _offsetPosition;
        
        if(executeEntity is GameEntityBase )
        {
            GameEntityBase requester = (GameEntityBase)executeEntity;
            requester.addDanmaku(_path,offsetPosition,_useFlip,offsetAngle);
        }
        else if(executeEntity is ProjectileEntityBase)
        {
            DanmakuManager.Instance().addDanmaku(_path,executeEntity.transform.position,offsetPosition,_useFlip,offsetAngle,executeEntity.getAllyInfo());
        }

        return true;
    }

#if UNITY_EDITOR
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
            else if(attributes[i].Name == "UseDirection")
                _useDirection = bool.Parse(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_path);
        BinaryHelper.writeVector3(ref binaryWriter, _offsetPosition);
        binaryWriter.Write(_useFlip);
        binaryWriter.Write(_useDirection);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _path = binaryReader.ReadString();
        _offsetPosition = BinaryHelper.readVector3(ref binaryReader);
        _useFlip = binaryReader.ReadBoolean();
        _useDirection = binaryReader.ReadBoolean();
    }
}

public class ActionFrameEvent_FrameTag : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_FrameTag;}

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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Tag")
                _frameTag = attributes[i].Value;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_frameTag);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _frameTag = binaryReader.ReadString();
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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Size")
                _zoom = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_zoom);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _zoom = binaryReader.ReadSingle();
    }
}

public class ActionFrameEvent_SetCameraDelay : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_SetCameraDelay;}

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        CameraControl.Instance().setDelay(true);
        return true;
    }

    public override void onExit(ObjectBase executeEntity, bool isForceEnd)
    {
        CameraControl.Instance().setDelay(false);

    }

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {

    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "DefenceType")
                _defenceType = (DefenceType)System.Enum.Parse(typeof(DefenceType), attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_defenceType);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _defenceType = (DefenceType)binaryReader.ReadInt32();
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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Speed")
                _speed = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_speed);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _speed = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_teleportTarget);
        binaryWriter.Write(_distanceOffset);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _teleportTarget = (TeleportTarget)binaryReader.ReadInt32();
        _distanceOffset = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_teleportTarget);
        binaryWriter.Write(_distanceOffset);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _teleportTarget = (TeleportTarget)binaryReader.ReadInt32();
        _distanceOffset = binaryReader.ReadSingle();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        if(buffKeyList == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(buffKeyList.Length);
            for(int i = 0; i < buffKeyList.Length; ++i)
            {
                binaryWriter.Write(buffKeyList[i]);
            }
        }
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);

        int buffKeyListCount = binaryReader.ReadInt32();
        if(buffKeyListCount == 0)
        {
            buffKeyList = null;
        }
        else
        {
            buffKeyList = new int[buffKeyListCount];
            for(int i = 0; i < buffKeyListCount; ++i)
            {
                buffKeyList[i] = binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        if(buffKeyList == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(buffKeyList.Length);
            for(int i = 0; i < buffKeyList.Length; ++i)
            {
                binaryWriter.Write(buffKeyList[i]);
            }
        }
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        int buffKeyListCount = binaryReader.ReadInt32();
        if(buffKeyListCount == 0)
        {
            buffKeyList = null;
        }
        else
        {
            buffKeyList = new int[buffKeyListCount];
            for(int i = 0; i < buffKeyListCount; ++i)
            {
                buffKeyList[i] = binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        if(buffKeyList == null)
        {
            binaryWriter.Write(0);
        }
        else
        {
            binaryWriter.Write(buffKeyList.Length);
            for(int i = 0; i < buffKeyList.Length; ++i)
            {
                binaryWriter.Write(buffKeyList[i]);
            }
        }
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);

        int buffKeyListCount = binaryReader.ReadInt32();
        if(buffKeyListCount == 0)
        {
            buffKeyList = null;
        }
        else
        {
            buffKeyList = new int[buffKeyListCount];
            for(int i = 0; i < buffKeyListCount; ++i)
            {
                buffKeyList[i] = binaryReader.ReadInt32();
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

#if UNITY_EDITOR
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
                AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AttackPreset") as AttackPreset;
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
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        CollisionInfoData collisionInfoData = _collisionInfo.getCollisionInfoData();
        binaryWriter.Write(collisionInfoData.getRadius());
        binaryWriter.Write(collisionInfoData.getAngle());
        binaryWriter.Write(collisionInfoData.getStartDistance());
        binaryWriter.Write(collisionInfoData.getRayRadius());
        binaryWriter.Write((int)collisionInfoData.getCollisionType());

        int ignoreDefenceTypeCount = _ignoreDefenceType == null ? 0 : _ignoreDefenceType.Length;
        binaryWriter.Write(ignoreDefenceTypeCount);
        for(int i = 0; i < ignoreDefenceTypeCount; ++i)
        {
            binaryWriter.Write((int)_ignoreDefenceType[i]);
        }

        binaryWriter.Write((int)_targetDirectionType);
        binaryWriter.Write((int)_attackType);
        BinaryHelper.writeVector3(ref binaryWriter, _pushVector);
        BinaryHelper.writeVector3(ref binaryWriter, _catchOffset);
        binaryWriter.Write((int)_attackMaterial);
        binaryWriter.Write(_attackTerm);
        binaryWriter.Write(_notifyAttackSuccess);
        binaryWriter.Write(_collisionCount);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        float radius = binaryReader.ReadSingle();
        float angle = binaryReader.ReadSingle();
        float startDistance = binaryReader.ReadSingle();
        float rayRadius = binaryReader.ReadSingle();
        CollisionType collisionType = (CollisionType)binaryReader.ReadInt32();
        CollisionInfoData data = new CollisionInfoData(radius,angle,startDistance,rayRadius, CollisionType.Attack);
        _collisionInfo = new CollisionInfo(data);

        int ignoreDefenceTypeCount = binaryReader.ReadInt32();
        if(ignoreDefenceTypeCount != 0)
            _ignoreDefenceType = new DefenceType[ignoreDefenceTypeCount];
        for(int i = 0; i < ignoreDefenceTypeCount; ++i)
        {
            _ignoreDefenceType[i] = (DefenceType)binaryReader.ReadInt32();
        }

        _targetDirectionType = (DirectionType)binaryReader.ReadInt32();
        _attackType = (AttackType)binaryReader.ReadInt32();
        _pushVector = BinaryHelper.readVector3(ref binaryReader);
        _catchOffset = BinaryHelper.readVector3(ref binaryReader);
        _attackMaterial = (CommonMaterial)binaryReader.ReadInt32();
        _attackTerm = binaryReader.ReadSingle();
        _notifyAttackSuccess = binaryReader.ReadBoolean();
        _collisionCount = binaryReader.ReadInt32();
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

#if UNITY_EDITOR
    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Log")
                _debugLog = attributes[i].Value;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_debugLog);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        base.deserialize(ref binaryReader);
        _debugLog = binaryReader.ReadString();
    }
}

