using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager _instance;
    
    public class DialogObject
    {
        public static float kDefaultScale = 0.21f;
        public int _objectId = 0;

        public Image _uiImage;
        public Vector2 _position;

        public DialogObject()
        {
            
        }

        public void active(int id, Transform parent, Sprite sprite, Vector2 position, bool flip)
        {
            if (_uiImage == null)
            {
                GameObject dialogObjectGameObject = new GameObject("DialogObject");
                _uiImage = dialogObjectGameObject.AddComponent<Image>();
                _uiImage.type = Image.Type.Simple;
                _uiImage.preserveAspect = true;
                _uiImage.rectTransform.pivot = new Vector2(0.5f, 0.0f);
                _uiImage.rectTransform.anchorMin = new Vector2(0.5f, 0.0f);
                _uiImage.rectTransform.anchorMax = new Vector2(0.5f, 0.0f);
                _uiImage.gameObject.SetActive(false);   
            }

            _objectId = id;
            _uiImage.transform.SetParent(parent, false);

            _uiImage.sprite = sprite;
            _uiImage.gameObject.SetActive(true);
            _uiImage.transform.position = position;

            if (flip)
                _uiImage.transform.localScale = new Vector3(-kDefaultScale, kDefaultScale, kDefaultScale);
            else
                _uiImage.transform.localScale = Vector3.one * kDefaultScale;

            _position = position;

            updateUI();
        }

        public void setSprite(Sprite sprite)
        {
            _uiImage.sprite = sprite;
            updateUI();
        }

        public void setSprite(Sprite sprite, bool flip)
        {
            _uiImage.sprite = sprite;
            setFlip(flip);
            updateUI();
        }

        public void setPosition(Vector2 position)
        {
            _position = position;
            updateUI();
        }

        public void setPosition(Vector2 position, bool flip)
        {
            _position = position;
            setFlip(flip);
            updateUI();
        }

        public void setFlip(bool flip)
        {
            if (flip)
                _uiImage.transform.localScale = new Vector3(-kDefaultScale, kDefaultScale, kDefaultScale);
            else
                _uiImage.transform.localScale = Vector3.one * kDefaultScale;
        }

        public void updateUI()
        {
            _uiImage.SetNativeSize();
            _uiImage.rectTransform.anchoredPosition = _position;
        }

        public void clear()
        {
            _uiImage.sprite = null;
            _uiImage.gameObject.SetActive(false);
            _position = Vector2.zero;
        }
    }

    public GameObject _dialogRootGameObject;
    public GameObject _dialogObjectCenterGameObject;
    public TextMeshProUGUI _characterNameTextMesh;
    public TextMeshProUGUI _dialogTextMesh;


    private SimplePool<DialogObject> _dialogObjectPool = new SimplePool<DialogObject>();
    private List<DialogObject> _activeObjectList = new List<DialogObject>();

    private DialogData _currentDialogData;
    private FMODUnity.StudioEventEmitter _currentAudioEmitter = null;

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

                // 다이얼로그 텍스트 설정
                StringKeyValueData valueData = LanguageManager._instance.getStringKeyValueFromIndex(ref dialogData._textDataName, dialogEvent._dialogIndex);
                string dialogText = valueData._value;
                _dialogTextMesh.text = dialogText;
                _dialogMaxVisibleCharacter = dialogText.Length;
                _wordPerSec = dialogEvent._wordPerSec;

                // 캐릭터 이름 설정
                if (string.IsNullOrEmpty(dialogEvent._displayCharacterKey))
                {
                    // _displayCharacterKey가 없으면 캐릭터 이름을 비움
                    _characterNameTextMesh.text = "";
                }
                else
                {
                    // _displayCharacterKey가 있으면 CharacterInfoManager를 통해 캐릭터 이름 가져오기
                    CharacterInfoData characterInfo = CharacterInfoManager.Instance().GetCharacterInfoData(dialogEvent._displayCharacterKey);
                    if (characterInfo != null)
                    {
                        _characterNameTextMesh.text = characterInfo._displayName;
                    }
                    else
                    {
                        // 캐릭터 정보를 찾을 수 없으면 키 자체를 표시하거나 비움
                        _characterNameTextMesh.text = "";
                        DebugUtil.assert(false, "Dialog Event : Character info not found for key {0}", dialogEvent._displayCharacterKey);
                    }
                }

                _dialogSpeaking = true;
                _dialogSpeakTimer = 0f;

                _currentAudioEmitter?.Stop();
            break;
            case DialogEventType.ActiveObject:
                DialogEventData_ActiveObject activeObjectEvent = dialogEventData as DialogEventData_ActiveObject;

                Sprite sprite = ResourceContainerEx.Instance().GetSprite(activeObjectEvent._spritePath);
                if (sprite == null)
                {
                    DebugUtil.assert(false, "Dialog Event ActiveObject : Sprite not found at path {0}", activeObjectEvent._spritePath);
                    return;
                }

                activeObject(activeObjectEvent._objectId, sprite, activeObjectEvent._position, activeObjectEvent._flip);
            break;
            case DialogEventType.SetSprite:
                DialogEventData_SetSprite setSpriteEvent = dialogEventData as DialogEventData_SetSprite;

                Sprite newSprite = ResourceContainerEx.Instance().GetSprite(setSpriteEvent._spritePath);
                if (newSprite == null)
                {
                    DebugUtil.assert(false, "Dialog Event SetSprite : Sprite not found at path {0}", setSpriteEvent._spritePath);
                    return;
                }

                setObjectSprite(setSpriteEvent._objectId, newSprite, setSpriteEvent._flip);
            break;
            case DialogEventType.SetPosition:
                DialogEventData_SetPosition setPositionEvent = dialogEventData as DialogEventData_SetPosition;

                setObjectPosition(setPositionEvent._objectId, setPositionEvent._position, setPositionEvent._flip);
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
        _characterNameTextMesh.text = "";

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
        _currentAudioEmitter?.Stop();
        _currentAudioEmitter = null;

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
        clearDialog();

        _dialogEntryIndex = dialogData.findDialogEntryIndex(entryKey);
        if (_dialogEntryIndex < 0)
        {
            DebugUtil.assert(false, "Dialog Entry Key [{0}] not found in DialogData", entryKey);
            return;
        }

        _currentDialogData = dialogData;
        _dialogEventIndex = 0;
        _isDialogEnd = false;

        _dialogRootGameObject.SetActive(true);
    }

    public void activeObject(int id, Sprite sprite, Vector2 position, bool flip)
    {
        DialogObject dialogObject = getActiveObject(id);
        if (dialogObject == null)
        {
            dialogObject = _dialogObjectPool.dequeue();
        }

        dialogObject.active(id, _dialogObjectCenterGameObject.transform, sprite, position, flip);
        _activeObjectList.Add(dialogObject);
    }

    public void setObjectSprite(int id, Sprite sprite)
    {
        DialogObject dialogObject = getActiveObject(id);
        if (dialogObject == null)
            return;

        dialogObject.setSprite(sprite);
    }

    public void setObjectSprite(int id, Sprite sprite, bool flip)
    {
        DialogObject dialogObject = getActiveObject(id);
        if (dialogObject == null)
            return;

        dialogObject.setSprite(sprite, flip);
    }

    public void setObjectPosition(int id, Vector2 position)
    {
        DialogObject dialogObject = getActiveObject(id);
        if (dialogObject == null)
            return;

        dialogObject.setPosition(position);
    }

    public void setObjectPosition(int id, Vector2 position, bool flip)
    {
        DialogObject dialogObject = getActiveObject(id);
        if (dialogObject == null)
            return;

        dialogObject.setPosition(position, flip);
    }

    public DialogObject getActiveObject(int id)
    {
        return _activeObjectList.Find(obj => obj._objectId == id);
    }
}
