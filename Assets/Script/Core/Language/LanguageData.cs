using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LanguageConfigData
{
    public SystemLanguage _language;
    public string _displayName = "";
    public string _dialogDataPath = "";

}

[Serializable]
public class StringKeyValueSetData
{
    public List<StringKeyValueData> _stringData;
}

[Serializable]
public class StringKeyValueData
{
    public int _key;
    public string _value;
}