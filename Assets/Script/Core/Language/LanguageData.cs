using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LanguageConfigData
{
    public SystemLanguage _language;
    public string _displayName = "";
    public string _textDataPath = "";

}

[Serializable]
public class StringKeyValueSetData
{
    public List<StringKeyValueData> _stringData = new List<StringKeyValueData>();

    public void AddString(int key, string value)
    {
        StringKeyValueData data = new StringKeyValueData();
        data._key = key;
        data._value = value;
        _stringData.Add(data);
    }

    public void UpdateString(int key, string value)
    {
        var existingData = _stringData.Find(x => x._key == key);
        if (existingData != null)
        {
            existingData._value = value;
        }
        else
        {
            AddString(key, value);
        }
    }

    public string GetString(int key)
    {
        var data = _stringData.Find(x => x._key == key);
        return data != null ? data._value : "";
    }

    public bool RemoveString(int key)
    {
        return _stringData.RemoveAll(x => x._key == key) > 0;
    }
}

[Serializable]
public class StringKeyValueData
{
    public int _key = -1;
    public string _value = "";
}