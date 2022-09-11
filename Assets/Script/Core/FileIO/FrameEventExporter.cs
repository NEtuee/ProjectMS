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

        if(outFrameEvent == null)
        {
            DebugUtil.assert(false,"invalid frame event type : {0}",type);
            return null;
        }

        outFrameEvent._startFrame = float.Parse(attributes[1].Value);
        outFrameEvent.loadFromXML(node);
        
        return outFrameEvent;
    }
}