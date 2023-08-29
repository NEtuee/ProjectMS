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
        public SerializedProperty _onEnterSequencerPathProperty = null;
        public SerializedProperty _onExitSequencerPathProperty = null;

        public GameObject _gizmoItem;
        public List<SpriteRenderer> _characterObjectList = new List<SpriteRenderer>();
        public List<StagePointCharacterSpawnData> _characterSpawnDataList = new List<StagePointCharacterSpawnData>();

        public bool syncPosition()
        {
            if(_stagePointData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = _stagePointData._stagePoint != _gizmoItem.transform.position;
            _stagePointData._stagePoint = _gizmoItem.transform.position;

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
    
    private class SequencerPathEditor
    {
        bool _listOpen = false;
        bool _viewerOpen = false;
        bool _updateFileList = true;
        
        string _currentFilePath = "";
        string _currentFolderName = "";
        string _label = "";

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
            if(GUILayout.Button(_listOpen ? "▼" : "▶", GUILayout.Width(25f)))
            {
                _listOpen = !_listOpen; 
                if(_listOpen == false)
                {
                    _viewerOpen = false;
                    clear();
                }
            }

            if(GUILayout.Button(_viewerOpen ? "○" : "•", GUILayout.Width(25f)))
            {
                _viewerOpen = !_viewerOpen;
                if(_viewerOpen)
                    _listOpen = true;
                clear();
            }

            GUILayout.Label(_label,GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            if(_listOpen)
            {
                drawSequencerList(ref targetList);
            }

            if(_viewerOpen)
            {
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
            List<string> stringList = new List<string>();
            stringList.AddRange(targetList);

            stringList.Add(_outFilePath);

            targetList = stringList.ToArray();
        }

        private void drawSequencerList(ref string[] targetList)
        {
            _sequencerViewerScroll = GUILayout.BeginScrollView(_sequencerViewerScroll, "box");

            GUILayout.Label("Sequencer List");
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
                if(GUILayout.Button("X",GUILayout.Width(25f)))
                    deleteIndex = index;

                Color colorOrigin = GUI.color;
                if(index == 0)
                    GUI.color = Color.green;
                GUILayout.Label(targetList[index]);
                if(index == 0)
                    GUI.color = colorOrigin;

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
                GUILayout.BeginHorizontal();
                GUILayout.Space(25f);
                if(GUILayout.Button(item.Name,buttonStyle))
                {
                    clear();
                    _outFilePath = item.FullName;
                    _outFilePath = _outFilePath.Replace(IOControl.PathForDocumentsFile("Assets\\Data\\SequencerGraph\\").Replace('/','\\'),"");

                    _viewerOpen = false;
                    return true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            return false;
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

    private List<StagePointDataEditObject> _editingStagePointList = new List<StagePointDataEditObject>();

    private GameObject _backgroundPrefabObject = null;
    
    private Vector2 _pointItemScroll = Vector2.zero;
    private Vector2 _characterSpawnScroll = Vector2.zero;

    private CharacterInfoView _characterInfoView = new CharacterInfoView();
    public SerializedObject _stageDataSerializedObject;
    public SerializedProperty _stageDataListProperty;

    private string[] _editItemMenuStrings = 
    {
        "Point",
        "Character",
        "Trigger",
    };

    private string[] _editMenuStrings = 
    {
        "Inspector",
        "Character Palette",
        "Sequencer Palette",
    };

    private SequencerPathEditor _onEnterSequencerPathEditor = new SequencerPathEditor("On Enter Sequencer Path");
    private SequencerPathEditor _onExitSequencerPathEditor = new SequencerPathEditor("On Exit Sequencer Path");

    private int _pointSelectedIndex = -1;
    private int _characterSelectedIndex = -1;
    private int _editItemMenuSelectedIndex = 0;
    private int _editMenuSelectedIndex = 0;


    private string _pointCharacterSearchString = "";
    private string[] _pointCharacterSearchStringList;
    private string _pointCharacterSearchStringCompare = "";

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
            StageData editStageData = EditorGUILayout.ObjectField("Edit Data", _editStageData, typeof(StageData), true) as StageData;
            GUI.enabled = editStageData != null;
            if(GUILayout.Button("Refresh"))
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
            _editStageData._backgroundPrefabPath = EditorGUILayout.TextField("Prefab Path",_editStageData._backgroundPrefabPath);

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
                onTriggerGUI();
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
            }
            if(_editMenuSelectedIndex == 1)
            {
                _characterInfoView.OnGUI();
                string addedCharacter = _characterInfoView.getAddedCharacter();
                if(addedCharacter != "")
                {
                    addCharacterToPoint(_pointSelectedIndex,addedCharacter);
                }
            }
        GUILayout.EndVertical();
    }

    private void onTriggerGUI()
    {
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
                        && searchStringCompare(stagePointData._characterSpawnData[i]._uniqueKey,_pointCharacterSearchStringList) == false))
                    continue;

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _characterSelectedIndex ? Color.green : currentColor;

                string targetName = stagePointData._characterSpawnData[i]._characterKey;
                if(stagePointData._characterSpawnData[i]._uniqueKey != "")
                    targetName += ": " + stagePointData._characterSpawnData[i]._uniqueKey;
                GUILayout.Label(targetName,GUILayout.Width(150f));

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
            || _characterSelectedIndex < 0)
            return;

        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointCharacterSpawnData characterSpawnData = _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData[_characterSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        GUILayout.Label("Local Position: " + characterSpawnData._localPosition.ToString());
        characterSpawnData._flip = EditorGUILayout.Toggle("Flip",characterSpawnData._flip);
        characterSpawnData._searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", characterSpawnData._searchIdentifier);
        characterSpawnData._activeType = (StageSpawnCharacterActiveType)EditorGUILayout.EnumPopup("Active Type", characterSpawnData._activeType);

        characterSpawnData._uniqueKey = EditorGUILayout.TextField("Unique Key",characterSpawnData._uniqueKey);

        
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

    private void onPointInspectorGUI()
    {
        if(_editingStagePointList == null || _editingStagePointList.Count == 0 || _editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0)
            return;

        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        GUILayout.Label("Position: " + stagePointData._stagePoint.ToString()); 
        stagePointData._maxLimitedDistance = EditorGUILayout.FloatField("Radius", stagePointData._maxLimitedDistance);
        stagePointData._cameraZoomSize = EditorGUILayout.FloatField("ZoomSize", stagePointData._cameraZoomSize);
        stagePointData._cameraZoomSpeed = EditorGUILayout.FloatField("ZoomSpeed", stagePointData._cameraZoomSpeed);
        stagePointData._lerpCameraZoom = EditorGUILayout.Toggle("LerpToNextZoom", stagePointData._lerpCameraZoom);

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
            if(GUILayout.Button("Add Point"))
            {
                addStagePoint(Vector3.zero);
            }
        GUILayout.EndHorizontal();


        _pointItemScroll = GUILayout.BeginScrollView(_pointItemScroll,"box");
            for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _pointSelectedIndex ? Color.green : currentColor;

                GUILayout.Label(i.ToString(),GUILayout.Width(25f));

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

    void Update() 
    {
        if(_editStageData == null)
            return;

        if(Application.isPlaying)
            return;

        if(_editStageData._stagePointData.Count != 0 && _editingStagePointList.Count == 0)
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

            repaint |= _editingStagePointList[i].syncPosition();

            if(i == 0 && _backgroundPrefabObject != null)
                _backgroundPrefabObject.transform.position = _editingStagePointList[i]._stagePointData._stagePoint;
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
            PrefabUtility.SaveAsPrefabAssetAndConnect(_backgroundPrefabObject, "Assets/Resources/" + _editStageData._backgroundPrefabPath + ".prefab",InteractionMode.AutomatedAction);
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
                    if(i == _pointSelectedIndex)
                    {
                        Handles.color = Color.green;
                        Handles.DrawLine(stagePointData._stagePoint,characterWorld);
                    }

                    if(stagePointData._characterSpawnData[index]._uniqueKey != "")
                        Handles.Label(characterWorld,stagePointData._characterSpawnData[index]._uniqueKey);

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
            if(_drawScreenToMousePoint || i == _pointSelectedIndex)
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

                drawArrow(stagePointData._stagePoint, _editStageData._stagePointData[i + 1]._stagePoint, 0.3f);
                Handles.DrawLine(stagePointData._stagePoint, _editStageData._stagePointData[i + 1]._stagePoint);
            }
        }
    }

    public void drawArrow(Vector3 start, Vector3 end, float arrowLength)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 arrowUp = new Vector3(-1f,1f).normalized * arrowLength;
        Vector3 arrowDown = new Vector3(-1f,-1f).normalized * arrowLength;

        float angle = Vector3.SignedAngle(Vector3.right, direction,Vector3.forward);
        Quaternion rotate = Quaternion.Euler(0f,0f,angle);
        arrowUp = rotate * arrowUp;
        arrowDown = rotate * arrowDown;

        Handles.DrawLine(end, end + arrowUp);
        Handles.DrawLine(end, end + arrowDown);
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

    private void PingTarget(GameObject obj)
    {
        EditorGUIUtility.PingObject(obj);
        Selection.activeGameObject = obj;
    }

    private void addStagePoint(Vector3 spawnPosition)
    {
        StagePointDataEditObject editObject = new StagePointDataEditObject();
        StagePointData stagePointData = new StagePointData(spawnPosition);
        stagePointData._cameraZoomSize = Camera.main.orthographicSize;

        _editStageData._stagePointData.Add(stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Add(editObject);

        selectPoint(_editStageData._stagePointData.Count - 1);

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

    private void loadStageData()
    {
        _pointSelectedIndex = 0;
        _characterSelectedIndex = 0;

        if(_editStageData == null)
            return;

        if(_backgroundPrefabObject != null)
            DestroyImmediate(_backgroundPrefabObject);
        
        if(_editStageData._backgroundPrefabPath != "")
        {
            GameObject prefab = ResourceContainerEx.Instance().GetPrefab(_editStageData._backgroundPrefabPath);
            if(prefab == null)
            {
                DebugUtil.assert(false, "StageData의 Background Prefab Path가 잘못되었습니다. 확인 필요");
                return;
            }

            _backgroundPrefabObject = Instantiate(prefab);
            _backgroundPrefabObject.transform.position = _editStageData._stagePointData.Count == 0 ? Vector3.zero : _editStageData._stagePointData[0]._stagePoint;
            _backgroundPrefabObject.transform.SetParent(_editItemParent.transform);
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

        _gizmoItemPool.Clear();
        _characterItemPool.Clear();
        _editingStagePointList.Clear();

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
        float camHeight = mainCamSize * 2f + radius * 2f;
		float camWidth = camHeight * ((float)800f / (float)600f) + radius * 2f;

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