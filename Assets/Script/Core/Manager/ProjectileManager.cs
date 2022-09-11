using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ManagerBase
{
    public override void assign()
    {
        base.assign();
        CacheUniqueID("ProjectileManager");
        RegisterRequest();
    }
}
