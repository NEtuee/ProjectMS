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
    
    public void Awake()
    {
        if(Application.isPlaying)
            MasterManager.instance._stageProcessor.registBackgroundLayer(this);
    }

    public void Update()
    {
        if(Application.isPlaying == false)
            updateCameraPosition();
    }

    private Vector3 getCameraPosition()
    {
        return MathEx.floorNoSign(Camera.main.transform.position,2);
    }

    public void updateCameraPosition()
    {
        if(Camera.main == null)
            return;
        if(_skybox)
        {
            Vector3 position = getCameraPosition();
            position.z = 0f;
            
            Vector3 resultPosition = position + position * -_movementRate;
            if(_lockX)
                resultPosition.x = position.x;
            if(_lockY)
                resultPosition.y = position.y;

            transform.position = MathEx.floorNoSign(resultPosition,2);
        }
        else
        {
            Vector3 position = getCameraPosition() * -_movementRate;
            position.z = 0f;

            if(_lockX)
                position.x = 0f;
            if(_lockY)
                position.y = 0f;

            transform.position = MathEx.floorNoSign(position,2);
        }
    }
}
