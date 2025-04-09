using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.Rendering;


[CustomEditor(typeof(AudioBoardEventSet))]
class AudioBoardEventSetEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
        serializedObject.Update();
	    Undo.RecordObject(target, $"Change AudioBoardSet Properties");

        if(GUILayout.Button("Open Editor"))
            AudioBoardEventGraphEditor.Open(target as AudioBoardEventSet);

    	serializedObject.ApplyModifiedProperties();
		if (GUI.changed) 
            EditorUtility.SetDirty(target);
    }
}

public class AudioBoardEventGraphEditor : EditorWindow
{
    private AudioBoardGraphView _graphView;
    private Toolbar _toolbar;
    private AudioBoardEventSet _audioBoardEventSet = null;


    [MenuItem("Editor/Audio Board")]
    public static void Open()
    {
        var window = GetWindow<AudioBoardEventGraphEditor>();
        window.titleContent = new GUIContent("Audio Board");
        window._audioBoardEventSet = null;
    }

    public static void Open(AudioBoardEventSet audioBoardEventSet)
    {
        var window = GetWindow<AudioBoardEventGraphEditor>();
        window.titleContent = new GUIContent(audioBoardEventSet == null ? "Empty" : audioBoardEventSet.name);
        window._audioBoardEventSet = audioBoardEventSet;
    }

    private void OnEnable()
    {
        _toolbar = new Toolbar();

        var saveButton = new ToolbarButton(() => saveGraph()) { text = "Save" };
        var loadButton = new ToolbarButton(() => loadGraph(null)) { text = "Load" };

        _toolbar.Add(saveButton);
        _toolbar.Add(loadButton);

        rootVisualElement.Insert(0,_toolbar);
    }

    private void saveGraph()
    {
        _graphView?.saveGraph();
    }

    private void loadGraph(AudioBoardEventSet audioBoardEventSet)
    {
        if(audioBoardEventSet == null)
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.ToLower();
            
            string filePath = EditorUtility.OpenFilePanel("Target AudioBoardEventSet",Application.dataPath,"asset");
            if(filePath == null || filePath == "")
                return;

            filePath = filePath.ToLower();
            filePath = "assets" + filePath.Remove(0, dataPath.Length);

            audioBoardEventSet = AssetDatabase.LoadAssetAtPath(filePath, typeof(AudioBoardEventSet)) as AudioBoardEventSet;
        }

        _audioBoardEventSet = audioBoardEventSet;

        if(_graphView != null)
            _graphView.loadGraph(_audioBoardEventSet);
    }

    private void OnDisable()
    {
        if(_toolbar != null)
            rootVisualElement.Remove(_toolbar);
        if(_graphView != null)
            rootVisualElement.Remove(_graphView);
    }

    private void OnGUI()
    {
        if(_audioBoardEventSet != null && _graphView == null)
        {
            constructGraphView();
        }

    }

    void constructGraphView()
    {
        _graphView = new AudioBoardGraphView(_audioBoardEventSet);

        _graphView.StretchToParentSize();
        rootVisualElement.Insert(0,_graphView);
    }
}

public class AudioBoardGraphView : GraphView
{
    class AddEventListDropdown : AdvancedDropdown
    {
        public delegate void OnItemSelected(string name, Vector2 position);
        public OnItemSelected _onItemSelected = new OnItemSelected((x, y)=>{});
        public AddEventListDropdown(AdvancedDropdownState state, Vector2 createPosition) : base(state)
        {
            _createPosition = createPosition;
        }

        private Vector2 _createPosition;

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Event List");

            int eventCount = (int)AudioBoardEventType.Count;
            for(int index = 0; index < eventCount; ++index)
            {
                AudioBoardEventType audioBoardEventType = (AudioBoardEventType)index;
                root.AddChild(new AdvancedDropdownItem(audioBoardEventType.ToString()));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _onItemSelected.Invoke(item.name, _createPosition);
        }
    }

    private AudioBoardEventSet _audioBoardEventSet;

    private List<AudioBoardEventBase> _removedAudioEventList = new List<AudioBoardEventBase>();
    private readonly Vector2 kDefaultNodeSize = new Vector2(200f, 100f);

    public AudioBoardGraphView(AudioBoardEventSet audioBoardEventSet)
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.RegisterCallback<KeyDownEvent>(evt =>
        {
            if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.S)
            {
                saveGraph();
                evt.StopImmediatePropagation();
            }
        });

        var grid = new GridBackground();
        grid.StretchToParentSize();
        Insert(0, grid);
        
        styleSheets.Add(EditorGUIUtility.Load("GraphViewStyle.uss") as StyleSheet);

        loadGraph(audioBoardEventSet);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 mousePosition = evt.localMousePosition;

        evt.menu.AppendAction("Create Audio Event Node", action => 
        {
            showDropdown(mousePosition);
        });

        evt.menu.AppendAction("Create Sticky Note", action => 
        {
            StickyNote stickyNoteNode = new StickyNote();
            stickyNoteNode.title = "New Note";
            stickyNoteNode.theme = StickyNoteTheme.Classic;
            stickyNoteNode.fontSize = StickyNoteFontSize.Small;

            Vector2 worldPosition = contentViewContainer.WorldToLocal(mousePosition);
            stickyNoteNode.SetPosition(new Rect(worldPosition, new Vector2(250f, 100f)));

            AddElement(stickyNoteNode);
        });
    }

    private void showDropdown(Vector2 position)
    {
        AddEventListDropdown dropdown = new AddEventListDropdown(new AdvancedDropdownState(), position);
        dropdown._onItemSelected += onEventDropdownSelected;
        dropdown.Show(new Rect(position.x, position.y, 200f, 0));
    }

    private void onEventDropdownSelected(string selectedItem, Vector2 position)
    {
        AudioBoardEventType audioBoardEventType;
        if(Enum.TryParse<AudioBoardEventType>(selectedItem, out audioBoardEventType) == false)
            return;

        AudioBoardEventNodeBase audioBoardEventNode = createNode(audioBoardEventType,position);
        audioBoardEventNode.constructField();
        audioBoardEventNode.constructPort();

        AddElement(audioBoardEventNode);
    }

    public AudioBoardEventNodeBase createNode(AudioBoardEventType audioBoardEventType, Vector2 position)
    {
        string nodeTypeName = "AudioBoardEventNode_" + audioBoardEventType.ToString();
        object typeInstance = Activator.CreateInstance(Type.GetType(nodeTypeName));

        AudioBoardEventNodeBase audioBoardEventNode = typeInstance as AudioBoardEventNodeBase;
        Vector2 worldPosition = contentViewContainer.WorldToLocal(position);
        audioBoardEventNode.SetPosition(new Rect(worldPosition, kDefaultNodeSize));

        return audioBoardEventNode;
    }

    public override EventPropagation DeleteSelection()
    {
        foreach (var element in selection)
        {
            if (element is AudioBoardEventNodeBase)
            {
                AudioBoardEventNodeBase audioBoardEventNode = (element as AudioBoardEventNodeBase);
                _removedAudioEventList.Add(audioBoardEventNode._audioBoardEvent);
                
                //audioBoardEventNode.onRemoveNode();
            }
        }

        return base.DeleteSelection();
    }
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            if (startPort != port &&
                startPort.direction != port.direction &&
                port.node != startPort.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    public void saveGraph()
    {
        if(_audioBoardEventSet == null)
        {
            DebugUtil.assert(false, "저장할 대상이 없음");
            return;
        }

        foreach(var item in _removedAudioEventList)
        {
            item?.onEventRemovedFromEditor();
        }
        _removedAudioEventList.Clear();

        _audioBoardEventSet._audioBoardEventBase.Clear();
        _audioBoardEventSet._nodeData.Clear();
        _audioBoardEventSet._noteData.Clear();
        _audioBoardEventSet._linkData.Clear();

        Dictionary<string, int> guidToAudioBoardEventIndex = new Dictionary<string, int>();

        foreach (var node in nodes)
        {
            if(node is AudioBoardEventEntryNode)
            {
                AudioBoardEventSet.NodeData nodeData = new AudioBoardEventSet.NodeData();
                nodeData._dataIndex = -1;
                nodeData._guid = "entry";
                nodeData._position = node.GetPosition().position;
                _audioBoardEventSet._nodeData.Add(nodeData);
            }
            else if(node is AudioBoardEventNodeBase)
            {
                AudioBoardEventNodeBase audioBoardEventNode = node as AudioBoardEventNodeBase;
                AudioBoardEventSet.NodeData nodeData = new AudioBoardEventSet.NodeData();
                nodeData._dataIndex = _audioBoardEventSet._audioBoardEventBase.Count;
                nodeData._guid = audioBoardEventNode._guid;
                nodeData._position = audioBoardEventNode.GetPosition().position;
                _audioBoardEventSet._nodeData.Add(nodeData);

                AudioBoardEventBase audioBoardEvent = audioBoardEventNode.exportGameEvent();
                _audioBoardEventSet._audioBoardEventBase.Add(audioBoardEvent);

                guidToAudioBoardEventIndex.Add(nodeData._guid, _audioBoardEventSet._audioBoardEventBase.Count - 1);
            }
        }

        foreach (var note in graphElements.OfType<StickyNote>())
        {
            _audioBoardEventSet._noteData.Add(new AudioBoardEventSet.NoteData
            {
                _title = note.title,
                _text = note.contents,
                _theme = note.theme,
                _fontSize = note.fontSize,
                _position = note.GetPosition()
            });
        }

        foreach (var edge in edges.ToList())
        {
            if (edge.input.node is AudioBoardEventNodeBase && edge.output.node is AudioBoardEventEntryNode)
            {
                AudioBoardEventNodeBase inputNode = edge.input.node as AudioBoardEventNodeBase;
                AudioBoardEventEntryNode outputNode = edge.output.node as AudioBoardEventEntryNode;

                _audioBoardEventSet._linkData.Add(new AudioBoardEventSet.LinkData
                {
                    _outputGuid = "entry",
                    _outputPort = edge.output.portName,
                    _inputGuid = inputNode._guid,
                    _inputPort = edge.input.portName
                });

                int entryIndex = guidToAudioBoardEventIndex[inputNode._guid];
                _audioBoardEventSet._entryIndex = entryIndex;
            }
            else if (edge.input.node is AudioBoardEventNodeBase && edge.output.node is AudioBoardEventNodeBase)
            {
                AudioBoardEventNodeBase inputNode = edge.input.node as AudioBoardEventNodeBase;
                AudioBoardEventNodeBase outputNode = edge.output.node as AudioBoardEventNodeBase;

                _audioBoardEventSet._linkData.Add(new AudioBoardEventSet.LinkData
                {
                    _outputGuid = outputNode._guid,
                    _outputPort = edge.output.portName,
                    _inputGuid = inputNode._guid,
                    _inputPort = edge.input.portName
                });

                int inputNodeIndex = guidToAudioBoardEventIndex[inputNode._guid];
                int outputNodeIndex = guidToAudioBoardEventIndex[outputNode._guid];

                AudioBoardEventBase audioBoardEventBase = _audioBoardEventSet._audioBoardEventBase[outputNodeIndex];
                
                if(audioBoardEventBase._nextEventIndex == null)
                    audioBoardEventBase._nextEventIndex = new int[0];

                audioBoardEventBase._nextEventIndex = audioBoardEventBase._nextEventIndex.Append(inputNodeIndex).ToArray();
            }
        }

        EditorUtility.SetDirty(_audioBoardEventSet);
        AssetDatabase.SaveAssets();

        Debug.Log($"{_audioBoardEventSet.name} 저장 완료");
    }

    public void loadGraph(AudioBoardEventSet audioBoardEventSet)
    {
        _audioBoardEventSet = audioBoardEventSet;

        if(_audioBoardEventSet == null)
            return;

        DeleteElements(graphElements.ToList());
        Dictionary<string, Node> guidToNode = new Dictionary<string, Node>();

        bool hasEntryNode = false;

        foreach (var nodeData in audioBoardEventSet._nodeData)
        {
            Node node = null;
            if(nodeData._dataIndex == -1 && nodeData._guid == "entry")
            {
                AudioBoardEventEntryNode entryNode = new AudioBoardEventEntryNode();
                entryNode.SetPosition(new Rect(nodeData._position, kDefaultNodeSize));

                hasEntryNode = true;

                node = entryNode;
            }
            else
            {
                AudioBoardEventBase targetEvent = audioBoardEventSet._audioBoardEventBase[nodeData._dataIndex];

                AudioBoardEventNodeBase audioBoardEventNode = createNode(targetEvent.getEventType(), nodeData._position);
                audioBoardEventNode._guid = nodeData._guid;
                audioBoardEventNode.importGameEvent(targetEvent);

                audioBoardEventNode.constructField();
                audioBoardEventNode.constructPort();

                node = audioBoardEventNode;
            }

            AddElement(node);
            guidToNode[nodeData._guid] = node;
        }

        foreach (var noteData in audioBoardEventSet._noteData)
        {
            StickyNote stickyNote = new StickyNote();
            stickyNote.SetPosition(noteData._position);
            stickyNote.title = noteData._title;
            stickyNote.contents = noteData._text;
            stickyNote.theme = noteData._theme;
            stickyNote.fontSize = noteData._fontSize;

            AddElement(stickyNote);
        }

        foreach (var link in audioBoardEventSet._linkData)
        {
            Node outputNode = guidToNode[link._outputGuid];
            Node inputNode = guidToNode[link._inputGuid];

            Port outputPort = outputNode.outputContainer.Children()
                .OfType<Port>()
                .FirstOrDefault(p => p.portName == link._outputPort);

            Port inputPort = inputNode.inputContainer.Children()
                .OfType<Port>()
                .FirstOrDefault(p => p.portName == link._inputPort);

            if (outputPort != null && inputPort != null)
            {
                Edge edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }

        if(hasEntryNode == false)
        {
            AudioBoardEventEntryNode entryNode = new AudioBoardEventEntryNode();
            entryNode.SetPosition(new Rect(Vector2.zero, kDefaultNodeSize));

            AddElement(entryNode);
        }

    }
}

public class AudioBoardEventEntryNode : Node
{
    public AudioBoardEventEntryNode()
    {
        title = "Entry";
        mainContainer.style.backgroundColor = new StyleColor(Color.green);

        var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
        output.portName = "Next";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }
}

public abstract class AudioBoardEventNodeBase : Node
{
    public abstract AudioBoardEventType getEventType();

    public AudioBoardEventBase _audioBoardEvent;
    public string _guid;

    public AudioBoardEventNodeBase()
    {
        _guid = System.Guid.NewGuid().ToString();
        _audioBoardEvent = AudioBoardEventBase.createAudioBoardEvent(getEventType());
        title = getEventType().ToString();
        mainContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
    }

    public virtual void constructField()
    {

    }

    public virtual void constructPort()
    {
        var input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, null);
        input.portName = "Prev";
        inputContainer.Add(input);

        var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
        output.portName = "Next";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }

    // public virtual void onRemoveNode()
    // {

    // }

    protected AudioBoardEventBase createAudioEventFromNodeData()
    {
        AudioBoardEventType audioBoardEventType = _audioBoardEvent.getEventType();
        AudioBoardEventBase audioBoardEvent = AudioBoardEventBase.createAudioBoardEvent(audioBoardEventType);
        
        return audioBoardEvent;
    }

    public virtual void importGameEvent(AudioBoardEventBase audioBoardEvent)
    {
        _audioBoardEvent.copyFrom(audioBoardEvent);
    }

    public virtual AudioBoardEventBase exportGameEvent()
    {
        AudioBoardEventBase audioBoardEvent = createAudioEventFromNodeData();
        audioBoardEvent.copyFrom(_audioBoardEvent);

        return audioBoardEvent;
    }
}

public class AudioBoardEventNode_Log : AudioBoardEventNodeBase
{
    public override AudioBoardEventType getEventType() => AudioBoardEventType.Log;

    public override void constructField()
    {
        base.constructField();

        AudioBoardEvent_Log audioBoardEvent = _audioBoardEvent as AudioBoardEvent_Log;

        style.width = 150;

        TextField textField = new TextField{value = audioBoardEvent._logText, multiline = true };
        textField.style.flexGrow = 1;
        textField.style.whiteSpace = WhiteSpace.Normal;
        textField.RegisterValueChangedCallback(evt => audioBoardEvent._logText = evt.newValue);

        mainContainer.Add(textField);
    }

}