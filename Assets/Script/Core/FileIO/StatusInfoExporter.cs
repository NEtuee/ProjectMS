using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;


public static class StatusInfoLoader
{
    public static Dictionary<string, StatusInfoData> readFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(path);
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

        Dictionary<string, StatusInfoData> StatusInfoDataList = new Dictionary<string, StatusInfoData>();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            StatusInfoData baseData = readStatusInfoData(projectileNodes[nodeIndex]);
            if(baseData == null)
                return null;

            StatusInfoDataList.Add(baseData._statusInfoName,baseData);
        }

        return StatusInfoDataList;
    }

    private static StatusInfoData readStatusInfoData(XmlNode node)
    {
        List<StatusDataFloat> statusInfoDataList = new List<StatusDataFloat>();

        XmlNodeList statusNodes = node.ChildNodes;

        for(int i = 0; i < statusNodes.Count; ++i)
        {
            if(statusNodes[i].Name == "Stat")
            {
                StatusDataFloat data = new StatusDataFloat();
                XmlAttributeCollection attributes = statusNodes[i].Attributes;
                for(int j = 0; j < attributes.Count; ++j)
                {
                    string attrName = attributes[j].Name;
                    string attrValue = attributes[j].Value;

                    if(attrName == "Type")
                        data._statusType = (StatusType)System.Enum.Parse(typeof(StatusType), attrValue);
                    else if(attrName == "Name")
                        data._statusName = attrValue;
                    else if(attrName == "Max")
                        data._maxValue = float.Parse(attrValue);
                    else if(attrName == "Min")
                        data._minValue = float.Parse(attrValue);
                    else if(attrName == "Init")
                        data._initialValue = float.Parse(attrValue);
                    else
                    {
                        DebugUtil.assert(false, "invalid attribute name from statusInfo: {0}",attrName);
                        continue;
                    }
                }

                statusInfoDataList.Add(data);
            }
        }

        StatusInfoData statusInfoData = new StatusInfoData(node.Name,statusInfoDataList.ToArray());

        return statusInfoData;
    }

}
