using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour
{
    public void Start()
    {
        ActionGraphConditionCompareData compareData = ActionGraphLoader.ReadConditionCompareData("((90.0 >= 45.0)) && ((90.0 < 135.0)) == false");
        
        for(int i = 0; i < compareData._compareTypeCount; ++i)
        {
            Debug.Log( compareData._compareTypeArray[i]);
        }

        for(int i = 0; i < compareData._conditionNodeDataCount; ++i)
        {
            Debug.Log( compareData._conditionNodeDataArray[i]._symbolName);
        }
    }

    public void Update()
    {
        
    }
}
