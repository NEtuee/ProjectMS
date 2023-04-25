using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataLoader : MonoBehaviour
{
    public string statusInfoPath = "Assets\\Data\\Example\\StatusInfo.xml";
    public string buffInfoPath = "Assets\\Data\\Example\\BuffInfo.xml";
    public string keyPresetPath = "Assets\\Data\\Example\\ActionKeyPreset.xml";
    public string weightRandomPath = "Assets\\Data\\Example\\ActionKeyPreset.xml";
    public string characterInfoPath = "Assets\\Data\\StaticData\\CharacterInfo.xml";

    void Awake()
    {
        StatusInfo.setStatusInfoDataDictionary(StatusInfoLoader.readFromXML(statusInfoPath));
        StatusInfo.setBuffDataDictionary(BuffDataLoader.readFromXML(buffInfoPath));
        ActionKeyInputManager.Instance().setPresetData(ActionKeyPresetDataLoader.readFromXML(keyPresetPath));
        WeightRandomManager.Instance().setWeightGroupData(WeightRandomExporter.readFromXML(weightRandomPath));
        CharacterInfoManager.Instance().SetCharacterInfo(ResourceContainerEx.Instance().getCharacterInfo(characterInfoPath));
    }
}
