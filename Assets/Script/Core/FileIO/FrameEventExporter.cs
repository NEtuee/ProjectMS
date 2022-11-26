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
        else if(type == "ApplyBuff")
            outFrameEvent = new ActionFrameEvent_ApplyBuff();
        else if(type == "ApplyBuffTarget")
            outFrameEvent = new ActionFrameEvent_ApplyBuffTarget();
        else if(type == "DeleteBuff")
            outFrameEvent = new ActionFrameEvent_DeleteBuff();
        else if(type == "TeleportToTarget")
            outFrameEvent = new ActionFrameEvent_TeleportToTarget();
        else if(type == "SetDefenceType")
            outFrameEvent = new ActionFrameEvent_SetDefenceType();
        else if(type == "Effect")
            outFrameEvent = new ActionFrameEvent_Effect();
        else if(type == "FrameTag")
            outFrameEvent = new ActionFrameEvent_SetFrameTag();
        else if(type == "Projectile")
            outFrameEvent = new ActionFrameEvent_Projectile();
        else if(type == "Danmaku")
            outFrameEvent = new ActionFrameEvent_Danmaku();
        else if(type == "SetAnimationSpeed")
            outFrameEvent = new ActionFrameEvent_SetAnimationSpeed();
        else if(type == "SetCameraDelay")
            outFrameEvent = new ActionFrameEvent_SetCameraDelay();
        else
        {
            DebugUtil.assert(false, "invalid frameEvent type: {0}",type);
            return null;
        }

        DebugUtil.assert((int)FrameEventType.Count == 13, "check here");


        if(outFrameEvent == null)
        {
            DebugUtil.assert(false,"invalid frame event type : {0}",type);
            return null;
        }

        for(int i = 0; i < attributes.Count; ++i)
        {
            string targetName = attributes[i].Name;
            attributes[i].Value = ActionGraphLoader.getGlobalVariable(attributes[i].Value);
            
            if(targetName == "StartFrame")
            {
                outFrameEvent._startFrame = float.Parse(attributes[i].Value);
                outFrameEvent._endFrame = outFrameEvent._startFrame;
            }
            else if(targetName == "EndFrame")
            {
                outFrameEvent._endFrame = float.Parse(attributes[i].Value);

                if(outFrameEvent._startFrame > outFrameEvent._endFrame)
                {
                    DebugUtil.assert(false,"start frame cant be greater than the end frame. {0}",node.Name);
                    return null;
                }
            }
        }

        outFrameEvent.loadFromXML(node);
        readChildFrameEvent(node,ref outFrameEvent);
        
        return outFrameEvent;
    }

    public static void readChildFrameEvent(XmlNode node, ref ActionFrameEventBase frameEvent)
    {
        if(frameEvent.getFrameEventType() == FrameEventType.FrameEvent_Effect)
            return;

        XmlNodeList childNodeList = node.ChildNodes;

        if(childNodeList == null || childNodeList.Count == 0)
            return;

        Dictionary<ChildFrameEventType, ChildFrameEventItem> childFrameEventList = new Dictionary<ChildFrameEventType, ChildFrameEventItem>();

        for(int i = 0; i < childNodeList.Count; ++i)
        {
            string targetName = childNodeList[i].Name;

            ChildFrameEventItem childItem = new ChildFrameEventItem();
            ChildFrameEventType eventType = ChildFrameEventType.Count;

            if(targetName == "OnHit")
                eventType = ChildFrameEventType.ChildFrameEvent_OnHit;
            else if(targetName == "OnGuard")
                eventType = ChildFrameEventType.ChildFrameEvent_OnGuard;
            else if(targetName == "OnParry")
                eventType = ChildFrameEventType.ChildFrameEvent_OnParry;
            else if(targetName == "OnEvade")
                eventType = ChildFrameEventType.ChildFrameEvent_OnEvade;
            else if(targetName == "OnGuardBreak")
                eventType = ChildFrameEventType.ChildFrameEvent_OnGuardBreak;
            else
                DebugUtil.assert(false, "invalid child frame event type: {0}", targetName);


            List<ActionFrameEventBase> actionFrameEventList = new List<ActionFrameEventBase>();
            XmlNodeList childNodes = childNodeList[i].ChildNodes;
            for(int j = 0; j < childNodes.Count; ++j)
            {
                actionFrameEventList.Add(readFromXMLNode(childNodes[j]));
            }

            actionFrameEventList.Sort((x,y)=>{
                return x._startFrame.CompareTo(y._startFrame);
            });

            childItem._childFrameEventCount = actionFrameEventList.Count;
            childItem._childFrameEvents = actionFrameEventList.ToArray();

            childFrameEventList.Add(eventType, childItem);
        }

        frameEvent._childFrameEventItems = childFrameEventList;
    }
}