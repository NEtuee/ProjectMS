using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager _instance;
    public OptionData _optionData = new OptionData();

    public void Awake()
    {
        _instance = this;
        if(_optionData.isDataValid())
            _optionData.loadData();
        else
            _optionData.createNewOption();
    }
}

public class OptionData
{
    public SystemLanguage _language;
    public ResolutionOption _resolutionOption;
    public int _bgmVolume = 100;
    public int _sfxVolume = 100;

    public bool isDataValid()
    {
        return PlayerPrefs.HasKey("lang");
    }

    public void createNewOption()
    {
        // if(LanguageManager._instance.isLanguageValid(Application.systemLanguage))
        //     _language = Application.systemLanguage;
        // else
        _language = SystemLanguage.English;

        _resolutionOption = ResolutionOption.res800x600;
        _bgmVolume = 100;
        _sfxVolume = 100;

        saveData();
    }

    public void setBGM(int volume)
    {
        _bgmVolume = volume;
        if(_bgmVolume > 100)
            _bgmVolume = 0;
        else if(_bgmVolume < 0)
            _bgmVolume = 100;
        
        saveData();
    }

    public void setSFX(int volume)
    {
        _sfxVolume = volume;
        if(_sfxVolume > 100)
            _sfxVolume = 0;
        else if(_sfxVolume < 0)
            _sfxVolume = 100;
        saveData();
    }

    public void setLang(SystemLanguage systemLanguage)
    {
        _language = systemLanguage;
        saveData();
    }

    public void setRes(ResolutionOption resolutionOption)
    {
        _resolutionOption = resolutionOption;
        saveData();
    }

    public void saveData()
    {
        PlayerPrefs.SetInt("lang",(int)_language);
        PlayerPrefs.SetInt("res", (int)_resolutionOption);
        PlayerPrefs.SetInt("bgm", _bgmVolume);
        PlayerPrefs.SetInt("sfx", _sfxVolume);
    }

    public void loadData()
    {
        _language = (SystemLanguage)PlayerPrefs.GetInt("lang");
        _resolutionOption = (ResolutionOption)PlayerPrefs.GetInt("res");
        _bgmVolume = PlayerPrefs.GetInt("bgm");
        _sfxVolume = PlayerPrefs.GetInt("sfx");
    }


}
