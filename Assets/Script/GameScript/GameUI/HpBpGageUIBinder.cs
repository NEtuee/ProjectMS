using UnityEngine;
using UnityEngine.UI;

public class HpBpGageUIBinder : UIObjectBinder
{
    public Image HpGageImage;
    public Image BpGageImgae;
    public Image QteImage;
    
    public override bool CheckValidLink(out string reason)
    {
        if (HpGageImage == null)
        {
            reason = "HP 게이지 이미지 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (BpGageImgae == null)
        {
            reason = "HP 게이지 이미지 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (QteImage == null)
        {
            reason = "Qte 게이지 이미지 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
