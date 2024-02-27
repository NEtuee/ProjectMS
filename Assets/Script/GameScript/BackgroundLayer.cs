using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BackgroundLayer : MonoBehaviour
{
    public float _movementRate = 1f;
    public bool _skybox = false;

    public bool _lockX = false;
    public bool _lockY = false;
    
    void Update()
    {
        if(Camera.main == null)
            return;
        
        if(_skybox)
        {
            Vector3 position = Camera.main.transform.position;
            position.z = 0f;
            
            Vector3 resultPosition = position + position * -_movementRate;
            if(_lockX)
                resultPosition.x = position.x;
            if(_lockY)
                resultPosition.y = position.y;

            transform.position = resultPosition;
        }
        else
        {
            Vector3 position = Camera.main.transform.position * -_movementRate;
            position.z = 0f;

            if(_lockX)
                position.x = 0f;
            if(_lockY)
                position.y = 0f;

            transform.position = position;
        }
    }
}
