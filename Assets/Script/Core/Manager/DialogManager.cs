using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager _instance;
    public class DialogObject
    {
        public DialogObjectData _dialogObjectData;

        public Image _uiImage;
        public Vector2 _position;

        public DialogObject()
        {
            
        }

        public void set(DialogObjectData dialogObjectData)
        {
            if (_uiImage == null)
            {
                GameObject newUiObject = new GameObject("DialogObject");
                _uiImage = newUiObject.AddComponent<Image>();

                newUiObject.SetActive(false);
            }
            
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

    private int _dialogEntryIndex = -1;
    private int _dialogEventIndex = -1;
    private bool _isDialogEnd = true;

    private int _dialogMaxVisibleCharacter = 0;
    private float _wordPerSec = 12f;
    private bool _dialogSpeaking = false;

    private float _dialogSpeakTimer = 0f;
    private bool _waitInput = false;

    private void Awake()
    {
        _instance = this;
        initialize();
    }

    public void initialize()
    {
        clearDialog();
    }

    public void progress(float deltaTime)
    {
        if (_isDialogEnd)
            return;
            
        updateInput();
        updateDialogEvent();
        updateDialog(deltaTime);
    }

    public void release()
    {

    }

    public void executeDialogEvent(DialogData dialogData, DialogEventDataBase dialogEventData)
    {
        switch (dialogEventData.getDialogEventType())
        {
            case DialogEventType.Dialog:
                DialogEventData_Dialog dialogEvent = dialogEventData as DialogEventData_Dialog;

                string dialogText = LanguageManager._instance.getTextFromFile(ref dialogData._textDataName, dialogEvent._dialogIndex);
                _dialogTextMesh.text = dialogText;
                _dialogTextMesh.maxVisibleCharacters = 0;
                _dialogMaxVisibleCharacter = dialogText.Length;
                _wordPerSec = dialogEvent._wordPerSec;

                _dialogSpeaking = true;
                _dialogSpeakTimer = 0f;
            break;
            default:
                DebugUtil.assert(false, "구현되지 않은 Dialog EventType {0}", dialogEventData.getDialogEventType());
            break;
        }

    }

    public bool isEventExecutable()
    {
        if (_dialogSpeaking)
            return false;

        if (_waitInput)
            return false;

        return true;
    }

    public void clearDialog()
    {
        foreach (var item in _activeObjectList)
        {
            item.clear();
            _dialogObjectPool.enqueue(item);
        }

        _dialogTextMesh.text = "";

        _activeObjectList.Clear();
        _dialogEventIndex = -1;
        _dialogEntryIndex = -1;
        _dialogMaxVisibleCharacter = 0;
        _wordPerSec = 1f;

        _currentDialogData = null;

        _waitInput = false;
        _isDialogEnd = true;
        _dialogSpeaking = false;
        _dialogSpeakTimer = 0f;

        _dialogRootGameObject.SetActive(false);

    }

    public void updateInput()
    {
        if (_waitInput)
        {
            _waitInput = Input.GetKeyDown(KeyCode.Mouse0) == false;
        }
    }

    public void updateDialog(float deltaTime)
    {
        if (_dialogSpeaking == false)
            return;

        _dialogSpeakTimer += deltaTime;
        int displayTextCount = (int)math.min(_dialogSpeakTimer * _wordPerSec, _dialogMaxVisibleCharacter);

        _dialogTextMesh.maxVisibleCharacters = displayTextCount;

        _dialogSpeaking = displayTextCount < _dialogMaxVisibleCharacter;
        _waitInput = _dialogSpeaking == false;
    }

    public void updateDialogEvent()
    {
        DialogEventEntryData entryData = _currentDialogData._dialogEventEntryList[_dialogEntryIndex];
        int dialogEventCount = entryData._dialogEventList.Count;

        while (isEventExecutable())
        {
            if (_dialogEventIndex < 0)
            {
                clearDialog();
                break;
            }

            executeDialogEvent(_currentDialogData, entryData._dialogEventList[_dialogEventIndex]);
            _dialogEventIndex = entryData._dialogEventList[_dialogEventIndex]._nextIndex;
        }
    }

    public void activeDialog(DialogData dialogData, string entryKey)
    {
        _dialogEntryIndex = dialogData.findDialogEntryIndex(entryKey);
        if (_dialogEntryIndex < 0)
            return;

        _currentDialogData = dialogData;
        _dialogEventIndex = _currentDialogData._dialogEventEntryList[_dialogEntryIndex]._entryEventIndex;
        _isDialogEnd = false;

        foreach (var item in dialogData._dialogObjectList)
        {
            DialogObject dialogObject = _dialogObjectPool.dequeue();
            dialogObject.set(item);

            _activeObjectList.Add(dialogObject);
        }

        _dialogRootGameObject.SetActive(true);
    }
}
