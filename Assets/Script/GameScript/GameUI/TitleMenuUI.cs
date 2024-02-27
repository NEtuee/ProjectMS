using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenuUI : IUIElement
{
    private TitleMenuUIBinder _binder;
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "타이틀 메뉴 ui 바인더가 없음";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as TitleMenuUIBinder;
    }

    public void Initialize()
    {
        BindButtonsAction();
        InitSlider();
        InitLanguageDropdown();
        InitResolutionDropdown();
        ActiveTitleMenu(false);
    }

    public void ActiveTitleMenu(bool active)
    {
        _binder.Root.SetActive(active);
        if (active == true)
        {
            ActiveButtons();
            _binder.OptionPanel.SetActive(false);
        }
    }

    private void ActiveButtons()
    {
        _binder.StartButton.interactable = true;
        _binder.OptionButton.interactable = true;
        _binder.ExitButton.interactable = true;
    }

    private void BindButtonsAction()
    {
        _binder.StartButton.onClick.AddListener(OnClickStart);
        _binder.OptionButton.onClick.AddListener(OnClickOption);
        _binder.ExitButton.onClick.AddListener(OnClickExit);
        _binder.ExitOptionButton.onClick.AddListener(OnClickExitOption);
    }

    private void OnClickStart()
    {
        _binder.StartButton.interactable = false;
        _binder.OptionButton.interactable = false;
        _binder.ExitButton.interactable = false;
        
        var introStageData = ResourceContainerEx.Instance().GetStageData("StageData/Stage_1_1");
        if (introStageData == null)
        {
            return;
        }
        
        MasterManager.instance._stageProcessor.stopStage(true);
        MasterManager.instance._stageProcessor.startStage(introStageData,Vector3.zero,Vector3.zero);
        
        _binder.Root.SetActive(false);
        Cursor.visible = false;
    }
    
    private void OnClickOption()
    {
        _binder.OptionPanel.SetActive(true);
    }

    private void OnClickExitOption()
    {
        _binder.OptionPanel.SetActive(false);
    }

    private void OnClickExit()
    {
        Application.Quit();
    }

    private void InitSlider()
    {
        _binder.BgmSlider.onValueChanged.AddListener(OnBgmValueChanged);
        _binder.SfxSlider.onValueChanged.AddListener(OnSfxValueChanged);
    }

    private void OnBgmValueChanged(float value)
    {
        
    }

    private void OnSfxValueChanged(float value)
    {
        
    }

    private void InitLanguageDropdown()
    {
        var optionDataList = new List<Dropdown.OptionData>();
        optionDataList.Add(new Dropdown.OptionData(TitleMenuUIBinder.LanguageOption.Korea.ToString()));
        optionDataList.Add(new Dropdown.OptionData(TitleMenuUIBinder.LanguageOption.Japan.ToString()));
        optionDataList.Add(new Dropdown.OptionData(TitleMenuUIBinder.LanguageOption.English.ToString()));

        _binder.LanguageDropdown.options = optionDataList;
        _binder.LanguageDropdown.onValueChanged.AddListener(OnLanguageValueChanged);

        var curLanguage = LanguageManager.GetLanguage();
        switch (curLanguage)
        {
            case SystemLanguage.Korean:
                _binder.LanguageDropdown.value = 0;
                break;
            case SystemLanguage.Japanese:
                _binder.LanguageDropdown.value = 1;
                break;
            case SystemLanguage.English:
                _binder.LanguageDropdown.value = 2;
                break;
        }
    }

    private void OnLanguageValueChanged(int value)
    {
        switch (value)
        {
            case 0:
                LanguageManager.SetLanguage(SystemLanguage.Korean);
                break;
            case 1:
                LanguageManager.SetLanguage(SystemLanguage.Japanese);
                break;
            default:
                LanguageManager.SetLanguage(SystemLanguage.English);
                break;
        }
    }

    private void InitResolutionDropdown()
    {
        var optionDataList = new List<Dropdown.OptionData>();
        optionDataList.Add(new Dropdown.OptionData("800 X 600"));
        optionDataList.Add(new Dropdown.OptionData("1600 X 1200"));

        _binder.ResolutionDropdown.options = optionDataList;
        _binder.ResolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);

        _binder.ResolutionDropdown.value = 0;
    }
    
    private void OnResolutionValueChanged(int value)
    {
        switch (value)
        {
            case 0:
                LanguageManager.SetLanguage(SystemLanguage.Korean);
                break;
            case 1:
                LanguageManager.SetLanguage(SystemLanguage.Japanese);
                break;
            default:
                LanguageManager.SetLanguage(SystemLanguage.English);
                break;
        }
    }
}
