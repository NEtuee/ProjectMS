using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BackgroundLayer : MonoBehaviour
{
    public float _movementRate = 1f;
    public bool _skybox = false;
    
    void Update()
    {
        if(Camera.main == null)
            return;
        
        if(_skybox)
        {
            Vector3 position = Camera.main.transform.position;
            position.z = 0f;
            transform.position = position + position * -_movementRate;
        }
        else
        {
            Vector3 position = Camera.main.transform.position * -_movementRate;
            position.z = 0f;

            transform.position = position;
        }
    }
}
