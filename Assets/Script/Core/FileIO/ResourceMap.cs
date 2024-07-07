using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class ResourceMapExporter : EditorWindow
{
    private static ResourceMapExporter _window;

    private string _text = "";

    [MenuItem("Tools/ResourceMapExporter", priority = 0)]
    private static void ShowWindow()
    {
        _window = (ResourceMapExporter)EditorWindow.GetWindow(typeof(ResourceMapExporter));
    }

    public void OnGUI()
    {
        if(GUILayout.Button("Test"))
        {
            ResourceMap resourceMap = new ResourceMap();
            resourceMap.buildResourceMap();

            resourceMap.writeBinary();
            resourceMap.readBinary();

            resourceMap.print();
        }
    }

}

public class ResourceMap
{
    private Dictionary<int, string> _resourceMap = new Dictionary<int, string>();
    private static string _resourceMapPath = "/Data/ResourceMap.akbin";
    private static string _prefix = "Assets/Resources/Data/";
    
    private static string _extension = ".akbin";

    public void print()
    {
        foreach(var item in _resourceMap)
        {
            Debug.Log(item.Key + ", " + item.Value);
        }
    }

    public void buildResourceMap()
    {
        string dataPath = IOControl.PathForDocumentsFile("Assets/Data/");

        List<string> fileList = new List<string>();
        List<string> fullPathList = new List<string>();
        IOControl.getFileListRecursive(dataPath, ".xml", ref fileList, ref fullPathList);

        _resourceMap.Clear();
        for(int index = 0; index < fileList.Count; ++index)
        {
            _resourceMap.Add(IOControl.computeHash(fullPathList[index]), _prefix + fileList[index].Replace(".xml", _extension));
        }
    }

    public void buildResourceBinaryAll()
    {
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
