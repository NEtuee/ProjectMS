#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public class NodeData
{
    public string _guid;
    public Vector2 _position;
    public int _dataIndex;
    public int _groupIndex = -1;
}

[Serializable]
public class NoteData
{
    public Rect _position;
    public string _title;
    public string _text;
    public StickyNoteTheme _theme;
    public StickyNoteFontSize _fontSize;
}

[Serializable]
public class GroupData
{
    public string _title;
    public Rect _position;
}

[Serializable]
public class LinkData
{
    public string _outputGuid;
    public string _outputPort;
    public string _inputGuid;
    public string _inputPort;
}
#endif