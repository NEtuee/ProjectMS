using System.Xml;

public enum FrameEventType
{
    FrameEvent_Test,
}

public abstract class ActionFrameEventBase
{
    public float _startFrame;
    
    public abstract FrameEventType getFrameEventType();
    public abstract void onExecute(ObjectBase executeEntity);
    public abstract void loadFromXML(XmlNode node);
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

