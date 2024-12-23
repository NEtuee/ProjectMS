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

        setBGM(SaveDataManager._instance._optionData._bgmVolume);
        setSFX(SaveDataManager._instance._optionData._sfxVolume);
        setResolution(SaveDataManager._instance._optionData._resolutionOption);

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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void bgmUp()
    {
        setBGM(SaveDataManager._instance._optionData._bgmVolume + 10);
        updateBGM();
        _bgmAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _bgmAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void bgmDown()
    {
        setBGM(SaveDataManager._instance._optionData._bgmVolume - 10);
        updateBGM();
        _bgmAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _bgmAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void sfxUp()
    {
        setSFX(SaveDataManager._instance._optionData._sfxVolume + 10);
        updateSFX();
        _sfxAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _sfxAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void sfxDown()
    {
        setSFX(SaveDataManager._instance._optionData._sfxVolume - 10);
        updateSFX();
        _sfxAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _sfxAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void resUp()
    {
        ResolutionOption res = SaveDataManager._instance._optionData._resolutionOption;

        if((int)res + 1 == (int)ResolutionOption.Count)
        {
            res = (ResolutionOption)0;
        }
        else
        {
            res = (ResolutionOption)((int)res + 1);
        }

        setResolution(res);
        updateRes();

        _resAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Up");
        _resAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void resDown()
    {
        ResolutionOption res = SaveDataManager._instance._optionData._resolutionOption;

        if((int)res == 0)
        {
            res = (ResolutionOption)((int)ResolutionOption.Count - 1);
        }
        else
        {
            res = (ResolutionOption)((int)res - 1);
        }

        setResolution(res);
        updateRes();

        _resAnimation.changeAnimationByCustomPreset("Sprites/UI/Option/ArrowButton/Down");
        _resAnimation.setOnAnimationEnd(()=>{
            _descriptionAnimation.SetTrigger("Clicked");
        });

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
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

        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void setBGM(int volume)
    {
        SaveDataManager._instance._optionData.setBGM(volume);
        SaveDataManager._instance._optionData.saveData();

        int realVolume = SaveDataManager._instance._optionData._bgmVolume;

        FMODAudioManager.Instance().SetGlobalParam(2, (float)realVolume * 0.01f);
        FMODAudioManager.Instance().SetGlobalParam(3, (float)realVolume * 0.01f);
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

        int realVolume = SaveDataManager._instance._optionData._sfxVolume;

        FMODAudioManager.Instance().SetGlobalParam(1, (float)realVolume * 0.01f);
    }

    public void updateSFX()
    {
        foreach(var item in _sfxText)
            item.text = (SaveDataManager._instance._optionData._sfxVolume) + "%";
    }

    public void setResolution(ResolutionOption res)
    {
        SaveDataManager._instance._optionData.setRes(res);

        switch(SaveDataManager._instance._optionData._resolutionOption)
        {
            case ResolutionOption.res800x600:
                Screen.SetResolution(800,600,FullScreenMode.Windowed);
            break;
            case ResolutionOption.res1024x768:
                Screen.SetResolution(1024,768,FullScreenMode.Windowed);
            break;
            case ResolutionOption.res1280x960:
                Screen.SetResolution(1280,960,FullScreenMode.Windowed);
            break;
            case ResolutionOption.res1400x1050:
                Screen.SetResolution(1400,1050,FullScreenMode.Windowed);
            break;
            case ResolutionOption.res1600x1200:
                Screen.SetResolution(1600,1200,FullScreenMode.Windowed);
            break;
            case ResolutionOption.resFull:
                Resolution currentResolution = Screen.currentResolution;
                Screen.SetResolution(currentResolution.width, currentResolution.height,FullScreenMode.FullScreenWindow);
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
            case ResolutionOption.res1024x768:
                foreach(var item in _resText)
                    item.text = "1024x768";
            break;
            case ResolutionOption.res1280x960:
                foreach(var item in _resText)
                    item.text = "1280x960";
            break;
            case ResolutionOption.res1400x1050:
                foreach(var item in _resText)
                    item.text = "1400x1050";
            break;
            case ResolutionOption.res1600x1200:
                foreach(var item in _resText)
                    item.text = "1600x1200";
            break;
            case ResolutionOption.resFull:
                foreach(var item in _resText)
                    item.text = "FULLSCREEN";
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
