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
        var previewToggleButton = new ToolbarToggle() { text = "Preview" };
        previewToggleButton.RegisterValueChangedCallback(evt => 
        {
            _graphView?.TogglePreviewPanel(evt.newValue);
        });

        _toolbar.Add(saveButton);
        _toolbar.Add(loadButton);
        _toolbar.Add(previewToggleButton);

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

    public void TogglePreview(bool isVisible)
    {
        _graphView?.TogglePreviewPanel(isVisible);
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

    // 프리뷰 패널 관련 필드들
    private VisualElement _previewPanel;
    private DialogPreviewController _previewController;
    private bool _isPreviewVisible = false;

    public DialogEditorGraphView(DialogData dialogData)
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // 포커스 가능하도록 설정 (키보드 이벤트를 받기 위해)
        focusable = true;

        // 마우스 클릭 시 포커스 설정
        this.RegisterCallback<MouseDownEvent>(evt =>
        {
            Focus();
        });

        this.RegisterCallback<KeyDownEvent>(evt =>
        {
            if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.S)
            {
                saveGraph();
                evt.StopImmediatePropagation();
            }
            else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.F)
            {
                FocusSearchField();
                evt.StopImmediatePropagation();
            }
            // 키보드 십자키로 그래프 뷰 이동
            else if (evt.keyCode == KeyCode.LeftArrow)
            {
                MoveView(Vector3.right * 50f);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.RightArrow)
            {
                MoveView(Vector3.left * 50f);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.UpArrow)
            {
                MoveView(Vector3.up * 50f);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.DownArrow)
            {
                MoveView(Vector3.down * 50f);
                evt.StopImmediatePropagation();
            }
            // Shift + 십자키로 빠른 이동
            else if (evt.shiftKey && evt.keyCode == KeyCode.LeftArrow)
            {
                MoveView(Vector3.right * 200f);
                evt.StopImmediatePropagation();
            }
            else if (evt.shiftKey && evt.keyCode == KeyCode.RightArrow)
            {
                MoveView(Vector3.left * 200f);
                evt.StopImmediatePropagation();
            }
            else if (evt.shiftKey && evt.keyCode == KeyCode.UpArrow)
            {
                MoveView(Vector3.up * 200f);
                evt.StopImmediatePropagation();
            }
            else if (evt.shiftKey && evt.keyCode == KeyCode.DownArrow)
            {
                MoveView(Vector3.down * 200f);
                evt.StopImmediatePropagation();
            }
            // Home 키로 원점으로 이동
            else if (evt.keyCode == KeyCode.Home)
            {
                ResetViewToCenter();
                evt.StopImmediatePropagation();
            }
        });

        var grid = new GridBackground();
        grid.StretchToParentSize();
        Insert(0, grid);
        
        styleSheets.Add(EditorGUIUtility.Load("GraphViewStyle.uss") as StyleSheet);

        // 검색 UI 구성
        CreateSearchUI();

        // 프리뷰 컨트롤러 초기화
        _previewController = new DialogPreviewController();

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
        gameEventNode._dialogData = _dialogData;
        gameEventNode._parentGraphView = this; // 부모 그래프 뷰 설정
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

        // DialogData의 _textDataName 설정
        _dialogData._textDataName = _dialogData.name.ToLower();

        // GUID to Entry/Event 매핑
        Dictionary<string, GameEventNode_Entry> guidToEntryNode = new Dictionary<string, GameEventNode_Entry>();
        Dictionary<string, GameEventNodeBase> guidToEventNode = new Dictionary<string, GameEventNodeBase>();
        HashSet<string> connectedEventGuids = new HashSet<string>(); // Entry에 연결된 이벤트 노드들

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
                guidToEventNode.Add(gameEventNode._guid, gameEventNode);
            }
        }

        // Entry별로 이벤트 리스트 구성 및 연결된 노드 추적
        foreach (var entryPair in guidToEntryNode)
        {
            var entryNode = entryPair.Value;
            DialogEventEntryData entryData = entryNode.exportEntryData();
            
            // Entry에서 시작하는 이벤트 체인 구축하고 연결된 노드들 수집
            BuildEventChain(entryNode, entryData, guidToEventNode, connectedEventGuids);
            
            _dialogData._dialogEventEntryList.Add(entryData);
        }

        // 연결된 이벤트 노드들만 NodeData에 추가
        foreach (var guid in connectedEventGuids)
        {
            if (guidToEventNode.ContainsKey(guid))
            {
                var gameEventNode = guidToEventNode[guid];
                NodeData nodeData = new NodeData();
                nodeData._guid = gameEventNode._guid;
                nodeData._position = gameEventNode.GetPosition().position;
                _dialogData._nodeData.Add(nodeData);
            }
        }

        // 링크 데이터 저장 (연결된 노드들만)
        foreach (var edge in edges.ToList())
        {
            bool isConnectedLink = false;
            
            // Entry 노드가 관련된 링크이거나, 양쪽 모두 연결된 이벤트 노드인 경우만 저장
            if (edge.output.node is GameEventNode_Entry)
            {
                isConnectedLink = true;
            }
            else if (edge.output.node is GameEventNodeBase outputNode && edge.input.node is GameEventNodeBase inputNode)
            {
                isConnectedLink = connectedEventGuids.Contains(outputNode._guid) && connectedEventGuids.Contains(inputNode._guid);
            }
            else if (edge.input.node is GameEventNode_Entry)
            {
                isConnectedLink = true;
            }

            if (isConnectedLink)
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
        }

        EditorUtility.SetDirty(_dialogData);
        AssetDatabase.SaveAssets();

        // 다이얼로그 텍스트 데이터 저장 (연결된 노드들만)
        SaveDialogTextData(connectedEventGuids);

        Debug.Log($"{_dialogData._textDataName} 저장 완료 (연결된 노드만 저장됨)");
    }

    private void SaveDialogTextData(HashSet<string> connectedEventGuids = null)
    {
        if (_dialogData == null || string.IsNullOrEmpty(_dialogData.name))
        {
            Debug.LogError("DialogData가 없거나 이름이 없습니다.");
            return;
        }

        // 모든 Dialog 이벤트에서 텍스트 데이터 수집
        StringKeyValueSetData textData = new StringKeyValueSetData();

        foreach (var entryData in _dialogData._dialogEventEntryList)
        {
            foreach (var eventData in entryData._dialogEventList)
            {
                if (eventData is DialogEventData_Dialog dialogEvent)
                {
                    // 연결된 노드만 필터링 (connectedEventGuids가 null이면 모든 노드 포함)
                    if (connectedEventGuids != null && !connectedEventGuids.Contains(dialogEvent._editorGuidString))
                        continue;

                    // 해당 Dialog 노드 찾기
                    GameEventNode_Dialog dialogNode = FindDialogNodeByGuid(dialogEvent._editorGuidString);
                    if (dialogNode != null && !string.IsNullOrEmpty(dialogNode._dialogText))
                    {
                        // GUID를 해시로 변환하여 키로 사용
                        int key = IOControl.computeHash(dialogEvent._editorGuidString);
                        
                        // StringKeyValueData 생성
                        StringKeyValueData stringData = new StringKeyValueData();
                        stringData._key = key;
                        stringData._value = dialogNode._dialogText;
                        
                        dialogEvent._dialogIndex = textData._stringData.Count;
                        textData._stringData.Add(stringData);
                    }
                }
            }
        }

        // 텍스트 데이터가 있는 경우에만 저장
        if (textData._stringData.Count > 0)
        {
            // DialogData의 이름을 파일명으로 사용
            string fileName = _dialogData.name;
            
            // LanguageInstanceManager를 통해 저장
            bool success = LanguageInstanceManager.Instance().SaveTextDataToXML(textData, fileName);
            if (success)
            {
                Debug.Log($"다이얼로그 텍스트 데이터 저장 완료: {fileName}.xml");
            }
            else
            {
                DebugUtil.assert(false, $"다이얼로그 텍스트 데이터 저장 실패: {fileName}.xml");
            }
        }
        else
        {
            DebugUtil.assert(false, "저장할 다이얼로그 텍스트가 없습니다.");
        }
    }

    private GameEventNode_Dialog FindDialogNodeByGuid(string guid)
    {
        foreach (var node in nodes)
        {
            if (node is GameEventNode_Dialog dialogNode && dialogNode._guid == guid)
            {
                return dialogNode;
            }
        }
        return null;
    }

    private void BuildEventChain(GameEventNode_Entry entryNode, DialogEventEntryData entryData, Dictionary<string, GameEventNodeBase> guidToEventNode, HashSet<string> connectedEventGuids)
    {
        // Entry에서 연결된 첫 번째 이벤트 찾기
        var firstEventNode = FindConnectedEventNode(entryNode);
        if (firstEventNode != null)
        {
            entryData._dialogEventList.Clear();
            BuildEventList(firstEventNode, entryData, guidToEventNode, new HashSet<string>(), connectedEventGuids);
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

    private void BuildEventList(GameEventNodeBase currentNode, DialogEventEntryData entryData, Dictionary<string, GameEventNodeBase> guidToEventNode, HashSet<string> visited, HashSet<string> connectedEventGuids)
    {
        if (visited.Contains(currentNode._guid))
            return; // 순환 참조 방지
            
        visited.Add(currentNode._guid);
        connectedEventGuids.Add(currentNode._guid); // 연결된 노드로 추가

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
                BuildEventList(nextNode, entryData, guidToEventNode, visited, connectedEventGuids);
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

        LanguageInstanceManager.Instance().LoadSingleTextData(_dialogData._textDataName);

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
                eventNode.constructField();
                eventNode.constructPort();
                eventNode.importGameEvent(eventData);

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

        // 프리뷰 패널이 활성화되어 있으면 데이터 업데이트
        if (_isPreviewVisible && _previewController != null)
        {
            _previewController.SetDialogData(_dialogData);
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

    private void FocusSearchField()
    {
        if (_searchField != null)
        {
            _searchField.Focus();
            _searchField.SelectAll();
        }
    }

    // 키보드를 통한 뷰 이동 메서드들
    private void MoveView(Vector3 delta)
    {
        var currentTransform = viewTransform;
        currentTransform.position += delta;
        UpdateViewTransform(currentTransform.position, currentTransform.scale);
    }

    private void ResetViewToCenter()
    {
        var currentTransform = viewTransform;
        currentTransform.position = Vector3.zero;
        currentTransform.scale = Vector3.one;
        UpdateViewTransform(currentTransform.position, currentTransform.scale);
    }

    // 프리뷰 패널 토글 기능
    public void TogglePreviewPanel(bool isVisible)
    {
        if (_previewPanel == null && isVisible)
        {
            CreatePreviewPanel();
        }
        
        if (_previewPanel != null)
        {
            _previewPanel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            _isPreviewVisible = isVisible;
            
            // 그래프 뷰 크기 조정 (프리뷰 패널 너비 820 = 800 + 여백)
            if (isVisible)
            {
                style.paddingRight = 820; // 프리뷰 패널 너비 + 여백
            }
            else
            {
                style.paddingRight = 0;
            }
            
            // 프리뷰 데이터 업데이트
            if (isVisible && _dialogData != null)
            {
                _previewController.SetDialogData(_dialogData);
                
                // Static Data 로딩 상태 확인
                if (!_previewController.IsStaticDataLoaded())
                {
                    Debug.Log("Dialog Preview: Static Data will be loaded when preview starts");
                }
            }
        }
    }

    private void CreatePreviewPanel()
    {
        _previewPanel = _previewController.CreatePreviewPanel();
        Add(_previewPanel);
    }

    // 특정 이벤트까지 미리보기를 진행하는 메서드
    public void ShowPreviewToEvent(GameEventNodeBase targetNode)
    {
        // 프리뷰 패널이 없으면 생성하고 표시
        if (!_isPreviewVisible)
        {
            TogglePreviewPanel(true);
        }
        
        if (_dialogData == null || _previewController == null)
        {
            Debug.LogWarning("Dialog Preview: No dialog data available for preview");
            return;
        }
        
        // 타겟 노드가 속한 엔트리를 찾기
        string entryKey = FindEntryForNode(targetNode);
        if (string.IsNullOrEmpty(entryKey))
        {
            Debug.LogWarning($"Dialog Preview: Could not find entry for node {targetNode.title}");
            return;
        }
        
        // 이벤트 GUID를 사용하여 미리보기 진행
        string eventGuid = targetNode._dialogEvent?._editorGuidString;
        if (!string.IsNullOrEmpty(eventGuid))
        {
            _previewController.SetDialogDataAndPlayToEvent(_dialogData, entryKey, eventGuid);
        }
        else
        {
            Debug.LogWarning($"Dialog Preview: Node {targetNode.title} has no valid GUID");
        }
    }

    // 노드가 속한 엔트리를 찾는 헬퍼 메서드
    private string FindEntryForNode(GameEventNodeBase targetNode)
    {
        // 현재 저장된 그래프에서 노드들의 연결 관계를 추적하여 엔트리를 찾기
        foreach (var element in graphElements)
        {
            if (element is GameEventNode_Entry entryNode)
            {
                // Entry 노드에서 시작하여 연결된 노드들을 추적
                if (IsNodeConnectedToEntry(entryNode, targetNode))
                {
                    var entryData = entryNode.exportEntryData();
                    return entryData?._entryKey;
                }
            }
        }
        
        return null;
    }

    // Entry 노드와 대상 노드가 연결되어 있는지 확인하는 헬퍼 메서드
    private bool IsNodeConnectedToEntry(GameEventNode_Entry entryNode, GameEventNodeBase targetNode)
    {
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        
        queue.Enqueue(entryNode);
        visited.Add(entryNode);
        
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            
            if (currentNode == targetNode)
            {
                return true;
            }
            
            // 출력 포트에서 연결된 다음 노드들을 확인
            foreach (var port in currentNode.outputContainer.Children().OfType<Port>())
            {
                foreach (var edge in port.connections)
                {
                    var connectedNode = edge.input.node;
                    if (!visited.Contains(connectedNode))
                    {
                        visited.Add(connectedNode);
                        queue.Enqueue(connectedNode);
                    }
                }
            }
        }
        
        return false;
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

    public DialogData _dialogData;
    public DialogEventDataBase _dialogEvent;
    public string _guid;
    public DialogEditorGraphView _parentGraphView;

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

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        
        // Entry 노드가 아닌 경우에만 Show Preview 옵션 추가
        if (getEventType() != DialogEventType.Entry)
        {
            evt.menu.AppendAction("Show Preview", (a) => ShowPreview(), DropdownMenuAction.AlwaysEnabled);
        }
    }

    private void ShowPreview()
    {
        if (_parentGraphView != null)
        {
            _parentGraphView.ShowPreviewToEvent(this);
        }
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
