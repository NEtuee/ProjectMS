using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHpBinder : UIObjectBinder
{
    public EnemyHpObject Prefab;
    public EnemyHpObjectMax3 PrefabMax3;
    public Vector3 FollowOffset;
    
    public override bool CheckValidLink(out string reason)
    {
        if (Prefab == null)
        {
            reason = "적 hp 프리팹이 연결되어 있지 않음";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
