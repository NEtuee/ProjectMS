using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // SpriteRenderer _spriteRenderer;
    // ActionGraph _actionGraph;
    // void Start()
    // {
    //     _actionGraph = new ActionGraph(ActionGraphLoader.readFromXML(IOControl.PathForDocumentsFile("Assets\\Data\\Example\\ActionGraphTest.xml")));
    //     _actionGraph.Initialize();

    //     _spriteRenderer = GetComponent<SpriteRenderer>();
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     string prevActionName = _actionGraph.getCurrentActionName();
    //     _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, Input.GetKey(KeyCode.A));

    //     if(_actionGraph.Progress(Time.deltaTime))
    //     {
    //         Debug.Log("execute : " + prevActionName + " -> " + _actionGraph.getCurrentActionName());
    //     }

    //     _spriteRenderer.sprite = _actionGraph.getCurrentSprite();
    // }
}
