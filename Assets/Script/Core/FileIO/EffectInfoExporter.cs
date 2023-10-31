using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class EffectInfoExporter
{
    public static Dictionary<string,EffectInfoDataBase> readFromXML(string path)
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

        Dictionary<string,EffectInfoDataBase> effectInfoDataDictionary = new Dictionary<string,EffectInfoDataBase>();

        XmlNodeList effectInfoNodes = xmlDoc.FirstChild.ChildNodes;
        for(int index = 0; index < effectInfoNodes.Count; ++index)
        {
            if(effectInfoNodes[index].Name == "ParticleEffect")
            {
                ParticleEffectInfoData effectInfoData = readParticleEffectInfoData(effectInfoNodes[index], path);
                if(effectInfoData == null)
                    return null;

                effectInfoDataDictionary.Add(effectInfoData._key, effectInfoData);
            }
        }

        return effectInfoDataDictionary;
    }

    private static ParticleEffectInfoData readParticleEffectInfoData(XmlNode node, string filePath)
    {
        if(node == null)
            return null;

        ParticleEffectInfoData effectInfoData = new ParticleEffectInfoData();
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrValue == "")
                continue;

            if(attrName == "Path")
            {
                effectInfoData._effectPath = attrValue;
            }
            else if(attrName == "Key")
            {
                effectInfoData._key = attrValue;
            }
            else if(attrName == "Offset")
            {
                string[] vector = attrValue.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert_fileOpen(false, "invalid vector3 data: {0}", node.BaseURI, XMLScriptConverter.getLineNumberFromXMLNode(node),attrValue);
                    return null;
                }

                effectInfoData._spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                effectInfoData._spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                effectInfoData._spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attrName == "ToTarget")
            {
                effectInfoData._toTarget = bool.Parse(attrValue);
            }
            else if(attrName == "UpdateType")
            {
                effectInfoData._effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attrValue);
            }
            else if(attrName == "Attach")
            {
                effectInfoData._attach = bool.Parse(attrValue);
            }
            else if(attrName == "AngleType")
            {
                effectInfoData._angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attrValue);
            }
            else if(attrName == "LifeTime")
            {
                effectInfoData._lifeTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "FollowDirection")
            {
                effectInfoData._followDirection = bool.Parse(attrValue);
            }
            else if(attrName == "CastShadow")
            {
                effectInfoData._castShadow = bool.Parse(attrValue);
            }
        }

        if(effectInfoData._key == "")
            DebugUtil.assert_fileOpen(false, "key is essential", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node));

        if(effectInfoData._effectPath == "")
            DebugUtil.assert_fileOpen(false, "effect path is essential", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node));

        return effectInfoData;
    }
}
