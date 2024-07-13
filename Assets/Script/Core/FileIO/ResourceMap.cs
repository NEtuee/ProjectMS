using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;

public class ResourceMapExporter : EditorWindow
{
    private static ResourceMapExporter _window;

    private string _text = "";
    private Vector2 _scrollPosition;

    private string _searchString = "";


    [MenuItem("Tools/ResourceMapExporter", priority = 0)]
    private static void ShowWindow()
    {
        _window = (ResourceMapExporter)EditorWindow.GetWindow(typeof(ResourceMapExporter));
    }

    public void OnGUI()
    {
        if(GUILayout.Button("Export"))
        {
            ResourceMap.Instance().buildResourceBinaryAll();

        }

        _searchString = EditorGUILayout.DelayedTextField("Search",_searchString);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        Dictionary<int, string> _resourceMap = ResourceMap.Instance().getResourceMap();
        foreach(var item in _resourceMap)
        {
            if(_searchString != "" && item.Value.ToLower().Contains(_searchString) == false)
                continue;

            GUILayout.Label(item.Key.ToString());
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();    
        foreach(var item in _resourceMap)
        {
            if(_searchString != "" && item.Value.ToLower().Contains(_searchString) == false)
                continue;

            GUILayout.Label(item.Value);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }

}
#endif
public class ResourceMap : Singleton<ResourceMap>
{
    private Dictionary<int, string> _resourceMap = new Dictionary<int, string>();
    private static string _resourceMapPath = "/Data/ResourceMap.akbin";
    private static string _prefix = "/Data/";
    
    private static string _extension = ".akbin";

    private static bool _exportBinary = false;

    public ResourceMap()
    {
#if UNITY_EDITOR

        if(Application.isPlaying == false)
        {
            buildResourceMap();
            writeBinary();
        }
#endif

        readBinary();
    }

    public Dictionary<int, string> getResourceMap() {return _resourceMap;}

    public static string getBinaryRootPath()
    {
#if UNITY_EDITOR
        if(_exportBinary)
            return Application.streamingAssetsPath;
            
        return IOControl.PathForDocumentsFile("Assets/Data/binDEV");
#else
        return Application.streamingAssetsPath;
#endif
    }

    public void print()
    {
        foreach(var item in _resourceMap)
        {
            Debug.Log(item.Key + ", " + item.Value);
        }
    }

    public string findResourcePath(string pathRaw)
    {
        int hashedPath = IOControl.computeHash(pathRaw.ToLower());
        DebugUtil.assert(_resourceMap.ContainsKey(hashedPath), "? [{0}]",pathRaw);
        return getBinaryRootPath() + (_resourceMap.ContainsKey(hashedPath) ? _resourceMap[hashedPath] : "");
    }
#if UNITY_EDITOR

    public void buildResourceMap()
    {
        List<string> fileList = new List<string>();
        List<string> fullPathList = new List<string>();
        IOControl.getFileListRecursive("Assets/Data/", ".xml", ref fileList, ref fullPathList);

        _resourceMap.Clear();
        for(int index = 0; index < fileList.Count; ++index)
        {
            _resourceMap.Add(IOControl.computeHash(fullPathList[index]), _prefix + fileList[index].Replace(".xml", _extension));
        }
    }
    public void buildResourceBinaryAll()
    {
        _exportBinary = true;

        StaticDataLoader.writeStaticDataAll(this);

        var characterInfo = CharacterInfoManager.Instance()._characterInfoData;
        if(characterInfo != null)
        {
            ActionGraphLoader actionGraphLoader = new ActionGraphLoader();
            AIGraphLoader aiGraphLoader = new AIGraphLoader();
            foreach(var item in characterInfo)
            {
                actionGraphLoader.readFromXMLAndExportToBinary(item.Value._actionGraphPath, findResourcePath(item.Value._actionGraphPath));
                aiGraphLoader.readFromXMLAndExportToBinary(item.Value._aiGraphPath, findResourcePath(item.Value._aiGraphPath));
            }
        }

        List<string> fileList = new List<string>();
        List<string> fullPathList = new List<string>();
        IOControl.getFileListRecursive("Assets/Data/DanmakuGraph",".xml", ref fileList, ref fullPathList);

        foreach(var item in fullPathList)
        {
            string pathReal = findResourcePath(item);
            ResourceContainerEx.Instance().GetDanmakuGraph(item).writeData(pathReal);
        }

        string projectileRealPath = findResourcePath("Assets/Data/ProjectileGraph/ProjectileGraph.xml");
        ResourceContainerEx.Instance().GetProjectileGraphBaseDataList("Assets/Data/ProjectileGraph/ProjectileGraph.xml").writeData(projectileRealPath);

        fileList.Clear();
        fullPathList.Clear();
        IOControl.getFileListRecursive("Assets/Data/SequencerGraph",".xml", ref fileList, ref fullPathList);

        foreach(var item in fullPathList)
        {
            string pathReal = findResourcePath(item);
            ResourceContainerEx.Instance().GetSequencerGraph(item).writeData(pathReal);
        }

        _exportBinary = false;
        AssetDatabase.Refresh();
    }

    public void writeBinary()
    {
        string fileFullPath = Application.streamingAssetsPath + _resourceMapPath;
        FileStream fileStream = new FileStream(fileFullPath, FileMode.OpenOrCreate, FileAccess.Write);
        BinaryWriter binaryWriter = new BinaryWriter(fileStream);

        binaryWriter.Write(_resourceMap.Count);
        foreach(var item in _resourceMap)
        {
            binaryWriter.Write(item.Key);
            binaryWriter.Write(item.Value);
        }

        binaryWriter.Close();
        fileStream.Close();
    }
#endif
    public void readBinary()
    {
        _resourceMap.Clear();

        string fileFullPath = Application.streamingAssetsPath + _resourceMapPath;
        FileStream fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);

        int count = binaryReader.ReadInt32();
        for(int i = 0; i < count; ++i)
        {
            int key = binaryReader.ReadInt32();
            string value = binaryReader.ReadString();

            _resourceMap.Add(key,value);
        }

        binaryReader.Close();
        fileStream.Close();
    }
}
