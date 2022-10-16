using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public string targetPath="";
    public AIGraphBaseData _aiBaseData;
    void Start()
    {
        _aiBaseData = AIGraphLoader.readFromXML(targetPath);
    }
}
