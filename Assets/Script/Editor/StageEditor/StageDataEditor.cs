using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class StageDataEditor : EditorWindow
{
    private class StagePointDataEditObject
    {
        public StagePointData _stagePointData;
        public SerializedProperty _onEnterSequencerPathProperty = null;
        public SerializedProperty _onExitSequencerPathProperty = null;

        public GameObject _gizmoItem;

        public bool syncPosition()
        {
            if(_stagePointData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = _stagePointData._stagePoint != _gizmoItem.transform.position;
            _stagePointData._stagePoint = _gizmoItem.transform.position;

            return syncSuccess;
        }
    }
    public StageData _editStageData;

    private static StageDataEditor _window;
    private GameObject _editItemParent = null;
    private GameObject _editItemGizmoParent = null;

    private Queue<GameObject> _gizmoItemPool = new Queue<GameObject>();

    private List<StagePointDataEditObject> _editingStagePointList = new List<StagePointDataEditObject>();

    private GameObject _backgroundPrefabObject = null;
    
    private Vector2 _pointItemScroll = Vector2.zero;

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
        "Point Inspector",
        "Character Palette",
        "Sequencer Palette",
    };

    private int _pointSelectedIndex = -1;
    private int _editItemMenuSelectedIndex = 0;
    private int _editMenuSelectedIndex = 0;

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

            if(GUILayout.Button("Save Data"))
                saveCurrentData();
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
                onPointInspectorGUI();
            if(_editMenuSelectedIndex == 1)
                _characterInfoView.OnGUI();
        GUILayout.EndVertical();
    }

    private void onTriggerGUI()
    {
    }

    private void onCharacterGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0)
            return;
        
        
    }

    private void onPointInspectorGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0)
            return;

        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        GUILayout.Label("Position: " + stagePointData._stagePoint.ToString());
        stagePointData._maxLimitedDistance = EditorGUILayout.FloatField("Radius", stagePointData._maxLimitedDistance);

        if(stagePointDataEditObject._onEnterSequencerPathProperty == null || stagePointDataEditObject._onExitSequencerPathProperty == null)
        {
            if(_stageDataListProperty != null)
            {
                SerializedProperty stagePointDataProperty = _stageDataListProperty.GetArrayElementAtIndex(_pointSelectedIndex);
                stagePointDataEditObject._onEnterSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onEnterSequencerPath");
                stagePointDataEditObject._onExitSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onExitSequencerPath");
            }
        }
        else
        {
            EditorGUILayout.PropertyField(stagePointDataEditObject._onEnterSequencerPathProperty);
            EditorGUILayout.PropertyField(stagePointDataEditObject._onExitSequencerPathProperty);
        }
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

    void Update() 
    {
        if(_editStageData == null)
            return;

        if(_editStageData._stagePointData.Count != 0 && _editingStagePointList.Count == 0)
            constructGizmoPoints();

        bool repaint = false;
        for(int i = 0; i < _editingStagePointList.Count; ++i)
        {
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

        EditorUtility.SetDirty(_editStageData);
        if(_backgroundPrefabObject != null)
            PrefabUtility.SaveAsPrefabAssetAndConnect(_backgroundPrefabObject, "Assets/Resources/" + _editStageData._backgroundPrefabPath + ".prefab",InteractionMode.AutomatedAction);
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

        for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
        {
            StagePointData stagePointData = _editStageData._stagePointData[i];
            Vector3 itemPosition = stagePointData._stagePoint;
            Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
            };

            Color currentColor = Handles.color;
            Handles.color = i == _pointSelectedIndex ? Color.green : currentColor;

            if(stagePointData._maxLimitedDistance > 0f)
                drawCircleWithHandle(stagePointData._stagePoint,stagePointData._maxLimitedDistance);

            Handles.Label(stagePointData._stagePoint, i.ToString());
            if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                selectPoint(i);

            Handles.color = currentColor;

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
        _editStageData._stagePointData.Add(stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Add(editObject);

        EditorUtility.SetDirty(_editStageData);
    }

    private void deleteStagePoint(int index)
    {
        if(index < 0 || _editStageData._stagePointData.Count <= index)
            return;

        _editStageData._stagePointData.RemoveAt(index);

        returnGizmoItem(_editingStagePointList[index]._gizmoItem);
        _editingStagePointList.RemoveAt(index);
    }

    private void loadStageData()
    {
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
            return;
        
        foreach(var item in _editStageData._stagePointData)
        {
            StagePointDataEditObject editObject = new StagePointDataEditObject();
            editObject._stagePointData = item;
            editObject._gizmoItem = getGizmoItem();
            editObject._gizmoItem.transform.position = item._stagePoint;

            _editingStagePointList.Add(editObject);
        }
    }

    private void clearStagePointList()
    {
        _gizmoItemPool.Clear();

        _editingStagePointList.Clear();

        if(_editItemParent == null)
            _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");

        if(_editItemParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        Transform gizmoParent = _editItemParent.transform.Find("Gizmos");
        if(gizmoParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        _editItemGizmoParent = gizmoParent.gameObject;

        for(int index = 0; index < _editItemGizmoParent.transform.childCount; ++index)
        {
            _gizmoItemPool.Enqueue(_editItemGizmoParent.transform.GetChild(index).gameObject);
        }
    }

    private void createOrFindEditorItem()
    {
        _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");
        if(_editItemParent != null)
        {
            _gizmoItemPool.Clear();
            for(int index = 0; index < _editItemParent.transform.childCount; ++index)
            {
                _gizmoItemPool.Enqueue(_editItemParent.transform.GetChild(index).gameObject);
            }

            return;
        }

        _editItemParent = new GameObject("EditorItemXXXXXX");
        _editItemParent.tag = "EditorItem";

        _editItemGizmoParent = new GameObject("Gizmos");
        _editItemGizmoParent.transform.SetParent(_editItemParent.transform);
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

    private void returnGizmoItem(GameObject gizmoItem)
    {
        gizmoItem.SetActive(false);
        _gizmoItemPool.Enqueue(gizmoItem);
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
}




public class CharacterInfoView
{
    private CharacterInfoData _selectedData = null;

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;

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