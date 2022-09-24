using System.Xml;
using System.Collections.Generic;

public enum FrameEventType
{
    FrameEvent_Test = 0,
    FrameEvent_Attack,
    FrameEvent_ApplyBuff,
    FrameEvent_ApplyBuffTarget,
    FrameEvent_TeleportToTarget,

    Count,
}

public enum ChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnGuard,
    ChildFrameEvent_OnParry,
    Count,
}

public class ChildFrameEventItem
{
    public ActionFrameEventBase[] _childFrameEvents;
    public int _childFrameEventCount;

}

public abstract class ActionFrameEventBase
{
    public float _startFrame;
    public float _endFrame;

    public Dictionary<ChildFrameEventType, ChildFrameEventItem> _childFrameEventItems = null;
    
    public abstract FrameEventType getFrameEventType();
    public abstract void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public abstract void loadFromXML(XmlNode node);

    public void executeChildFrameEvent(ChildFrameEventType eventType, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        if(_childFrameEventItems == null || _childFrameEventItems.ContainsKey(eventType) == false)
            return;
        
        ChildFrameEventItem childFrameEventItem = _childFrameEventItems[eventType];
        for(int i = 0; i < childFrameEventItem._childFrameEventCount; ++i)
        {
            childFrameEventItem._childFrameEvents[i].onExecute(executeEntity, targetEntity);
        }
    }
}

public class ActionFrameEvent_TeleportToTarget : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TeleportToTarget;}

    private float _distanceOffset = 0f;

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false || targetEntity is GameEntityBase == false)
            return;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        GameEntityBase target = (GameEntityBase)targetEntity;

        UnityEngine.Vector3 direction = (requester.transform.position - target.transform.position).normalized;

        requester.transform.position = target.transform.position + direction * (requester.getCollisionInfo().getRadius() + target.getCollisionInfo().getRadius() + _distanceOffset);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "DistanceOffset")
                _distanceOffset = float.Parse(attributes[i].Value);
        }
    }
}

public class ActionFrameEvent_ApplyBuff : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyBuff;}

    private int[] buffKeyList = null;

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        ((GameEntityBase)executeEntity).applyActionBuffList(buffKeyList);
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
                    buffKeyList[j] = int.Parse(buffList[j]);
                }

            }
        }
    }
}

public class ActionFrameEvent_ApplyBuffTarget : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ApplyBuffTarget;}

    private int[] buffKeyList = null;

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false || targetEntity is GameEntityBase == false)
            return;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        GameEntityBase target = (GameEntityBase)targetEntity;

        target.applyActionBuffList(buffKeyList);
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
                    buffKeyList[j] = int.Parse(buffList[j]);
                }

            }
        }
    }
}

public class ActionFrameEvent_Attack : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Attack;}

    private CollisionInfo _collisionInfo;
    private CollisionDelegate _collisionDelegate;

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        _collisionInfo.updateCollisionInfo(executeEntity.transform.position,executeEntity.getDirection());
        _collisionInfo.drawCollosionArea(UnityEngine.Color.red,1f);
        CollisionManager.Instance().collisionRequest(_collisionInfo,executeEntity,_collisionDelegate);
    }   

    public void attackProcess(CollisionSuccessData successData)
    {
        _collisionInfo.drawCollosionArea(UnityEngine.Color.green,1f);

        if(successData._requester is GameEntityBase == false || successData._target is GameEntityBase == false)
            return;
        
        GameEntityBase requester = (GameEntityBase)successData._requester;
        GameEntityBase target = (GameEntityBase)successData._target;

        ChildFrameEventType eventType = ChildFrameEventType.Count;

        if(target.getDefenceType() == DefenceType.Guard)
        {
            requester.setAttackState(AttackState.AttackGuarded);
            target.setDefenceState(DefenceState.DefenceSuccess);

            eventType = ChildFrameEventType.ChildFrameEvent_OnGuard;
        }
        else if(target.getDefenceType() == DefenceType.Parry)
        {
            requester.setAttackState(AttackState.AttackParried);
            target.setDefenceState(DefenceState.ParrySuccess);

            eventType = ChildFrameEventType.ChildFrameEvent_OnParry;
        }
        else if(target.getDefenceType() == DefenceType.Empty)
        {
            requester.setAttackState(AttackState.AttackSuccess);
            target.setDefenceState(DefenceState.Hit);

            eventType = ChildFrameEventType.ChildFrameEvent_OnHit;
        }

        executeChildFrameEvent(eventType, requester, target);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        float radius = 0f;
        float angle = 0f;


        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Radius")
            {
                radius = float.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "Angle")
            {
                angle = float.Parse(attributes[i].Value);
            }

        }

        CollisionInfoData data = new CollisionInfoData(radius,angle, CollisionType.Attack);
        _collisionInfo = new CollisionInfo(data);

        _collisionDelegate = attackProcess;
    }
}

public class ActionFrameEvent_Test : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Test;}

    private string _debugLog = "";

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        UnityEngine.Debug.Log("Test Frame Event : " + _debugLog);
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

