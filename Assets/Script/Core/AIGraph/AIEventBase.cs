using System.Collections.Generic;
using System.Xml;
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

    Count,
}

public abstract class AIEventBase
{
    public abstract AIEventType getFrameEventType();
    public virtual void initialize(){}
    public abstract void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public abstract void loadFromXML(XmlNode node);

}

public class AIEvent_KillEntity : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_KillEntity;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity.gameObject.SetActive(false);
        executeEntity.DeregisterRequest();
    }

    public override void loadFromXML(XmlNode node) 
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
}


public class AIEvent_SetDirectionToTarget : AIEventBase
{
    float _directionAngle = 0f;
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
        executeGameEntity.setAiDirection(direction);
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
                _directionAngle = float.Parse(attrValue);
            }
        }
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
                angleDirection = float.Parse(attrValue);
            }
        }
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
}

public class AIChildFrameEventItem
{
    public bool _consume = false;

    public AIEventBase[] _childFrameEvents;
    public int _childFrameEventCount;

}