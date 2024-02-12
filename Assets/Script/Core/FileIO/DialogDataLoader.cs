using System;
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
                    
                commandList.AddRange(command);
            }
            
            dataDic[textNodes[nodeIndex].Name] = commandList;
        }

        return dataDic;
    }

    private static IList<BubbleCommend> ReadCommend(XmlNode node)
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

    private static IList<BubbleCommend> CreateShowText(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        string text = string.Empty;
        float interval = 0.01f;
        bool isEndLine = true;
        bool isClear = false;
        
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if (attrName == "Text")
            {
                text = attrValue;
            }
            else if (attrName == "Interval")
            {
                interval = float.Parse(attrValue);
            }
            else if (attrName == "EndLine")
            {
                isEndLine = Boolean.Parse(attrValue);
            }
            else if (attrName == "Clear")
            {
                isClear = Boolean.Parse(attrValue);
            }
        }

        var result = new List<BubbleCommend>();

        if (isClear == true)
        {
            result.Add(new Clear());
        }
        
        result.Add(new ShowText(interval, text));
        
        if (isEndLine == true)
        {
            result.Add(new AddLineAlignment());
        }
        
        return result;
    }
    
    private static IList<BubbleCommend> CreateWait(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        float time = 0.0f;
        string input = string.Empty;
        
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if (attrName == "Time")
            {
                time = float.Parse(attrValue);
            }
            else if (attrName == "Input")
            {
                input = attrValue;
            }
        }
        
        var result = new List<BubbleCommend>();
        if (input != string.Empty)
        {
            result.Add(new WaitInput(input));
        }
        else
        {
            result.Add(new Wait(time));
        }

        return result;
    }
    
    private static BubbleCommend CreateLine(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        return new AddLineAlignment();
    }
    
    private static  IList<BubbleCommend> CreateSetColor(XmlNode node)
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
        
        var result = new List<BubbleCommend>();
        result.Add(new SetTextColor(colorHex));
        return result;
    }
    
    private static IList<BubbleCommend> CreateEndColor(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }
        
        var result = new List<BubbleCommend>();
        result.Add(new EndTextColor());
        return result;
    }
    
    private static IList<BubbleCommend> CreateSetBold(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        var result = new List<BubbleCommend>();
        result.Add(new SetBold());
        return result;
    }
    
    private static IList<BubbleCommend> CreateEndBold(XmlNode node)
    {
        if (node == null)
        {
            return null;
        }

        var result = new List<BubbleCommend>();
        result.Add(new EndBold());
        return result;
    }
}
