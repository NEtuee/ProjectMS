using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataLoader : MonoBehaviour
{
    public string statusInfoPath = "Assets\\Data\\Example\\StatusInfo.xml";
    public string buffInfoPath = "Assets\\Data\\Example\\BuffInfo.xml";

    
    void Awake()
    {
        StatusInfo.setStatusInfoDataDictionary(StatusInfoLoader.readFromXML(statusInfoPath));
        StatusInfo.setBuffDataDictionary(BuffDataLoader.readFromXML(buffInfoPath));
    }
}
