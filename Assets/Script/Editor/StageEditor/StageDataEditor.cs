using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework.Internal;

[InitializeOnLoad]
public class StageDataEditor : EditorWindow
{
    private class StagePointDataEditObject
    {
        public StagePointData _stagePointData;

        public GameObject _gizmoItem;
        public List<SpriteRenderer> _characterObjectList = new List<SpriteRenderer>();
        public List<StagePointCharacterSpawnData> _characterSpawnDataList = new List<StagePointCharacterSpawnData>();

        public bool syncPosition(bool isMiniStage)
        {
            if(_stagePointData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = false;
            
            if(isMiniStage == false)
            {
                syncSuccess = _stagePointData._stagePoint != _gizmoItem.transform.position;
                _stagePointData._stagePoint = _gizmoItem.transform.position;
            }
            else
            {
                _gizmoItem.transform.position = Vector3.zero;
            }
            
            if(syncSuccess == false)
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    if(_characterObjectList[index] == null)
                    {
                        _characterObjectList.RemoveAt(index);
                        _characterSpawnDataList.RemoveAt(index);

                        _stagePointData._characterSpawnData = _characterSpawnDataList.ToArray();
                        --index;
                        continue;
                    }

                    syncSuccess |= _stagePointData._characterSpawnData[index]._localPosition == _characterObjectList[index].transform.position - _stagePointData._stagePoint;
                    _stagePointData._characterSpawnData[index]._localPosition = _characterObjectList[index].transform.position - _stagePointData._stagePoint;
                }
            }
            else
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    _characterObjectList[index].transform.position = _stagePointData._stagePoint + _stagePointData._characterSpawnData[index]._localPosition;
                }
            }
            

            return syncSuccess;
        }
    }
    
    private class MiniStageDataEditObject
    {
        public MiniStageListItem    _miniStageData = null;
        public GameObject           _gizmoItem = null;

        private Vector3             _pivotPosition = Vector3.zero;

        public List<SpriteRenderer> _characterObjectList = new List<SpriteRenderer>();

        public bool syncPosition(Vector3 pivotPosition)
        {
            if(_miniStageData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = _pivotPosition != pivotPosition;
            if(syncSuccess)
            {
                _pivotPosition = pivotPosition;
                _gizmoItem.transform.position = _pivotPosition + _miniStageData._localStagePosition;
            }
            else
            {
                _miniStageData._localStagePosition = _gizmoItem.transform.position - pivotPosition;
            }

            if(_miniStageData._data._stagePointData.Count > 0)
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    _characterObjectList[index].transform.position = _miniStageData._data._stagePointData[0]._characterSpawnData[index]._localPosition + _gizmoItem.transform.position;
                }
            }

            return syncSuccess;
        }
    }

    private class SequencerPathEditor
    {
        bool _listOpen = false;
        bool _viewerOpen = false;
        bool _updateFileList = true;
        
        string _currentFilePath = "";
        string _currentFolderName = "";
        string _label = "";

        string _searchString = "";
        string[] _searchStringSplit = null;

        string _outFilePath = "";

        Vector2 _sequencerViewerScroll = Vector2.zero;
        Vector2 _fileViewerScroll = Vector2.zero;

        List<System.IO.DirectoryInfo> _directoryInfoList = new List<System.IO.DirectoryInfo>();
        List<System.IO.FileInfo> _fileInfoList = new List<System.IO.FileInfo>();

        Stack<System.IO.DirectoryInfo> _openFolderPath = new Stack<System.IO.DirectoryInfo>();

        public SequencerPathEditor(string label)
        {
            _label = label;
        }


        public void draw(ref string[] targetList)
        {
            EditorGUILayout.BeginHorizontal();

            Color colorOrigin = GUI.color;
            GUI.color = _listOpen ? Color.green : colorOrigin;
            if(GUILayout.Button(_listOpen ? "▼" : "▶", GUILayout.Width(25f)))
            {
                _listOpen = !_listOpen; 
                if(_listOpen == false)
                {
                    _viewerOpen = false;
                    clear();
                }
            }
            GUI.color = colorOrigin;

            GUI.color = _viewerOpen ? Color.green : colorOrigin;
            if(GUILayout.Button(_viewerOpen ? "○" : "•", GUILayout.Width(25f)))
            {
                _viewerOpen = !_viewerOpen;
                if(_viewerOpen)
                    _listOpen = true;
                clear();
            }
            GUI.color = colorOrigin;

            GUI.color = _listOpen ? Color.green : colorOrigin;
            GUILayout.Label(_label,GUILayout.ExpandWidth(true));
            GUI.color = colorOrigin;

            EditorGUILayout.EndHorizontal();

            if(_listOpen)
            {
                drawSequencerList(ref targetList);
            }

            if(_viewerOpen)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if(drawFileList())
                    addItem(ref targetList);
            }

        }

        private void deleteItem(int index, ref string[] targetList)
        {
            List<string> stringList = new List<string>();
            stringList.AddRange(targetList);

            stringList.RemoveAt(index);

            targetList = stringList.ToArray();
        }

        private void addItem(ref string[] targetList)
        {
            if(targetList == null)
            {
                targetList = new string[1];
                targetList[0] = _outFilePath;

                return;
            }
            List<string> stringList = new List<string>();
            stringList.AddRange(targetList);

            stringList.Add(_outFilePath);

            targetList = stringList.ToArray();
        }

        private void drawSequencerList(ref string[] targetList)
        {
            _sequencerViewerScroll = GUILayout.BeginScrollView(_sequencerViewerScroll, "box");
            GUILayout.BeginHorizontal();

            GUILayout.Label("Sequencer List");
            if(GUILayout.Button("New", GUILayout.Width(40f)))
            {
                string defaultName = "NewSequencer" + ".xml";
                string filePath = EditorUtility.SaveFilePanel(
                    "New Sequencer",
                    "Assets/Data/SequencerGraph/",
                    defaultName,
                    "xml"
                );

                var file = File.CreateText(filePath);
                if(file != null)
                {
                    string templatePath = IOControl.PathForDocumentsFile("Assets/ScriptTemplates/79-Action Script__Sequencer Graph-NewSequencerGraph.xml.txt");
                    _outFilePath = filePath.Replace(IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph/").Replace('\\','/'),"");

                    StreamReader streamReader = new StreamReader(templatePath);
                    
                    string templateResult = streamReader.ReadToEnd();
                    templateResult = templateResult.Replace("#SCRIPTNAME#", _outFilePath.Remove(_outFilePath.IndexOf('.'), 4));

                    file.WriteLine(templateResult);
                    file.Flush();
                    file.Close();

                    addItem(ref targetList);

                    FileDebugger.OpenFileWithCursor(filePath,0);
                    _outFilePath = "";
                }
            }

            GUILayout.EndHorizontal();

            int deleteIndex = -1;
            for(int index = 0; index < targetList.Length; ++index)
            {
                GUILayout.BeginHorizontal();

                GUI.enabled = index > 0;
                if(GUILayout.Button("▲",GUILayout.Width(25f)))
                {
                    string temp = targetList[index];
                    targetList[index] = targetList[index - 1];
                    targetList[index - 1] = temp;
                }

                GUI.enabled = index < targetList.Length - 1;
                if(GUILayout.Button("▼",GUILayout.Width(25f)))
                {
                    string temp = targetList[index];
                    targetList[index] = targetList[index + 1];
                    targetList[index + 1] = temp;
                }

                GUI.enabled = true;
                if(GUILayout.Button("Open", GUILayout.Width(45f)))
                {
                    string fullPath = IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph/") + targetList[index];
                    FileDebugger.OpenFileWithCursor(fullPath,0);
                }

                if(GUILayout.Button("X",GUILayout.Width(25f)))
                    deleteIndex = index;

                GUILayout.Label(targetList[index]);
                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            GUILayout.EndScrollView();

            if(deleteIndex >= 0)
                deleteItem(deleteIndex, ref targetList);
        }

        private bool drawFileList()
        {
            if(_updateFileList)
                IOControl.getAllFileList(_currentFilePath,".xml",ref _directoryInfoList,ref _fileInfoList);

            GUILayout.BeginVertical("box");

            Color colorOrigin = GUI.color;
            GUI.color = Color.green;
            GUILayout.Label("File Selector");

            GUI.color = colorOrigin;

            string searchString = EditorGUILayout.TextField("Search",_searchString);
            if(_searchString != searchString)
            {
                _searchString = searchString;
                _searchStringSplit = _searchString.Split(' ');
            }

            _fileViewerScroll = GUILayout.BeginScrollView(_fileViewerScroll, "box");

            GUILayout.BeginHorizontal();
            
            GUILayout.Space(25f);
            if(GUILayout.Button("<", GUILayout.Width(25f)) && _openFolderPath.Count > 0)
            {
                var folder = _openFolderPath.Pop();
                _currentFilePath = folder.FullName;
                _currentFolderName = folder.Name;
                
                _updateFileList = true;
                return false;
            }
            
            GUILayout.Label(_currentFolderName);

            GUILayout.EndHorizontal();

            foreach(var item in _directoryInfoList)
            {
                if(stringSearch(item.Name) == false)
                    continue;

                GUILayout.BeginHorizontal();
                if(GUILayout.Button(">", GUILayout.Width(25f)))
                {
                    _openFolderPath.Push(item.Parent);

                    _currentFilePath = item.FullName;
                    _currentFolderName = item.Name;

                    _updateFileList = true;
                }
                GUILayout.Label(item.Name);
                GUILayout.EndHorizontal();
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;

            foreach(var item in _fileInfoList)
            {
                if(stringSearch(item.Name) == false)
                    continue;

                GUILayout.BeginHorizontal();
                GUILayout.Space(25f);
                if(GUILayout.Button("Open", GUILayout.Width(45f)))
                {
                    string fullPath = item.FullName;
                    FileDebugger.OpenFileWithCursor(fullPath,0);
                }
                
                if(GUILayout.Button(item.Name,buttonStyle))
                {
                    clear();
                    _outFilePath = item.FullName;
                    _outFilePath = _outFilePath.Replace(IOControl.PathForDocumentsFile("Assets\\Data\\SequencerGraph\\").Replace('/','\\'),"").Replace('\\','/');

                    _viewerOpen = false;
                    return true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            return false;
        }

        public bool stringSearch(string targetString)
        {
            if(_searchStringSplit == null)
                return true;

            string lower = targetString.ToLower();

            foreach(var item in _searchStringSplit)
            {
                if(lower.Contains(item.ToLower()) == false)
                    return false;
            }
            
            return true;
        }

        public void clear()
        {
            _currentFilePath = IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph");
            _currentFolderName = "SequencerGraph";
            _outFilePath = "";
            _fileViewerScroll = Vector2.zero;
            _directoryInfoList.Clear();
            _fileInfoList.Clear();
            _openFolderPath.Clear();

            _updateFileList = true;
        }
    }
    
    public StageData _editStageData;

    private static StageDataEditor _window;
    private GameObject _editItemParent = null;
    private GameObject _editItemGizmoParent = null;
    private GameObject _editItemCharacterParent = null;

    private Queue<GameObject> _gizmoItemPool = new Queue<GameObject>();
    private Queue<SpriteRenderer> _characterItemPool = new Queue<SpriteRenderer>();

    private List<StagePointDataEditObject>  _editingStagePointList = new List<StagePointDataEditObject>();
    private List<MiniStageDataEditObject>   _editingMiniStageDataList = new List<MiniStageDataEditObject>();

    private GameObject _backgroundPrefabObject = null;
    
    private Vector2 _pointItemScroll = Vector2.zero;
    private Vector2 _characterSpawnScroll = Vector2.zero;
    private Vector2 _miniStageScroll = Vector2.zero;

    private CharacterInfoView _characterInfoView = new CharacterInfoView();
    private MiniStageListView _miniStageListView = new MiniStageListView();
    public SerializedObject _stageDataSerializedObject;
    public SerializedProperty _stageDataListProperty;

    private string[] _editItemMenuStrings = 
    {
        "Point",
        "Character",
        "MiniStage",
    };

    private string[] _editMenuStrings = 
    {
        "Inspector",
        "Character Palette",
        "MiniStage Palette",
    };

    private SequencerPathEditor _onEnterSequencerPathEditor = new SequencerPathEditor("On Enter Sequencer Path");
    private SequencerPathEditor _onExitSequencerPathEditor = new SequencerPathEditor("On Exit Sequencer Path");

    private int _pointSelectedIndex = -1;
    private int _characterSelectedIndex = -1;
    private int _editItemMenuSelectedIndex = 0;
    private int _editMenuSelectedIndex = 0;

    private int _miniStageSelectedIndex = 0;


    private string _pointCharacterSearchString = "";
    private string[] _pointCharacterSearchStringList;
    private string _pointCharacterSearchStringCompare = "";

    private string _miniStageSearchString = "";
    private string[] _miniStageSearchStringList;
    private string _miniStageSearchStringCompare = "";

    private bool _drawScreenToMousePoint = false;

    [MenuItem("Tools/StageDataEditor", priority = 0)]
    public static void ShowWindow()
    {
        _window = (StageDataEditor)EditorWindow.GetWindow(typeof(StageDataEditor));
    }

    private void Awake()
    {
        createOrFindEditorItem();
        constructGizmoPoints();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnGUI()
    {
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl("");
            Repaint();
        }

        bool reloadData = false;
        GUILayout.BeginHorizontal();
            StageData editStageData = EditorGUILayout.ObjectField("Stage Data", _editStageData, typeof(StageData), true) as StageData;
            MiniStageData editMiniStage = _miniStageListView.getNewMiniStageData();
            if(editMiniStage != null)
            {
                editStageData = editMiniStage;
            }

            if(GUILayout.Button("New", GUILayout.Width(40f)))
            {
                bool createNew = true;
                if(editStageData != null)
                    createNew = EditorUtility.DisplayDialog("alert","이미 편집중인 스테이지가 존재합니다. 새로 생성 하시겠습니까?","네","아니오");

                if(createNew)
                {
                    editStageData = ScriptableObject.CreateInstance<StageData>();
                    editStageData._stageName = "NewStage";

                    StagePointData stagePointData = new StagePointData(Vector3.zero);
                    stagePointData._cameraZoomSize = Camera.main.orthographicSize;
                    editStageData._stagePointData.Add(stagePointData);
                }
            }

            GUI.enabled = editStageData != null;
            if(GUILayout.Button("Refresh", GUILayout.Width(70f)))
                reloadData = true;
            GUI.enabled = true;
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if(Application.isPlaying)
        {
            if(_editItemParent != null && _editItemParent.activeSelf)
            {
                _editItemParent.SetActive(false);
            }
            return;
        }
        else
        {
            if(_editItemParent != null && _editItemParent.activeSelf == false)
            {
                _editItemParent.SetActive(true);
            }
        }
        
        if(reloadData || editStageData != _editStageData)
        {
            _editStageData = editStageData;
            constructGizmoPoints();
            loadStageData();
        }

        if(_editStageData == null)
            return;

        GUILayout.BeginVertical("box");
            _editStageData._stageName = EditorGUILayout.TextField("Stage Name",_editStageData._stageName);

            GUILayout.BeginHorizontal();
            _editStageData._backgroundPrefabPath = EditorGUILayout.ObjectField("Background Prefab",_editStageData._backgroundPrefabPath, typeof(GameObject), true) as GameObject;
            if(_editStageData._backgroundPrefabPath == null && _backgroundPrefabObject != null)
                DestroyImmediate(_backgroundPrefabObject);

            if(GUILayout.Button("New", GUILayout.Width(40f)) && _editItemParent != null)
            {
                bool createNew = true;
                if(_editStageData._backgroundPrefabPath != null)
                    createNew = EditorUtility.DisplayDialog("alert","이미 편집중인 배경이 존재합니다. 새로 생성 하시겠습니까?","네","아니오");

                if(createNew)
                {
                    string defaultName = _editStageData._stageName + ".prefab";
                    string filePath = EditorUtility.SaveFilePanel(
                        "Save Stage Background",
                        "Assets/Resources/Prefab/StageBackground/",
                        defaultName,
                        "prefab"
                    );

                    if (string.IsNullOrEmpty(filePath)) 
                        return;

                    if(_backgroundPrefabObject != null)
                        DestroyImmediate(_backgroundPrefabObject);

                    _backgroundPrefabObject = new GameObject();
                    _backgroundPrefabObject.name = filePath.Remove(0, filePath.LastIndexOf("/") + 1);
                    _backgroundPrefabObject.name = _backgroundPrefabObject.name.Replace(".prefab","");
                    _backgroundPrefabObject.transform.position = _editStageData._stagePointData.Count == 0 ? Vector3.zero : _editStageData._stagePointData[0]._stagePoint;
                    _backgroundPrefabObject.transform.SetParent(_editItemParent.transform);

                    filePath = FileUtil.GetProjectRelativePath(filePath);
                    _editStageData._backgroundPrefabPath = PrefabUtility.SaveAsPrefabAsset(_backgroundPrefabObject,filePath);
                }
            }
            GUILayout.EndHorizontal();

            Color currentColor = GUI.color;
            GUI.color = _drawScreenToMousePoint ? Color.green : Color.red;

            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Draw Camera Bound"))
                    _drawScreenToMousePoint = !_drawScreenToMousePoint;

                GUI.color = currentColor;

                if(GUILayout.Button("Save Data"))
                    saveCurrentData();
            GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.BeginVertical("box",GUILayout.Height(200f));
            _editItemMenuSelectedIndex = GUILayout.SelectionGrid(_editItemMenuSelectedIndex,_editItemMenuStrings,_editItemMenuStrings.Length);
            GUILayout.Space(5f);
            if(_editItemMenuSelectedIndex == 0)
                onPointGUI();
            else if(_editItemMenuSelectedIndex == 1)
                onCharacterGUI();
            else if(_editItemMenuSelectedIndex == 2)
                onMiniStageGUI();
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            _editMenuSelectedIndex = GUILayout.SelectionGrid(_editMenuSelectedIndex,_editMenuStrings,_editMenuStrings.Length);
            GUILayout.Space(5f);
            if(_editMenuSelectedIndex == 0)
            {
                if(_editItemMenuSelectedIndex == 0)
                    onPointInspectorGUI();
                else if(_editItemMenuSelectedIndex == 1)
                    onCharacterInspectorGUI();
                else if(_editItemMenuSelectedIndex == 2)
                    onMiniStageInspectorGUI();
            }
            else if(_editMenuSelectedIndex == 1)
            {
                _characterInfoView.OnGUI();
                string addedCharacter = _characterInfoView.getAddedCharacter();
                if(addedCharacter != "")
                {
                    addCharacterToPoint(_pointSelectedIndex,addedCharacter);
                }
            }
            else if(_editMenuSelectedIndex == 2)
            {
                _miniStageListView.OnGUI();
                MiniStageData miniStageData = _miniStageListView.getAddedMiniStage();
                if(miniStageData != null)
                    addMiniStageToStage(miniStageData);
            }
        GUILayout.EndVertical();
    }

    private void onMiniStageGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0 )
            return;
        
        _miniStageSearchString = EditorGUILayout.TextField("Search",_miniStageSearchString);
        if(_miniStageSearchStringCompare != _miniStageSearchString)
        {
            if(_miniStageSearchString == "")
                _miniStageSearchStringList = null;
            else
                _miniStageSearchStringList = _miniStageSearchString.Split(' ');
            _miniStageSearchStringCompare = _miniStageSearchString;
        }

        _miniStageScroll = GUILayout.BeginScrollView(_miniStageScroll,"box");

            for(int i = 0; i < _editStageData._miniStageData.Count; ++i)
            {
                if(_pointCharacterSearchString != "" && (searchStringCompare(_editStageData._miniStageData[i]._data._stageName,_miniStageSearchStringList) == false 
                        && searchStringCompare(_editStageData._miniStageData[i]._data.name,_miniStageSearchStringList) == false))
                    continue;

                if(_editStageData._miniStageData[i]._data == null)
                    deleteMiniStage(i--);

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _miniStageSelectedIndex ? Color.green : currentColor;

                string targetName = _editStageData._miniStageData[i]._data.name + ": " + _editStageData._miniStageData[i]._data._stageName;

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectMiniStage(i);

                if(GUILayout.Button("Delete MiniStage"))
                {
                    deleteMiniStage(i--);
                    if(i == _miniStageSelectedIndex)
                        _miniStageSelectedIndex = -1;
                }

                GUI.color = currentColor;

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();
    }

    private void onCharacterGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0 )
            return;
        
        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        if(stagePointData == null || stagePointData._characterSpawnData == null)
            return;

        _pointCharacterSearchString = EditorGUILayout.TextField("Search",_pointCharacterSearchString);
        if(_pointCharacterSearchStringCompare != _pointCharacterSearchString)
        {
            if(_pointCharacterSearchString == "")
                _pointCharacterSearchStringList = null;
            else
                _pointCharacterSearchStringList = _pointCharacterSearchString.Split(' ');
            _pointCharacterSearchStringCompare = _pointCharacterSearchString;
        }

        _characterSpawnScroll = GUILayout.BeginScrollView(_characterSpawnScroll,"box");
            
            for(int i = 0; i < stagePointData._characterSpawnData.Length; ++i)
            {
                if(_pointCharacterSearchString != "" && (searchStringCompare(stagePointData._characterSpawnData[i]._characterKey,_pointCharacterSearchStringList) == false 
                        && searchStringCompare(stagePointData._characterSpawnData[i]._uniqueKey,_pointCharacterSearchStringList) == false
                        && searchStringCompare(stagePointData._characterSpawnData[i]._uniqueGroupKey,_pointCharacterSearchStringList) == false))
                    continue;

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _characterSelectedIndex ? Color.green : currentColor;

                string targetName = stagePointData._characterSpawnData[i]._characterKey;
                if(stagePointData._characterSpawnData[i]._uniqueKey != "")
                    targetName += " [Key: " + stagePointData._characterSpawnData[i]._uniqueKey + "]";
                if(stagePointData._characterSpawnData[i]._uniqueGroupKey != "")
                    targetName += " [Group: " + stagePointData._characterSpawnData[i]._uniqueGroupKey + "]";

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectCharacter(_pointSelectedIndex, i);

                if(GUILayout.Button("Delete Character"))
                {
                    deleteCharacter(_pointSelectedIndex, i);
                    if(i == _characterSelectedIndex)
                        _characterSelectedIndex = -1;
                }

                GUI.color = currentColor;

                GUILayout.EndHorizontal();
            }
        GUILayout.EndScrollView();
    }

    private bool searchStringCompare(string target, string[] searchStringList)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

    private void onCharacterInspectorGUI()
    {
        if(_editStageData == null || _editStageData._stagePointData == null 
            || _editStageData._stagePointData.Count <= _pointSelectedIndex 
            || _pointSelectedIndex < 0 
            || _editStageData._stagePointData[_pointSelectedIndex] == null
            || _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData == null 
            || _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData.Length <= _characterSelectedIndex 
            || _characterSelectedIndex < 0
            || _editingStagePointList.Count <= _pointSelectedIndex)
            return;
 
        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointCharacterSpawnData characterSpawnData = _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData[_characterSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        Vector3 worldPositionOrigin = characterSpawnData._localPosition + stagePointData._stagePoint;
        Vector3 localPositionOrigin = characterSpawnData._localPosition;

        Vector3 newWorldPositionOrigin = EditorGUILayout.Vector3Field("World Position",worldPositionOrigin);
        if(worldPositionOrigin != newWorldPositionOrigin)
        {
            characterSpawnData._localPosition = newWorldPositionOrigin - stagePointData._stagePoint;
            localPositionOrigin = characterSpawnData._localPosition;
            stagePointDataEditObject._characterSpawnDataList[_characterSelectedIndex]._localPosition = characterSpawnData._localPosition;
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].transform.position = stagePointData._stagePoint + characterSpawnData._localPosition;
        }

        characterSpawnData._localPosition = EditorGUILayout.Vector3Field("Local Position",characterSpawnData._localPosition);
        if(characterSpawnData._localPosition != localPositionOrigin)
        {
            stagePointDataEditObject._characterSpawnDataList[_characterSelectedIndex]._localPosition = characterSpawnData._localPosition;
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].transform.position = stagePointData._stagePoint + characterSpawnData._localPosition;
        }

        characterSpawnData._flip = EditorGUILayout.Toggle("Flip",characterSpawnData._flip);
        characterSpawnData._searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", characterSpawnData._searchIdentifier);
        characterSpawnData._activeType = (StageSpawnCharacterActiveType)EditorGUILayout.EnumPopup("Active Type", characterSpawnData._activeType);

        characterSpawnData._uniqueKey = EditorGUILayout.TextField("Unique Key",characterSpawnData._uniqueKey);
        characterSpawnData._uniqueGroupKey = EditorGUILayout.TextField("Unique Group Key",characterSpawnData._uniqueGroupKey);

        
        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        if(characterInfo.ContainsKey(characterSpawnData._characterKey) == false)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        CharacterInfoData characterInfoData = characterInfo[characterSpawnData._characterKey];
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        if(baseData == null)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        List<string> actionNameList = new List<string>();
        for(int index = 1; index < baseData._actionNodeCount; ++index)
        {
            actionNameList.Add(baseData._actionNodeData[index]._nodeName);
        }

        int currentIndex = actionNameList.FindIndex((x)=>{return x == characterSpawnData._startAction;});
        if(currentIndex == -1)
            currentIndex = baseData._defaultActionIndex - 1;

        string newStartAction = actionNameList[EditorGUILayout.Popup("Start Action", currentIndex,actionNameList.ToArray())];
        if(newStartAction != characterSpawnData._startAction)
        {
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].sprite = getActionSpriteFromCharacter(characterInfoData, newStartAction);
            characterSpawnData._startAction = newStartAction;
        }
        
        stagePointDataEditObject._characterObjectList[_characterSelectedIndex].flipX = characterSpawnData._flip;
    }

    private void onMiniStageInspectorGUI()
    {
        if(_editingMiniStageDataList == null || _editingMiniStageDataList.Count == 0 || _editStageData._miniStageData.Count <= _miniStageSelectedIndex || _miniStageSelectedIndex < 0)
            return;

        MiniStageListItem miniStageListItem = _editStageData._miniStageData[_miniStageSelectedIndex];
        MiniStageDataEditObject miniStageDataEditObject = _editingMiniStageDataList[_miniStageSelectedIndex];

        GUILayout.BeginVertical("box");
        GUILayout.Label("Trigger");
        bool isChanged = false;
        SearchIdentifier searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", miniStageListItem._overrideTargetSearchIdentifier);
        float triggerWidth = EditorGUILayout.FloatField("Width", miniStageListItem._overrideTriggerWidth);
        float triggerHeight = EditorGUILayout.FloatField("Height", miniStageListItem._overrideTriggerHeight);
        Vector3 triggerOffset = EditorGUILayout.Vector3Field("Offset", miniStageListItem._overrideTriggerOffset);

        isChanged |= searchIdentifier != miniStageListItem._overrideTargetSearchIdentifier;
        isChanged |= triggerWidth != miniStageListItem._overrideTriggerWidth;
        isChanged |= triggerHeight != miniStageListItem._overrideTriggerHeight;
        isChanged |= triggerOffset != miniStageListItem._overrideTriggerOffset;
        GUILayout.EndVertical();

        if(isChanged)
        {
            miniStageListItem._overrideTargetSearchIdentifier = searchIdentifier;
            miniStageListItem._overrideTriggerWidth = triggerWidth;
            miniStageListItem._overrideTriggerHeight = triggerHeight;
            miniStageListItem._overrideTriggerOffset = triggerOffset;
            SceneView.RepaintAll();
        }
    }

    private void onPointInspectorGUI()
    {
        if(_editingStagePointList == null || _editingStagePointList.Count == 0 || _editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0)
            return;

        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        GUILayout.Label("Position: " + stagePointData._stagePoint.ToString()); 
        if(_editStageData._isMiniStage == false)
        {
            stagePointData._pointName = EditorGUILayout.TextField("Name",stagePointData._pointName);
            stagePointData._maxLimitedDistance = EditorGUILayout.FloatField("CameraBound Radius", stagePointData._maxLimitedDistance);
            stagePointData._cameraZoomSize = EditorGUILayout.FloatField("ZoomSize", stagePointData._cameraZoomSize);
            stagePointData._cameraZoomSpeed = EditorGUILayout.FloatField("ZoomSpeed", stagePointData._cameraZoomSpeed);
            stagePointData._lerpCameraZoom = EditorGUILayout.Toggle("LerpToNextZoom", stagePointData._lerpCameraZoom);
        }
        else if(_editStageData is MiniStageData)
        {
            MiniStageData miniStageData = _editStageData as MiniStageData;
            GUILayout.BeginVertical("box");
            GUILayout.Label("Trigger");
            bool isChanged = false;
            SearchIdentifier searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", miniStageData._targetSearchIdentifier);
            float triggerWidth = EditorGUILayout.FloatField("Width", miniStageData._triggerWidth);
            float triggerHeight = EditorGUILayout.FloatField("Height", miniStageData._triggerHeight);
            Vector3 triggerOffset = EditorGUILayout.Vector3Field("Offset", miniStageData._triggerOffset);

            isChanged |= searchIdentifier != miniStageData._targetSearchIdentifier;
            isChanged |= triggerWidth != miniStageData._triggerWidth;
            isChanged |= triggerHeight != miniStageData._triggerHeight;
            isChanged |= triggerOffset != miniStageData._triggerOffset;
            GUILayout.EndVertical();

            if(isChanged)
            {
                miniStageData._targetSearchIdentifier = searchIdentifier;
                miniStageData._triggerWidth = triggerWidth;
                miniStageData._triggerHeight = triggerHeight;
                miniStageData._triggerOffset = triggerOffset;
                SceneView.RepaintAll();
            }
        }

        _onEnterSequencerPathEditor.draw(ref stagePointData._onEnterSequencerPath);
        _onExitSequencerPathEditor.draw(ref stagePointData._onExitSequencerPath);

        // if(stagePointDataEditObject._onEnterSequencerPathProperty == null || stagePointDataEditObject._onExitSequencerPathProperty == null)
        // {
        //     if(_stageDataListProperty != null && _stageDataListProperty.hasChildren && _stageDataListProperty.arraySize > _pointSelectedIndex)
        //     {
        //         SerializedProperty stagePointDataProperty = _stageDataListProperty.GetArrayElementAtIndex(_pointSelectedIndex);
        //         stagePointDataEditObject._onEnterSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onEnterSequencerPath");
        //         stagePointDataEditObject._onExitSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onExitSequencerPath");
        //     }
        // }
        // else
        // {
        //     EditorGUILayout.PropertyField(stagePointDataEditObject._onEnterSequencerPathProperty);
        //     EditorGUILayout.PropertyField(stagePointDataEditObject._onExitSequencerPathProperty);
        // }
    }

    private void onPointGUI()
    {
        GUILayout.BeginHorizontal();

            if(_editStageData._isMiniStage)
            {
                MiniStageData miniStageData = _editStageData as MiniStageData;
                GUI.enabled = miniStageData._stagePointData.Count < 1;
            }

            if(GUILayout.Button("Add Point"))
            {
                addStagePoint();
            }

            GUI.enabled = GUI.enabled ? _pointSelectedIndex >= 0 && _pointSelectedIndex < _editingStagePointList.Count - 1 : false;
            if(GUILayout.Button("Insert Point Next"))
            {
                insertNextStagePoint(_pointSelectedIndex);
            }
            GUI.enabled = true;

        GUILayout.EndHorizontal();


        _pointItemScroll = GUILayout.BeginScrollView(_pointItemScroll,"box");
            for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _pointSelectedIndex ? Color.green : currentColor;

                GUI.enabled = i > 0;
                if(GUILayout.Button("▲",GUILayout.Width(25f)))
                {
                    StagePointData temp = _editStageData._stagePointData[i];
                    _editStageData._stagePointData[i] = _editStageData._stagePointData[i - 1];
                    _editStageData._stagePointData[i - 1] = temp;

                    StagePointDataEditObject temp2 = _editingStagePointList[i];
                    _editingStagePointList[i] = _editingStagePointList[i - 1];
                    _editingStagePointList[i - 1] = temp2;

                    _pointSelectedIndex = i - 1;
                    SceneView.RepaintAll();
                }

                GUI.enabled = i < _editStageData._stagePointData.Count - 1;
                if(GUILayout.Button("▼",GUILayout.Width(25f)))
                {
                    StagePointData temp = _editStageData._stagePointData[i];
                    _editStageData._stagePointData[i] = _editStageData._stagePointData[i + 1];
                    _editStageData._stagePointData[i + 1] = temp;

                    StagePointDataEditObject temp2 = _editingStagePointList[i];
                    _editingStagePointList[i] = _editingStagePointList[i + 1];
                    _editingStagePointList[i + 1] = temp2;

                    _pointSelectedIndex = i + 1;
                    SceneView.RepaintAll();
                }

                GUI.enabled = true;
                GUILayout.Label(_editStageData._stagePointData[i]._pointName,GUILayout.Width(150f));

                if(GUILayout.Button("Pick"))
                    selectPoint(i);

                if(GUILayout.Button("Delete Point"))
                {
                    deleteStagePoint(i);
                    if(i == _pointSelectedIndex)
                        _pointSelectedIndex = -1;
                }

                GUI.color = currentColor;

                GUILayout.EndHorizontal();
            }
        GUILayout.EndScrollView();
    }

    private void addCharacterToPoint(int index, string characterKey)
    {
        if(_editStageData._stagePointData.Count <= index || index < 0)
            return;

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        if(characterInfo.ContainsKey(characterKey) == false)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        StagePointData stagePointData = _editStageData._stagePointData[index];
        StagePointDataEditObject stagePointDataEdit = _editingStagePointList[index];
        
        StagePointCharacterSpawnData spawnData = new StagePointCharacterSpawnData();
        spawnData._characterKey = characterKey;
        spawnData._flip = false;
        spawnData._localPosition = Vector3.zero;

        SpriteRenderer characterEditItem = getCharacterItem();
        characterEditItem.sprite = getFirstActionSpriteFromCharacter(characterInfo[characterKey]);
        characterEditItem.sortingOrder = 10;
        characterEditItem.transform.position = stagePointData._stagePoint;

        stagePointDataEdit._characterSpawnDataList.Add(spawnData);
        stagePointDataEdit._characterObjectList.Add(characterEditItem);
        stagePointData._characterSpawnData = stagePointDataEdit._characterSpawnDataList.ToArray();
    }

    private void addMiniStageToStage( MiniStageData miniStageData)
    {
        MiniStageListItem listItem = new MiniStageListItem();
        listItem._localStagePosition = Vector3.zero;
        listItem._data = miniStageData;
        
        listItem._overrideTargetSearchIdentifier = miniStageData._targetSearchIdentifier;
        listItem._overrideTriggerWidth = miniStageData._triggerWidth;
        listItem._overrideTriggerHeight = miniStageData._triggerHeight;
        listItem._overrideTriggerOffset = miniStageData._triggerOffset;

        MiniStageDataEditObject editObject = new MiniStageDataEditObject();
        editObject._miniStageData = listItem;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = _editStageData._stagePointData[0]._stagePoint;

        _editingMiniStageDataList.Add(editObject);
        _editStageData._miniStageData.Add(listItem);

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        for(int index = 0; index < miniStageData._stagePointData[0]._characterSpawnData.Length; ++index)
        {
            var spawnData = miniStageData._stagePointData[0]._characterSpawnData[index];
            SpriteRenderer characterEditItem = getCharacterItem();
            characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[spawnData._characterKey],spawnData._startAction);
            characterEditItem.sortingOrder = 10;
            characterEditItem.transform.position = editObject._gizmoItem.transform.position + spawnData._localPosition;
            characterEditItem.flipX = spawnData._flip;
    
            editObject._characterObjectList.Add(characterEditItem);
        }

        selectMiniStage(_editingMiniStageDataList.Count - 1);
    }

    void Update() 
    {
        if(_editStageData == null)
            return;

        if(Application.isPlaying)
            return;

        if(_editStageData._stagePointData.Count != 0 && _editingStagePointList.Count == 0 ||
            _editStageData._miniStageData.Count != 0 && _editingMiniStageDataList.Count == 0)
            constructGizmoPoints();

        bool repaint = false;
        for(int i = 0; i < _editingStagePointList.Count; ++i)
        {
            if(_editingStagePointList[i]._gizmoItem == null)
            {
                deleteStagePoint(i);
                --i;
                continue;
            }

            repaint |= _editingStagePointList[i].syncPosition(_editStageData._isMiniStage);

            if(i == 0 && _backgroundPrefabObject != null)
                _backgroundPrefabObject.transform.position = _editingStagePointList[i]._stagePointData._stagePoint;
        }

        if(_editStageData._stagePointData.Count != 0)
        {
            for(int i = 0; i < _editingMiniStageDataList.Count; ++i)
            {
                if(_editingMiniStageDataList[i]._gizmoItem == null)
                {
                    deleteMiniStage(i);
                    --i;
                    continue;
                }

                repaint |= _editingMiniStageDataList[i].syncPosition(_editStageData._stagePointData[0]._stagePoint);
            }
        }

        if(repaint)
            Repaint();
    }

    public void saveCurrentData()
    {
        if(_editStageData == null)
            return;

        roundPixelPerfect();

        EditorUtility.SetDirty(_editStageData);
        if(_backgroundPrefabObject != null)
        {
            string filePath = "";
            if(AssetDatabase.Contains(_editStageData._backgroundPrefabPath) == false)
            {
                DebugUtil.assert(false, "Background Prefab이 Scene Object입니다. Prefab을 넣어 주세요");
                return;
                // string defaultName = _editStageData._stageName + ".prefab";
                // filePath = EditorUtility.SaveFilePanel(
                //     "Save Stage Background",
                //     "Assets/Resources/Prefab/StageBackground/",
                //     defaultName,
                //     "prefab"
                // );
    
                // if (string.IsNullOrEmpty(filePath)) 
                //     return;
    
                // filePath = FileUtil.GetProjectRelativePath(filePath);
                // AssetDatabase.CreateAsset(_backgroundPrefabObject, filePath);
                // AssetDatabase.SaveAssets();
            }
            else
            {
                filePath = AssetDatabase.GetAssetPath(_editStageData._backgroundPrefabPath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(_backgroundPrefabObject, filePath,InteractionMode.AutomatedAction);
            }
        }

        if(AssetDatabase.Contains(_editStageData) == false)
        {
            string defaultName = _editStageData._stageName + ".asset";
            string path = EditorUtility.SaveFilePanel(
                "Save Stage Data",
                "Assets/Resources/StageData/",
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(path)) 
                return;

            path = FileUtil.GetProjectRelativePath(path);
            AssetDatabase.CreateAsset(_editStageData, path);
            AssetDatabase.SaveAssets();
        }
    }

    public void roundPixelPerfect()
    {
        foreach(var item in _editingStagePointList)
        {
            item._stagePointData._stagePoint = MathEx.round(item._stagePointData._stagePoint, 2);
            item._gizmoItem.transform.position = item._stagePointData._stagePoint;

            for(int index = 0; index < item._characterSpawnDataList.Count; ++index)
            {
                item._characterSpawnDataList[index]._localPosition = MathEx.round(item._characterSpawnDataList[index]._localPosition, 2);
                item._characterObjectList[index].transform.position = item._stagePointData._stagePoint + item._characterSpawnDataList[index]._localPosition;
            }
        }

        if(_backgroundPrefabObject != null)
            roundPixelPerfectRecursive(_backgroundPrefabObject.transform);
    }

    public void roundPixelPerfectRecursive(Transform root)
    {
        root.transform.position = MathEx.round(root.transform.position,2);

        for(int index = 0; index < root.childCount; ++index)
        {
            roundPixelPerfectRecursive(root.GetChild(index));
        }
    }

    public void setEditStageData(StageData stageData)
    {
        _editStageData = stageData;
        constructGizmoPoints();
    }

    void OnFocus() 
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;

        if(_editItemParent != null)
            DestroyImmediate(_editItemParent);
    }
 
    private void OnSceneGUI(SceneView sceneView) 
    {
        if(_editStageData == null)
            return;

        if(Application.isPlaying)
            return;

        // if(_drawScreenToMousePoint)
        // {
        //     Vector3 mousePosition = Event.current.mousePosition;
        //     mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        //     mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        //     mousePosition.z = 0f;

        //     float mainCamSize = Camera.main.orthographicSize;
        //     float camHeight = mainCamSize * 2f;
		//     float camWidth = camHeight * ((float)800f / (float)600f);

        //     Rect rectangle = new Rect();
        //     rectangle.Set(mousePosition.x - (camWidth * 0.5f),mousePosition.y - (camHeight * 0.5f),camWidth,camHeight);
        //     Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.blue);
        // }

        if(_editStageData._isMiniStage)
        {
            MiniStageData miniStageData = _editStageData as MiniStageData;

            Rect rectangle = new Rect();
            rectangle.Set(miniStageData._triggerOffset.x - (miniStageData._triggerWidth * 0.5f),miniStageData._triggerOffset.y - (miniStageData._triggerHeight * 0.5f),miniStageData._triggerWidth,miniStageData._triggerHeight);
            Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.green);
        }

        for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
        {
            StagePointData stagePointData = _editStageData._stagePointData[i];
            if(stagePointData == null)
                continue;

            Vector3 itemPosition = stagePointData._stagePoint;
            Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
            };

            Color currentColor = Handles.color;
            
            if(stagePointData._characterSpawnData != null)
            {
                for(int index = 0; index < stagePointData._characterSpawnData.Length; ++index)
                {
                    Vector3 characterWorld = stagePointData._stagePoint + stagePointData._characterSpawnData[index]._localPosition;

                    Handles.color = i == _pointSelectedIndex ? Color.green : Color.gray;
                    Handles.DrawLine(stagePointData._stagePoint,characterWorld);

                    characterWorld += Vector3.right * 0.1f;
                    if(stagePointData._characterSpawnData[index]._uniqueKey != "")
                        Handles.Label(characterWorld,"Unique Key: " + stagePointData._characterSpawnData[index]._uniqueKey);
                    
                    if(stagePointData._characterSpawnData[index]._uniqueGroupKey != "")
                        Handles.Label(characterWorld + Vector3.down * 0.1f,"Unique Group Key: " + stagePointData._characterSpawnData[index]._uniqueGroupKey);

                    Handles.color = currentColor;
                    Handles.Label(characterWorld + Vector3.down * 0.2f,stagePointData._characterSpawnData[index]._activeType.ToString());
                }
            }
            
            Handles.color = i == _pointSelectedIndex ? Color.green : currentColor;

            if(stagePointData._maxLimitedDistance > 0f)
                drawCircleWithHandle(stagePointData._stagePoint,stagePointData._maxLimitedDistance);

            Handles.Label(stagePointData._stagePoint, i.ToString());
            if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                selectPoint(i);

            Handles.color = currentColor;
            if(_drawScreenToMousePoint || (i == _pointSelectedIndex && _editStageData._isMiniStage == false))
            {
                drawInGameScreenSection(stagePointData._stagePoint,stagePointData._cameraZoomSize,stagePointData._maxLimitedDistance);
                if(stagePointData._maxLimitedDistance > 0f)
                    drawInGameScreenSection(stagePointData._stagePoint,stagePointData._cameraZoomSize,0f);
            }

            if(_drawScreenToMousePoint && i > 0)
                drawScreenSectionConnectLine(
                    _editStageData._stagePointData[i - 1]._stagePoint,_editStageData._stagePointData[i - 1]._cameraZoomSize,_editStageData._stagePointData[i - 1]._maxLimitedDistance,
                    stagePointData._stagePoint,stagePointData._cameraZoomSize,stagePointData._maxLimitedDistance);

            if(i < _editStageData._stagePointData.Count - 1 )
            {
                Vector3 direction = _editStageData._stagePointData[i + 1]._stagePoint - stagePointData._stagePoint;
                direction.Normalize();

                Vector3 right = Vector3.Cross(direction,Vector3.forward);
                right.Normalize();

                Handles.DrawLine(stagePointData._stagePoint + right * stagePointData._maxLimitedDistance, 
                                _editStageData._stagePointData[i + 1]._stagePoint + right * _editStageData._stagePointData[i + 1]._maxLimitedDistance);
                Handles.DrawLine(stagePointData._stagePoint - right * stagePointData._maxLimitedDistance, 
                                _editStageData._stagePointData[i + 1]._stagePoint - right * _editStageData._stagePointData[i + 1]._maxLimitedDistance);

                Color arrowColor = i == _pointSelectedIndex ? Color.green : currentColor;
                drawArrow(stagePointData._stagePoint, _editStageData._stagePointData[i + 1]._stagePoint, 0.3f, arrowColor);
            }
        }

        if(_editStageData._stagePointData.Count != 0)
        {
            for(int i = 0; i < _editStageData._miniStageData.Count; ++i)
            {
                MiniStageListItem miniStageListItem = _editStageData._miniStageData[i];
                MiniStageData miniStageData = miniStageListItem._data;
                Vector3 itemPosition = miniStageListItem._localStagePosition + _editStageData._stagePointData[0]._stagePoint;
                Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                    Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
                };

                if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                {
                    _editMenuSelectedIndex = 2;
                    selectMiniStage(i);
                }

                Rect rectangle = new Rect();
                Vector3 centerPosition = itemPosition + miniStageListItem._overrideTriggerOffset;
                rectangle.Set(centerPosition.x - (miniStageListItem._overrideTriggerWidth * 0.5f),centerPosition.y - (miniStageListItem._overrideTriggerHeight * 0.5f),miniStageListItem._overrideTriggerWidth,miniStageListItem._overrideTriggerHeight);
                Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.green);
            }
        }
    }

    public void drawArrow(Vector3 start, Vector3 end, float arrowLength, Color color)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 arrowUp = new Vector3(-1f,1f).normalized * arrowLength;
        Vector3 arrowDown = new Vector3(-1f,-1f).normalized * arrowLength;

        float angle = Vector3.SignedAngle(Vector3.right, direction,Vector3.forward);
        Quaternion rotate = Quaternion.Euler(0f,0f,angle);
        arrowUp = rotate * arrowUp;
        arrowDown = rotate * arrowDown;

        Color beforeColor = Handles.color;
        Handles.color = color;

        Handles.DrawLine(start, end);
        Handles.DrawLine(end, end + arrowUp);
        Handles.DrawLine(end, end + arrowDown);

        Handles.color = beforeColor;
    }

    private void selectPoint(int index)
    {
        PingTarget(_editingStagePointList[index]._gizmoItem);
        Repaint();

        _pointSelectedIndex = index;
        _characterSelectedIndex = -1;
    }

    private void selectCharacter(int pointIndex, int characterIndex)
    {
        PingTarget(_editingStagePointList[pointIndex]._characterObjectList[characterIndex].gameObject);
        Repaint();

        _characterSelectedIndex = characterIndex;
    }

    private void selectMiniStage(int miniStageIndex)
    {
        PingTarget(_editingMiniStageDataList[miniStageIndex]._gizmoItem);
        _miniStageSelectedIndex = miniStageIndex;
    }

    private void PingTarget(GameObject obj)
    {
        EditorGUIUtility.PingObject(obj);
        Selection.activeGameObject = obj;
    }

    private void addStagePoint()
    {
        Vector3 spawnPosition = Vector3.zero;
        if(_editingStagePointList.Count == 1)
        {
            spawnPosition = _editingStagePointList[0]._stagePointData._stagePoint + Vector3.right;
        }
        else if(_editingStagePointList.Count > 1)
        {
            spawnPosition = _editingStagePointList[_editingStagePointList.Count - 1]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[_editingStagePointList.Count - 1]._gizmoItem.transform.position - _editingStagePointList[_editingStagePointList.Count - 2]._gizmoItem.transform.position).normalized;
        }

        StagePointDataEditObject editObject = new StagePointDataEditObject();
        StagePointData stagePointData = new StagePointData(spawnPosition);
        stagePointData._cameraZoomSize = Camera.main.orthographicSize;
        stagePointData._pointName = "New Point " + _editingStagePointList.Count;


        
        _editStageData._stagePointData.Add(stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Add(editObject);

        selectPoint(_editStageData._stagePointData.Count - 1);

        EditorUtility.SetDirty(_editStageData);
    }

    private void insertNextStagePoint(int index)
    {
        if(index < 0 && _editingStagePointList.Count - 1 >= index)
            return;

        Vector3 spawnPosition = Vector3.zero;
        if(index == _editingStagePointList.Count)
        {
            spawnPosition = _editingStagePointList[index]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[index]._gizmoItem.transform.position - _editingStagePointList[index - 1]._gizmoItem.transform.position).normalized;
        }
        else
        {
            spawnPosition = _editingStagePointList[index]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[index + 1]._gizmoItem.transform.position - _editingStagePointList[index]._gizmoItem.transform.position) * 0.5f;
        }

        StagePointDataEditObject editObject = new StagePointDataEditObject();
        StagePointData stagePointData = new StagePointData(spawnPosition);
        stagePointData._cameraZoomSize = Camera.main.orthographicSize;
        stagePointData._pointName = "New Point" + (index + 1);

        _editStageData._stagePointData.Insert(index + 1, stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Insert(index + 1, editObject);

        selectPoint(index + 1);

        EditorUtility.SetDirty(_editStageData);
    }

    private void deleteStagePoint(int index)
    {
        if(index < 0 || _editStageData._stagePointData.Count <= index)
            return;

        _editStageData._stagePointData.RemoveAt(index);

        for(int characterIndex = 0; characterIndex < _editingStagePointList[index]._characterObjectList.Count; ++characterIndex)
        {
            SpriteRenderer characterItem = _editingStagePointList[index]._characterObjectList[characterIndex];
            returnCharacterItem(characterItem);
        }

        returnGizmoItem(_editingStagePointList[index]._gizmoItem);
        _editingStagePointList.RemoveAt(index);
    }

    private void deleteCharacter(int pointIndex, int characterIndex)
    {
        if(_editStageData._stagePointData.Count <= pointIndex || pointIndex < 0 ||
            _editStageData._stagePointData[pointIndex]._characterSpawnData.Length <= characterIndex || characterIndex < 0)
            return;

        _editingStagePointList[pointIndex]._characterSpawnDataList.RemoveAt(characterIndex);
        _editStageData._stagePointData[pointIndex]._characterSpawnData = _editingStagePointList[pointIndex]._characterSpawnDataList.ToArray();

        returnCharacterItem(_editingStagePointList[pointIndex]._characterObjectList[characterIndex]);
        _editingStagePointList[pointIndex]._characterObjectList.RemoveAt(characterIndex);
    }

    private void deleteMiniStage(int miniStageIndex)
    {
        if(_editStageData._miniStageData.Count <= miniStageIndex || miniStageIndex < 0 )
            return;

        _editStageData._miniStageData.RemoveAt(miniStageIndex);

        for(int characterIndex = 0; characterIndex < _editingMiniStageDataList[miniStageIndex]._characterObjectList.Count; ++characterIndex)
        {
            SpriteRenderer characterItem = _editingMiniStageDataList[miniStageIndex]._characterObjectList[characterIndex];
            returnCharacterItem(characterItem);
        }

        returnGizmoItem(_editingMiniStageDataList[miniStageIndex]._gizmoItem);
        _editingMiniStageDataList.RemoveAt(miniStageIndex);
    }

    private void loadStageData()
    {
        _pointSelectedIndex = 0;
        _characterSelectedIndex = 0;
        _miniStageSelectedIndex = 0;

        _editMenuSelectedIndex = 0;
        _editItemMenuSelectedIndex = 0;

        if(_editStageData == null)
            return;

        if(_backgroundPrefabObject != null)
            DestroyImmediate(_backgroundPrefabObject);
        
        if(_editStageData._backgroundPrefabPath != null)
        {
            _backgroundPrefabObject = Instantiate(_editStageData._backgroundPrefabPath);
            _backgroundPrefabObject.transform.position = _editStageData._stagePointData.Count == 0 ? Vector3.zero : _editStageData._stagePointData[0]._stagePoint;
            _backgroundPrefabObject.transform.SetParent(_editItemParent.transform);
        }

        for(int index = 0; index < _editStageData._stagePointData.Count; ++index)
        {
            if(_editStageData._stagePointData[index]._pointName == "")
                _editStageData._stagePointData[index]._pointName = "New Point " + index;
        }

        _stageDataSerializedObject = new SerializedObject(_editStageData);
        _stageDataListProperty = _stageDataSerializedObject.FindProperty("_stagePointData");
    }

    private void constructGizmoPoints()
    {
        clearStagePointList();

        if(_editStageData == null)
        {
            if(_backgroundPrefabObject != null)
                DestroyImmediate(_backgroundPrefabObject);
            return;
        }

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        
        foreach(var item in _editStageData._stagePointData)
        {
            StagePointDataEditObject editObject = new StagePointDataEditObject();
            editObject._stagePointData = item;
            editObject._gizmoItem = getGizmoItem();
            editObject._gizmoItem.transform.position = item._stagePoint;

            editObject._characterSpawnDataList = new List<StagePointCharacterSpawnData>();
            if(item._characterSpawnData != null)
            {
                foreach(var spawnData in item._characterSpawnData)
                {
                    editObject._characterSpawnDataList.Add(spawnData);
                }
            }
            for(int index = 0; index < editObject._characterSpawnDataList.Count; ++index)
            {
                SpriteRenderer characterEditItem = getCharacterItem();
                characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[editObject._characterSpawnDataList[index]._characterKey],editObject._characterSpawnDataList[index]._startAction);
                characterEditItem.sortingOrder = 10;
                characterEditItem.transform.position = item._stagePoint + editObject._characterSpawnDataList[index]._localPosition;
                characterEditItem.flipX = editObject._characterSpawnDataList[index]._flip;

                editObject._characterObjectList.Add(characterEditItem);
            }

            _editingStagePointList.Add(editObject);
        }

        if(_editStageData._stagePointData.Count != 0)
        {
            foreach(var item in _editStageData._miniStageData)
            {
                MiniStageDataEditObject editObject = new MiniStageDataEditObject();
                editObject._miniStageData = item;
                editObject._gizmoItem = getGizmoItem();
                editObject._gizmoItem.transform.position = _editStageData._stagePointData[0]._stagePoint + item._localStagePosition;
                _editingMiniStageDataList.Add(editObject);

                if(item._data._stagePointData.Count == 0)
                    continue;

                for(int index = 0; index < item._data._stagePointData[0]._characterSpawnData.Length; ++index)
                {
                    var spawnData = item._data._stagePointData[0]._characterSpawnData[index];
                    SpriteRenderer characterEditItem = getCharacterItem();
                    characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[spawnData._characterKey],spawnData._startAction);
                    characterEditItem.sortingOrder = 10;
                    characterEditItem.transform.position = editObject._gizmoItem.transform.position + spawnData._localPosition;
                    characterEditItem.flipX = spawnData._flip;
    
                    editObject._characterObjectList.Add(characterEditItem);
                }

            }
            
        }
    }

    private void clearStagePointList()
    {
        for(int index = 0; index < _editingStagePointList.Count; ++index)
        {
            for(int characterIndex = 0; characterIndex < _editingStagePointList[index]._characterObjectList.Count; ++characterIndex)
            {
                SpriteRenderer characterItem = _editingStagePointList[index]._characterObjectList[characterIndex];
                returnCharacterItem(characterItem);
            }

            returnGizmoItem(_editingStagePointList[index]._gizmoItem);
        }

        for(int index = 0; index < _editingMiniStageDataList.Count; ++index)
        {
            for(int characterIndex = 0; characterIndex < _editingMiniStageDataList[index]._characterObjectList.Count; ++characterIndex)
            {
                SpriteRenderer characterItem = _editingMiniStageDataList[index]._characterObjectList[characterIndex];
                returnCharacterItem(characterItem);
            }

            returnGizmoItem(_editingMiniStageDataList[index]._gizmoItem);
        }

        _gizmoItemPool.Clear();
        _characterItemPool.Clear();
        _editingStagePointList.Clear();
        _editingMiniStageDataList.Clear();

        if(_editItemParent == null)
            _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");

        if(_editItemParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        Transform gizmoParent = _editItemParent.transform.Find("Editor_Gizmos");
        if(gizmoParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        Transform characterParent = _editItemParent.transform.Find("Editor_Characters");
        if(characterParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        _editItemGizmoParent = gizmoParent.gameObject;
        _editItemCharacterParent = characterParent.gameObject;

        for(int index = 0; index < gizmoParent.childCount; ++index)
        {
            Transform child = gizmoParent.GetChild(index);
            if(child.name.Contains("Editor_") == false)
            {
                child.parent = null;
                DestroyImmediate(child.gameObject);
                --index;
            }
        }

        for(int index = 0; index < _editItemGizmoParent.transform.childCount; ++index)
        {
            _gizmoItemPool.Enqueue(_editItemGizmoParent.transform.GetChild(index).gameObject);
        }

        for(int index = 0; index < _editItemCharacterParent.transform.childCount; ++index)
        {
            _characterItemPool.Enqueue(_editItemCharacterParent.transform.GetChild(index).GetComponent<SpriteRenderer>());
        }
    }

    private void createOrFindEditorItem()
    {
        _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");
        if(_editItemParent != null)
        {
            _gizmoItemPool.Clear();
            Transform gizmoParent = _editItemParent.transform.Find("Editor_Gizmos");
            if(gizmoParent == null)
            {
                gizmoParent = new GameObject("Editor_Gizmos").transform;
                gizmoParent.transform.SetParent(_editItemParent.transform);
            }
            else
            {
                for(int index = 0; index < gizmoParent.childCount; ++index)
                {
                    _gizmoItemPool.Enqueue(gizmoParent.GetChild(index).gameObject);
                }
            }
            _editItemGizmoParent = gizmoParent.gameObject;

            _characterItemPool.Clear();
            Transform characterParent = _editItemParent.transform.Find("Editor_Characters");
            if(characterParent == null)
            {
                characterParent = new GameObject("Editor_Characters").transform;
                characterParent.transform.SetParent(_editItemParent.transform);
            }
            else
            {
                for(int index = 0; index < characterParent.childCount; ++index)
                {
                    _characterItemPool.Enqueue(characterParent.GetChild(index).GetComponent<SpriteRenderer>());
                }
            }
            _editItemCharacterParent = characterParent.gameObject;

            for(int index = 0; index < gizmoParent.childCount; ++index)
            {
                Transform child = gizmoParent.GetChild(index);
                if(child.name.Contains("Editor_") == false)
                {
                    child.parent = null;
                    DestroyImmediate(child.gameObject);
                    --index;
                }
            }

            return;
        }

        _editItemParent = new GameObject("EditorItemXXXXXX");
        _editItemParent.tag = "EditorItem";

        _editItemGizmoParent = new GameObject("Editor_Gizmos");
        _editItemGizmoParent.transform.SetParent(_editItemParent.transform);

        _editItemCharacterParent = new GameObject("Editor_Characters");
        _editItemCharacterParent.transform.SetParent(_editItemParent.transform);
    }

    private GameObject getGizmoItem()
    {
        if(_editItemParent == null || _editItemGizmoParent == null)
            return null;

        GameObject gizmoItem = null;
        if(_gizmoItemPool.Count == 0)
            gizmoItem = new GameObject("GizmoItem");
        else
            gizmoItem = _gizmoItemPool.Dequeue();
        
        gizmoItem.SetActive(true);
        gizmoItem.transform.SetParent(_editItemGizmoParent.transform);

        return gizmoItem;
    }

    private SpriteRenderer getCharacterItem()
    {
        if(_editItemParent == null || _editItemCharacterParent == null)
            return null;
        
        SpriteRenderer characterItem = null;
        if(_characterItemPool.Count == 0)
            characterItem = new GameObject("CharacterItem").AddComponent<SpriteRenderer>();
        else
            characterItem = _characterItemPool.Dequeue();
        
        characterItem.gameObject.SetActive(true);
        characterItem.transform.SetParent(_editItemCharacterParent.transform);
        characterItem.sprite = null;
        characterItem.flipX = false;

        return characterItem;
    }

    private void returnGizmoItem(GameObject gizmoItem)
    {
        if(gizmoItem == null)
            return;

        gizmoItem.SetActive(false);
        _gizmoItemPool.Enqueue(gizmoItem);
    }

    private void returnCharacterItem(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.gameObject.SetActive(false);
        _characterItemPool.Enqueue(spriteRenderer);
    }

    private Rect getInGameScreenSection(Vector3 position, float zoomSize, float radius)
    {
        float mainCamSize = zoomSize;
        float camHeight = (mainCamSize) * 2f;
		float camWidth = camHeight * ((float)800f / (float)600f);

        camHeight += radius * 2f;
        camWidth += radius * 2f;

        Rect rectangle = new Rect();
        rectangle.Set(position.x - (camWidth * 0.5f),position.y - (camHeight * 0.5f),camWidth,camHeight);

        return rectangle;
    }

    private enum Side
    {
        LeftTop,
        LeftBottom,
        RightTop,
        RightBottom,
        Count,
    };

    private void drawScreenSectionConnectLine(Vector3 one, float oneZoomSize,float oneRadius, Vector3 two, float twoZoomSize, float twoRadius)
    {
        Side side = Side.Count;
        if(one.x > two.x && one.y > two.y)
            side = Side.RightTop;
        else if(one.x > two.x && one.y < two.y)
            side = Side.RightBottom;
        else if(one.x < two.x && one.y > two.y)
            side = Side.LeftTop;
        else// if(one.x < two.x && one.y < two.y)
            side = Side.LeftBottom;
        
        Rect oneRect = getInGameScreenSection(one,oneZoomSize,oneRadius);
        Rect twoRect = getInGameScreenSection(two,twoZoomSize,twoRadius);

        Color currentColor = Handles.color;
        Handles.color = Color.blue;
        switch(side)
        {
            case Side.LeftTop:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMin),new Vector3(twoRect.xMin,twoRect.yMin));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMax),new Vector3(twoRect.xMax,twoRect.yMax));
            }
            break;
            case Side.LeftBottom:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMax),new Vector3(twoRect.xMin,twoRect.yMax));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMin),new Vector3(twoRect.xMax,twoRect.yMin));
            }
            break;
            case Side.RightTop:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMax),new Vector3(twoRect.xMin,twoRect.yMax));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMin),new Vector3(twoRect.xMax,twoRect.yMin));
            }
            break;
            case Side.RightBottom:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMin),new Vector3(twoRect.xMin,twoRect.yMin));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMax),new Vector3(twoRect.xMax,twoRect.yMax));
            }
            break;
        }

        Handles.color = currentColor;
    }

    private void drawInGameScreenSection(Vector3 position, float zoomSize, float radius)
    {
        Rect rectangle = getInGameScreenSection(position, zoomSize, radius);
        Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.blue);
    }

    private void drawCircleWithHandle(Vector3 position, float radius)
    {
        for(int i = 0; i < 36; ++i)
        {
            float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
            float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

            float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
            float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

            Handles.DrawLine(new Vector3(x,y) * radius + position,new Vector3(x2,y2) * radius + position);
        }
    }

    private Sprite getFirstActionSpriteFromCharacter(CharacterInfoData characterInfoData)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[baseData._defaultActionIndex]._animationInfoIndex][0];

        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }

    private Sprite getActionSpriteFromCharacter(CharacterInfoData characterInfoData, string actionName)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        if(baseData == null)
            return null;

        int targetIndex = -1;
        for(int index = 0; index < baseData._actionNodeData.Length; ++index)
        {
            if(baseData._actionNodeData[index]._nodeName == actionName)
            {
                targetIndex = index;
                break;
            }
        }

        if(targetIndex < 0)
            targetIndex = baseData._defaultActionIndex;

        if(baseData._actionNodeData[targetIndex]._animationInfoIndex == -1)
            return null;

        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[targetIndex]._animationInfoIndex][0];
        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }
}


public class MiniStageListView
{
    static private string _miniStagePath = "StageData/MiniStageData/";
   
    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private MiniStageData _addedMiniStageData = null;
    private MiniStageData _newMiniStageData = null;

    public void OnGUI()
    {
        if(GUILayout.Button("New"))
        {
            string defaultName = ".asset";
            string filePath = EditorUtility.SaveFilePanel(
                "Save Mini Stage",
                "Assets/Resources/StageData/MiniStageData/",
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(filePath)) 
                return;

            filePath = FileUtil.GetProjectRelativePath(filePath);
            MiniStageData miniStgaeData = ScriptableObject.CreateInstance<MiniStageData>();
            miniStgaeData._stageName = "NewStage";

            StagePointData stagePointData = new StagePointData(Vector3.zero);
            stagePointData._cameraZoomSize = Camera.main.orthographicSize;
            miniStgaeData._stagePointData.Add(stagePointData);

            AssetDatabase.CreateAsset(miniStgaeData, filePath);
            AssetDatabase.SaveAssets();

            _newMiniStageData = miniStgaeData;
        }

        _searchString = EditorGUILayout.TextField("Search",_searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        UnityEngine.Object[] miniStageDataArray = Resources.LoadAll(_miniStagePath,typeof(MiniStageData));
        if(miniStageDataArray == null)
            return;

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        foreach(var item in miniStageDataArray)
        {
            MiniStageData data = item as MiniStageData;
            if(_searchString != "" && (searchStringCompare(data._stageName) == false))
                continue;

            GUILayout.BeginHorizontal("box");

            if(GUILayout.Button("Show",GUILayout.Width(50f)))
            {
                PingTarget(data);
            }

            if(GUILayout.Button("Edit",GUILayout.Width(50f)))
            {
                _newMiniStageData = data;
            }
            
            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                _addedMiniStageData = data;
            }

            GUILayout.Label(data.name + ": " + data._stageName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void PingTarget(ScriptableObject obj)
    {
        EditorUtility.FocusProjectWindow();
        EditorGUIUtility.PingObject(obj);
    }

    public MiniStageData getAddedMiniStage()
    {
        MiniStageData data = _addedMiniStageData;
        _addedMiniStageData = null;

        return data;
    }

    public MiniStageData getNewMiniStageData()
    {
        MiniStageData data = _newMiniStageData;
        _newMiniStageData = null;

        return data;
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

public class CharacterInfoView
{
    private CharacterInfoData _selectedData = null;

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private Vector2 _characterInfoScrollPosition;
    private string _addedCharacterKey = "";

    private Texture _characterTexture = null;

    public void OnGUI()
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

                Sprite characterSprite = getFirstActionSpriteFromCharacter(item.Value);
                _characterTexture = characterSprite?.texture;
            }

            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                _addedCharacterKey = item.Key;
            }

            GUILayout.Label(item.Key + ": " + item.Value._displayName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if(_selectedData != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal("box");

            if(_characterTexture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(_characterTexture.width, _characterTexture.height);

                EditorGUIUtility.ScaleAroundPivot(Vector3.one,rect.center);
                GUI.DrawTexture(rect, _characterTexture,ScaleMode.ScaleToFit);
            }

            GUILayout.BeginVertical();

            _characterInfoScrollPosition = GUILayout.BeginScrollView(_characterInfoScrollPosition);

            EditorGUILayout.LabelField(_selectedData._displayName);
            EditorGUILayout.LabelField("ActionGraph: " + _selectedData._actionGraphPath);
            EditorGUILayout.LabelField("AIGraph: " + _selectedData._aiGraphPath);
            
            EditorGUILayout.LabelField("Status: " + _selectedData._statusName);
            EditorGUILayout.LabelField("Radius: " + _selectedData._characterRadius);
            EditorGUILayout.LabelField("HeadUpOffset: " + _selectedData._headUpOffset);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
    }

    private Sprite getFirstActionSpriteFromCharacter(CharacterInfoData characterInfoData)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[baseData._defaultActionIndex]._animationInfoIndex][0];

        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }

    public string getAddedCharacter()
    {
        string characterKey = _addedCharacterKey;
        _addedCharacterKey = "";

        return characterKey;
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