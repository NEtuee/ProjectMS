using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashPointUIBinder : UIObjectBinder
{
    public Image[] DashPointImages;

    public Color _color1;
    public Color _color2;
    public Color _color3;
    public Color _color4;

    public override bool CheckValidLink(out string reason)
    {
        if (DashPointImages == null || DashPointImages.Length != 4)
        {
            reason = "대쉬 포인트 바인더에 이미지 컴포넌트가 4개가 아닙니다. 갯수를 확인해주세요.";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
