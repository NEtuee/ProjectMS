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

    private Queue<GameObject> _gizmoItemPool = new Queue<GameObject>();
    private List<GameObject> _usingGizmoItems = new List<GameObject>();

    private List<StagePointDataEditObject> _editingStagePointList = new List<StagePointDataEditObject>();
    
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
        if(_editItemParent == null)
        {
            GameObject editorItemObject = GameObject.FindGameObjectWithTag("EditorItem");
            if(editorItemObject == null)
            {
                Debug.Log("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
                _window?.Close();
                return;
            }

            _gizmoItemPool.Clear();
            for(int index = 0; index < editorItemObject.transform.childCount; ++index)
            {
                _gizmoItemPool.Enqueue(editorItemObject.transform.GetChild(index).gameObject);
            }
        }

        StageData editStageData = EditorGUILayout.ObjectField("Edit Data", _editStageData, typeof(StageData), true) as StageData;
        if(editStageData != _editStageData)
        {
            _editStageData = editStageData;
            constructGizmoPoints();
        }

        if(_editStageData == null)
            return;

        for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
        {
            GUILayout.BeginHorizontal();

            if(GUILayout.Button("Pick", GUILayout.Width(50f)))
                selectPoint(i);
            GUILayout.Label(i + ", " + _editStageData._stagePointData[i]._stagePoint);

            GUILayout.EndHorizontal();
        }

        if(GUILayout.Button("Add"))
        {
            addStagePoint(Vector3.zero);
        }

        if(GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(_editStageData);
        }

        
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
        }

        if(repaint)
            Repaint();
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

        if(_editItemParent == null)
            return;
        
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

            if(stagePointData._maxLimitedDistance > 0f)
                drawCircleWithHandle(stagePointData._stagePoint,stagePointData._maxLimitedDistance);

            Handles.Label(stagePointData._stagePoint, i.ToString());
            if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                selectPoint(i);

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
                Handles.DrawLine(stagePointData._stagePoint, _editStageData._stagePointData[i + 1]._stagePoint);
            }
        }
    }

    private void selectPoint(int index)
    {
        PingTarget(_editingStagePointList[index]._gizmoItem);
        Repaint();
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

        EditorUtility.SetDirty(_editStageData);
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
        _usingGizmoItems.Clear();
        _gizmoItemPool.Clear();

        _editingStagePointList.Clear();

        if(_editItemParent == null)
            _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");

        if(_editItemParent != null)
        {
            for(int index = 0; index < _editItemParent.transform.childCount; ++index)
            {
                _gizmoItemPool.Enqueue(_editItemParent.transform.GetChild(index).gameObject);
            }
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
    }

    private GameObject getGizmoItem()
    {
        if(_editItemParent == null)
            return null;

        GameObject gizmoItem = null;
        if(_gizmoItemPool.Count == 0)
            gizmoItem = new GameObject("GizmoItem");
        else
            gizmoItem = _gizmoItemPool.Dequeue();
        
        gizmoItem.SetActive(true);
        gizmoItem.transform.SetParent(_editItemParent.transform);

        _usingGizmoItems.Add(gizmoItem);
        return gizmoItem;
    }

    private void returnGizmoItem(GameObject gizmoItem)
    {
        _usingGizmoItems.Remove(gizmoItem);
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
