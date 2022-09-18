using System.Xml;

public enum FrameEventType
{
    FrameEvent_Test = 0,
    FrameEvent_Attack,
    Count,
}

public abstract class ActionFrameEventBase
{
    public float _startFrame;
    public float _endFrame;
    
    public abstract FrameEventType getFrameEventType();
    public abstract void onExecute(ObjectBase executeEntity);
    public abstract void loadFromXML(XmlNode node);
}

public class ActionFrameEvent_Attack : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Attack;}

    private CollisionInfo _collisionInfo;
    private CollisionDelegate _collisionDelegate;



    public override void onExecute(ObjectBase executeEntity)
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

        if(target.getDefenceType() == DefenceType.Guard)
        {
            requester.setAttackState(AttackState.AttackBlocked);
            target.setDefenceState(DefenceState.DefenceSuccess);
        }
        else if(target.getDefenceType() != DefenceType.Guard)
        {
            requester.setAttackState(AttackState.AttackSuccess);
            target.setDefenceState(DefenceState.Hit);
        }
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

    public override void onExecute(ObjectBase executeEntity)
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

