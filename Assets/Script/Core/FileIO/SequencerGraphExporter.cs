using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class SequencrGraphLoader : LoaderBase<SequencerGraphBaseData>
{
    static string _currentFileName = "";

    public override SequencerGraphBaseData createNewDataInstance()
    {
        return new SequencerGraphBaseData();
    }

    public override SequencerGraphBaseData readFromXML(string path)
    {
        _currentFileName = path;
        
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(_currentFileName),readerSettings))
            {
                xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0} \n{1}",path,ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }


        SequencerGraphBaseData baseData = new SequencerGraphBaseData();

        XmlAttributeCollection firstNodeAttribute = xmlDoc.FirstChild.Attributes;
        for(int i = 0; i < firstNodeAttribute.Count; ++i)
        {
            string attrName = firstNodeAttribute[i].Name;
            string attrValue = firstNodeAttribute[i].Value;

            if(attrName == "Name")
            {
                baseData._sequencerName = attrValue;
            }
        }

        XmlNodeList nodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            XmlNode phaseNode = nodes[nodeIndex];
            SequencerGraphPhaseData phaseData = readPhaseData(phaseNode);

            if(phaseNode.Name == "InitializePhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.Initialize] = phaseData;
            else if(phaseNode.Name == "UpdatePhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.Update] = phaseData;
            else if(phaseNode.Name == "EndPhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.End] = phaseData;
        }

        for(int index = 0; index < (int)SequencerGraphPhaseType.Count; ++index)
        {
            if(baseData._sequencerGraphPhase[index] == null)
                baseData._sequencerGraphPhase[index] = new SequencerGraphPhaseData();
        }

        return baseData;
    }

    private static SequencerGraphPhaseData readPhaseData(XmlNode node)
    {
        SequencerGraphPhaseData phaseData = new SequencerGraphPhaseData();

        List<SequencerGraphEventBase> eventList = new List<SequencerGraphEventBase>();
        XmlNodeList eventNodes = node.ChildNodes;
        for(int i = 0; i < eventNodes.Count; ++i)
        {
            SequencerGraphEventBase eventData = readEventData(eventNodes[i]);
            eventList.Add(eventData);
        }

        phaseData._sequencerGraphEventList = eventList.ToArray();
        phaseData._sequencerGraphEventCount = eventList.Count;
        return phaseData;
    }

    public static SequencerGraphEventBase readEventData(XmlNode node)
    {
        SequencerGraphEventType eventType = (SequencerGraphEventType)System.Enum.Parse(typeof(SequencerGraphEventType), node.Name);;
        SequencerGraphEventBase spawnEvent = SequencerGraphEventBase.getSequencerGraphEventBase(eventType);
            
        if(spawnEvent == null)
        {
            DebugUtil.assert(false,"invalid sequencer graph event type: {0} [Line: {1}] [FileName: {2}]", node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            return null;
        }

        spawnEvent.loadXml(node);
        return spawnEvent;
    }

}
