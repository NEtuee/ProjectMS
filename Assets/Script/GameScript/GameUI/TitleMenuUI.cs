using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenuUI : IUIElement
{
    private TitleMenuUIBinder _binder;
    
    private TitleMenuUIBinder.ResolutionOption _resolution = TitleMenuUIBinder.ResolutionOption.res800x600;
    private int _language = 0; // jpn, eng, kor

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
        InitLanguageButton();
        InitResolution();
        ActiveTitleMenu(false);

        _binder.OptionRoot.SetActive(false);
        _binder.CuttingLine.gameObject.SetActive(true);
    }

    public void ActiveTitleMenu(bool active)
    {
        _binder.Root.SetActive(active);
        if (active == true)
            ActiveButtons(true);
    }

    private void ActiveButtons(bool value)
    {
        _binder.StartButton.interactable = value;
        _binder.LanguageLeftButton.interactable = value;
        _binder.LanguageRightButton.interactable = value;
        _binder.ResolutionLeftButton.interactable = value;
        _binder.ResolutionRightButton.interactable = value;
    }

    private void BindButtonsAction()
    {
        _binder.StartButton.onClick.AddListener(OnClickStart);

        _binder.LanguageLeftButton.onClick.AddListener(languageButtonLeft);
        _binder.LanguageRightButton.onClick.AddListener(languageButtonRight);

        _binder.ResolutionLeftButton.onClick.AddListener(resoulutionButtonLeft);
        _binder.ResolutionRightButton.onClick.AddListener(resoulutionButtonRight);
    }

    private void OnClickStart()
    {
        ActiveButtons(false);
        
        var introStageData = ResourceContainerEx.Instance().GetStageData("StageData/Stage_1_1");
        if (introStageData == null)
            return;
        
        MasterManager.instance._stageProcessor.stopStage(true);
        MasterManager.instance._stageProcessor.startStage(introStageData,Vector3.zero,Vector3.zero);
        
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);

        _binder.Root.SetActive(false);
        Cursor.visible = false;
    }

    private void OnClickExit()
    {
        Application.Quit();
    }

    private void InitSlider()
    {
    }

    private void OnVolumeChanged(float value)
    {
    }

    private void OnSfxValueChanged(float value)
    {
        
    }

    private void InitLanguageButton()
    {
        var curLanguage = LanguageManager.GetLanguage();
        switch (curLanguage)
        {
            case SystemLanguage.Japanese:
                _language = 0;
                break;
            case SystemLanguage.English:
                _language = 1;
                break;
            case SystemLanguage.Korean:
                _language = 2;
                break;
        }

        updageLanguge();
    }

    private Vector2 _scrollOffset = Vector2.zero;
    public void UpdateByManager()
    {
        if(_binder.OptionRoot.activeInHierarchy || _binder.Root.activeInHierarchy == false)
            return;

        _scrollOffset.x += 0.04f * Time.deltaTime;
        _binder.CuttingLine.material.mainTextureOffset = _scrollOffset;

        if(Input.anyKeyDown)
        {
            _binder.OptionRoot.SetActive(true);
            _binder.CuttingLine.gameObject.SetActive(false);
        }
    }

    private void languageButtonLeft()
    {
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
        _language = _language - 1 < 0 ? 2 : _language - 1;
        updageLanguge();
    }

    private void languageButtonRight()
    {
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
        _language = _language + 1 > 2 ? 0 : _language + 1;
        updageLanguge();
    }

    private void updageLanguge()
    {
        switch (_language)
        {
            case 0:
                LanguageManager.SetLanguage(SystemLanguage.Japanese);
                _binder.LanguageText.text="<b>JPN</b>";
                break;
            case 1:
                LanguageManager.SetLanguage(SystemLanguage.English);
                _binder.LanguageText.text="<b>ENG</b>";
                break;
            case 2:
                LanguageManager.SetLanguage(SystemLanguage.Korean);
                _binder.LanguageText.text="<b>KOR</b>";
                break;
        }
    }

    private void InitResolution()
    {
        _resolution = TitleMenuUIBinder.ResolutionOption.res800x600;
        OnResolutionValueChanged();
    }

    private void resoulutionButtonLeft()
    {
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
        _resolution = _resolution == TitleMenuUIBinder.ResolutionOption.res800x600 ? TitleMenuUIBinder.ResolutionOption.res1600x1200 : TitleMenuUIBinder.ResolutionOption.res800x600;
        OnResolutionValueChanged();
    }

    private void resoulutionButtonRight()
    {
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
        _resolution = _resolution == TitleMenuUIBinder.ResolutionOption.res800x600 ? TitleMenuUIBinder.ResolutionOption.res1600x1200 : TitleMenuUIBinder.ResolutionOption.res800x600;
        OnResolutionValueChanged();
    }
    
    private void OnResolutionValueChanged()
    {
        switch(_resolution)
        {
            case TitleMenuUIBinder.ResolutionOption.res800x600:
                Screen.SetResolution(800,600,FullScreenMode.Windowed);
                _binder.ResolutionImage.sprite = _binder.resolution800;
            break;
            case TitleMenuUIBinder.ResolutionOption.res1600x1200:
                Screen.SetResolution(1600,1200,FullScreenMode.Windowed);
                _binder.ResolutionImage.sprite = _binder.resolution1600;
            break;
        }
    }
}
