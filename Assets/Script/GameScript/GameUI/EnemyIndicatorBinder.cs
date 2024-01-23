using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicatorBinder : UIObjectBinder
{
    public override bool CheckValidLink(out string reason)
    {
        reason = string.Empty;
        return true;
    }
}
