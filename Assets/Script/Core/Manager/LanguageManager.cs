using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageManager 
{
    private static readonly string _dialogTextDataKorea = "Assets/Data/SubtitleMap/DialogText_Kor.xml";
    private static readonly string _dialogTextDataJapan = "Assets/Data/SubtitleMap/DialogText_Jp.xml";
    private static readonly string _dialogTextDataEnglish = "Assets/Data/SubtitleMap/DialogText_En.xml";

    public static void InitLocalize()
    {
        var language = GetLanguage();
        SetLanguage(language);
    }

    public static SystemLanguage GetLanguage()
    {
        var saveLanguageSetting = PlayerPrefs.GetInt("Akane_Language");
        var languageEnum = (SystemLanguage)saveLanguageSetting;

        if (saveLanguageSetting == 0)
        {
            languageEnum = Application.systemLanguage;
            PlayerPrefs.SetInt("Akane_Language", (int)languageEnum);
        }
        
        if (languageEnum == SystemLanguage.Korean ||
            languageEnum == SystemLanguage.Japanese)
        {
            return languageEnum;
        }

        return SystemLanguage.English;
    }

    public static void SetLanguage(SystemLanguage language)
    {
        PlayerPrefs.SetInt("Akane_Language", (int)language);

        if (Application.isPlaying == true)
        {
            SetDialogLanguage(language);
        }
    }

    private static void SetDialogLanguage(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Korean:
                DialogTextManager.Instance().Init(DialogDataLoader.readFromXML(_dialogTextDataKorea));
                break;
            case SystemLanguage.Japanese:
                DialogTextManager.Instance().Init(DialogDataLoader.readFromXML(_dialogTextDataJapan));
                break;
            default:
                DialogTextManager.Instance().Init(DialogDataLoader.readFromXML(_dialogTextDataEnglish));
                break;
        }
    }
}
