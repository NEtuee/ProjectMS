using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;


public class AllyInfoLoader : LoaderBase<AllyInfoDataList>
{
    public override AllyInfoDataList createNewDataInstance()
    {
        return new AllyInfoDataList();
    }

    public override AllyInfoDataList readFromXML(string path)
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

        AllyInfoDataList allyInfoDataDictionary = new AllyInfoDataList();

        XmlNodeList allyInfoNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < allyInfoNodes.Count; ++nodeIndex)
        {
            AllyInfoData allyInfoData = new AllyInfoData();
            allyInfoData._index = nodeIndex;
            allyInfoData._key = allyInfoNodes[nodeIndex].Name;

            allyInfoDataDictionary._allyInfoDataDic.Add(allyInfoNodes[nodeIndex].Name, allyInfoData);
        }

        allyInfoNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < allyInfoNodes.Count; ++nodeIndex)
        {
            string attrName = allyInfoNodes[nodeIndex].Name;
            AllyInfoData allyInfo = allyInfoDataDictionary._allyInfoDataDic[attrName];

            readAllyInfo(allyInfoNodes[nodeIndex], ref allyInfo, ref allyInfoDataDictionary);
        }

        return allyInfoDataDictionary;
    }


    public static void readAllyInfo(XmlNode node, ref AllyInfoData allyInfo, ref AllyInfoDataList allyInfoDataDictionary)
    {
        List<int> allyGroup = new List<int>();
        List<int> enemyGroup = new List<int>();
        List<int> neutralGroup = new List<int>();

        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            string[] splittedList = attrValue.Split(' ');
            foreach(var item in splittedList)
            {
                if(allyInfoDataDictionary._allyInfoDataDic.ContainsKey(item) == false)
                {
                    DebugUtil.assert_fileOpen(false,"존재하지 않는 AllyInfo Key [{0}]를 사용하고 있습니다. 오타는 아닌가요?",node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node),item);
                    return;
                }
            }

            if(attrName == "AllyGroup")
            {
                foreach(var item in splittedList)
                {
                    allyGroup.Add(allyInfoDataDictionary._allyInfoDataDic[item]._index);
                }
            }
            else if(attrName == "EnemyGroup")
            {
                foreach(var item in splittedList)
                {
                    enemyGroup.Add(allyInfoDataDictionary._allyInfoDataDic[item]._index);
                }
            }
            else if(attrName == "NeutralGroup")
            {
                foreach(var item in splittedList)
                {
                    neutralGroup.Add(allyInfoDataDictionary._allyInfoDataDic[item]._index);
                }
            }
        }

        allyInfo._allyGroup = allyGroup.Count == 0 ? null : allyGroup.ToArray();
        allyInfo._enemyGroup = enemyGroup.Count == 0 ? null : enemyGroup.ToArray();
        allyInfo._neutralGroup = neutralGroup.Count == 0 ? null : neutralGroup.ToArray();
    }
}
