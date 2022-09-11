using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraControl : ObjectBase
{
    public Transform targetTransform;
    public override void assign()
    {
        base.assign();
        
    }
    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("ObjectManager"));
    }
    
    //fix
    public override void afterProgress(float deltaTime)
    {
        base.fixedProgress(deltaTime);
        Vector3 newPosition = Vector3.Lerp(this.transform.position,targetTransform.position,0.3f);
        newPosition.z = -10f;
        transform.position = newPosition;
    }
}
