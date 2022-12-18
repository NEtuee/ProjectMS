using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameEditorMaster : MonoBehaviour
{
    private static string _hotKey_EditorOnOff = "EditorHotKey_EditorOnOff";
    private static string _hotKey_UpdateFrame = "EditorHotKey_UpdateFrame";

    private List<EditorWindowBase> _currentWindows = new List<EditorWindowBase>();
    private GameObject _editorParent;
    private EditorWindowBase _currentFocusWindow;
    private bool _activeEditor = false;

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

    private void editorOn()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_pauseUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = true;
        _editorParent?.SetActive(true);
    }

    private void editorOff()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_playUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = false;
        _editorParent?.SetActive(false);
    }
}
