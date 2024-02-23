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

    public static void loadStaticData()
    {
        EffectInfoManager.Instance().setEffectInfoData(EffectInfoExporter.readFromXML(effectInfoPath));
        StatusInfo.setStatusInfoDataDictionary(StatusInfoLoader.readFromXML(statusInfoPath));
        StatusInfo.setBuffDataDictionary(BuffDataLoader.readFromXML(buffInfoPath));
        ActionKeyInputManager.Instance().setPresetData(ActionKeyPresetDataLoader.readFromXML(keyPresetPath));
        WeightRandomManager.Instance().setWeightGroupData(WeightRandomExporter.readFromXML(weightRandomPath));
        CharacterInfoManager.Instance().SetCharacterInfo(ResourceContainerEx.Instance().getCharacterInfo(characterInfoPath));
        LanguageManager.InitLocalize();
    }
}
