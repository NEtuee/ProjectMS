using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollowerBinder : UIObjectBinder
{
    public RectTransform FollowObject;
    public Vector3 Offset;
    
    [Range(0.01f, 0.1f)]
    public float Smooth = 0.075f;
    
    public override bool CheckValidLink(out string reason)
    {
        reason = string.Empty;
        return true;
    }
}
