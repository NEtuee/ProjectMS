using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterInfoViewer : EditorWindow
{
    private static CharacterInfoViewer _window;
    private CharacterInfoData _selectedData = null;

    [MenuItem("Tools/CharacterInfoViewer", priority = 0)]
    private static void ShowWindow()
    {
        _window = (CharacterInfoViewer)EditorWindow.GetWindow(typeof(CharacterInfoViewer));
    }

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;

    private void OnGUI()
    {
        _searchString = EditorGUILayout.TextField("Search",_searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        const string kCharacterInfoPath = "Assets\\Data\\StaticData\\CharacterInfo.xml";
        Dictionary<string,CharacterInfoData> characterInfo = ResourceContainerEx.Instance().getCharacterInfo(kCharacterInfoPath);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        foreach(var item in characterInfo)
        {
            if(_searchString != "" && (searchStringCompare(item.Key) == false && searchStringCompare(item.Value._displayName) == false))
                continue;

            GUILayout.BeginHorizontal("box");
            
            if(GUILayout.Button("Show",GUILayout.Width(50f)))
            {
                _selectedData = item.Value;
            }

            GUILayout.Label(item.Key + ": " + item.Value._displayName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if(_selectedData != null)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(_selectedData._displayName);
            EditorGUILayout.LabelField("ActionGraph: " + _selectedData._actionGraphPath);
            EditorGUILayout.LabelField("AIGraph: " + _selectedData._aiGraphPath);
            
            EditorGUILayout.LabelField("Status: " + _selectedData._statusName);
            EditorGUILayout.LabelField("Radius: " + _selectedData._characterRadius);
            EditorGUILayout.LabelField("HeadUpOffset: " + _selectedData._headUpOffset);

            GUILayout.EndVertical();
        }
    }

    private bool searchStringCompare(string target)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in _searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

}
