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
        
        transform.position = Camera.main.transform.position * -_movementRate;
    }
}
