using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntityBase : GameEntityBase
{
    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("SceneCharacterManager"));
    }

    public override void fixedProgress(float deltaTime)
    {
        base.fixedProgress(deltaTime);

        getMovementControl().addFrameToWorld(this.transform);
    }
}
