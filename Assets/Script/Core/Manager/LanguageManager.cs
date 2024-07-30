using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;

public class LanguageManager : ManagerBase
{
    public static LanguageManager _instance;
    public SystemLanguage _currentSystemLang = SystemLanguage.English;

    private Dictionary<SystemLanguage, SimpleStringData> _simpleStringDic = new Dictionary<SystemLanguage, SimpleStringData>();
    private List<Tuple<SystemLanguage, string>> _languageList = new List<Tuple<SystemLanguage, string>>();

    private static readonly string _dialogTextDataKorea = "Assets/Data/SubtitleMap/DialogText_Kor.xml";
    private static readonly string _dialogTextDataJapan = "Assets/Data/SubtitleMap/DialogText_Jp.xml";
    private static readonly string _dialogTextDataEnglish = "Assets/Data/SubtitleMap/DialogText_En.xml";

    public override void assign()
    {
        _instance = this;
        
        base.assign();
        CacheUniqueID("LanguageManager");
        RegisterRequest();

        loadAllLanguage();
    }

    public override void initialize()
    {
        base.initialize();

        InitLocalize();
    }

    public void InitLocalize()
    {
        var language = SaveDataManager._instance._optionData._language;
        SetLanguage(language);
    }

    public void loadAllLanguage()
    {
        List<string> fileList = new List<string>();
        IOControl.getFileList("Assets/Data/SimpleString/",".xml", ref fileList);

        foreach(var item in fileList)
        {
            SimpleStringData data = SimpleStringExporter.readFromXML(IOControl.PathForDocumentsFile("Assets/Data/SimpleString/") + item);
            if(data == null)
                continue;

            _simpleStringDic.Add(data._systemLanguage, data);
            _languageList.Add(new Tuple<SystemLanguage, string>(data._systemLanguage, data._displayLanguage));
        }
    }

    public List<Tuple<SystemLanguage, string>> getLanguageList()
    {
        return _languageList;
    }

    public SystemLanguage getSystemLanguage(int index)
    {
        if(index < 0)
            index = _languageList.Count - 1;
        else if(index >= _languageList.Count)
            index = 0;
            
        return _languageList[index].Item1;
    }

    public bool isValidLanguageIndex(int index)
    {
        return true;
    }

    public int getLanguageIndex(SystemLanguage systemLanguage)
    {
        for(int i = 0; i < _languageList.Count; ++i)
        {
            if(_languageList[i].Item1 == systemLanguage)
                return i;
        }

        return -1;
    }

    public bool isLanguageValid(SystemLanguage systemLanguage)
    {
        return _simpleStringDic.ContainsKey(systemLanguage);
    }

    public string getString(string key)
    {
        if(_simpleStringDic.ContainsKey(_currentSystemLang) == false)
        {
            DebugUtil.assert(false, "invalid language [{0}]", _currentSystemLang.ToString());
            return "NULL";
        }

        return _simpleStringDic[_currentSystemLang].getString(key);
    }

    public string getDisplay()
    {
        if(_simpleStringDic.ContainsKey(_currentSystemLang) == false)
        {
            DebugUtil.assert(false, "invalid language [{0}]", _currentSystemLang.ToString());
            return "NULL";
        }

        return _simpleStringDic[_currentSystemLang]._displayLanguage;
    }

    public TMP_FontAsset getFont()
    {
        if(_simpleStringDic.ContainsKey(_currentSystemLang) == false)
        {
            DebugUtil.assert(false, "invalid language [{0}]", _currentSystemLang.ToString());
            return null;
        }

        return _simpleStringDic[_currentSystemLang].getFont();
    }

    public void SetLanguage(SystemLanguage language)
    {
        if (Application.isPlaying == true)
        {
            SetDialogLanguage(language);
        }

        var msg = MessagePack(MessageTitles.system_languageChanged, _boradcastWithoutSenderNumber, null);
        HandleBroadcastMessage(msg);

        CallReceiveMessageProcessing();
    }

    private void SetDialogLanguage(SystemLanguage language)
    {
        _currentSystemLang = language;

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

public static class SimpleStringExporter
{
    private static string _currentFileName = "";

    public static SimpleStringData readFromXML(string path)
    {
        _currentFileName = path;

        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        Dictionary<string, XmlNodeList> branchSetDic = new Dictionary<string, XmlNodeList>();

        XmlNode node = xmlDoc.FirstChild;
        
        if(node.Name.Equals("SimpleString") == false)
        {
            DebugUtil.assert_fileOpen(false,"wrong xml type. name : {0} [FileName {1}]", _currentFileName,0,node.Name);
            return null;
        }

        return loadSimpleStringData(node);
    }

    public static SimpleStringData loadSimpleStringData(XmlNode xml)
    {
        SimpleStringData data = new SimpleStringData();
        bool loadLang = false;

        foreach(XmlAttribute item in xml.Attributes)
        {
            if(item.Name == "Language")
            {
                data._systemLanguage = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), item.Value);
                loadLang = true;
            }
            else if(item.Name == "Display")
            {
                data._displayLanguage = item.Value;
            }
            else if(item.Name == "Font")
            {
                data._fontAssetKey = item.Value;
            }
        }

        if(loadLang == false)
        {
            DebugUtil.assert(false,"Language is Must");
            return null;
        }

        Dictionary<string, string> text = new Dictionary<string, string>();
        foreach(XmlNode texts in xml.ChildNodes)
        {
            data._stringData.Add(texts.Name, texts.InnerText);
        }

        data._stringData.Add("Display",data._displayLanguage);
        return data;
    }
}

public class SimpleStringData
{
    public SystemLanguage _systemLanguage = SystemLanguage.English;
    public string _displayLanguage = "ENG";
    public string _fontAssetKey;
    public Dictionary<string,string> _stringData = new Dictionary<string, string>();

    public TMPro.TMP_FontAsset getFont()
    {
        return ResourceContainerEx.Instance().getFont("Fonts/" + _fontAssetKey);
    }

    public string getString(string key)
    {
        if(_stringData.ContainsKey(key) == false)
        {
            DebugUtil.assert(false, "invalid simple string key [{0}]", key);
            return "NULL";
        }
        return _stringData[key];
    }
}