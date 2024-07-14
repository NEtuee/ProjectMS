using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class CharacterInfoLoader : LoaderBase<CharacterInfoDataList>
{
    public override CharacterInfoDataList createNewDataInstance()
    {
        return new CharacterInfoDataList();
    }

    public override CharacterInfoDataList readFromXML(string path)
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

        CharacterInfoDataList characterInfoDataDictionary = new CharacterInfoDataList();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            CharacterInfoData characterInfoData = readCharacterInfo(projectileNodes[nodeIndex]);
            if(characterInfoData == null)
                return null;

            characterInfoDataDictionary._characterInfoDataDic.Add(projectileNodes[nodeIndex].Name, characterInfoData);
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
            else if(attrName == "HeadUpOffset")
                characterInfoData._headUpOffset = float.Parse(attrValue);
            else if(attrName == "AllyInfo")
                characterInfoData._allyInfoKey = attrValue;
            else if(attrName == "DefaultMaterial")
                characterInfoData._defaultMaterial = (CommonMaterial)System.Enum.Parse(typeof(CommonMaterial), attrValue);
            else if(attrName == "IndicatorVisible")
                characterInfoData._indicatorVisible = bool.Parse(attrValue);
            else if(attrName == "UseCameraBoundLock")
                characterInfoData._useCameraBoundLock = bool.Parse(attrValue);
            else if(attrName == "UseHPInterface")
                characterInfoData._useHpInterface = bool.Parse(attrValue);
            else if(attrName == "SelfCollision")
                characterInfoData._selfCollision = bool.Parse(attrValue);
            else if(attrName == "ImmortalCharacter")
                characterInfoData._immortalCharacter = bool.Parse(attrValue);
            else if(attrName == "UseInputBuffer")
                characterInfoData._useInpuBuffer = bool.Parse(attrValue);
        }

        return characterInfoData;
    }
}
