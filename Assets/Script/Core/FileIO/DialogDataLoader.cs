using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class DialogDataLoader 
{
    public static Dictionary<string, List<BubbleCommend>> readFromXML(string path)
    {
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(path,readerSettings))
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
        
        Dictionary<string, List<BubbleCommend>> dataDic = new Dictionary<string, List<BubbleCommend>>();
        
        XmlNodeList textNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < textNodes.Count; ++nodeIndex)
        {
            XmlNodeList commandNodes = textNodes[nodeIndex].ChildNodes;
            List<BubbleCommend> commandList = new List<BubbleCommend>(commandNodes.Count);
            
            for (int commandIndex = 0; commandIndex < commandNodes.Count; commandIndex++)
            {
                var command = ReadCommend(commandNodes[commandIndex]);
                if (command == null)
                {
                    continue;
                }
                    
                commandList.Add(command);
            }
            
            dataDic[textNodes[nodeIndex].Name] = commandList;
        }

        return dataDic;
    }

    private static BubbleCommend ReadCommend(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        switch (node.Name)
        {
            case "ShowText":
                return CreateShowText(node);
            case "Wait":
                return CreateWait(node);
            case "Line":
                return CreateLine(node);
            case "SetColor":
                return CreateSetColor(node);
            case "EndColor":
                return CreateEndColor(node);
            case "SetBold":
                return CreateSetBold(node);
            case "EndBold":
                return CreateEndBold(node);
        }

        return null;
    }

    private static BubbleCommend CreateShowText(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        string text = string.Empty;
        float interval = 0.01f;
        
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrName == "Text")
                text = attrValue;
            else if(attrName == "Interval")
                interval = float.Parse(attrValue);
        }

        return new ShowText(interval, text);
    }
    
    private static BubbleCommend CreateWait(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        float time = 0.0f;
        
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if (attrName == "Time")
            {
                time = float.Parse(attrValue);
            }
        }

        return new Wait(time);
    }
    
    private static BubbleCommend CreateLine(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        return new AddLineAlignment();
    }
    
    private static BubbleCommend CreateSetColor(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        string colorHex = "ffffff";
        
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrName == "Color")
                colorHex = attrValue;
        }

        return new SetTextColor(colorHex);
    }
    
    private static BubbleCommend CreateEndColor(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        return new EndTextColor();
    }
    
    private static BubbleCommend CreateSetBold(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        return new SetBold();
    }
    
    private static BubbleCommend CreateEndBold(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        return new EndBold();
    }
}
