using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;


public class StageGraphLoader : LoaderBase<StageGraphBaseData>
{
    public override StageGraphBaseData readFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
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

        if(spawnEvent == null)
        {
            DebugUtil.assert(false,"invalid stage graph event type: {0}", node.Name);
            return null;
        }

        spawnEvent.loadXml(node);
        return spawnEvent;
    }

    private static BuffData readBuffData(XmlNode node, ref Dictionary<int, BuffData> buffDataList)
    {
        BuffData buffData = new BuffData();
        buffData._buffName = node.Name;

        XmlAttributeCollection buffDataNodes = node.Attributes;

        for(int i = 0; i < buffDataNodes.Count; ++i)
        {
            string attrName = buffDataNodes[i].Name;
            string attrValue = buffDataNodes[i].Value;

            if(attrName == "Parent")
            {
                int targetKey = int.Parse(attrValue);
                if(buffDataList.ContainsKey(targetKey) == false)
                {
                    DebugUtil.assert(false, "target Buff is not exists: Key {0}", targetKey);
                    return null;
                }

                buffData.copy(buffDataList[targetKey]);
                continue;
            }

            if(attrName == "Key")
                buffData._buffKey = int.Parse(attrValue);
            else if(attrName == "StatusName")
                buffData._targetStatusName = attrValue;
            else if(attrName == "UpdateType")
                buffData._buffUpdateType = (BuffUpdateType)System.Enum.Parse(typeof(BuffUpdateType), attrValue);
            else if(attrName == "ApplyType")
                buffData._buffApplyType = (BuffApplyType)System.Enum.Parse(typeof(BuffApplyType), attrValue);
            else if(attrName == "Factor")
                buffData._buffVaryStatFactor = float.Parse(attrValue);
            else if(attrName == "Time")
                buffData._buffCustomValue0 = float.Parse(attrValue);
            else if(attrName == "TargetStatusName")
                buffData._buffCustomStatusName = attrValue;
            else if(attrName == "MinValue")
                buffData._buffCustomValue0 = float.Parse(attrValue);
            else if(attrName == "MaxValue")
                buffData._buffCustomValue1 = float.Parse(attrValue);
            else
            {
                DebugUtil.assert(false, "invalid attribute name from buffInfo: {0}",attrName);
                continue;
            }

        }

        return buffData;
    }

}
