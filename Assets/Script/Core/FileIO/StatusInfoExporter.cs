using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class StatusInfoLoader
{
#if UNITY_EDITOR
    public static StatusInfoDataList readFromXMLAndExportToBinary(string path, string binaryOutputPath)
    {
        StatusInfoDataList statusInfoDataList = readFromXML(path);
        statusInfoDataList.writeData(binaryOutputPath);

        return statusInfoDataList;
    }
#endif
    public static StatusInfoDataList readData(string binaryPath)
    {
        StatusInfoDataList data = new StatusInfoDataList();
        data.readData(binaryPath);

        return data;
    }

    public static StatusInfoDataList readFromXML(string path)
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

        StatusInfoDataList statusInfoDataList = new StatusInfoDataList();
        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        try
        {
            for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
            {
                StatusInfoData baseData = readStatusInfoData(projectileNodes[nodeIndex]);
                if(baseData == null)
                    return null;

                statusInfoDataList._statusInfoList.Add(baseData._statusInfoName,baseData);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml parsing exception : {0}\n",ex.Message,xmlDoc.BaseURI);
            return null;
        }

        return statusInfoDataList;
    }

    private static StatusInfoData readStatusInfoData(XmlNode node)
    {
        List<StatusDataFloat> statusInfoDataList = new List<StatusDataFloat>();
        List<StatusGraphicInterfaceData> graphicInterfaceDataList = new List<StatusGraphicInterfaceData>();

        HashSet<string> nameCheck = new HashSet<string>();
        XmlNodeList statusNodes = node.ChildNodes;

        bool useHPEffect = false;
        uint defaultLevel = 0;

        for(int i = 0; i < statusNodes.Count; ++i)
        {
            if(statusNodes[i].Name == "Stat")
            {
                StatusDataFloat data = new StatusDataFloat();
                List<StatusDataFloat.LevelData> levelDataList = new List<StatusDataFloat.LevelData>();
                StatusDataFloat.LevelData defaultLevelData = new StatusDataFloat.LevelData();

                XmlAttributeCollection attributes = statusNodes[i].Attributes;
                for(int j = 0; j < attributes.Count; ++j)
                {
                    string attrName = attributes[j].Name;
                    string attrValue = attributes[j].Value;

                    if(attrName == "Type")
                    {
                        data._statusType = (StatusType)System.Enum.Parse(typeof(StatusType), attrValue);
                        data._statusName = data._statusType.ToString();
                    }
                    else if(attrName == "Name")
                        data._statusName = attrValue;
                    else if(attrName == "Max")
                        defaultLevelData._maxValue = float.Parse(attrValue);
                    else if(attrName == "Min")
                        defaultLevelData._minValue = float.Parse(attrValue);
                    else if(attrName == "Init")
                        defaultLevelData._initialValue = float.Parse(attrValue);
                    else
                    {
                        DebugUtil.assert(false, "invalid attribute name from statusInfo: {0}",attrName);
                        continue;
                    }
                }

                foreach(XmlNode childNode in statusNodes[i].ChildNodes)
                {
                    if(childNode.Name == "Stat")
                    {
                        StatusDataFloat.LevelData levelData = new StatusDataFloat.LevelData();
                        XmlAttributeCollection childAttributes = childNode.Attributes;

                        int level = 0;

                        for(int j = 0; j < childAttributes.Count; ++j)
                        {
                            string attrName = childAttributes[j].Name;
                            string attrValue = childAttributes[j].Value;

                            if(attrName == "Max")
                                levelData._maxValue = float.Parse(attrValue);
                            else if(attrName == "Min")
                                levelData._minValue = float.Parse(attrValue);
                            else if(attrName == "Init")
                                levelData._initialValue = float.Parse(attrValue);
                            else if(attrName == "Level")
                                level = int.Parse(attrValue);
                            else
                            {
                                DebugUtil.assert(false, "invalid attribute name from statusInfo: {0}",attrName);
                                continue;
                            }
                        }

                        if(level < 0)
                            continue;
                        
                        levelData._level = (uint)level;
                        levelDataList.Add(levelData);
                    }

                }

                levelDataList.Add(defaultLevelData);

                if(nameCheck.Contains(data._statusName))
                {
                    DebugUtil.assert(false,"status overlapError name: {0}",data._statusName);
                    return null;
                }

                nameCheck.Add(data._statusName);
                data._statusLevelData = levelDataList.ToArray();

                statusInfoDataList.Add(data);
            }
            else if(statusNodes[i].Name == "DeclareGraphicInterface")
            {
                StatusGraphicInterfaceData data = new StatusGraphicInterfaceData();
                XmlAttributeCollection attributes = statusNodes[i].Attributes;
                for(int j = 0; j < attributes.Count; ++j)
                {
                    string attrName = attributes[j].Name;
                    string attrValue = attributes[j].Value;

                    if(attrName == "Target")
                        data._targetStatus = attrValue;
                    else if(attrName == "Color")
                        data._interfaceColor = XMLScriptConverter.valueToLinearColor(attrValue);
                    else if(attrName == "HorizontalGap")
                        data._horizontalGap = float.Parse(attrValue);
                    else
                    {
                        DebugUtil.assert(false, "invalid attribute name from DeclareGraphicInterface: {0}",attrName);
                        continue;
                    }
                }

                graphicInterfaceDataList.Add(data);
            }
            else if(statusNodes[i].Name == "UseHPEffect")
            {
                useHPEffect = true;
            }
        }

        StatusInfoData statusInfoData = new StatusInfoData(node.Name,useHPEffect,defaultLevel,statusInfoDataList.ToArray(),graphicInterfaceDataList.ToArray());

        return statusInfoData;
    }

}
