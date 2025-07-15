using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Scriptable Object/DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeReference] public List<DialogEventEntryData> _dialogEventEntryList = new List<DialogEventEntryData>();

    [SerializeReference] public string _textDataName = "";

    public int findDialogEntryIndex(string key)
    {
        for (int index = 0; index < _dialogEventEntryList.Count; ++index)
        {
            if (_dialogEventEntryList[index]._entryKey == key)
                return index;
        }

        return -1;
    }

    static public DialogEventDataBase createDialogEvent(DialogEventType dialogEventType)
    {
        string nodeTypeName = "DialogEventData_" + dialogEventType.ToString();
        object typeInstance = Activator.CreateInstance(Type.GetType(nodeTypeName));

        return typeInstance as DialogEventDataBase;
    }

    public void clearAll()
    {
        _dialogEventEntryList.Clear();

#if UNITY_EDITOR
        _nodeData.Clear();
        _noteData.Clear();
        _groupData.Clear();
        _linkData.Clear();
#endif
    }

#if UNITY_EDITOR
    [SerializeReference] public List<NodeData> _nodeData = new List<NodeData>();
    [SerializeReference] public List<NoteData> _noteData = new List<NoteData>();
    [SerializeReference] public List<GroupData> _groupData = new List<GroupData>();
    [SerializeReference] public List<LinkData> _linkData = new List<LinkData>();
#endif
}

[Serializable]
public class DialogEventEntryData
{
    public string _entryKey;

    [SerializeReference] public List<DialogEventDataBase> _dialogEventList = new List<DialogEventDataBase>();

#if UNITY_EDITOR
    public string _editorGuidString;
#endif
}

public enum DialogEventType
{
    Entry,
    Dialog,
    ActiveObject,
    SetSprite,
    SetPosition,
    Count,
}

[Serializable]
public abstract class DialogEventDataBase
{
    public abstract DialogEventType getDialogEventType();
    public int _nextIndex = -1;

#if UNITY_EDITOR
    public string _editorGuidString;
#endif

}

[Serializable]
public class DialogEventData_Dialog : DialogEventDataBase
{
    public override DialogEventType getDialogEventType() => DialogEventType.Dialog;

    public int _dialogIndex;
    public string _displayCharacterKey;
    public float _wordPerSec = 12f;

}

[Serializable]
public class DialogEventData_ActiveObject : DialogEventDataBase
{
    public override DialogEventType getDialogEventType() => DialogEventType.ActiveObject;

#if UNITY_EDITOR
    public string _objectIdString;
#endif

    public int _objectId = 0;
    public string _spritePath = "";
    public Vector2 _position = Vector2.zero;
    public bool _flip = false;
}

[Serializable]
public class DialogEventData_SetSprite : DialogEventDataBase
{
    public override DialogEventType getDialogEventType() => DialogEventType.SetSprite;

#if UNITY_EDITOR
    public string _objectIdString;
#endif

    public int _objectId = 0;
    public string _spritePath = "";
    public bool _flip = false;
}

[Serializable]
public class DialogEventData_SetPosition : DialogEventDataBase
{
    public override DialogEventType getDialogEventType() => DialogEventType.SetPosition;

#if UNITY_EDITOR
    public string _objectIdString;
#endif

    public int _objectId = 0;
    public Vector2 _position = Vector2.zero;
    public bool _flip = false;
}