using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class ActionKeyPresetDataLoader
{
#if UNITY_EDITOR
    public static ActionKeyPresetDataList readFromXMLAndExportToBinary(string path, string binaryOutputPath)
    {
        var data = readFromXML(path);
        data.writeData(binaryOutputPath);
        return data;
    }
#endif
    public static ActionKeyPresetDataList readData(string binaryPath)
    {
        ActionKeyPresetDataList data = new ActionKeyPresetDataList();
        data.readData(binaryPath);

        return data;
    }

    public static ActionKeyPresetDataList readFromXML(string path)
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

        List<ActionKeyPresetData> presetDataList = new List<ActionKeyPresetData>();
        ActionKeyPresetDataList actionKeyPresetDataList = new ActionKeyPresetDataList();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            ActionKeyPresetData presetData = readPresetData(projectileNodes[nodeIndex]);
            if(presetData == null)
                return null;

            presetDataList.Add(presetData);
        }

        actionKeyPresetDataList._actionKeyPresetList = presetDataList.ToArray();

        return actionKeyPresetDataList;
    }

    private static ActionKeyPresetData readPresetData(XmlNode node)
    {
        ActionKeyPresetData presetData = new ActionKeyPresetData();
        presetData._actionKeyName = node.Name;

        XmlAttributeCollection buffDataNodes = node.Attributes;

        for(int i = 0; i < buffDataNodes.Count; ++i)
        {
            string attrName = buffDataNodes[i].Name;
            string attrValue = buffDataNodes[i].Value;

            if(attrName == "PressType")
                presetData._pressType = (ActionKeyPressType)System.Enum.Parse(typeof(ActionKeyPressType), attrValue);
            else if(attrName == "MultiInputType")
                presetData._multiInputType = (ActionKeyMultiInputType)System.Enum.Parse(typeof(ActionKeyMultiInputType), attrValue);
            else if(attrName == "Threshold")
                presetData._multiInputThreshold = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "Key_KM")
            {
                string[] keyList = attrValue.Split(' ');
                int index = (int)ControllerEx.ControllerType.KeyboardMouse;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else if(attrName == "Key_XBOX")
            {
                string[] keyList = attrValue.Split(' ');
                int index = (int)ControllerEx.ControllerType.XboxController;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else if(attrName == "Key_PS")
            {
                string[] keyList = attrValue.Split(' ');
                int index = (int)ControllerEx.ControllerType.PSController;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute name from buffInfo: {0}",attrName);
                continue;
            }

        }


        return presetData;
    }

}
