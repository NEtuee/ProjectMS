using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBubbleBinder : UIObjectBinder
{
    public Image BackGroundImage;
    public Text TextComp;
    public float WidthPadding = 10f;
    public float HeightPadding = 10f;
    public Vector3 FollowOffset;

    public override bool CheckValidLink(out string reason)
    {
        if (BackGroundImage == null)
        {
            reason = "말풍선 바인더에 백그라운드 이미지 컴포넌트가 없습니다";
            return false;
        }
        
        if (TextComp == null)
        {
            reason = "말풍선 바인더에 텍스트 컴포넌트가 없습니다";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }
}
