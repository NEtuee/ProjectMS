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
    
    // 검색 기능 관련 필드들
    private TextField _searchField;
    private Label _searchResultLabel;
    private Button _prevButton;
    private Button _nextButton;
    private List<GameEventNode_Dialog> _searchResults = new List<GameEventNode_Dialog>();
    private int _currentSearchIndex = -1;
    private string _lastSearchText = "";

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

        // 검색 UI 구성
        CreateSearchUI();

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
        if(_dialogData == null)
        {
            Debug.LogError("저장할 대상이 없음");
            return;
        }

        // 기존 데이터 초기화
        _dialogData.clearAll();

        // GUID to Entry/Event 매핑
        Dictionary<string, GameEventNode_Entry> guidToEntryNode = new Dictionary<string, GameEventNode_Entry>();
        Dictionary<string, GameEventNodeBase> guidToEventNode = new Dictionary<string, GameEventNodeBase>();

        // 노드 데이터 수집
        foreach (var node in nodes)
        {
            if(node is GameEventNode_Entry entryNode)
            {
                // Entry 노드 저장
                NodeData nodeData = new NodeData();
                nodeData._guid = entryNode._guid;
                nodeData._position = entryNode.GetPosition().position;
                _dialogData._nodeData.Add(nodeData);

                guidToEntryNode.Add(entryNode._guid, entryNode);
            }
            else if(node is GameEventNodeBase gameEventNode)
            {
                // 일반 이벤트 노드 저장
                NodeData nodeData = new NodeData();
                nodeData._guid = gameEventNode._guid;
                nodeData._position = gameEventNode.GetPosition().position;
                _dialogData._nodeData.Add(nodeData);

                guidToEventNode.Add(gameEventNode._guid, gameEventNode);
            }
        }

        // Entry별로 이벤트 리스트 구성
        foreach (var entryPair in guidToEntryNode)
        {
            var entryNode = entryPair.Value;
            DialogEventEntryData entryData = entryNode.exportEntryData();
            
            // Entry에서 시작하는 이벤트 체인 구축
            BuildEventChain(entryNode, entryData, guidToEventNode);
            
            _dialogData._dialogEventEntryList.Add(entryData);
        }

        // 링크 데이터 저장
        foreach (var edge in edges.ToList())
        {
            LinkData linkData = new LinkData();
            
            if (edge.output.node is GameEventNodeBase outputNode)
                linkData._outputGuid = outputNode._guid;
            else if (edge.output.node is GameEventNode_Entry outputEntryNode)
                linkData._outputGuid = outputEntryNode._guid;
                
            linkData._outputPort = edge.output.portName;
            
            if (edge.input.node is GameEventNodeBase inputNode)
                linkData._inputGuid = inputNode._guid;
            else if (edge.input.node is GameEventNode_Entry inputEntryNode)
                linkData._inputGuid = inputEntryNode._guid;
                
            linkData._inputPort = edge.input.portName;
            
            _dialogData._linkData.Add(linkData);
        }

        EditorUtility.SetDirty(_dialogData);
        AssetDatabase.SaveAssets();
        Debug.Log($"{_dialogData.name} 저장 완료");
    }

    private void BuildEventChain(GameEventNode_Entry entryNode, DialogEventEntryData entryData, Dictionary<string, GameEventNodeBase> guidToEventNode)
    {
        // Entry에서 연결된 첫 번째 이벤트 찾기
        var firstEventNode = FindConnectedEventNode(entryNode);
        if (firstEventNode != null)
        {
            entryData._dialogEventList.Clear();
            BuildEventList(firstEventNode, entryData, guidToEventNode, new HashSet<string>());
        }
    }

    private GameEventNodeBase FindConnectedEventNode(Node fromNode)
    {
        foreach (var edge in edges)
        {
            if (edge.output.node == fromNode && edge.input.node is GameEventNodeBase eventNode)
            {
                return eventNode;
            }
        }
        return null;
    }

    private void BuildEventList(GameEventNodeBase currentNode, DialogEventEntryData entryData, Dictionary<string, GameEventNodeBase> guidToEventNode, HashSet<string> visited)
    {
        if (visited.Contains(currentNode._guid))
            return; // 순환 참조 방지
            
        visited.Add(currentNode._guid);

        // 현재 노드의 이벤트 데이터 추가
        var eventData = currentNode.exportGameEvent();
        if (eventData != null)
        {
            int currentIndex = entryData._dialogEventList.Count;
            entryData._dialogEventList.Add(eventData);

            // 다음 노드 찾기
            var nextNode = FindConnectedEventNode(currentNode);
            if (nextNode != null)
            {
                eventData._nextIndex = currentIndex + 1;
                BuildEventList(nextNode, entryData, guidToEventNode, visited);
            }
            else
            {
                eventData._nextIndex = -1; // 마지막 노드
            }
        }
    }

    public void loadGraph(DialogData dialogData)
    {
        _dialogData = dialogData;

        if(_dialogData == null)
            return;

        // 기존 요소들 제거
        DeleteElements(graphElements.ToList());
        Dictionary<string, Node> guidToNode = new Dictionary<string, Node>();

        // Entry 노드들 생성
        foreach (var entryData in _dialogData._dialogEventEntryList)
        {
            GameEventNode_Entry entryNode = new GameEventNode_Entry();
            entryNode.importEntryData(entryData);
            entryNode.constructField();
            entryNode.constructPort();

            // Entry 노드 위치 찾기 (NodeData에서)
            var nodeData = _dialogData._nodeData.Find(n => n._guid == entryData._editorGuidString);
            if (nodeData != null)
            {
                entryNode.SetPosition(new Rect(nodeData._position, kDefaultNodeSize));
                entryNode._guid = nodeData._guid;
            }
            else
            {
                DebugUtil.assert(false, "Node Data not found for entry: " + entryData._entryKey);
            }

            AddElement(entryNode);
            guidToNode[entryNode._guid] = entryNode;

            // Entry에 속한 이벤트 노드들 생성
            for (int i = 0; i < entryData._dialogEventList.Count; i++)
            {
                var eventData = entryData._dialogEventList[i];
                var eventNode = createNode(eventData.getDialogEventType(), Vector2.zero);
                eventNode.importGameEvent(eventData);
                eventNode.constructField();
                eventNode.constructPort();

                // 이벤트 노드 위치 찾기
                var eventNodeData = _dialogData._nodeData.Find(n => n._guid == eventData._editorGuidString);
                if (eventNodeData != null)
                {
                    eventNode.SetPosition(new Rect(eventNodeData._position, kDefaultNodeSize));
                    eventNode._guid = eventNodeData._guid;
                }
                else
                {
                    DebugUtil.assert(false, "Node Data not found for event: " + eventData._editorGuidString);
                }

                AddElement(eventNode);
                guidToNode[eventNode._guid] = eventNode;
            }
        }

        // 링크 복원
        foreach (var link in _dialogData._linkData)
        {
            if (guidToNode.ContainsKey(link._outputGuid) && guidToNode.ContainsKey(link._inputGuid))
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
        }

    }

    private void CreateSearchUI()
    {
        // 검색 컨테이너 생성
        var searchContainer = new VisualElement();
        searchContainer.style.position = Position.Absolute;
        searchContainer.style.top = 30; // 상단바 아래로 이동
        searchContainer.style.left = 10;
        searchContainer.style.flexDirection = FlexDirection.Row;
        searchContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.9f));
        searchContainer.style.paddingTop = 5;
        searchContainer.style.paddingBottom = 5;
        searchContainer.style.paddingLeft = 5;
        searchContainer.style.paddingRight = 5;
        searchContainer.style.borderTopLeftRadius = 5;
        searchContainer.style.borderTopRightRadius = 5;
        searchContainer.style.borderBottomLeftRadius = 5;
        searchContainer.style.borderBottomRightRadius = 5;

        // 검색 텍스트 필드
        _searchField = new TextField();
        _searchField.style.width = 200;
        _searchField.style.marginRight = 5;
        
        // 텍스트 편집 완료 시에만 검색 실행
        _searchField.RegisterCallback<FocusOutEvent>(evt => OnSearchTextCompleted(true));
        _searchField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnSearchTextCompleted();
                evt.StopPropagation();
            }
        });
        
        searchContainer.Add(_searchField);

        // 검색 결과 라벨
        _searchResultLabel = new Label("0/0");
        _searchResultLabel.style.color = Color.white;
        _searchResultLabel.style.marginRight = 5;
        _searchResultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _searchResultLabel.style.minWidth = 40;
        searchContainer.Add(_searchResultLabel);

        // 이전 버튼
        _prevButton = new Button(OnPrevButtonClicked);
        _prevButton.text = "◀";
        _prevButton.style.width = 30;
        _prevButton.style.marginRight = 2;
        _prevButton.SetEnabled(false);
        searchContainer.Add(_prevButton);

        // 다음 버튼
        _nextButton = new Button(OnNextButtonClicked);
        _nextButton.text = "▶";
        _nextButton.style.width = 30;
        _nextButton.SetEnabled(false);
        searchContainer.Add(_nextButton);

        Add(searchContainer);
    }

    private void OnSearchTextChanged(ChangeEvent<string> evt)
    {
        SearchDialogNodes(evt.newValue);
    }

    private void OnSearchTextCompleted(bool isFocusOut = false)
    {
        string currentSearchText = _searchField.value;
        
        // 검색어가 이전과 같고 검색 결과가 있으면 다음 결과로 이동
        if(isFocusOut == false)
        {
            if (!string.IsNullOrEmpty(currentSearchText) && 
                currentSearchText == _lastSearchText && 
                _searchResults.Count > 0)
            {
                OnNextButtonClicked();
            }
            else
            {
                // 새로운 검색 수행
                SearchDialogNodes(currentSearchText);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(currentSearchText) && currentSearchText != _lastSearchText )
            {
                SearchDialogNodes(currentSearchText);
            }
        }
    }

    private void SearchDialogNodes(string searchText)
    {
        _searchResults.Clear();
        _currentSearchIndex = -1;
        _lastSearchText = searchText; // 검색어 기록

        if (string.IsNullOrEmpty(searchText))
        {
            UpdateSearchUI();
            return;
        }

        // 모든 Dialog 노드에서 검색
        foreach (var node in nodes)
        {
            if (node is GameEventNode_Dialog dialogNode)
            {
                if (dialogNode._dialogText.ToLower().Contains(searchText.ToLower()))
                {
                    _searchResults.Add(dialogNode);
                }
            }
        }

        if (_searchResults.Count > 0)
        {
            _currentSearchIndex = 0;
            FocusSearchResult();
        }

        UpdateSearchUI();
    }

    private void OnPrevButtonClicked()
    {
        if (_searchResults.Count == 0) return;
        
        _currentSearchIndex--;
        if (_currentSearchIndex < 0)
            _currentSearchIndex = _searchResults.Count - 1;
        
        FocusSearchResult();
        UpdateSearchUI();
    }

    private void OnNextButtonClicked()
    {
        if (_searchResults.Count == 0) return;
        
        _currentSearchIndex++;
        if (_currentSearchIndex >= _searchResults.Count)
            _currentSearchIndex = 0;
        
        FocusSearchResult();
        UpdateSearchUI();
    }

    private void FocusSearchResult()
    {
        if (_currentSearchIndex < 0 || _currentSearchIndex >= _searchResults.Count)
            return;

        var targetNode = _searchResults[_currentSearchIndex];
        
        // 선택 초기화
        ClearSelection();
        
        // 해당 노드 선택
        AddToSelection(targetNode);
        
        // 노드 위치로 뷰 이동
        var nodePosition = targetNode.GetPosition().center;
        var currentTransform = viewTransform;
        currentTransform.position = -nodePosition * currentTransform.scale + contentRect.size * 0.5f;
        UpdateViewTransform(currentTransform.position, currentTransform.scale);
    }

    private void UpdateSearchUI()
    {
        if (_searchResults.Count == 0)
        {
            _searchResultLabel.text = "0/0";
            _prevButton.SetEnabled(false);
            _nextButton.SetEnabled(false);
        }
        else
        {
            _searchResultLabel.text = $"{_currentSearchIndex + 1}/{_searchResults.Count}";
            _prevButton.SetEnabled(true);
            _nextButton.SetEnabled(true);
        }
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

        if(getEventType() != DialogEventType.Entry)
        {
            _dialogEvent = DialogData.createDialogEvent(getEventType());
            _dialogEvent._editorGuidString = _guid;
            _dialogEvent._nextIndex = -1;
        }

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

    public virtual DialogEventDataBase exportGameEvent()
    {
        return _dialogEvent;
    }

    public virtual void importGameEvent(DialogEventDataBase eventData)
    {
        _dialogEvent = eventData;
    }
}

public class GameEventNode_Dialog : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.Dialog;

    private VisualElement dialogueListContainer;
    private TextField characterNameField;
    private TextField dialogTextField;
    private FloatField wordPerSecField;

    public string _dialogText = "Enter dialog text here...";

    public override void constructField()
    {
        style.width = 300;

        dialogueListContainer = new VisualElement();
        dialogueListContainer.style.flexDirection = FlexDirection.Column;
        dialogueListContainer.style.paddingBottom = 4;

        // 캐릭터 이름 필드
        characterNameField = new TextField("Character Name");
        if (_dialogEvent is DialogEventData_Dialog dialogData)
        {
            characterNameField.value = dialogData._displayCharacterKey ?? "";
            characterNameField.RegisterValueChangedCallback(evt =>
            {
                dialogData._displayCharacterKey = evt.newValue;
            });
        }
        characterNameField.style.marginBottom = 4;
        dialogueListContainer.Add(characterNameField);

        // 텍스트 출력 속도 필드
        wordPerSecField = new FloatField("Words Per Second");
        if (_dialogEvent is DialogEventData_Dialog dialogData2)
        {
            wordPerSecField.value = dialogData2._wordPerSec;
            wordPerSecField.RegisterValueChangedCallback(evt =>
            {
                dialogData2._wordPerSec = evt.newValue;
            });
        }
        wordPerSecField.style.marginBottom = 4;
        dialogueListContainer.Add(wordPerSecField);

        // 대화 텍스트 필드 (여러 줄)
        dialogTextField = new TextField("Dialog Text");
        dialogTextField.multiline = true;
        dialogTextField.style.flexGrow = 1;
        dialogTextField.style.whiteSpace = WhiteSpace.Normal;
        dialogTextField.value = "Enter dialog text here...";
        dialogTextField.RegisterValueChangedCallback(evt =>
        {
            _dialogText = evt.newValue;
        });
        dialogTextField.style.marginBottom = 4;
        dialogueListContainer.Add(dialogTextField);

        mainContainer.Add(dialogueListContainer);
    }

    public override DialogEventDataBase exportGameEvent()
    {
        if (_dialogEvent is DialogEventData_Dialog dialogData)
        {
            // UI에서 데이터 업데이트
            dialogData._displayCharacterKey = characterNameField?.value ?? "";
            dialogData._wordPerSec = wordPerSecField?.value ?? 12f;
        }
        return _dialogEvent;
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        if (eventData is DialogEventData_Dialog dialogData)
        {
            _dialogEvent = dialogData;
            _dialogText = ""; // 텍스트는 별도 관리이므로 초기화
        }
    }

}

public class GameEventNode_Entry : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.Entry;
    public DialogEventEntryData _entryData;

    public GameEventNode_Entry()
    {
        title = "Entry";
        mainContainer.style.backgroundColor = new StyleColor(Color.green);

        _entryData = new DialogEventEntryData();
        _entryData._entryKey = "default";
        _entryData._editorGuidString = _guid;
    }

    public override void constructPort()
    {
        var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
        output.portName = "Next";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override void constructField()
    {
        style.width = 250;

        var entryKeyField = new TextField("Entry Key");
        entryKeyField.value = _entryData._entryKey;
        entryKeyField.RegisterValueChangedCallback(evt =>
        {
            _entryData._entryKey = evt.newValue;
            title = _entryData._entryKey;
        });
        entryKeyField.style.backgroundColor = new StyleColor(Color.gray);
        mainContainer.Add(entryKeyField);
    }

    public override DialogEventDataBase exportGameEvent()
    {
        return null; // Entry 노드는 이벤트 데이터를 갖지 않음
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        // Entry 노드는 이벤트 데이터를 갖지 않음
    }

    public DialogEventEntryData exportEntryData()
    {
        return _entryData;
    }

    public void importEntryData(DialogEventEntryData entryData)
    {
        _entryData = entryData;
        title = _entryData._entryKey;
    }
}