using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

public enum AudioBoardEventType
{
    Log,
    Count,
}

[CreateAssetMenu(fileName = "AudioBoard", menuName = "Scriptable Object/Audio Board")]
public class AudioBoardEventSet : ScriptableObject
{
    public enum AudioBoardEventEntryType
    {
        Custom,
        Count,
    }

    [Serializable]
    public struct AudioBoardEventEntry
    {
        public AudioBoardEventEntryType _entryType;
        public string _entryName;
        public int _entryNameHash;
        public int _entryIndex;
    }

    [SerializeReference] public List<AudioBoardEventBase> _audioBoardEventBase = new List<AudioBoardEventBase>();
    [SerializeField] public List<AudioBoardEventEntry> _eventEntry = new List<AudioBoardEventEntry>();

#if UNITY_EDITOR

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

    [SerializeReference] public List<NodeData> _nodeData = new List<NodeData>();
    [SerializeReference] public List<NoteData> _noteData = new List<NoteData>();
    [SerializeReference] public List<GroupData> _groupData = new List<GroupData>();
    [SerializeReference] public List<LinkData> _linkData = new List<LinkData>();
#endif
}

[Serializable]
public abstract class AudioBoardEventBase
{
    public virtual AudioBoardEventType getEventType() => AudioBoardEventType.Count;
    public int[] _nextEventIndex = null;

    public string GUID{get{return _guidString;}}

    private string _guidString;

    public abstract void entry();
    public abstract bool update();
    public abstract void end();

    public virtual int getNextEventIndex()
    {
        return (_nextEventIndex == null || _nextEventIndex.Length == 0) ? -1 : _nextEventIndex[0];
    }

#if UNITY_EDITOR
    static public AudioBoardEventBase createAudioBoardEvent(AudioBoardEventType gameEventType)
    {
        string nodeTypeName = "AudioBoardEvent_" + gameEventType.ToString();
        object typeInstance = Activator.CreateInstance(Type.GetType(nodeTypeName));

        return typeInstance as AudioBoardEventBase;
    }

    public virtual void copyFrom(AudioBoardEventBase audioBoardEvent)
    {
        _guidString = audioBoardEvent._guidString;
    }

    public virtual void onEventRemovedFromEditor()
    {
        
    }
#endif
}

[Serializable]
public class AudioBoardEvent_Log : AudioBoardEventBase
{
    public override AudioBoardEventType getEventType() => AudioBoardEventType.Log;

    [SerializeField] public string _logText = "";

    public override void entry()
    {
        Debug.Log(_logText);
    }

    public override bool update()
    {
        return true;
    }

    public override void end()
    {
        
    }

#if UNITY_EDITOR

    public override void copyFrom(AudioBoardEventBase audioBoardEvent)
    {
        base.copyFrom(audioBoardEvent);

        AudioBoardEvent_Log baseEvent = (audioBoardEvent as AudioBoardEvent_Log);
        _logText = baseEvent._logText;
    }

#endif
}