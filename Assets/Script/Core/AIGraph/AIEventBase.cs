using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.IO;
using UnityEngine;

public enum AIEventType
{
    AIEvent_Test,
    AIEvent_SetAngleDirection,
    AIEvent_SetDirectionToTarget,
    AIEvent_SetAction,
    AIEvent_ClearTarget,
    AIEvent_ExecuteState,
    AIEvent_TerminatePackage,
    AIEvent_KillEntity,
    AIEvent_RotateDirection,
    AIEvent_CallAIEvent,
    AIEvent_SetCustomValue,
    AIEvent_AddCustomValue,
    AIEvent_SequencerSignal,
    AIEvent_AttachRotateSlot,
    AIEvent_DetachRotateSlot,
    AIEvent_ChangeAlly,
    Count,
}

public enum AIChildEventType
{
    AIChildEvent_OnExecute = 0,
    AIChildEvent_OnExit,
    AIChildEvent_OnFrame,
    AIChildEvent_OnUpdate,

    AIChildEvent_OnAttack,
    AIChildEvent_OnAttacked,

    AIChildEvent_OnGuard,
    AIChildEvent_OnGuarded,

    AIChildEvent_OnParry,
    AIChildEvent_OnParried,

    AIChildEvent_OnEvade,
    AIChildEvent_OnEvaded,

    AIChildEvent_OnGuardBreak,
    AIChildEvent_OnGuardBroken,

    AIChildEvent_OnGuardBreakFail,
    AIChildEvent_OnAttackGuardBreakFail,

    AIChildEvent_OnCatchTarget,
    AIChildEvent_OnCatched,

    AIChildEvent_OnThrowed,

    AIChildEvent_Custom,
    

    Count,
}

public abstract class AIEventBase
{
    public ActionGraphConditionCompareData      _conditionCompareData = null;

    public abstract AIEventType getFrameEventType();
    public virtual void initialize(){}
    public abstract void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public abstract void loadFromXML(XmlNode node);

    public virtual void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write((int)getFrameEventType());
        binaryWriter.Write(_conditionCompareData != null);
        if(_conditionCompareData != null)
            _conditionCompareData.serialize(ref binaryWriter);
    }

    public abstract void deserialize(ref BinaryReader binaryReader);

    public bool checkCondition(GameEntityBase targetEntity)
    {
        if(_conditionCompareData == null)
            return true;

        return targetEntity.processActionCondition(_conditionCompareData);
    }

    public static AIEventBase buildFrameEvent(ref BinaryReader binaryReader)
    {
        AIEventType aiEventType = (AIEventType)binaryReader.ReadInt32();
        AIEventBase aiEvent = getFrameEvent(aiEventType);

        bool hasCondition = binaryReader.ReadBoolean();
        if(hasCondition)
        {
            aiEvent._conditionCompareData = new ActionGraphConditionCompareData();
            aiEvent._conditionCompareData.deserialize(ref binaryReader);
        }

        aiEvent.deserialize(ref binaryReader);

        return aiEvent;
    }

    public static AIEventBase getFrameEvent(AIEventType aiEventType)
    {
        AIEventBase aiEvent = null;
        if(aiEventType == AIEventType.AIEvent_Test)
        {
            aiEvent = new AIEvent_Test();
        }
        else if(aiEventType == AIEventType.AIEvent_SetAngleDirection)
        {
            aiEvent = new AIEvent_SetAngleDirection();
        }
        else if(aiEventType == AIEventType.AIEvent_SetDirectionToTarget)
        {
            aiEvent = new AIEvent_SetDirectionToTarget();
        }
        else if(aiEventType == AIEventType.AIEvent_SetAction)
        {
            aiEvent = new AIEvent_SetAction();
        }
        else if(aiEventType == AIEventType.AIEvent_ClearTarget)
        {
            aiEvent = new AIEvent_ClearTarget();
        }
        else if(aiEventType == AIEventType.AIEvent_ExecuteState)
        {
            aiEvent = new AIEvent_ExecuteState();
        }
        else if(aiEventType == AIEventType.AIEvent_TerminatePackage)
        {
            aiEvent = new AIEvent_TerminatePackage();
        }
        else if(aiEventType == AIEventType.AIEvent_KillEntity)
        {
            aiEvent = new AIEvent_KillEntity();
        }
        else if(aiEventType == AIEventType.AIEvent_RotateDirection)
        {
            aiEvent = new AIEvent_RotateDirection();
        }
        else if(aiEventType == AIEventType.AIEvent_CallAIEvent)
        {
            aiEvent = new AIEvent_CallAIEvent();
        }
        else if(aiEventType == AIEventType.AIEvent_SetCustomValue)
        {
            aiEvent = new AIEvent_SetCustomValue();
        }
        else if(aiEventType == AIEventType.AIEvent_AddCustomValue)
        {
            aiEvent = new AIEvent_AddCustomValue();
        }
        else if(aiEventType == AIEventType.AIEvent_SequencerSignal)
        {
            aiEvent = new AIEvent_SequencerSignal();
        }
        else if(aiEventType == AIEventType.AIEvent_AttachRotateSlot)
        {
            aiEvent = new AIEvent_AttachRotateSlot();
        }
        else if(aiEventType == AIEventType.AIEvent_DetachRotateSlot)
        {
            aiEvent = new AIEvent_DetachRotateSlot();
        }
        else if(aiEventType == AIEventType.AIEvent_ChangeAlly)
        {
            aiEvent = new AIEvent_ChangeAlly();
        }
        else
        {
            DebugUtil.assert(false,"유효하지 않은 AI 이벤트 타입 입니다. 오타는 아닌가요?");
            return null;
        }

        return aiEvent;
    }

}
public class AIEvent_DetachRotateSlot : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_DetachRotateSlot;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).detachToSlot();
    }

    public override void loadFromXML(XmlNode node) 
    {
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class AIEvent_ChangeAlly : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_ChangeAlly;}

    private string _allyInfoKey = "";
    private bool _resetTarget = false;

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        AllyInfoData data = AllyInfoManager.Instance().GetAllyInfoData(_allyInfoKey);
        if(_resetTarget)
            (executeEntity as GameEntityBase).setTargetEntity(null);

        (executeEntity as GameEntityBase).setAllyInfo(data);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Key")
                _allyInfoKey = attrValue;
            else if(attrName == "ResetTarget")
                _resetTarget = bool.Parse(attrValue);
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_allyInfoKey);
        binaryWriter.Write(_resetTarget);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _allyInfoKey = binaryReader.ReadString();
        _resetTarget = binaryReader.ReadBoolean();
    }
}

public class AIEvent_AttachRotateSlot : AIEventBase
{
    private SetRotateSlotType _rotateSlotType = SetRotateSlotType.None;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_AttachRotateSlot;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        if(_rotateSlotType == SetRotateSlotType.None)
            return;

        GameEntityBase executeTargetEntity = null;
        switch(_rotateSlotType)
        {
            case SetRotateSlotType.Target:
            {
                executeTargetEntity = (executeEntity as GameEntityBase).getCurrentTargetEntity();
            }
            break;
            case SetRotateSlotType.Summoner:
            {
                if(executeEntity.getSummonObject() is GameEntityBase == false)
                    return;

                executeTargetEntity = executeEntity.getSummonObject() as GameEntityBase;
            }
            break;
        }

        (executeEntity as GameEntityBase).attachToSlot(executeTargetEntity);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "TargetType")
                _rotateSlotType = (SetRotateSlotType)System.Enum.Parse(typeof(SetRotateSlotType), attrValue);
        }

        if(_rotateSlotType == SetRotateSlotType.None)
            DebugUtil.assert(false, "AIEvent_SetRotateSlot의 _rotateSlotType은 필수 입력 해야 합니다.");
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write((int)_rotateSlotType);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _rotateSlotType = (SetRotateSlotType)binaryReader.ReadInt32();
    }
}

public class AIEvent_SequencerSignal : AIEventBase
{
    private string _signal = "";

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SequencerSignal;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        MasterManager.instance._stageProcessor.addSequencerSignal(_signal);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Signal")
                _signal = attrValue;
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_signal);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _signal = binaryReader.ReadString();
    }
}

public class AIEvent_AddCustomValue : AIEventBase
{
    private string _customValueName = "";
    private float _value;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_AddCustomValue;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).addCustomValue(_customValueName, _value);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Name")
            {
                _customValueName = attrValue;
            }
            else if(attrName == "Value")
            {
                _value = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customValueName);
        binaryWriter.Write(_value);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _customValueName = binaryReader.ReadString();
        _value = binaryReader.ReadSingle();
    }
}

public class AIEvent_SetCustomValue : AIEventBase
{
    private string _customValueName = "";
    private float _value;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetCustomValue;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).setCustomValue(_customValueName,_value);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Name")
            {
                _customValueName = attrValue;
            }
            else if(attrName == "Value")
            {
                _value = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customValueName);
        binaryWriter.Write(_value);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _customValueName = binaryReader.ReadString();
        _value = binaryReader.ReadSingle();
    }
}

public class AIEvent_CallAIEvent : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_CallAIEvent;}

    
    public string _customAiEventName = "";
    
    public CallAIEventTargetType _targetType = CallAIEventTargetType.Self;

    public override void initialize()
    {
    }

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        ObjectBase executeTargetEntity = null;
        switch(_targetType)
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
        }

        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return;

        (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
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
                _targetType = (CallAIEventTargetType)System.Enum.Parse(typeof(CallAIEventTargetType), attrValue);
            }

        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customAiEventName);
        binaryWriter.Write((int)_targetType);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _customAiEventName = binaryReader.ReadString();
        _targetType = (CallAIEventTargetType)binaryReader.ReadInt32();
    }
}

public class AIEvent_RotateDirection : AIEventBase
{
    private float _time;
    private float _rotateAngle;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_RotateDirection;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).setAIDirectionRotateProcess(_time,_rotateAngle);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Time")
            {
                _time = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "RotateAngle")
            {
                _rotateAngle = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_time);
        binaryWriter.Write(_rotateAngle);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _time = binaryReader.ReadSingle();
        _rotateAngle = binaryReader.ReadSingle();
    }
}

public class AIEvent_KillEntity : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_KillEntity;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity.deactive();
        executeEntity.DeregisterRequest();
    }

    public override void loadFromXML(XmlNode node) 
    {
        
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
    }
}


public class AIEvent_SetAction : AIEventBase
{
    string actionName = "";
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetAction;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAction(actionName);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Action")
            {
                actionName = attrValue;
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(actionName);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        actionName = binaryReader.ReadString();
    }
}

public class AIEvent_TerminatePackage : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_TerminatePackage;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.terminateAIPackage();
    }

    public override void loadFromXML(XmlNode node) 
    {

    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
    }
}

public class AIEvent_ExecuteState : AIEventBase
{
    public int targetStateIndex = -1;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_ExecuteState;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAIState(targetStateIndex);
    }

    public override void loadFromXML(XmlNode node) 
    {

    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(targetStateIndex);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        targetStateIndex = binaryReader.ReadInt32();
    }
}

public class AIEvent_ClearTarget : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_ClearTarget;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setTargetEntity(null);
    }

    public override void loadFromXML(XmlNode node) {}

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
    }
}


public class AIEvent_SetDirectionToTarget : AIEventBase
{
    float _directionAngle = 0f;
    float _rotateSpeed = -1f;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetDirectionToTarget;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        if(executeGameEntity.getCurrentTargetEntity() == null)
            return;

        Vector3 direction = executeGameEntity.getCurrentTargetEntity().transform.position - executeGameEntity.transform.position;
        direction.Normalize();
        direction = Quaternion.Euler(0f,0f,_directionAngle) * direction;

        if(_rotateSpeed == -1f)
        {
            executeGameEntity.setAiDirection(direction);
        }
        else
        {
            Vector3 currentDirection = executeGameEntity.getAiDirection();
            float angle = Vector3.SignedAngle(currentDirection, direction, Vector3.forward);
            float theta = Mathf.MoveTowardsAngle(0f,angle,_rotateSpeed * GlobalTimer.Instance().getSclaedDeltaTime());

            executeGameEntity.setAiDirection(Quaternion.Euler(0f,0f,theta) * currentDirection);
        }
        
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Angle")
            {
                _directionAngle = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "Speed")
            {
                _rotateSpeed = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_directionAngle);
        binaryWriter.Write(_rotateSpeed);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _directionAngle = binaryReader.ReadSingle();
        _rotateSpeed = binaryReader.ReadSingle();
    }
}

public class AIEvent_SetAngleDirection : AIEventBase
{
    public float angleDirection;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetAngleDirection;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAiDirection(angleDirection);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Angle")
            {
                angleDirection = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(angleDirection);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        angleDirection = binaryReader.ReadSingle();
    }
}

public class AIEvent_Test : AIEventBase
{
    string log = "";
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_Test;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        DebugUtil.log(log);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Log")
            {
                log = attrValue;
            }
        }
    }

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(log);
    }

    public override void deserialize(ref BinaryReader binaryReader)
    {
        log = binaryReader.ReadString();
    }
}

public class AIChildFrameEventItem
{
    public bool _consume = false;

    public AIEventBase[] _childFrameEvents;
    public int _childFrameEventCount;


    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_consume);
        binaryWriter.Write(_childFrameEventCount);
        for(int i = 0; i < _childFrameEventCount; ++i)
        {
            _childFrameEvents[i].serialize(ref binaryWriter);
        }
    }

    public void deserialize(ref BinaryReader binaryReader)
    {
        _consume = binaryReader.ReadBoolean();
        _childFrameEventCount = binaryReader.ReadInt32();
        if(_childFrameEventCount != 0)
        {
            _childFrameEvents = new AIEventBase[_childFrameEventCount];
            for(int i = 0; i < _childFrameEvents.Length; ++i)
            {
                _childFrameEvents[i] = AIEventBase.buildFrameEvent(ref binaryReader);
            }
        }
    }
}

public class CustomAIChildFrameEventItem : AIChildFrameEventItem
{
    public string _eventName = "";
}