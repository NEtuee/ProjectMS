using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public ActionGraphConditionCompareData compareData;
    public string targetFormula="(TargetDistance > 0.7) && true";
    void Start()
    {
        compareData = ActionGraphLoader.ReadConditionCompareData(targetFormula);
        ActionGraph graph = new ActionGraph();
        Debug.Log(graph.processActionCondition(compareData));
    }
}
