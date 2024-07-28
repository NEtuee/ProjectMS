using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionUIControl : MonoBehaviour
{
    enum DetailUIType
    {
        Lang,
        BGM,
        SFX,
        Res,
        None,
    }

    public GameObject _menuObject;
    public Image _detailObject;

    public Image _langObject;
    public Image _BGMObject;
    public Image _SFXObject;
    public Image _ResObject;

    public Animator _textAnimation;
    public Animator _descriptionAnimation;

    public TextMeshProUGUI   _langText;
    public TextMeshProUGUI[] _bgmText;
    public TextMeshProUGUI[] _sfxText;
    public TextMeshProUGUI[] _resText;

    public LocalizationText _detailText;
    public LocalizationText _descriptionText;

    private AnimationPlayer _detailAnimation = new AnimationPlayer();

    private AnimationPlayer _langAnimation = new AnimationPlayer();
    private AnimationPlayer _bgmAnimation = new AnimationPlayer();
    private AnimationPlayer _sfxAnimation = new AnimationPlayer();
    private AnimationPlayer _resAnimation = new AnimationPlayer();

    private DetailUIType _detailUIType = DetailUIType.Lang;

    public void initialize()
    {
        _menuObject.SetActive(true);
        _detailObject.gameObject.SetActive(false);

        _detailAnimation.initialize();
        _bgmAnimation.initialize();
        _langAnimation.initialize();
        _sfxAnimation.initialize();
        _resAnimation.initialize();

        _descriptionAnimation.SetTrigger("Idle");
        _textAnimation.SetTrigger("Idle");

        updateBGM();
        updateSFX();
        updateRes();
        updateLang();
    }

    public void Start()
    {
        initialize();
    }

    public void Update()
    {
        float deltaTime = Time.deltaTime;
        _detailAnimation.progress(deltaTime,null);

        if(_detailObject.gameObject.activeInHierarchy)
        {
            _detailObject.sprite = _detailAnimation.getCurrentSprite();

            if(_detailAnimation.isEnd())
            {
                _detailText.gameObject.SetActive(true);
                _descriptionText.gameObject.SetActive(true);
                _menuObject.SetActive(false);

                switch(_detailUIType)
                {
                    case DetailUIType.Lang:
                        _langObject.gameObject.SetActive(true);
                        _BGMObject.gameObject.SetActive(false);
                        _SFXObject.gameObject.SetActive(false);
                        _ResObject.gameObject.SetActive(false);

                        _langAnimation.progress(deltaTime,null);
                        _langObject.sprite = _langAnimation.getCurrentSprite();

                    break;
                    case DetailUIType.BGM:
                        _langObject.gameObject.SetActive(false);
                        _BGMObject.gameObject.SetActive(true);
                        _SFXObject.gameObject.SetActive(false);
                        _ResObject.gameObject.SetActive(false);

                        _bgmAnimation.progress(deltaTime,null);
                        _BGMObject.sprite = _bgmAnimation.getCurrentSprite();

                    break;
                    case DetailUIType.SFX:
                        _langObject.gameObject.SetActive(false);
                        _BGMObject.gameObject.SetActive(false);
                        _SFXObject.gameObject.SetActive(true);
                        _ResObject.gameObject.SetActive(false);

                        _sfxAnimation.progress(deltaTime,null);
                        _SFXObject.sprite = _sfxAnimation.getCurrentSprite();

                    break;
                    case DetailUIType.Res:
                        _langObject.gameObject.SetActive(false);
                        _BGMObject.gameObject.SetActive(false);
                        _SFXObject.gameObject.SetActive(false);
                        _ResObject.gameObject.SetActive(true);

                        _ResObject.sprite = _resAnimation.getCurrentSprite();
                        _resAnimation.progress(deltaTime,null);

                    break;
                }
            }
        }
        
    }

    public void onMousePress()
    {
        _textAnimation.SetTrigger("Press");
    }

    public void onMouseUp()
    {
        _textAnimation.SetTrigger("Up");
    }

    public void updateLanguage()
    {
        _detailText.updateLanguage();
        _descriptionText.updateLanguage();
    }

    public void updateDetailText(string key)
    {
        _detailText.updateString(key);
    }

    public void updateDescriptionText(string key)
    {
        _descriptionText.updateString(key);
    }

    public void closeDetail()
    {
        _detailUIType = DetailUIType.None;
        
        _langObject.gameObject.SetActive(false);
        _BGMObject.gameObject.SetActive(false);
        _SFXObject.gameObject.SetActive(false);
        _ResObject.gameObject.SetActive(false);

        _detailText.gameObject.SetActive(false);
        _descriptionText.gameObject.SetActive(false);

        _detailAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/DetailBox/Closing");
        _detailAnimation.setOnAnimationEnd(()=>{
            _detailObject.gameObject.SetActive(false);
            _menuObject.SetActive(true);
        });
    }

    public void openLangDetail()
    {
        _detailObject.gameObject.SetActive(true);
        _detailAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/DetailBox/Opening");
        _langAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Idle");
        _detailUIType = DetailUIType.Lang;

        _langObject.gameObject.SetActive(false);
        _BGMObject.gameObject.SetActive(false);
        _SFXObject.gameObject.SetActive(false);
        _ResObject.gameObject.SetActive(false);

        _detailText.gameObject.SetActive(true);
        _descriptionText.gameObject.SetActive(false);

        _descriptionAnimation.SetTrigger("Appear");
    }

    public void openBGMDetail()
    {
        _detailObject.gameObject.SetActive(true);
        _detailAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/DetailBox/Opening");
        _bgmAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Idle");
        _detailUIType = DetailUIType.BGM;

        _langObject.gameObject.SetActive(false);
        _BGMObject.gameObject.SetActive(false);
        _SFXObject.gameObject.SetActive(false);
        _ResObject.gameObject.SetActive(false);

        _detailText.gameObject.SetActive(true);
        _descriptionText.gameObject.SetActive(false);

        _descriptionAnimation.SetTrigger("Appear");
    }

    public void openSFXDetail()
    {
        _detailObject.gameObject.SetActive(true);
        _detailAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/DetailBox/Opening");
        _sfxAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Idle");
        _detailUIType = DetailUIType.SFX;

        _langObject.gameObject.SetActive(false);
        _BGMObject.gameObject.SetActive(false);
        _SFXObject.gameObject.SetActive(false);
        _ResObject.gameObject.SetActive(false);

        _detailText.gameObject.SetActive(true);
        _descriptionText.gameObject.SetActive(false);

        _descriptionAnimation.SetTrigger("Appear");
    }

    public void openResDetail()
    {
        _detailObject.gameObject.SetActive(true);
        _detailAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/DetailBox/Opening");
        _resAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Idle");
        _detailUIType = DetailUIType.Res;

        _langObject.gameObject.SetActive(false);
        _BGMObject.gameObject.SetActive(false);
        _SFXObject.gameObject.SetActive(false);
        _ResObject.gameObject.SetActive(false);

        _detailText.gameObject.SetActive(true);
        _descriptionText.gameObject.SetActive(false);

        _descriptionAnimation.SetTrigger("Appear");
    }

    public void bgmUp()
    {
        setBGM(SaveDataManager._instance._optionData._bgmVolume + 10);
        updateBGM();
        _bgmAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _bgmAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void bgmDown()
    {
        setBGM(SaveDataManager._instance._optionData._bgmVolume - 10);
        updateBGM();
        _bgmAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _bgmAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void sfxUp()
    {
        setSFX(SaveDataManager._instance._optionData._sfxVolume + 10);
        updateSFX();
        _sfxAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _sfxAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void sfxDown()
    {
        setSFX(SaveDataManager._instance._optionData._sfxVolume - 10);
        updateSFX();
        _sfxAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _sfxAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void resUp()
    {
        if(SaveDataManager._instance._optionData._resolutionOption == ResolutionOption.res800x600)
            setResolution(ResolutionOption.res1600x1200);
        updateRes();

        _resAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _resAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void resDown()
    {
        if(SaveDataManager._instance._optionData._resolutionOption == ResolutionOption.res1600x1200)
            setResolution(ResolutionOption.res800x600);
        updateRes();

        _resAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _resAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });
    }

    public void langUp()
    {
        updateLanguage();
        
        int index = LanguageManager._instance.getLanguageIndex(SaveDataManager._instance._optionData._language) + 1;
        if(LanguageManager._instance.isValidLanguageIndex(index))
            setLang(LanguageManager._instance.getSystemLanguage(index));
        updateLang();

        _langAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _langAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
            updateLanguage();
        });
    }

    public void langDown()
    {
        updateLanguage();

        int index = LanguageManager._instance.getLanguageIndex(SaveDataManager._instance._optionData._language) - 1;
        if(LanguageManager._instance.isValidLanguageIndex(index))
            setLang(LanguageManager._instance.getSystemLanguage(index));
        updateLang();

        _langAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _langAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
            updateLanguage();
        });
    }

    public void setBGM(int volume)
    {
        SaveDataManager._instance._optionData.setBGM(volume);
        SaveDataManager._instance._optionData.saveData();

        FMODAudioManager.Instance().SetGlobalParam(2, (float)volume * 0.01f);
        FMODAudioManager.Instance().SetGlobalParam(3, (float)volume * 0.01f);
    }

    public void updateBGM()
    {
        foreach(var item in _bgmText)
            item.text = (SaveDataManager._instance._optionData._bgmVolume) + "%";
    }

    public void setSFX(int volume)
    {
        SaveDataManager._instance._optionData.setSFX(volume);
        SaveDataManager._instance._optionData.saveData();

        FMODAudioManager.Instance().SetGlobalParam(1, (float)volume * 0.01f);
    }

    public void updateSFX()
    {
        foreach(var item in _sfxText)
            item.text = (SaveDataManager._instance._optionData._sfxVolume) + "%";
    }

    public void setResolution(ResolutionOption res)
    {
        SaveDataManager._instance._optionData.setRes(res);

        switch(res)
        {
            case ResolutionOption.res800x600:
                Screen.SetResolution(800,600,FullScreenMode.Windowed);
            break;
            case ResolutionOption.res1600x1200:
                Screen.SetResolution(1600,1200,FullScreenMode.Windowed);
            break;
        }
    }

    public void updateRes()
    {
        switch(SaveDataManager._instance._optionData._resolutionOption)
        {
            case ResolutionOption.res800x600:
                foreach(var item in _resText)
                    item.text = "800x600";
            break;
            case ResolutionOption.res1600x1200:
                foreach(var item in _resText)
                    item.text = "1600x1200";
            break;
        }
    }

    public void setLang(SystemLanguage systemLanguage)
    {
        SaveDataManager._instance._optionData.setLang(systemLanguage);
        LanguageManager._instance.SetLanguage(systemLanguage);
    }

    public void updateLang()
    {
        _langText.font = LanguageManager._instance.getFont();
        _langText.text = LanguageManager._instance.getDisplay();
    }
}
