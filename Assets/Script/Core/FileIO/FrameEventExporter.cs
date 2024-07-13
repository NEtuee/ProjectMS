using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;

public static class FrameEventLoader
{
#if UNITY_EDITOR

    public static ActionFrameEventBase readFromXMLNode(XmlNode node, string filePath)
    {
        ActionFrameEventBase outFrameEvent = null;

        XmlAttributeCollection attributes = node.Attributes;

        string type = attributes[0].Value;
        FrameEventType frameEventType = (FrameEventType)System.Enum.Parse(typeof(FrameEventType), "FrameEvent_" + type);
        outFrameEvent = ActionFrameEventBase.getFrameEvent(frameEventType);

        DebugUtil.assert((int)FrameEventType.Count == 45, "check here");


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
                if(outFrameEvent._executeTimingType == FrameEventExecuteTimingType.TimeBase)
                {
                    DebugUtil.assert_fileOpen(false,"Start/EndFrame 과 Start/EndTime은 같이 쓰일 수 없습니다. [filePath: {0}] [nodeName: {1}] [lineNumber: {2}]",filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),filePath,node.Name,XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return null;
                }

                outFrameEvent._executeTimingType = FrameEventExecuteTimingType.FrameBase;
                outFrameEvent._startFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
                outFrameEvent._endFrame = outFrameEvent._startFrame;
            }
            else if(targetName == "EndFrame")
            {
                if(outFrameEvent._executeTimingType == FrameEventExecuteTimingType.TimeBase)
                {
                    DebugUtil.assert_fileOpen(false,"Start/EndFrame 과 Start/EndTime은 같이 쓰일 수 없습니다. [filePath: {0}] [nodeName: {1}] [lineNumber: {2}]",filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),filePath,node.Name,XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return null;
                }

                outFrameEvent._executeTimingType = FrameEventExecuteTimingType.FrameBase;
                outFrameEvent._endFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);

                if(outFrameEvent._startFrame > outFrameEvent._endFrame)
                {
                    DebugUtil.assert(false,"스타트 프레임은 엔드 프레임보다 커질 수 없습니다. [node :{0}] [Line : {1}] [FilePath : {2}]",node.Name, XMLScriptConverter.getLineFromXMLNode(node),filePath);
                    return null;
                }
            }
            else if(targetName == "StartTime")
            {
                if(outFrameEvent._executeTimingType == FrameEventExecuteTimingType.FrameBase)
                {
                    DebugUtil.assert_fileOpen(false,"Start/EndFrame 과 Start/EndTime은 같이 쓰일 수 없습니다. [filePath: {0}] [nodeName: {1}] [lineNumber: {2}]",filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),filePath,node.Name,XMLScriptConverter.getLineNumberFromXMLNode(node));                    return null;
                }

                outFrameEvent._executeTimingType = FrameEventExecuteTimingType.TimeBase;
                outFrameEvent._startFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
                outFrameEvent._endFrame = outFrameEvent._startFrame;
            }
            else if(targetName == "EndTime")
            {
                if(outFrameEvent._executeTimingType == FrameEventExecuteTimingType.FrameBase)
                {
                    DebugUtil.assert_fileOpen(false,"Start/EndFrame 과 Start/EndTime은 같이 쓰일 수 없습니다. [filePath: {0}] [nodeName: {1}] [lineNumber: {2}]",filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),filePath,node.Name,XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return null;
                }

                outFrameEvent._executeTimingType = FrameEventExecuteTimingType.TimeBase;
                outFrameEvent._endFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);

                if(outFrameEvent._startFrame > outFrameEvent._endFrame)
                {
                    DebugUtil.assert(false,"스타트 타임은 엔드 타임보다 커질 수 없습니다. [node :{0}] [Line : {1}] [FilePath : {2}]",node.Name, XMLScriptConverter.getLineFromXMLNode(node),filePath);
                    return null;
                }
            }
            else if(targetName == "Condition")
            {
                outFrameEvent._conditionCompareData = ActionGraphLoader.ReadConditionCompareData(attributes[i].Value, ActionGraphLoader.getGlobalVariableContainer(),node,filePath);
            }
        }

        if(outFrameEvent._executeTimingType == FrameEventExecuteTimingType.None)
        {
            outFrameEvent._executeTimingType = FrameEventExecuteTimingType.TimeBase;
            outFrameEvent._startFrame = 0f;
            outFrameEvent._endFrame = 0f;
        }

        outFrameEvent.loadFromXML(node);
        readChildFrameEvent(node,ref outFrameEvent, filePath);
        
        return outFrameEvent;
    }
    public static void readChildFrameEvent(XmlNode node, ref ActionFrameEventBase frameEvent, string filePath)
    {
        if(frameEvent.getFrameEventType() == FrameEventType.FrameEvent_Effect)
            return;

        XmlNodeList childNodeList = node.ChildNodes;

        if(childNodeList == null || childNodeList.Count == 0)
            return;

        Dictionary<ChildFrameEventType, ChildFrameEventItem> childFrameEventList = new Dictionary<ChildFrameEventType, ChildFrameEventItem>();

        for(int i = 0; i < childNodeList.Count; ++i)
        {
            ChildFrameEventType eventType = ChildFrameEventType.Count;
            List<ActionFrameEventBase> frameEventList = new List<ActionFrameEventBase>();
            if(readChildFrameEventList(childNodeList[i],ref eventType, ref frameEventList, filePath) == false)
                continue;

            ChildFrameEventItem childItem = new ChildFrameEventItem();
            childItem._childFrameEventCount = frameEventList.Count;
            childItem._childFrameEvents = frameEventList.ToArray();

            childFrameEventList.Add(eventType, childItem);
        }

        frameEvent._childFrameEventItems = childFrameEventList;
    }

    

    public static bool readChildFrameEventList(XmlNode node, ref ChildFrameEventType eventType, ref List<ActionFrameEventBase> childFrameEventList, string filePath)
    {
        string targetName = node.Name;
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
        else if(targetName == "OnGuardBreakFail")
            eventType = ChildFrameEventType.ChildFrameEvent_OnGuardBreakFail;
        else if(targetName == "OnCatch")
            eventType = ChildFrameEventType.ChildFrameEvent_OnCatch;
        else if(targetName == "OnKill")
            eventType = ChildFrameEventType.ChildFrameEvent_OnKill;
        else if(targetName == "OnBegin")
            eventType = ChildFrameEventType.ChildFrameEvent_OnBegin;
        else if(targetName == "OnEnter")
            eventType = ChildFrameEventType.ChildFrameEvent_OnEnter;
        else if(targetName == "OnExit")
            eventType = ChildFrameEventType.ChildFrameEvent_OnExit;
        else if(targetName == "OnEnd")
            eventType = ChildFrameEventType.ChildFrameEvent_OnEnd;
        else
            return false;

        childFrameEventList.Clear();
        XmlNodeList childNodes = node.ChildNodes;
        for(int j = 0; j < childNodes.Count; ++j)
        {
            childFrameEventList.Add(readFromXMLNode(childNodes[j],filePath));
        }

        childFrameEventList.Sort((x,y)=>{
            return x._startFrame.CompareTo(y._startFrame);
        });

        return true;
    }
#endif

}