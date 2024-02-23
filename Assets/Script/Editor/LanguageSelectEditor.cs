using UnityEditor;
using UnityEngine;

public static class LanguageSelectEditor 
{
    [MenuItem("Tools/Localize/Kor")]
    private static void SetKorea()
    {
        LanguageManager.SetLanguage(SystemLanguage.Korean);
        Debug.Log("언어설정 한국어로 바뀜");
    }
    
    [MenuItem("Tools/Localize/Jp")]
    private static void SetJapan()
    {
        LanguageManager.SetLanguage(SystemLanguage.Japanese);
        Debug.Log("언어설정 일본어로 바뀜");
    }
    
    [MenuItem("Tools/Localize/En")]
    private static void SetEnglish()
    {
        LanguageManager.SetLanguage(SystemLanguage.English);
        Debug.Log("언어설정 영어로 바뀜");
    }
}
