using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : Singleton<DialogManager>
{
    public class DialogObject
    {
        public DialogObjectData _dialogObjectData;

        public Image _uiImage;
        public Vector2 _position;

        public DialogObject()
        {
            GameObject newUiObject = new GameObject("DialogObject");
            _uiImage = newUiObject.AddComponent<Image>();

            newUiObject.SetActive(false);
        }

        public void set(DialogObjectData dialogObjectData)
        {
            _dialogObjectData = dialogObjectData;
        }

        public void clear()
        {
            _dialogObjectData = null;
            _uiImage.sprite = null;
            _uiImage.gameObject.SetActive(false);
            _position = Vector2.zero;
        }
    }

    public GameObject _dialogRootGameObject;
    public TextMeshProUGUI _characterNameTextMesh;
    public TextMeshProUGUI _dialogTextMesh;


    private SimplePool<DialogObject> _dialogObjectPool = new SimplePool<DialogObject>();
    private List<DialogObject> _activeObjectList = new List<DialogObject>();

    private DialogData _currentDialogData;

    private int _dialogEntryIndex = 0;
    private int _dialogEventIndex = 0;
    private bool _isDialogEnd = false;

    private int _dialogTextIndex = 0;

    public void initialize()
    {

    }

    public void progress(float deltaTime)
    {
        updateDialogEvent();
    }

    public void release()
    {

    }

    public void executeDialogEvent(DialogEventDataBase dialogEventData)
    {
        switch (dialogEventData.getDialogEventType())
        {
            case DialogEventType.Dialog:
                DialogEventData_Dialog dialogEvent = dialogEventData as DialogEventData_Dialog;
                //dialogEvent._
            break;
            default:
                DebugUtil.assert(false, "구현되지 않은 Dialog EventType {0}", dialogEventData.getDialogEventType());
            break;
        }

    }

    public bool isEventExecutable()
    {
        

        return true;
    }

    public void clearDialog()
    {
        foreach (var item in _activeObjectList)
        {
            item.clear();
            _dialogObjectPool.enqueue(item);
        }

        _activeObjectList.Clear();
        _dialogEventIndex = 0;
    }

    public void updateDialogEvent()
    {
        if (_currentDialogData == null)
            return;

        if (_isDialogEnd)
            return;

        DialogEventEntryData entryData = _currentDialogData._dialogEventEntryList[_dialogEntryIndex];
        int dialogEventCount = entryData._dialogEventList.Count;
        for (int index = _dialogEventIndex; index < dialogEventCount; ++index)
        {
            if (isEventExecutable() == false)
                return;

            executeDialogEvent(entryData._dialogEventList[index]);
        }
    }

    public void activeDialog(DialogData dialogData, int entryIndex)
    {
        _currentDialogData = dialogData;
        _dialogEntryIndex = entryIndex;
        _dialogEventIndex = 0;

        foreach (var item in dialogData._dialogObjectList)
        {
            DialogObject dialogObject = _dialogObjectPool.dequeue();
            dialogObject.set(item);

            _activeObjectList.Add(dialogObject);
        }

        _dialogRootGameObject.SetActive(true);
    }
}
