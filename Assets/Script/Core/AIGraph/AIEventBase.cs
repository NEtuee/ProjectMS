using System.Collections.Generic;
using System.Xml;

public enum AIEventType
{
    AIEvent_Test,
    Count,
}

public enum AIChildEventType
{
    AIChildEvent_OnExecute = 0,
    AIChildEvent_OnExit,
    AIChildEvent_OnFrame,
    AIChildEvent_OnHit,
    AIChildEvent_OnAttacked,
    AIChildEvent_OnGuarded,
    AIChildEvent_OnParried,

    Count,
}

public abstract class AIEventBase
{
    public abstract AIEventType getFrameEventType();
    public virtual void initialize(){}
    public abstract void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public abstract void loadFromXML(XmlNode node);

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