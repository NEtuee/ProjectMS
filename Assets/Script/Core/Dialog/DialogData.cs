using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Scriptable Objects/DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeReference] public List<DialogObjectData> _dialogObjectList = new List<DialogObjectData>();
    [SerializeReference] public List<DialogEventEntryData> _dialogEventEntryList = new List<DialogEventEntryData>();

#if UNITY_EDITOR
    [SerializeReference] public List<NodeData> _nodeData = new List<NodeData>();
    [SerializeReference] public List<NoteData> _noteData = new List<NoteData>();
    [SerializeReference] public List<GroupData> _groupData = new List<GroupData>();
    [SerializeReference] public List<LinkData> _linkData = new List<LinkData>();
#endif
}

[Serializable]
public class DialogObjectData
{
#if UNITY_EDITOR
    public string _objectId;
#endif

    public int _objectIdHash;

    public bool _startEnable = false;

}

[Serializable]
public class DialogEventEntryData
{
    public string _entryKey;

    public int _entryEventIndex = 0;
    public List<DialogEventDataBase> _dialogEventList = new List<DialogEventDataBase>();
}

public enum DialogEventType
{
    Dialog,
}

[Serializable]
public abstract class DialogEventDataBase
{
    public abstract DialogEventType getDialogEventType();

#if UNITY_EDITOR
    public string GUID { get { return _editorGuidString; } }
    private string _editorGuidString;

    public void createNewGUIDForEditor()
    {
        _editorGuidString = System.Guid.NewGuid().ToString();
    }
#endif

}

[Serializable]
public class DialogEventData_Dialog : DialogEventDataBase
{
    public override DialogEventType getDialogEventType() => DialogEventType.Dialog;

#if UNITY_EDITOR
    public string _dialogGUID;
#endif

    public int _dialogGUIDHash;
    public string _displayCharacterKey;
    public float _wordPerSec = 12f;

}