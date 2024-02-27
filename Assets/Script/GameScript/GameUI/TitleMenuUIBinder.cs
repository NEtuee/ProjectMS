using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenuUIBinder : UIObjectBinder
{
    public GameObject Root;
    
    [Header("Title Menu")]
    public Button StartButton;
    public Button OptionButton;
    public Button ExitButton;

    [Header("Option")]
    public GameObject OptionPanel;
    public Button ExitOptionButton;
    public Slider BgmSlider;
    public Slider SfxSlider;
    public Dropdown LanguageDropdown;
    public Dropdown ResolutionDropdown;
    
    public enum LanguageOption
    {
        Korea = 0,
        Japan = 1,
        English = 2,
    }
    
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
        
        if (StartButton == null)
        {
            reason = "메인메뉴 ui에 스타트 버튼이 없음";
            return false;
        }
        
        if (OptionButton == null)
        {
            reason = "메인메뉴 ui에 옵션 버튼이 없음";
            return false;
        }
        
        if (ExitButton == null)
        {
            reason = "메인메뉴 ui에 끝내기 버튼이 없음";
            return false;
        }
        
        if (BgmSlider == null)
        {
            reason = "메인메뉴 ui에 bgm슬라이더가 없음";
            return false;
        }
        
        if (SfxSlider == null)
        {
            reason = "메인메뉴 ui에 sfx슬라이더가 없음";
            return false;
        }
        
        if (LanguageDropdown == null)
        {
            reason = "메인메뉴 ui에 언어 드롭다운이 없음";
            return false;
        }
        
        if (ResolutionDropdown == null)
        {
            reason = "메인메뉴 ui에 해상도 드롭다운이 없음";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }
}
