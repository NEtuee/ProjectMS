using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBubblePoolBinder : UIObjectBinder
{
    public TextBubbleObject TextBubblePrefab;
    public Vector2 RandomRange = new Vector2(8f, 8f);
    
    public override bool CheckValidLink(out string reason)
    {
        if (TextBubblePrefab == null)
        {
            reason = "말풍선 복사 프리팹이 연결되어 있지 않음";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }
}
