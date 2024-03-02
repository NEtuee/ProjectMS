using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenuUIBinder : UIObjectBinder
{
    public GameObject Root;
    public GameObject OptionRoot;

    [Header("Title Menu")]
    public Button StartButton;

    [Header("Option")]
    public Slider VolumeSlider;

    public Button ResolutionLeftButton;
    public Button ResolutionRightButton;

    public Button LanguageLeftButton;
    public Button LanguageRightButton;

    public Image ResolutionImage;
    public Image CuttingLine;
    public Text LanguageText;

    public Sprite resolution800;
    public Sprite resolution1600;
    
    public enum ResolutionOption
    {
        res800x600 = 0,
        res1600x1200 = 1
    }
    
    public override bool CheckValidLink(out string reason)
    {
        if (Root == null)
        {
            reason = "메인메뉴 ui 루트 오브젝트가 없음";
            return false;
        }

        if (OptionRoot == null)
        {
            reason = "메인메뉴 ui 루트 오브젝트가 없음";
            return false;
        }
        
        if (StartButton == null)
        {
            reason = "메인메뉴 ui에 스타트 버튼이 없음";
            return false;
        }
        
        if (VolumeSlider == null)
        {
            reason = "메인메뉴 ui에 VolumeSlider 없음";
            return false;
        }
        
        if (ResolutionLeftButton == null)
        {
            reason = "메인메뉴 ui에 ResolutionLeftButton 없음";
            return false;
        }
        
        if (ResolutionRightButton == null)
        {
            reason = "메인메뉴 ui에 ResolutionRightButton 없음";
            return false;
        }
        
        if (LanguageLeftButton == null)
        {
            reason = "메인메뉴 ui에 LanguageLeftButton 없음";
            return false;
        }
        
        if (LanguageRightButton == null)
        {
            reason = "메인메뉴 ui에 LanguageRightButton 없음";
            return false;
        }

        if (ResolutionImage == null)
        {
            reason = "메인메뉴 ui에 ResolutionImage 없음";
            return false;
        }

        if (CuttingLine == null)
        {
            reason = "메인메뉴 ui에 CuttingLine 없음";
            return false;
        }

        if (LanguageText == null)
        {
            reason = "메인메뉴 ui에 LanguageText 없음";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }
}
