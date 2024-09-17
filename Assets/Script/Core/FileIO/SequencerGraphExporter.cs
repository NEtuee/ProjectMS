using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;


public class SequencerGraphSetLoader : LoaderBase<SequencerGraphSetBaseData>
{
    static string _currentFileName = "";

    public override SequencerGraphSetBaseData createNewDataInstance()
    {
        return new SequencerGraphSetBaseData();
    }

    public override SequencerGraphSetBaseData readFromXML(string path)
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

        if(xmlDoc.FirstChild.Name != "SequencerGraphSet")
        {
            DebugUtil.assert(false,"Data Type Error");
            return null;
        }

        SequencerGraphSetBaseData baseData = new SequencerGraphSetBaseData();

        List<SequencerGraphBaseData> sequencerGraphBaseDataList = new List<SequencerGraphBaseData>();

        XmlNode parentNode = xmlDoc.FirstChild;
        
        XmlNodeList nodeList = parentNode.ChildNodes;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            SequencerGraphBaseData seq = SequencrGraphLoader.readSequencerGraphData(nodeList[i]);
            if(seq == null)
                return null;

            sequencerGraphBaseDataList.Add(seq);
        }

        XmlAttributeCollection attriubtes = parentNode.Attributes;
        for(int i = 0; i < attriubtes.Count; ++i)
        {
            string attrName = attriubtes[i].Name;
            string attrValue = attriubtes[i].Value;

            if(attrName == "BGM")
            {
                baseData._bgmKey = int.Parse(attrValue);
            }
            else if(attrName == "StartSequencer")
            {
                for(int j = 0; j < sequencerGraphBaseDataList.Count; ++i)
                {
                    if(attrValue == sequencerGraphBaseDataList[j]._sequencerName)
                    {
                        baseData._startIndex = j;
                        break;
                    }
                }

                DebugUtil.assert_fileOpen(false, "해당 Sequencer를 찾을 수 없습니다 [{0}]", path, XMLScriptConverter.getLineNumberFromXMLNode(parentNode), attrValue);
            }
        }

        baseData._sequencerGraphSet = sequencerGraphBaseDataList.ToArray();
        return baseData;
    }
}


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

        return readSequencerGraphData(xmlDoc.FirstChild);
    }

    public static List<XmlNode> readMacro(string path, ref List<XmlNode> nodes)
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

        for(int i = 0; i < xmlDoc.FirstChild.ChildNodes.Count; ++i)
        {
            nodes.Add(xmlDoc.FirstChild.ChildNodes[i]);
        }

        return nodes;
    }

    public static SequencerGraphBaseData readSequencerGraphData(XmlNode node)
    {
        SequencerGraphBaseData baseData = new SequencerGraphBaseData();

        XmlAttributeCollection firstNodeAttribute = node.Attributes;
        for(int i = 0; i < firstNodeAttribute.Count; ++i)
        {
            string attrName = firstNodeAttribute[i].Name;
            string attrValue = firstNodeAttribute[i].Value;

            if(attrName == "Name")
            {
                baseData._sequencerName = attrValue;
            }
        }

        List<XmlNode> macroNodes = new List<XmlNode>();

        XmlNodeList nodes = node.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            XmlNode phaseNode = nodes[nodeIndex];

            if(phaseNode.Name == "Include")
            {
                for(int i = 0; i < phaseNode.Attributes.Count; ++i)
                {
                    if(phaseNode.Attributes[i].Name == "Path")
                    {
                        readMacro(phaseNode.Attributes[i].Value, ref macroNodes);
                        break;
                    }
                }

                continue;
            }

            SequencerGraphPhaseData phaseData = readPhaseData(phaseNode, ref macroNodes);

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

    private static SequencerGraphPhaseData readPhaseData(XmlNode node, ref List<XmlNode> macroNodes)
    {
        SequencerGraphPhaseData phaseData = new SequencerGraphPhaseData();

        List<SequencerGraphEventBase> eventList = new List<SequencerGraphEventBase>();
        XmlNodeList eventNodes = node.ChildNodes;
        for(int i = 0; i < eventNodes.Count; ++i)
        {
            if(eventNodes[i].Name == "UseMacro")
            {
                for(int x = 0; x < eventNodes[i].Attributes.Count; ++x)
                {
                    if(eventNodes[i].Attributes[x].Name != "Name")
                        continue;

                    string value = eventNodes[i].Attributes[x].Value;
                    foreach(var item in macroNodes)
                    {
                        if(item.Name != value)
                            continue;

                        for(int index = 0; index < item.ChildNodes.Count; ++index)
                        {
                            SequencerGraphEventBase macroEventData = readEventData(item.ChildNodes[index]);
                            eventList.Add(macroEventData);

                        }
                        
                        break;
                    }

                    break;
                }

                continue;
            }

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
