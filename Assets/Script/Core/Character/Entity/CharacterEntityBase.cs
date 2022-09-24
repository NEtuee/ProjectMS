using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEntityBase : GameEntityBase
{
    

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("SceneCharacterManager"));
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        getMovementControl().addFrameToWorld(this.transform);

    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

    }


    
}
