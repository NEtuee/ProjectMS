using System.Xml;
using System.Collections.Generic;

public enum FrameEventType
{
    FrameEvent_Test = 0,
    FrameEvent_Attack,
    FrameEvent_ApplyBuff,
    FrameEvent_ApplyBuffTarget,
    FrameEvent_DeleteBuff,
    FrameEvent_TeleportToTarget,
    FrameEvent_SetDefenceType,
    FrameEvent_Effect,
    FrameEvent_SetFrameTag,

    Count,
}

public enum ChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnGuard,
    ChildFrameEvent_OnParry,
    ChildFrameEvent_OnEvade,
    ChildFrameEvent_OnGuardBreak,
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
    public virtual void initialize(){}
    public abstract bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public virtual void onExit(ObjectBase executeEntity){}
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

    public override void onExit(ObjectBase executeEntity)
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


public class ActionFrameEvent_TeleportToTarget : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TeleportToTarget;}

    private float _distanceOffset = 0f;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false || targetEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase requester = (GameEntityBase)executeEntity;
        GameEntityBase target = (GameEntityBase)targetEntity;

        UnityEngine.Vector3 direction = (requester.transform.position - target.transform.position).normalized;

        requester.transform.position = target.transform.position + direction * (requester.getCollisionInfo().getRadius() + target.getCollisionInfo().getRadius() + _distanceOffset);

        return true;
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
                    buffKeyList[j] = int.Parse(buffList[j]);
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

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(targetEntity is GameEntityBase == false)
            return false;
        
        GameEntityBase target = (GameEntityBase)targetEntity;

        target.applyActionBuffList(buffKeyList);

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
                    buffKeyList[j] = int.Parse(buffList[j]);
                }

            }
        }
    }
}

public class ActionFrameEvent_Attack : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Attack;}

    private CollisionInfo           _collisionInfo;
    private CollisionDelegate       _collisionDelegate;
    private System.Action           _collisionEndEvent;
    
    private DefenceType[]           _ignoreDefenceType = null;

    private AttackType              _attackType;
    

    private HashSet<ObjectBase> _collisionList = new HashSet<ObjectBase>();
    private List<CollisionSuccessData> _collisionOrder = new List<CollisionSuccessData>();

    public override void initialize()
    {
        _collisionList.Clear();
        _collisionOrder.Clear();
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        _collisionInfo.updateCollisionInfo(executeEntity.transform.position,executeEntity.getDirection());
        _collisionInfo.drawCollosionArea(UnityEngine.Color.red,1f);
        CollisionManager.Instance().collisionRequest(_collisionInfo,executeEntity,_collisionDelegate,_collisionEndEvent);

        return true;
    }   

    public void attackProcess()
    {
        for(int i = 0; i < _collisionOrder.Count; ++i)
        {
            if(attackTarget(_collisionOrder[i]) == false)
                break;
        }

        _collisionOrder.Clear();
    }

    public void attackPrepare(CollisionSuccessData successData)
    {
        if(successData._requester is ObjectBase == false || successData._target is GameEntityBase == false)
            return;

        float distanceSq = (((GameEntityBase)successData._target).transform.position - successData._startPoint).sqrMagnitude;
        for(int i = 0; i < _collisionOrder.Count; ++i)
        {
            GameEntityBase target = (GameEntityBase)_collisionOrder[i]._target;

            if((target.transform.position - successData._startPoint).sqrMagnitude > distanceSq)
            {
                _collisionOrder.Insert(i,successData);
                return;
            }
        }

        _collisionOrder.Add(successData);
    }

    private bool attackTarget(CollisionSuccessData successData)
    {
        _collisionInfo.drawCollosionArea(UnityEngine.Color.green,1f);

        ObjectBase requester = (ObjectBase)successData._requester;
        GameEntityBase target = (GameEntityBase)successData._target;

        if(target._searchIdentifier == requester._searchIdentifier)
            return true;

        if(_collisionList.Contains(target) == true)
            return true;
        else
            _collisionList.Add(target);

        ChildFrameEventType eventType = ChildFrameEventType.Count;

        UnityEngine.Vector3 toTargetDirection = (requester.transform.position - target.transform.position).normalized;

        target.setAttackPoint(successData._startPoint);

        float attackInAngle = UnityEngine.Vector3.Angle(target.getCurrentDefenceDirection(), (successData._startPoint - target.transform.position).normalized);

        bool guardSuccess = (attackInAngle < target.getDefenceAngle() * 0.5f);
        bool canIgnore = canIgnoreDefenceType(target.getDefenceType());

        bool attackSuccess = false;

        if(((guardSuccess == false || target.getDefenceType() == DefenceType.Empty) && target.getDefenceType() != DefenceType.Evade) || canIgnore)
        {
            if(_attackType == AttackType.Default)
            {
                requester.setAttackState(AttackState.AttackSuccess);
                target.setDefenceState(DefenceState.Hit);

                if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnAttack);
                target.executeAIEvent(AIChildEventType.AIChildEvent_OnAttacked);

                eventType = ChildFrameEventType.ChildFrameEvent_OnHit;

            }
            else if(_attackType == AttackType.GuardBreak)
            {
                requester.setAttackState(AttackState.AttackGuardBreak);
                target.setDefenceState(DefenceState.GuardBroken);

                if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnGuardBreak);
                target.executeAIEvent(AIChildEventType.AIChildEvent_OnGuardBroken);

                eventType = ChildFrameEventType.ChildFrameEvent_OnGuardBreak;
            }

            attackSuccess = true;

        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Guard)
        {
            requester.setAttackState(AttackState.AttackGuarded);
            target.setDefenceState(DefenceState.DefenceSuccess);

            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnGuarded);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnGuard);

            eventType = ChildFrameEventType.ChildFrameEvent_OnGuard;
        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Parry)
        {
            requester.setAttackState(AttackState.AttackParried);
            target.setDefenceState(DefenceState.ParrySuccess);
            
            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnParried);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnParry);

            eventType = ChildFrameEventType.ChildFrameEvent_OnParry;
        }
        else if(guardSuccess && target.getDefenceType() == DefenceType.Evade)
        {
            requester.setAttackState(AttackState.AttackEvade);
            target.setDefenceState(DefenceState.EvadeSuccess);

            if(requester is GameEntityBase)
                    ((GameEntityBase)requester).executeAIEvent(AIChildEventType.AIChildEvent_OnEvaded);
            target.executeAIEvent(AIChildEventType.AIChildEvent_OnEvade);

            eventType = ChildFrameEventType.ChildFrameEvent_OnEvade;
        }
        

        executeChildFrameEvent(eventType, requester, target);

        return attackSuccess;
    }

    private bool canIgnoreDefenceType(DefenceType defenceType)
    {
        if(_ignoreDefenceType == null || _ignoreDefenceType.Length == 0)
            return false;

        for(int i = 0; i < _ignoreDefenceType.Length; ++i)
        {
            if(_ignoreDefenceType[i] == defenceType)
                return true;
        }

        return false;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        float radius = 0f;
        float angle = 0f;
        _attackType = AttackType.Default;


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

        }

        CollisionInfoData data = new CollisionInfoData(radius,angle, CollisionType.Attack);
        _collisionInfo = new CollisionInfo(data);

        _collisionDelegate = attackPrepare;
        _collisionEndEvent = attackProcess;
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

