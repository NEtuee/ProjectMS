using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticDataLoader
{
    public static string statusInfoPath = "Assets/Data/StaticData/StatusInfo.xml";
    public static string buffInfoPath = "Assets/Data/StaticData/BuffInfo.xml";
    public static string keyPresetPath = "Assets/Data/StaticData/ActionKeyPreset.xml";
    public static string weightRandomPath = "Assets/Data/StaticData/WeightRandom.xml";
    public static string characterInfoPath = "Assets/Data/StaticData/CharacterInfo.xml";
    public static string effectInfoPath = "Assets/Data/StaticData/EffectInfo.xml";
    public static string allyInfoPath = "Assets/Data/StaticData/AllyInfo.xml";

#if UNITY_EDITOR

    public static void writeStaticDataAll(ResourceMap resourceMap)
    {
        {
            string characterInfoPathReal = resourceMap.findResourcePath(characterInfoPath);
            var item = ResourceContainerEx.Instance().getCharacterInfoList(characterInfoPath);
            CharacterInfoManager.Instance().SetCharacterInfo(item._characterInfoDataDic);

            item.writeData(characterInfoPathReal);
        }

        {
            string keyPresetPathReal = resourceMap.findResourcePath(keyPresetPath);
            ActionKeyInputManager.Instance().setPresetData(ActionKeyPresetDataLoader.readFromXMLAndExportToBinary(keyPresetPath, keyPresetPathReal));
        }

        {
            string weightRandomPathReal = resourceMap.findResourcePath(weightRandomPath);
            WeightRandomManager.Instance().setWeightGroupData(WeightRandomExporter.readFromXMLAndExportToBinary(weightRandomPath, weightRandomPathReal));
        }

        {
            string effectInfoPathReal = resourceMap.findResourcePath(effectInfoPath);
            EffectInfoManager.Instance().setEffectInfoData(EffectInfoExporter.readFromXMLAndExportToBinary(effectInfoPath, effectInfoPathReal));
        }

        {
            string statusInfoPathReal = resourceMap.findResourcePath(statusInfoPath);
            StatusInfo.setStatusInfoDataDictionary(StatusInfoLoader.readFromXMLAndExportToBinary(statusInfoPath,statusInfoPathReal));
        }

        {
            string buffInfoPathReal = resourceMap.findResourcePath(buffInfoPath);
            StatusInfo.setBuffDataDictionary(BuffDataLoader.readFromXMLAndExportToBinary(buffInfoPath, buffInfoPathReal));
        }

        {
            string allyInfoPathReal = resourceMap.findResourcePath(allyInfoPath);
            var data = ResourceContainerEx.Instance().GetAllyInfoDataList(allyInfoPath);
            AllyInfoManager.Instance().SetAllyInfo(data._allyInfoDataDic);
            data.writeData(allyInfoPathReal);
        }

    }
#endif

    public static void readStaticDataAll(ResourceMap resourceMap)
    {
        {
            CharacterInfoManager.Instance().SetCharacterInfo(ResourceContainerEx.Instance().getCharacterInfo(characterInfoPath));
        }

        {
            string keyPresetPathReal = resourceMap.findResourcePath(keyPresetPath);
            ActionKeyInputManager.Instance().setPresetData(ActionKeyPresetDataLoader.readData(keyPresetPathReal));
        }

        {
            string weightRandomPathReal = resourceMap.findResourcePath(weightRandomPath);
            WeightRandomManager.Instance().setWeightGroupData(WeightRandomExporter.readData( weightRandomPathReal));
        }

        {
            string effectInfoPathReal = resourceMap.findResourcePath(effectInfoPath);
            EffectInfoManager.Instance().setEffectInfoData(EffectInfoExporter.readData(effectInfoPathReal));
        }

        {
            string statusInfoPathReal = resourceMap.findResourcePath(statusInfoPath);
            StatusInfo.setStatusInfoDataDictionary(StatusInfoLoader.readData(statusInfoPathReal));
        }

        {
            string buffInfoPathReal = resourceMap.findResourcePath(buffInfoPath);
            StatusInfo.setBuffDataDictionary(BuffDataLoader.readData(buffInfoPathReal));
        }

        {
            AllyInfoManager.Instance().SetAllyInfo(ResourceContainerEx.Instance().getAllyInfo(allyInfoPath));
        }

        LanguageManager.InitLocalize();
    }

    public static void loadStaticData()
    {
#if UNITY_EDITOR
        writeStaticDataAll(ResourceMap.Instance());
#endif

        readStaticDataAll(ResourceMap.Instance());
        LanguageManager.InitLocalize();
    }
}
