using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollowerBinder : UIObjectBinder
{
    public RectTransform FollowObject;
    public Vector3 Offset;
    public float WidthAdjust = 80.0f;
    public float HeightAdjust = 40.0f;
    public CanvasGroup Group;
    
    [Range(0.01f, 0.1f)]
    public float Smooth = 0.075f;
    
    public override bool CheckValidLink(out string reason)
    {
        if (FollowObject == null)
        {
            reason = "팔로우 타겟 오브젝트가 없음";
            return false;
        }

        if (Group == null)
        {
            reason = "캔버스 그룹이 없음";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }
}
