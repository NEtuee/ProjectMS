using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SequencerGraph
{
    public class SampleGraphEditorWindow : EditorWindow
    {
        [MenuItem("CustomWindow/Open SampleGraphView")]
        public static void Open()
        {
            GetWindow<SampleGraphEditorWindow>("SampleGraphWindow");
        }

        private void OnEnable()
        {
            var graphView = new SequenceGraphView(this)
            {
                style = { flexGrow = 1 }
            };
            rootVisualElement.Add(graphView);
            
            rootVisualElement.Add(new Button(() => { GraphFile.GraphToXmlFile(graphView, "Test");}){text = "Generate"});
        }
    }

    public class SequenceGraphView : GraphView
    {
        public EditorWindow OwnerWindow { get; private set; }

        public InitializePhaseNode InitializePhase { get; private set; }
        public UpdatePhaseNode UpdatePhase { get; private set; }
        public EndPhaseNode EndPhase { get; private set; }

        public SequenceGraphView(EditorWindow owner) : base()
        {
            OwnerWindow = owner;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);

            InitializePhase = new InitializePhaseNode();
            UpdatePhase = new UpdatePhaseNode();
            EndPhase = new EndPhaseNode();
            AddElement(InitializePhase);
            AddElement(UpdatePhase);
            AddElement(EndPhase);

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
        private SequenceGraphView _graphView;

        public void Initialize(SequenceGraphView graphView)
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
                        (type.IsSubclassOf(typeof(SequenceNode))) &&
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

            var node = Activator.CreateInstance(type) as SequenceNode;
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
