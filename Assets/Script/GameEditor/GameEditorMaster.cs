using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEditorMaster : MonoBehaviour
{
    public static GameEditorMaster _instance;
    public bool _actionDebugAll = false;
    public bool _aiDebugAll = false;
    public bool _statusDebugAll = false;
    public bool _animationDebugAll = false;

    public Slider   _timeMagnitudeSlider;
    public Text     _timeMagnitudeText;

    private static string _hotKey_EditorOnOff = "EditorHotKey_EditorOnOff";
    private static string _hotKey_UpdateFrame = "EditorHotKey_UpdateFrame";

    private List<EditorWindowBase> _currentWindows = new List<EditorWindowBase>();
    private GameObject _editorParent;
    private EditorWindowBase _currentFocusWindow;
    private bool _activeEditor = false;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _editorParent = transform.Find("Editors").gameObject;
    }

    private void Update()
    {
        updateHotkeysAlways();

        if(_activeEditor == false)
            return;

        updateHotkeysEditor();

        _currentFocusWindow?.updateHotKey();

        foreach(EditorWindowBase window in _currentWindows)
        {
            if(window == _currentFocusWindow)
                continue;

            window.updateHotKey();
        }

        _currentFocusWindow?.mainUpdate(Time.deltaTime);

        foreach(EditorWindowBase window in _currentWindows)
        {
            if(window == _currentFocusWindow)
                continue;

            window.mainUpdate(Time.deltaTime);
        }

#if UNITY_EDITOR
        updateCharacterPick();
#endif
    }

    void updateHotkeysAlways()
    {
        if(ActionKeyInputManager.Instance().keyCheck(_hotKey_EditorOnOff))
        {
            if(_activeEditor)
                editorOff();
            else
                editorOn();
        }
        else if(ActionKeyInputManager.Instance().keyCheck(_hotKey_UpdateFrame))
        {
            updateFrame();
        }
    }

    void updateHotkeysEditor()
    {

    }
#if UNITY_EDITOR
    public void updateCharacterPick()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) == false)
            return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = MathEx.deleteZ(mousePosition);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        var characters = sceneCharacterManager.getCurrentlyEnabledCharacters();

        foreach(var character in characters.Values)
        {
            BoundBox boundBox = character.getCollisionInfo().getBoundBox();
            if(boundBox.intersection(mousePosition))
            {
                Selection.activeGameObject = character.gameObject;
                EditorGUIUtility.PingObject(character.gameObject);
                break;
            }
        }
    }
#endif
    public void setMainWindow(EditorWindowBase window)
    {
        _currentFocusWindow = window;
    }

    private void updateFrame()
    {
        editorOn();
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_updateFrame,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);
    }

    public void updateTimeMagnitude()
    {
        GlobalTimer.Instance().setDebugTimeRatio(_timeMagnitudeSlider.value);
        _timeMagnitudeText.text = "Magnitude: " + _timeMagnitudeSlider.value.ToString();
    }

    public void setActionDebug() {_actionDebugAll = !_actionDebugAll;}
    public void setAiDebug() {_aiDebugAll = !_aiDebugAll;}
    public void setStatusDebug() {_statusDebugAll = !_statusDebugAll;}
    public void setAnimationDebug() {_animationDebugAll = !_animationDebugAll;}

    public bool isActionDebug() {return _actionDebugAll;}
    public bool isAiDebug() {return _aiDebugAll;}
    public bool isStatusDebug() {return _statusDebugAll;}
    public bool isAnimationDebug() {return _animationDebugAll;}

    private void editorOn()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_pauseUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = true;
        _editorParent?.SetActive(true);

        Cursor.visible = true;
    }

    private void editorOff()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_playUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = false;
        _editorParent?.SetActive(false);

        Cursor.visible = false;
    }
}
