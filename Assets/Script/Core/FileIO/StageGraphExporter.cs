using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class StageGraphLoader : LoaderBase<StageGraphBaseData>
{
    static string _currentFileName = "";
    public override StageGraphBaseData readFromXML(string path)
    {
        _currentFileName = path;
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }


        StageGraphBaseData baseData = new StageGraphBaseData();

        XmlAttributeCollection firstNodeAttribute = xmlDoc.FirstChild.Attributes;
        for(int i = 0; i < firstNodeAttribute.Count; ++i)
        {
            string attrName = firstNodeAttribute[i].Name;
            string attrValue = firstNodeAttribute[i].Value;

            if(attrName == "Name")
            {
                baseData._stageName = attrValue;
            }
        }

        XmlNodeList nodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            XmlNode phaseNode = nodes[nodeIndex];
            StageGraphPhaseData phaseData = readPhaseData(phaseNode);

            if(phaseNode.Name == "InitializePhase")
                baseData._stageGraphPhase[(int)StageGraphPhaseType.Initialize] = phaseData;
            else if(phaseNode.Name == "UpdatePhase")
                baseData._stageGraphPhase[(int)StageGraphPhaseType.Update] = phaseData;
        }

        return baseData;
    }

    private static StageGraphPhaseData readPhaseData(XmlNode node)
    {
        StageGraphPhaseData phaseData = new StageGraphPhaseData();

        List<StageGraphEventBase> eventList = new List<StageGraphEventBase>();
        XmlNodeList eventNodes = node.ChildNodes;
        for(int i = 0; i < eventNodes.Count; ++i)
        {
            StageGraphEventBase eventData = readEventData(eventNodes[i]);
            eventList.Add(eventData);
        }

        phaseData._stageGraphEventList = eventList.ToArray();
        phaseData._stageGraphEventCount = eventList.Count;
        return phaseData;
    }

    private static StageGraphEventBase readEventData(XmlNode node)
    {
        StageGraphEventBase spawnEvent = null;
        if(node.Name == "SpawnCharacter")
        {
            spawnEvent = new StageGraphEvent_SpawnCharacter();
        }
        else if(node.Name == "WaitSecond")
        {
            spawnEvent = new StageGraphEvent_WaitSecond(); 
        }
        else if(node.Name == "SetCameraTarget")
        {
            spawnEvent = new StageGraphEvent_SetCameraTarget();
        }
        else if(node.Name == "SetAudioListner")
        {
            spawnEvent = new StageGraphEvent_SetAudioListner();
        }
        else if(node.Name == "SetCrossHair")
        {
            spawnEvent = new StageGraphEvent_SetCrossHair();
        }
        else if(node.Name == "SetHPSphere")
        {
            spawnEvent = new StageGraphEvent_SetHPSphere();
        }
        else if(node.Name == "WaitTargetDead")
        {
            spawnEvent = new StageGraphEvent_WaitTargetDead();
        }
        else if(node.Name == "TeleportTargetTo")
        {
            spawnEvent = new StageGraphEvent_TeleportTargetTo();
        }
        else if(node.Name == "ApplyPostProcessProfile")
        {
            spawnEvent = new StageGraphEvent_ApplyPostProcessProfile();
        }


        if(spawnEvent == null)
        {
            DebugUtil.assert(false,"invalid stage graph event type: {0} [Line: {1}] [FileName: {2}]", node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            return null;
        }

        spawnEvent.loadXml(node);
        return spawnEvent;
    }

}
