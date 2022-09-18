using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;

public static class FrameEventLoader
{
    public static ActionFrameEventBase readFromXMLNode(XmlNode node)
    {
        ActionFrameEventBase outFrameEvent = null;

        XmlAttributeCollection attributes = node.Attributes;



        string type = attributes[0].Value;
        if(type == "Test")
            outFrameEvent = new ActionFrameEvent_Test();
        else if(type == "Attack")
            outFrameEvent = new ActionFrameEvent_Attack();

        DebugUtil.assert((int)FrameEventType.Count == 2, "check loader");






        if(outFrameEvent == null)
        {
            DebugUtil.assert(false,"invalid frame event type : {0}",type);
            return null;
        }

        outFrameEvent._startFrame = float.Parse(attributes[1].Value);
        outFrameEvent._endFrame = outFrameEvent._startFrame;

        if(attributes[2].Name == "EndFrame")
            outFrameEvent._endFrame = float.Parse(attributes[2].Value);

        outFrameEvent.loadFromXML(node);
        
        return outFrameEvent;
    }
}