using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using System.Linq;

public class LanguageManager : ManagerBase
{
    public static LanguageManager _instance;
    public SystemLanguage _currentSystemLang = SystemLanguage.Unknown;

    //이전 언어 데이터를 다 들고있을 필요가 없음. 고쳐야한다

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

        // LanguageInstanceManager 초기화
        LanguageInstanceManager.Instance();
        loadAllLanguage();
        InitLocalize();
    }

    public override void initialize()
    {
        base.initialize();
    }

    public void InitLocalize()
    {
        var language = SystemLanguage.Korean;// SaveDataManager._instance._optionData._language;
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

    public bool loadTextData(SystemLanguage language)
    {
        // LanguageInstanceManager 사용
        LanguageInstanceManager.Instance().CurrentLanguage = language;
        return LanguageInstanceManager.Instance().LoadTextData(language);
    }

    public bool saveTextDataToXML(StringKeyValueSetData textData, string fileName)
    {
        return LanguageInstanceManager.Instance().SaveTextDataToXML(textData, fileName);
    }

    public bool saveTextDataToXML(StringKeyValueSetData textData, string fileName, SystemLanguage language)
    {
        return LanguageInstanceManager.Instance().SaveTextDataToXML(textData, fileName, language);
    }

    public string getTextDataPath()
    {
        return LanguageInstanceManager.Instance().GetTextDataPath();
    }

    public string getTextDataPath(SystemLanguage language)
    {
        return LanguageInstanceManager.Instance().GetTextDataPath(language);
    }

    public string getTextFromFile(ref string fileName, int index)
    {
        return LanguageInstanceManager.Instance().GetTextFromFile(fileName, index);
    }

    public StringKeyValueData getStringKeyValueFromIndex(ref string fileName, int index)
    {
        return LanguageInstanceManager.Instance().GetStringKeyValueFromIndex(fileName, index);
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
            // DebugUtil.assert(false, "invalid language [{0}]", _currentSystemLang.ToString());
            // return null;
            _currentSystemLang = SystemLanguage.Korean;
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
        if(_currentSystemLang == language)
            return;
        
        _currentSystemLang = language;
        
        // LanguageInstanceManager 사용
        LanguageInstanceManager.Instance().CurrentLanguage = language;
        LanguageInstanceManager.Instance().LoadTextData(language);

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

/// <summary>
/// Static 언어 데이터 관리 클래스 - 에디터와 런타임에서 모두 사용 가능
/// </summary>
public class LanguageInstanceManager : Singleton<LanguageInstanceManager>
{
    private Dictionary<SystemLanguage, LanguageConfigData> _languageConfigDic = new Dictionary<SystemLanguage, LanguageConfigData>();
    private Dictionary<string, StringKeyValueSetData> _textDic = new Dictionary<string, StringKeyValueSetData>();
    private SystemLanguage _currentLanguage = SystemLanguage.Korean;
    
    private readonly string _languageConfigPath = "Data/LanguageConfig.xml";

    /// <summary>
    /// 언어 설정 초기화
    /// </summary>
    public LanguageInstanceManager()
    {
        LoadLanguageConfig();
    }

    /// <summary>
    /// 현재 언어 설정
    /// </summary>
    public SystemLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set => _currentLanguage = value;
    }

    /// <summary>
    /// 언어 설정 파일 로드
    /// </summary>
    public bool LoadLanguageConfig()
    {
        try
        {
            XDocument doc = XDocument.Load(IOControl.PathForDocumentsFile(_languageConfigPath));
            XElement rootElement = doc.Element("LanguageConfig");

            if (rootElement == null)
                throw new Exception("Root Not Exists");

            _languageConfigDic.Clear();

            foreach (var item in rootElement.Elements())
            {
                if (Enum.IsDefined(typeof(SystemLanguage), item.Name.LocalName) == false)
                    throw new Exception($"{item.Name} is not systemLanguage");

                LanguageConfigData configData = new LanguageConfigData();
                configData._language = Enum.Parse<SystemLanguage>(item.Name.LocalName);
                configData._displayName = item.Attribute("Display").Value; 
                configData._textDataPath = item.Attribute("TextPath").Value;

                _languageConfigDic.Add(configData._language, configData);
            }

            DebugUtil.log("언어 설정 로드 완료: {0}개 언어", _languageConfigDic.Count);
            return true;
        }
        catch (Exception ex)
        {
            DebugUtil.assert(false, "LanguageConfig 로드 실패! :{0}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 특정 언어의 모든 텍스트 데이터 로드
    /// </summary>
    public bool LoadTextData(SystemLanguage language)
    {
        if (_languageConfigDic.ContainsKey(language) == false)
        {
            DebugUtil.assert(false, "TextData not found : {0}", language);
            return false;
        }

        _textDic.Clear();
        LanguageConfigData configData = _languageConfigDic[language];

        try
        {
            List<string> fileList = new List<string>();
            List<string> fullPathList = new List<string>();
            IOControl.getFileListRecursive(configData._textDataPath, ".xml", ref fileList, ref fullPathList);

            for(int index = 0; index < fullPathList.Count; ++index)
            {
                XDocument doc = XDocument.Load(fullPathList[index]);
                XElement rootElement = doc.Element("TextData");

                if (rootElement == null)
                    throw new Exception("Root Not Exists");

                StringKeyValueSetData dialogData = new StringKeyValueSetData();
                foreach (var item in rootElement.Elements())
                {
                    StringKeyValueData valuedata = new StringKeyValueData();
                    valuedata._key = int.Parse(item.Name.LocalName.Remove(0,1));
                    valuedata._value = item.Attribute("Value").Value;

                    dialogData._stringData.Add(valuedata);
                }

                _textDic.Add(System.IO.Path.GetFileNameWithoutExtension(fileList[index]), dialogData);
            }

            DebugUtil.log("텍스트 데이터 로드 완료: {0}개 파일", _textDic.Count);
            return true;
        }
        catch (Exception ex)
        {
            DebugUtil.assert(false, "TextData 로드 실패! :{0}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 단일 텍스트 파일 로드
    /// </summary>
    public bool LoadSingleTextData(string fileName, SystemLanguage language)
    {
        if (_languageConfigDic.ContainsKey(language) == false)
        {
            DebugUtil.assert(false, "Language config not found : {0}", language);
            return false;
        }

        LanguageConfigData configData = _languageConfigDic[language];
        
        // 확장자가 없으면 .xml 추가
        string targetFileName = fileName;
        if (!targetFileName.EndsWith(".xml"))
        {
            targetFileName += ".xml";
        }

        string fullPath = System.IO.Path.Combine(IOControl.PathForDocumentsFile(configData._textDataPath), targetFileName);

        try
        {
            // 파일이 존재하는지 확인
            if (!System.IO.File.Exists(fullPath))
                return false;

            XDocument doc = XDocument.Load(fullPath);
            XElement rootElement = doc.Element("TextData");

            if (rootElement == null)
                throw new Exception("Root Element 'TextData' not found");

            StringKeyValueSetData textData = new StringKeyValueSetData();
            foreach (var item in rootElement.Elements())
            {
                StringKeyValueData valueData = new StringKeyValueData();
                valueData._key = int.Parse(item.Name.LocalName.Remove(0, 1)); // "_0" -> "0"
                valueData._value = item.Attribute("Value").Value;

                textData._stringData.Add(valueData);
            }

            // 기존 데이터가 있으면 덮어씌우고, 없으면 추가
            string fileKey = System.IO.Path.GetFileNameWithoutExtension(targetFileName);
            if (_textDic.ContainsKey(fileKey))
            {
                _textDic[fileKey] = textData; // 덮어씌우기
                DebugUtil.log("텍스트 데이터 업데이트: {0}", fileKey);
            }
            else
            {
                _textDic.Add(fileKey, textData); // 새로 추가
                DebugUtil.log("텍스트 데이터 추가: {0}", fileKey);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugUtil.assert(false, "단일 텍스트 데이터 로드 실패! 파일: {0}, 오류: {1}", fileName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 현재 언어로 단일 텍스트 파일 로드
    /// </summary>
    public bool LoadSingleTextData(string fileName)
    {
        return LoadSingleTextData(fileName, _currentLanguage);
    }

    /// <summary>
    /// 텍스트 데이터를 XML로 저장
    /// </summary>
    public bool SaveTextDataToXML(StringKeyValueSetData textData, string fileName, SystemLanguage language)
    {
        if (_languageConfigDic.ContainsKey(language) == false)
        {
            DebugUtil.assert(false, "Language config not found : {0}", language);
            return false;
        }

        LanguageConfigData configData = _languageConfigDic[language];
        string fullPath = System.IO.Path.Combine(IOControl.PathForDocumentsFile(configData._textDataPath), fileName);

        // 확장자가 없으면 .xml 추가
        if (!fullPath.EndsWith(".xml"))
        {
            fullPath += ".xml";
        }

        try
        {
            // 디렉토리가 없으면 생성
            string directory = System.IO.Path.GetDirectoryName(fullPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // XML 문서 생성
            XDocument doc = new XDocument();
            XElement rootElement = new XElement("TextData");

            // StringKeyValueData를 XML 요소로 변환
            foreach (var stringData in textData._stringData)
            {
                XElement element = new XElement("_" + stringData._key.ToString());
                element.SetAttributeValue("Value", stringData._value);
                rootElement.Add(element);
            }

            doc.Add(rootElement);

            // 파일 저장
            doc.Save(fullPath);

            DebugUtil.log("텍스트 데이터 저장 완료: {0}", fullPath);
            return true;
        }
        catch (Exception ex)
        {
            DebugUtil.assert(false, "텍스트 데이터 저장 실패! :{0}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 현재 언어로 텍스트 데이터 저장
    /// </summary>
    public bool SaveTextDataToXML(StringKeyValueSetData textData, string fileName)
    {
        return SaveTextDataToXML(textData, fileName, _currentLanguage);
    }

    /// <summary>
    /// 텍스트 데이터 경로 가져오기
    /// </summary>
    public string GetTextDataPath(SystemLanguage language)
    {
        if (_languageConfigDic.ContainsKey(language) == false)
        {
            DebugUtil.assert(false, "Language config not found : {0}", language);
            return "";
        }

        return _languageConfigDic[language]._textDataPath;
    }

    /// <summary>
    /// 현재 언어의 텍스트 데이터 경로 가져오기
    /// </summary>
    public string GetTextDataPath()
    {
        return GetTextDataPath(_currentLanguage);
    }

    public string GetTextFromFile(string fileName, int key)
    {
        string fileKey = fileName;

        if (_textDic.ContainsKey(fileKey))
        {
            var stringData = _textDic[fileKey]._stringData.Find(x => x._key == key);

            return stringData?._value ?? "";
        }
        return "";
    }

    public string GetTextFromFileFromIndex(string fileName, int index)
    {
        string fileKey = fileName;

        if (_textDic.ContainsKey(fileKey))
            return _textDic[fileKey]._stringData[index]._value;

        return "NULL";
    }

    public StringKeyValueData GetStringKeyValue(string fileName, int key)
    {
        string fileKey = fileName;
        if (_textDic.ContainsKey(fileKey))
        {
            return _textDic[fileKey]._stringData.Find(x => x._key == key);
        }
        return null;
    }

    public StringKeyValueData GetStringKeyValueFromIndex(string fileName, int index)
    {
        string fileKey = fileName;
        if (_textDic.ContainsKey(fileKey))
            return _textDic[fileKey]._stringData[index];

        DebugUtil.assert(false, "StringKeyValueData not found for file: [{0}], index: [{1}]", fileName, index);
        return null;
    }

    /// <summary>
    /// 모든 텍스트 데이터 클리어
    /// </summary>
    public void ClearTextData()
    {
        _textDic.Clear();
    }

    /// <summary>
    /// 언어 설정 데이터 가져오기
    /// </summary>
    public LanguageConfigData GetLanguageConfig(SystemLanguage language)
    {
        return _languageConfigDic.ContainsKey(language) ? _languageConfigDic[language] : null;
    }

    /// <summary>
    /// 지원되는 언어 목록 가져오기
    /// </summary>
    public List<SystemLanguage> GetSupportedLanguages()
    {
        return new List<SystemLanguage>(_languageConfigDic.Keys);
    }
}
