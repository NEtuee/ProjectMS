using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BackgroundLayer : MonoBehaviour
{
    public float _movementRate = 1f;
    void Update()
    {
        if(Camera.main == null)
            return;
        
        Vector3 position = Camera.main.transform.position * -_movementRate;
        position.z = 0f;

        transform.position = position;
    }
}
