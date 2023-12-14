using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AkaneSequencerGraph
{
    public class AkaneSequencerGraphEditorWindow : EditorWindow
    {
        public AkaneSequencerGraphData CurrentData;

        private AkaneSequencerGraphView _akaneSequencerGraphView;
        private string _assetPath;
        
        [MenuItem("CustomWindow/Open SampleGraphView")]
        public static void Open(AkaneSequencerGraphData data, string path)
        {
            var editorWindow = GetWindow<AkaneSequencerGraphEditorWindow>("SampleGraphWindow");
            editorWindow.Init(data, path);
        }

        private void OnEnable()
        {
            var graphView = new AkaneSequencerGraphView(this)
            {
                style = { flexGrow = 1 }
            };
            _akaneSequencerGraphView = graphView;
            rootVisualElement.Add(graphView);

            rootVisualElement.Add(new Button(() => { GraphFile.GraphToXmlFile(graphView, "Test");}){text = "Generate"});
            rootVisualElement.Add(new Button(() =>
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance<AkaneSequencerGraphData>();
                var data = (AkaneSequencerGraphData) scriptableObject;
                
                data.InitalizePhaseNodeList.Clear();
                data.UpdatePhaseNodeList.Clear();
                data.EndPhaseNodeList.Clear();
                data.TaskNodeList.Clear();
                data.EdgeList.Clear();

                data.InitializePhase = graphView.InitializePhase;
                data.UpdatePhase = graphView.UpdatePhase;
                data.EndPhase = graphView.EndPhase;
                
                var (phaseNode, childNodeList) = graphView.GetInitializePhase();
                data.InitalizePhaseNodeList.AddRange(childNodeList);
                
                (phaseNode, childNodeList) = graphView.GetUpdatePhase();
                data.UpdatePhaseNodeList.AddRange(childNodeList);
                
                (phaseNode, childNodeList) = graphView.GetEndPhase();
                data.EndPhaseNodeList.AddRange(childNodeList);

                data.TaskNodeList.AddRange(graphView.GetTaskNodeList());
                
                foreach (var edge in graphView.edges.ToList())
                {
                    var to = edge.input.node as AkaneSequenceNode;
                    var from = edge.output.node as AkaneSequenceNode;

                    if (from == null || to == null)
                    {
                        continue;
                    }

                    var edgeData = new EdgeSaveData(from.Guid, to.Guid);
                    
                    data.EdgeList.Add(edgeData);
                }
                
                EditorUtility.SetDirty(scriptableObject);
                AssetDatabase.CreateAsset(scriptableObject, _assetPath);
                AssetDatabase.SaveAssets();
            }){text = "Save"});
        }

        public void Init(AkaneSequencerGraphData data, string assetPath)
        {
            CurrentData = data;
            _assetPath = assetPath;
            
            if (_akaneSequencerGraphView != null)
            {
                _akaneSequencerGraphView.InitNode(data.InitalizePhaseNodeList);
                _akaneSequencerGraphView.InitNode(data.UpdatePhaseNodeList);
                _akaneSequencerGraphView.InitNode(data.EndPhaseNodeList); 
                _akaneSequencerGraphView.InitNode(data.TaskNodeList);
                _akaneSequencerGraphView.InitPhaseNode(data.InitializePhase, data.UpdatePhase, data.EndPhase);

                var allNode = _akaneSequencerGraphView.nodes.ToList().Select(node => node as AkaneSequenceNode).ToList();

                foreach (var edgeData in data.EdgeList)
                {
                    var from = allNode.FirstOrDefault(x => x?.Guid == edgeData.From);
                    var to = allNode.FirstOrDefault(x => x?.Guid == edgeData.To);
                    
                    if (from == null || to == null) continue;
                    
                    var input = to.inputContainer.Children().FirstOrDefault(x => x is Port) as Port;
                    var output = from.outputContainer.Children().FirstOrDefault(x => x is Port) as Port;

                    if (input == null || output == null) continue;
                    
                    var edge = new Edge() {input = input, output = output};
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    _akaneSequencerGraphView.Add(edge);
                }
            }
        }
    }

    public class AkaneSequencerGraphView : GraphView
    {
        public EditorWindow OwnerWindow { get; private set; }

        public InitializePhaseNode InitializePhase { get; private set; }
        public UpdatePhaseNode UpdatePhase { get; private set; }
        public EndPhaseNode EndPhase { get; private set; }

        public AkaneSequencerGraphView(EditorWindow owner) : base()
        {
            OwnerWindow = owner;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var searchWindowProvider = new NodeSearchWindowProvider();
            searchWindowProvider.Initialize(this);

            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
            };

            // var styleSheet = (StyleSheet)EditorGUIUtility.Load("DialogueSystem/DSGraphViewStyles.uss");
            // styleSheets.Add(styleSheet);
        }

        public void InitNode(List<AkaneSequenceNode> nodeList)
        {
            foreach (var node in nodeList)
            {
                AddElement(node);
            }
        }
        
        public void InitPhaseNode(InitializePhaseNode init, UpdatePhaseNode update, EndPhaseNode end)
        {
            if (init != null)
            {
                InitializePhase = init;
            }
            else
            {
                InitializePhase = new InitializePhaseNode(Guid.NewGuid().ToString());
            }
            AddElement(InitializePhase);

            if (update != null)
            {
                UpdatePhase = update;
            }
            else
            {
                UpdatePhase = new UpdatePhaseNode(Guid.NewGuid().ToString());
            }
            AddElement(UpdatePhase);
            
            if (end != null)
            {
                EndPhase = end;
            }
            else
            {
                EndPhase = new EndPhaseNode(Guid.NewGuid().ToString());
            }
            AddElement(EndPhase);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (startPort.node == port.node ||
                    startPort.direction == port.direction ||
                    startPort.portType != port.portType)
                {
                    continue;
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }
        
        public TaskEventNode GetTaskEventNode(string key)
        {
            var nodeList = nodes.ToList();
            foreach (var node in nodeList)
            {
                if (node is TaskEventNode taskEventNode)
                {
                    if (taskEventNode.TaskKey == key)
                    {
                        return taskEventNode;
                    }
                }
            }

            return null;
        }

        public List<AkaneSequenceNode> GetTaskNodeList()
        {
            var result = new List<AkaneSequenceNode>();
            
            var nodeList = nodes.ToList();
            foreach (var node in nodeList)
            {
                if (node is TaskEventNode taskEventNode)
                {
                    result.Add(taskEventNode);
                    
                    var firstEdge = taskEventNode.NextPort.connections.FirstOrDefault();
                    if (firstEdge == null)
                    {
                        continue;
                    }
            
                    var currentNode = firstEdge.input.node as EventNode;

                    while (currentNode != null)
                    {
                        result.Add(currentNode);
                
                        var edge = currentNode.NextPort.connections.FirstOrDefault();
                        if (edge == null)
                        {
                            break;
                        }

                        currentNode = edge.input.node as EventNode;;
                    }
                }
            }

            return result;
        }

        public (ReservedPhaseNode phaseNode, List<EventNode> childNodeList) GetInitializePhase()
        {
            return GetPhaseNodeList(InitializePhase);
        }
        
        public (ReservedPhaseNode phaseNode, List<EventNode> childNodeList) GetUpdatePhase()
        {
            return GetPhaseNodeList(UpdatePhase);
        }
        
        public (ReservedPhaseNode phaseNode, List<EventNode> childNodeList) GetEndPhase()
        {
            return GetPhaseNodeList(EndPhase);
        }
        
        private (ReservedPhaseNode phaseNode, List<EventNode> childNodeList) GetPhaseNodeList(ReservedPhaseNode phaseNode)
        {
            if (phaseNode == null)
            {
                return (null, null);
            }
            
            var firstEdge = phaseNode.NextPort.connections.FirstOrDefault();
            if (firstEdge == null)
            {
                return (phaseNode,  new List<EventNode>());
            }
            
            var currentNode = firstEdge.input.node as EventNode;
            var nodeList = new List<EventNode>();

            while (true)
            {
                if (currentNode == null)
                {
                    break;
                }
                
                nodeList.Add(currentNode);
                
                var edge = currentNode.NextPort.connections.FirstOrDefault();
                if (edge == null)
                {
                    break;
                }

                currentNode = edge.input.node as EventNode;
            }

            return (phaseNode, nodeList);
        }
    }

    public class NodeSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private AkaneSequencerGraphView _graphView;

        public void Initialize(AkaneSequencerGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract &&
                        (type.IsSubclassOf(typeof(AkaneSequenceNode))) &&
                        type != typeof(InitializePhaseNode) &&
                        type != typeof(UpdatePhaseNode) &&
                        type != typeof(EndPhaseNode))
                    {
                        entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
                    }
                }
            }

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as System.Type;
            if (type == null)
            {
                return false;
            }

            var guid = Guid.NewGuid().ToString();
            var node = Activator.CreateInstance(type, guid) as AkaneSequenceNode;
            if (node == null)
            {
                return false;
            }

            var editorWindow = _graphView.OwnerWindow;
            var worldPosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(
                editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var localPosition = _graphView.WorldToLocal(worldPosition);

            var rect = node.GetPosition();
            rect.position = localPosition;

            node.SetPosition(rect);
            _graphView.AddElement(node);
            return true;
        }
    }
}
