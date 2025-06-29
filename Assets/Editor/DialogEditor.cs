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

public class DialogEditor : EditorWindow
{
    private DialogEditorGraphView _graphView;
    private Toolbar _toolbar;
    private DialogData _dialogData = null;

    [MenuItem("Editor/Dialog Editor")]
    public static void Open()
    {
        var window = GetWindow<DialogEditor>();
        window.titleContent = new GUIContent("Empty");
        window._dialogData = null;
    }

    public static void Open(DialogData dialogData)
    {
        var window = GetWindow<DialogEditor>();
        window.titleContent = new GUIContent(dialogData == null ? "Empty" : dialogData.name);
        window._dialogData = dialogData;
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

    private void loadGraph(DialogData dialogData)
    {
        if(dialogData == null)
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.ToLower();
            
            string filePath = EditorUtility.OpenFilePanel("Target DialogData",Application.dataPath,"asset");
            if(filePath == null || filePath == "")
                return;

            filePath = filePath.ToLower();
            filePath = "assets" + filePath.Remove(0, dataPath.Length);

            dialogData = AssetDatabase.LoadAssetAtPath(filePath, typeof(DialogData)) as DialogData;
        }

        _dialogData = dialogData;

        if(_graphView != null)
            _graphView.loadGraph(_dialogData);
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
        if(_dialogData != null && _graphView == null)
        {
            constructGraphView();
        }

    }

    void constructGraphView()
    {
        _graphView = new DialogEditorGraphView(_dialogData);

        _graphView.StretchToParentSize();
        rootVisualElement.Insert(0,_graphView);
    }
}

public class DialogEditorGraphView : GraphView
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

            int eventCount = (int)DialogEventType.Count;
            for(int index = 0; index < eventCount; ++index)
            {
                DialogEventType gameEventType = (DialogEventType)index;
                root.AddChild(new AdvancedDropdownItem(gameEventType.ToString()));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _onItemSelected.Invoke(item.name, _createPosition);
        }
    }

    private DialogData _dialogData;
    private readonly Vector2 kDefaultNodeSize = new Vector2(200f, 100f);

    public DialogEditorGraphView(DialogData dialogData)
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

        loadGraph(dialogData);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 mousePosition = evt.localMousePosition;

        evt.menu.AppendAction("Create Dialog Event Node", action => 
        {
            showDropdown(mousePosition);
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
        DialogEventType gameEventType;
        if(Enum.TryParse<DialogEventType>(selectedItem, out gameEventType) == false)
            return;

        GameEventNodeBase gameEventNode = createNode(gameEventType,position);
        gameEventNode.constructField();
        gameEventNode.constructPort();

        AddElement(gameEventNode);
    }

    public GameEventNodeBase createNode(DialogEventType gameEventType, Vector2 position)
    {
        string nodeTypeName = "GameEventNode_" + gameEventType.ToString();
        object typeInstance = Activator.CreateInstance(Type.GetType(nodeTypeName));

        GameEventNodeBase gameEventNode = typeInstance as GameEventNodeBase;
        Vector2 worldPosition = contentViewContainer.WorldToLocal(position);
        gameEventNode.SetPosition(new Rect(worldPosition, kDefaultNodeSize));

        return gameEventNode;
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
        // if(_dialogData == null)
        // {
        //     DebugUtil.assert(false, "저장할 대상이 없음");
        //     return;
        // }

        // _dialogData._nodeData.Clear();
        // _dialogData._linkData.Clear();

        // Dictionary<string, int> guidToGameEventIndex = new Dictionary<string, int>();

        // foreach (var node in nodes)
        // {
        //     if(node is GameEventEntryNode)
        //     {
        //         NodeData nodeData = new NodeData();
        //         nodeData._dataIndex = -1;
        //         nodeData._guid = "entry";
        //         nodeData._position = node.GetPosition().position;
        //         _dialogData._nodeData.Add(nodeData);
        //     }
        //     else if(node is GameEventNodeBase)
        //     {
        //         GameEventNodeBase gameEventNode = node as GameEventNodeBase;
        //         NodeData nodeData = new NodeData();
        //         nodeData._dataIndex = _dialogData._gameEventBase.Count;
        //         nodeData._guid = gameEventNode._guid;
        //         nodeData._position = gameEventNode.GetPosition().position;
        //         _dialogData._nodeData.Add(nodeData);

        //         DialogEventDataBase gameEvent = gameEventNode.exportGameEvent();
        //         _dialogData._gameEventBase.Add(gameEvent);

        //         guidToGameEventIndex.Add(nodeData._guid, _dialogData._gameEventBase.Count - 1);
        //     }
        // }

        // foreach (var edge in edges.ToList())
        // {
        //     if (edge.input.node is GameEventNodeBase && edge.output.node is GameEventEntryNode)
        //     {
        //         GameEventNodeBase inputNode = edge.input.node as GameEventNodeBase;
        //         GameEventEntryNode outputNode = edge.output.node as GameEventEntryNode;

        //         _dialogData._linkData.Add(new DialogData.GameEventLinkData
        //         {
        //             _outputGuid = "entry",
        //             _outputPort = edge.output.portName,
        //             _inputGuid = inputNode._guid,
        //             _inputPort = edge.input.portName
        //         });

        //         int entryIndex = guidToGameEventIndex[inputNode._guid];
        //         _dialogData._entryIndex = entryIndex;
        //     }
        //     else if (edge.input.node is GameEventNodeBase && edge.output.node is GameEventNodeBase)
        //     {
        //         GameEventNodeBase inputNode = edge.input.node as GameEventNodeBase;
        //         GameEventNodeBase outputNode = edge.output.node as GameEventNodeBase;

        //         _dialogData._linkData.Add(new DialogData.GameEventLinkData
        //         {
        //             _outputGuid = outputNode._guid,
        //             _outputPort = edge.output.portName,
        //             _inputGuid = inputNode._guid,
        //             _inputPort = edge.input.portName
        //         });

        //         int inputNodeIndex = guidToGameEventIndex[inputNode._guid];
        //         int outputNodeIndex = guidToGameEventIndex[outputNode._guid];

        //         DialogEventDataBase gameEventBase = _dialogData._gameEventBase[outputNodeIndex];
                
        //         if(gameEventBase._nextEventIndex == null)
        //             gameEventBase._nextEventIndex = new int[0];

        //         gameEventBase._nextEventIndex = gameEventBase._nextEventIndex.Append(inputNodeIndex).ToArray();
        //     }
        // }

        // EditorUtility.SetDirty(_dialogData);
        // AssetDatabase.SaveAssets();

        // Debug.Log($"{_dialogData.name} 저장 완료");
    }

    public void loadGraph(DialogData dialogData)
    {
        // _dialogData = dialogData;

        // if(_dialogData == null)
        //     return;

        // DeleteElements(graphElements.ToList());
        // Dictionary<string, Node> guidToNode = new Dictionary<string, Node>();

        // bool hasEntryNode = false;

        // foreach (var nodeData in dialogData._nodeData)
        // {
        //     Node node = null;
        //     if(nodeData._dataIndex == -1 && nodeData._guid == "entry")
        //     {
        //         GameEventEntryNode entryNode = new GameEventEntryNode();
        //         entryNode.SetPosition(new Rect(nodeData._position, kDefaultNodeSize));

        //         hasEntryNode = true;

        //         node = entryNode;
        //     }
        //     else
        //     {
        //         DialogEventDataBase targetEvent = dialogData._gameEventBase[nodeData._dataIndex];

        //         GameEventNodeBase gameEventNode = createNode(targetEvent.getEventType(), nodeData._position);
        //         gameEventNode._guid = nodeData._guid;
        //         gameEventNode.importGameEvent(targetEvent);

        //         gameEventNode.constructField();
        //         gameEventNode.constructPort();

        //         node = gameEventNode;
        //     }

        //     AddElement(node);
        //     guidToNode[nodeData._guid] = node;
        // }

        // foreach (var link in dialogData._linkData)
        // {
        //     Node outputNode = guidToNode[link._outputGuid];
        //     Node inputNode = guidToNode[link._inputGuid];

        //     Port outputPort = outputNode.outputContainer.Children()
        //         .OfType<Port>()
        //         .FirstOrDefault(p => p.portName == link._outputPort);

        //     Port inputPort = inputNode.inputContainer.Children()
        //         .OfType<Port>()
        //         .FirstOrDefault(p => p.portName == link._inputPort);

        //     if (outputPort != null && inputPort != null)
        //     {
        //         Edge edge = outputPort.ConnectTo(inputPort);
        //         AddElement(edge);
        //     }
        // }

        // if(hasEntryNode == false)
        // {
        //     GameEventEntryNode entryNode = new GameEventEntryNode();
        //     entryNode.SetPosition(new Rect(Vector2.zero, kDefaultNodeSize));

        //     AddElement(entryNode);
        // }

    }
}

public class GameEventEntryNode : Node
{
    public GameEventEntryNode()
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

public abstract class GameEventNodeBase : Node
{
    public abstract DialogEventType getEventType();

    public DialogEventDataBase _dialogEvent;
    public string _guid;

    public GameEventNodeBase()
    {
        _guid = System.Guid.NewGuid().ToString();
        _dialogEvent = DialogData.createDialogEvent(getEventType());
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
}

public class GameEventNode_Dialog : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.Dialog;

    private VisualElement dialogueListContainer;


    public override void constructField()
    {
        style.width = 250;

        dialogueListContainer = new VisualElement();
        dialogueListContainer.style.flexDirection = FlexDirection.Column;
        dialogueListContainer.style.paddingBottom = 4;

        mainContainer.Add(dialogueListContainer);
    }

}