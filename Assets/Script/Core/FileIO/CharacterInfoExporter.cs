using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class CharacterInfoLoader
{
    public static Dictionary<string,CharacterInfoData> readFromXML(string path)
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

        Dictionary<string,CharacterInfoData> characterInfoDataDictionary = new Dictionary<string,CharacterInfoData>();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            CharacterInfoData characterInfoData = readCharacterInfo(projectileNodes[nodeIndex]);
            if(characterInfoData == null)
                return null;

            characterInfoDataDictionary.Add(projectileNodes[nodeIndex].Name, characterInfoData);
        }

        return characterInfoDataDictionary;
    }


    public static CharacterInfoData readCharacterInfo(XmlNode node)
    {
        CharacterInfoData characterInfoData = new CharacterInfoData();
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrName == "DisplayName")
                characterInfoData._displayName = attrValue;
            else if(attrName == "ActionGraph")
                characterInfoData._actionGraphPath = attrValue;
            else if(attrName == "AIGraph")
                characterInfoData._aiGraphPath = attrValue;
            else if(attrName == "StatusInfoName")
                characterInfoData._statusName = attrValue;
            else if(attrName == "CollisionRadius")
                characterInfoData._characterRadius = float.Parse(attrValue);
        }

        return characterInfoData;
    }
}
