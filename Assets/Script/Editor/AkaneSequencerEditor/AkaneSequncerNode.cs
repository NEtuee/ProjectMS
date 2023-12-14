using System;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace AkaneSequencerGraph
{
    [Serializable]
    public class AkaneSequenceNode : Node
    {
        public string Guid => _guid;
        protected string _guid;

        public AkaneSequenceNode(string guid)
        {
            _guid = guid;
        }
    }

    public abstract class ReservedPhaseNode : AkaneSequenceNode
    {
        public Port NextPort { get; private set; }

        public abstract string GetOpenContext();
        public abstract string GetCloseContext();

        protected ReservedPhaseNode(string guid) : base(guid)
        {
            capabilities -= Capabilities.Deletable;

            NextPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            NextPort.portName = "Start";
            outputContainer.Add(NextPort);
        }
    }

    public sealed class InitializePhaseNode : ReservedPhaseNode
    {
        public override string GetOpenContext() => "<InitializePhase>";
        public override string GetCloseContext() => "</InitializePhase>";

        public InitializePhaseNode(string guid) : base(guid)
        {
            title = "InitializePhase";
        }
    }

    public sealed class UpdatePhaseNode : ReservedPhaseNode
    {
        public override string GetOpenContext() => "<UpdatePhase>";
        public override string GetCloseContext() => "</UpdatePhase>";

        public UpdatePhaseNode(string guid) : base(guid)
        {
            title = "UpdatePhase";
        }
    }

    public sealed class EndPhaseNode : ReservedPhaseNode
    {
        public override string GetOpenContext() => "<EndPhase>";
        public override string GetCloseContext() => "</EndPhase>";

        public EndPhaseNode(string guid) : base(guid)
        {
            title = "EndPhase";
        }
    }

    public sealed class TaskEventNode : AkaneSequenceNode
    {
        public Port NextPort { get; private set; }

        public string TaskKey => _taskKeyField.value;
        
        private readonly EnumField _processEnumField;
        private readonly TextField _taskKeyField;
        
        public TaskEventNode(string guid) : base(guid)
        {
            title = "Task";
            
            NextPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            NextPort.portName = "Start";
            outputContainer.Add(NextPort);
            
            _processEnumField = new EnumField("ProcessType", SequencerGraphProcessor.TaskProcessType.StepByStep);
            mainContainer.Add(_processEnumField);

            _taskKeyField = new TextField("TaskKey");
            mainContainer.Add(_taskKeyField);
        }

        public string GetContext()
        {
            var eventContextSpace = "          ";
            var sb = new StringBuilder();
            sb.Append(eventContextSpace);
            sb.AppendLine($"<Task ProcessType = \"{_processEnumField.value}\"/>");

            var edge = NextPort.connections.FirstOrDefault();
            if (edge == null)
            {
                sb.Append(eventContextSpace);
                sb.AppendLine($"</Task>");
                return sb.ToString();
            }

            var node = edge.input.node as EventNode;
            if (node == null)
            {
                sb.Append(eventContextSpace);
                sb.AppendLine($"</Task>");
                return sb.ToString();
            }

            while (node != null)
            {
                sb.Append(eventContextSpace);
                sb.AppendLine(node.GetResultContext());

                var nextEdge = node.NextPort.connections.FirstOrDefault();
                if (nextEdge == null)
                {
                    break;
                }
                
                node = nextEdge.input.node as EventNode;
            }
            
            sb.Append(eventContextSpace);
            sb.AppendLine($"</Task>");
            return sb.ToString();
        }
    }

    public abstract class EventNode : AkaneSequenceNode
    {
        public Port PrevPort { get; private set; }
        public Port NextPort { get; private set; }

        protected EventNode(string guid) : base(guid)
        {
            PrevPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
            PrevPort.portName = "Prev";
            inputContainer.Add(PrevPort);

            NextPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            NextPort.portName = "Next";
            outputContainer.Add(NextPort);
        }

        public abstract string GetResultContext();
        protected abstract string Context();
        
        protected string ParseVectorString(Vector3Field vector3Field)
        {
            var vector = vector3Field.value;
            return $"{vector.x} {vector.y} {vector.z}";
        }

        protected string ParseColorString(ColorField colorField)
        {
            var color = colorField.value;
            return $"{color.r} {color.g} {color.b} {color.a}";
        }
    }
    
    // Notion Link
    // https://www.notion.so/kukipasta/Sequencer-Graph-0e09514b7f9449a0a5a7eeab40e813a7?pvs=4

    public sealed class SpawnCharacter : EventNode
    {
        private readonly TextField _characterKeyTextField;
        private readonly Vector3Field _positionField;
        private readonly TextField _positionMakerTextField;
        private readonly EnumField _searchIdentifierEnumField;
        private readonly TextField _uniqueKeyTextFiled;
        
        public SpawnCharacter(string guid) : base(guid)
        {
            title = "SpawnCharacter";

            _characterKeyTextField = new TextField("CharacterKey");
            _positionField = new Vector3Field("Position");
            _positionMakerTextField = new TextField("PositionMaker");
            _searchIdentifierEnumField = new EnumField("SearchIdentifier", SearchIdentifier.Player);
            _uniqueKeyTextFiled = new TextField("UniqueKey");

            _characterKeyTextField.style.minWidth = new StyleLength(20f);
            _positionField.style.minWidth = new StyleLength(20f);
            _positionMakerTextField.style.minWidth = new StyleLength(20f);
            _searchIdentifierEnumField.style.minWidth = new StyleLength(20f);
            _uniqueKeyTextFiled.style.minWidth = new StyleLength(20f);

            mainContainer.Add(_characterKeyTextField);
            mainContainer.Add(_positionField);
            mainContainer.Add(_positionMakerTextField);
            mainContainer.Add(_searchIdentifierEnumField);
            mainContainer.Add(_uniqueKeyTextFiled);
        }

        public override string GetResultContext() => string.Format(
            Context(),
            _characterKeyTextField.text,
            ParseVectorString(_positionField),
            _positionMakerTextField.text,
            _searchIdentifierEnumField.value,
            _uniqueKeyTextFiled.text);

        protected override string Context() => "<SpawnCharacter CharacterKey = \"{0}\" Position = \"{1}\" PositionMarker = \"{2}\" SearchIdentifier = \"{3}\" UniqueKey = \"{4}\"/>";
    }

    public sealed class WaitSecondNode : EventNode
    {
        private readonly FloatField _timeTextField;
        
        public WaitSecondNode(string guid) : base(guid)
        {
            title = "WaitSecond";

            _timeTextField = new FloatField("Time");

            _timeTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_timeTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _timeTextField.value);
        protected override string Context() => "<WaitSecond Time = \"{0}\"/>";
    }
    
    public sealed class SetCameraTargetNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        
        public SetCameraTargetNode(string guid) : base(guid)
        {
            title = "SetCameraTarget";
            
            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_uniqueKeyTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text);

        protected override string Context() => "<SetCameraTarget UniqueKey = \"{0}\" CameraMode = \"TargetCenterMode\"/>";
    }
    
    public sealed class SetCameraPositionNode : EventNode
    {
        private readonly Vector3Field _targetPositionVectorField;
        
        public SetCameraPositionNode(string guid) : base(guid)
        {
            title = "SetCameraPosition";
            
            _targetPositionVectorField = new Vector3Field("TargetPosition");
            _targetPositionVectorField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_targetPositionVectorField);
        }

        public override string GetResultContext() => string.Format(Context(), ParseVectorString(_targetPositionVectorField));

        protected override string Context() => "<SetCameraPosition TargetPosition = \"{0}\"/>";
    }
    
    public sealed class SetAudioListnerNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        
        public SetAudioListnerNode(string guid) : base(guid)
        {
            title = "SetAudioListner";
            
            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_uniqueKeyTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text);

        protected override string Context() => "<SetAudioListner UniqueKey = \"{0}\"/>";
    }
    
    public sealed class SetCrossHairNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        
        public SetCrossHairNode(string guid) : base(guid)
        {
            title = "SetCrossHair";
            
            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_uniqueKeyTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text);

        protected override string Context() => "<SetCrossHair UniqueKey = \"{0}\"/>";
    }
    
    public sealed class SetHPSphereNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        
        public SetHPSphereNode(string guid) : base(guid)
        {
            title = "SetHPSphere";
            
            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_uniqueKeyTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text);

        protected override string Context() => "<SetHPSphere UniqueKey = \"{0}\"/>";
    }
    
    public sealed class TeleportTargetToNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly Vector3Field _positionVectorField;
        private readonly TextField _positionMakerTextField;
        
        public TeleportTargetToNode(string guid) : base(guid)
        {
            title = "TeleportTargetTo";
            
            _uniqueKeyTextField = new TextField("UniqueKey");
            _positionVectorField = new Vector3Field("Position");
            _positionMakerTextField = new TextField("PositionMaker");
            
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            _positionVectorField.style.minWidth = new StyleLength(20f);
            _positionMakerTextField.style.minWidth = new StyleLength(20f);
            
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_positionVectorField);
            mainContainer.Add(_positionMakerTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text, ParseVectorString(_positionVectorField), _positionMakerTextField.text);

        protected override string Context() => "<TeleportTargetTo UniqueKey = \"{0}\" Position = \"{1}\" PositionMarker = \"{2}\"/>";
    }
    
    public sealed class ApplyPostProcessProfileNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _pathTextField.text, _blendTimeFloatField.value, _applyTypeEnumField.value);

        protected override string Context() => "<ApplyPostProcessProfile Path = \"{0}\" BlendTime = \"{1}\" ApplyType = \"{2}\"/>";

        private readonly TextField _pathTextField;
        private readonly FloatField _blendTimeFloatField;
        private readonly EnumField _applyTypeEnumField;

        public ApplyPostProcessProfileNode(string guid) : base(guid)
        {
            title = "ApplyPostProcessProfile";

            _pathTextField = new TextField("Path");
            _blendTimeFloatField = new FloatField("BlendTime");
            _applyTypeEnumField = new EnumField("ApplyType", PostProcessProfileApplyType.BaseBlend);

            mainContainer.Add(_pathTextField);
            mainContainer.Add(_blendTimeFloatField);
            mainContainer.Add(_applyTypeEnumField);
        }
    }

    public sealed class CallAIEventNode : EventNode
    {
        public override string GetResultContext() => string.Format(
            Context(),
            _eventNameTextField.text,
            _uniqueKeyTextField.text,
            _eventTargetTypeEnumField.value,
            _rangeFloatField.value,
            _searchIdentifierEnumField.value);

        protected override string Context() => "<CallAIEvent EventName = \"{0}\" UniqueKey = \"{1}\" EventTargetType = \"{2}\" Range = \"{3}\" SearchIdentifier = \"{4}\"/>";

        private readonly TextField _eventNameTextField;
        private readonly TextField _uniqueKeyTextField;
        private readonly EnumField _eventTargetTypeEnumField;
        private readonly FloatField _rangeFloatField;
        private readonly EnumField _searchIdentifierEnumField;

        public CallAIEventNode(string guid) : base(guid)
        {
            title = "CallAIEvent";

            _eventNameTextField = new TextField("EventName");
            _uniqueKeyTextField = new TextField("UniqueKey");
            _eventTargetTypeEnumField = new EnumField("EventTargetType", CallAIEventTargetType.Range);
            _rangeFloatField = new FloatField("Range");
            _searchIdentifierEnumField = new EnumField("SearchIdentifier", SearchIdentifier.Player);
            
            _eventNameTextField.style.minWidth = new StyleLength(20f);
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);
            _eventTargetTypeEnumField.style.minWidth = new StyleLength(20f);
            _rangeFloatField.style.minWidth = new StyleLength(20f);
            _searchIdentifierEnumField.style.minWidth = new StyleLength(20f);

            mainContainer.Add(_eventNameTextField);
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_eventTargetTypeEnumField);
            mainContainer.Add(_rangeFloatField);
            mainContainer.Add(_searchIdentifierEnumField);
        }
    }
    
    public sealed class WaitSignalNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _signalTextField.text);

        protected override string Context() => "<WaitSignal Signal = \"{0}\"/>";

        private readonly TextField _signalTextField;

        public WaitSignalNode(string guid) : base(guid)
        {
            title = "WaitSignal";

            _signalTextField = new TextField("Signal");

            mainContainer.Add(_signalTextField);
        }
    }

    public sealed class SetCameraZoomNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _sizeFloatField.value, _speedFloatField.value, _forceToggle.value);

        protected override string Context() => "<SetCameraZoom Size = \"{0}\" Speed = \"{1}\" Force = \"{2}\"/>";

        private readonly FloatField _sizeFloatField;
        private readonly FloatField _speedFloatField;
        private readonly Toggle _forceToggle;

        public SetCameraZoomNode(string guid) : base(guid)
        {
            title = "SetCameraZoom";

            _sizeFloatField = new FloatField("Size");
            _speedFloatField = new FloatField("Speed");
            _forceToggle = new Toggle("Force");

            mainContainer.Add(_sizeFloatField);
            mainContainer.Add(_speedFloatField);
            mainContainer.Add(_forceToggle);
        }
    }

    public sealed class ZoomEffectNode : EventNode
    {
        private readonly FloatField _zoomEffectFloatField;

        public ZoomEffectNode(string guid) : base(guid)
        {
            title = "ZoomEffect";
            
            _zoomEffectFloatField = new FloatField("ZoomEffect");
            
            mainContainer.Add(_zoomEffectFloatField);
        }

        public override string GetResultContext() => string.Format(Context(), _zoomEffectFloatField.value);

        protected override string Context() => "<ZoomEffect Factor = \"{0}\"/>";
    }
    
    public sealed class FadeInNode : EventNode
    {
        private readonly FloatField _lambdaFloatField;

        public FadeInNode(string guid) : base(guid)
        {
            title = "FadeIn";

            _lambdaFloatField = new FloatField("Lambda");
            
            mainContainer.Add(_lambdaFloatField);
        }

        public override string GetResultContext() => string.Format(Context(), _lambdaFloatField.value);

        protected override string Context() => "<FadeIn Lambda = \"{0}\"/>";
    }
    
    public sealed class FadeOutNode : EventNode
    {
        private readonly FloatField _lambdaFloatField;

        public FadeOutNode(string guid) : base(guid)
        {
            title = "FadeOut";

            _lambdaFloatField = new FloatField("Lambda");
            
            mainContainer.Add(_lambdaFloatField);
        }

        public override string GetResultContext() => string.Format(Context(), _lambdaFloatField.value);

        protected override string Context() => "<FadeOut Lambda = \"{0}\"/>";
    }
    
    public sealed class ForceQuitNode : EventNode
    {
        public ForceQuitNode(string guid) : base(guid)
        {
            title = "ForceQuit";
        }

        public override string GetResultContext() => string.Format(Context());

        protected override string Context() => "<ForceQuit/>";
    }

    public sealed class BlockInputNode : EventNode
    {
        private readonly Toggle _enableToggle;
        
        public BlockInputNode(string guid) : base(guid)
        {
            title = "BlockInput";

            _enableToggle = new Toggle("Enable");
            
            mainContainer.Add(_enableToggle);
        }

        public override string GetResultContext() => string.Format(Context(), _enableToggle.value);

        protected override string Context() => "<BlockInput Enable = \"{0}\"/>";
    }
    
    public sealed class BlockAINode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly TextField _uniqueGroupTextField;
        private readonly Toggle _enableToggle;
        
        public BlockAINode(string guid) : base(guid)
        {
            title = "BlockAI";

            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueGroupTextField = new TextField("UniqueGroup");
            _enableToggle = new Toggle("Enable");
            
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_uniqueGroupTextField);
            mainContainer.Add(_enableToggle);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text, _uniqueGroupTextField.text, _enableToggle.value);

        protected override string Context() => "<BlockAINode UniqueKey = \"{0}\" UniqueGroup = \"{1}\" Enable = \"{2}\"/>";
    }
    
    public sealed class SetActionNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly TextField _uniqueGroupTextField;
        private readonly TextField _actionTextField;
        
        public SetActionNode(string guid) : base(guid)
        {
            title = "SetAction";

            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueGroupTextField = new TextField("UniqueGroup");
            _actionTextField = new TextField("Action");
            
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_uniqueGroupTextField);
            mainContainer.Add(_actionTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text, _uniqueGroupTextField.text, _actionTextField.text);

        protected override string Context() => "<SetAction UniqueKey = \"{0}\" UniqueGroup = \"{1}\" Action = \"{2}\"/>";
    }
    
    public sealed class PlayAnimationNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly TextField _pathTextField;
        
        public PlayAnimationNode(string guid) : base(guid)
        {
            title = "PlayAnimation";

            _uniqueKeyTextField = new TextField("UniqueKey");
            _pathTextField = new TextField("Path");
            
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_pathTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text, _pathTextField.text);

        protected override string Context() => "<PlayAnimationNode UniqueKey = \"{0}\" Path = \"{1}\"/>";
    }
    
    public sealed class AIMoveNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly TextField _startActionTextField;
        private readonly TextField _loopActionTextField;
        private readonly TextField _endActionTextField;
        private readonly Vector3Field _endPositionVectorField;
        private readonly TextField _endPositionMakerTextField;

        public AIMoveNode(string guid) : base(guid)
        {
            title = "AIMove";

            _uniqueKeyTextField = new TextField("UniqueKey");
            _startActionTextField = new TextField("StartAction");
            _loopActionTextField = new TextField("LoopAction");
            _endActionTextField = new TextField("EndAction");
            _endPositionVectorField = new Vector3Field("EndPosition");
            _endPositionMakerTextField = new TextField("EndPositionMarker");

            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_startActionTextField);
            mainContainer.Add(_loopActionTextField);
            mainContainer.Add(_endActionTextField);
            mainContainer.Add(_endPositionVectorField);
            mainContainer.Add(_endPositionMakerTextField);
        }

        public override string GetResultContext() => string.Format(Context(), 
            _uniqueKeyTextField.text, 
            _startActionTextField.text,
            _loopActionTextField.text,
            _endActionTextField.text,
            ParseVectorString(_endPositionVectorField),
            _endPositionMakerTextField.text);

        protected override string Context() => "<AIMove UniqueKey = \"{0}\" StartAction = \"{1}\" LoopAction = \"{2}\" EndAction = \"{3}\" EndPosition = \"{4}\" EndPositionMarker = \"{5}\"/>";
    }
    
    public sealed class QTEFenceNode : EventNode
    {
        private readonly TextField _keyNameTextField;
        
        public QTEFenceNode(string guid) : base(guid)
        {
            title = "QTEFence";

            _keyNameTextField = new TextField("KeyName");
            
            mainContainer.Add(_keyNameTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _keyNameTextField.text);

        protected override string Context() => "<QTEFence KeyName = \"{0}\"/>";
    }
    
    public sealed class DeadFenceNode : EventNode
    {
        public DeadFenceNode(string guid) : base(guid)
        {
            title = "DeadFence";
        }

        public override string GetResultContext() => string.Format(Context());

        protected override string Context() => "<DeadFence/>";
    }
    
    public sealed class SetHideUINode : EventNode
    {
        private readonly Toggle _hideToggle;
        
        public SetHideUINode(string guid) : base(guid)
        {
            title = "SetHideUI";

            _hideToggle = new Toggle("Hide");
            
            mainContainer.Add(_hideToggle);
        }

        public override string GetResultContext() => string.Format(Context(), _hideToggle.value);

        protected override string Context() => "<SetHideUI Hide = \"{0}\"/>";
    }
    
    public sealed class ShakeEffectNode : EventNode
    {
        private readonly FloatField _scaleFloatField;
        private readonly FloatField _timeFloatField;
        private readonly FloatField _blendTimeFloatField;
        
        public ShakeEffectNode(string guid) : base(guid)
        {
            title = "ShakeEffect";

            _scaleFloatField = new FloatField("Scale");
            _timeFloatField = new FloatField("Time");
            _blendTimeFloatField = new FloatField("BlendTime");
            
            mainContainer.Add(_scaleFloatField);
            mainContainer.Add(_timeFloatField);
            mainContainer.Add(_blendTimeFloatField);
        }

        public override string GetResultContext() => string.Format(Context(), _scaleFloatField.value, _timeFloatField.value, _blendTimeFloatField.value);

        protected override string Context() => "<ShakeEffect Scale = \"{0}\" Time = \"{1}\" BlendTime = \"{2}\"/>";
    }
    
    public sealed class NextStageNode : EventNode
    {
        private readonly TextField _pathTextField;
        
        public NextStageNode(string guid) : base(guid)
        {
            title = "NextStage";

            _pathTextField = new TextField("Path");
            
            mainContainer.Add(_pathTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _pathTextField.text);

        protected override string Context() => "<NextStage Path = \"{0}\"/>";
    }
    
    public sealed class ToastMessageNode : EventNode
    {
        private readonly TextField _textTextField;
        private readonly FloatField _timeTextField;
        private readonly ColorField _colorField;
        
        public ToastMessageNode(string guid) : base(guid)
        {
            title = "ToastMessage";

            _textTextField = new TextField("Text");
            _timeTextField = new FloatField("Time");
            _colorField = new ColorField("Color");
            
            mainContainer.Add(_textTextField);
            mainContainer.Add(_timeTextField);
            mainContainer.Add(_colorField);
        }

        public override string GetResultContext() => string.Format(Context(), _textTextField.text, _timeTextField.value, ParseColorString(_colorField));

        protected override string Context() => "<ToastMessage Text = \"{0}\" Time = \"{1}\" Color = \"{2}\"/>";
    }
    
    public sealed class TaskCallNode : EventNode
    {
        public string TaskKey => _taskKeyField.value;
        
        private readonly TextField _taskKeyField;
    
        public TaskCallNode(string guid) : base(guid)
        {
            title = "TaskCall";

            _taskKeyField = new TextField("Task Key");
            mainContainer.Add(_taskKeyField);
        }
    
        public override string GetResultContext() => string.Empty;
    
        protected override string Context() => string.Empty;
    }
    
    // public sealed class TaskNode : EventNode
    // {
    //     private readonly EnumField _processEnumField;
    //
    //     public TaskNode(string guid) : base(guid)
    //     {
    //         title = "Task";
    //
    //         _processEnumField = new EnumField("ProcessType", SequencerGraphProcessor.TaskProcessType.StepByStep);
    //
    //         mainContainer.Add(_processEnumField);
    //     }
    //
    //     public override string GetResultContext() => string.Format(Context(), _processEnumField.value);
    //
    //     protected override string Context() => "<Task ProcessType = \"{0}\"/>";
    // }
    //
    // public sealed class TaskEndNode : EventNode
    // {
    //     public TaskEndNode(string guid) : base(guid)
    //     {
    //         title = "TaskEnd";
    //     }
    //
    //     public override string GetResultContext() => string.Format(Context());
    //
    //     protected override string Context() => "</Task>";
    // }
    
    public sealed class LetterBoxShowNode : EventNode
    {
        public LetterBoxShowNode(string guid) : base(guid)
        {
            title = "LetterBoxShow";
        }

        public override string GetResultContext() => string.Format(Context());

        protected override string Context() => "<LetterBoxShow/>";
    }
    
    public sealed class LetterBoxHide : EventNode
    {
        public LetterBoxHide(string guid) : base(guid)
        {
            title = "LetterBoxHide";
        }

        public override string GetResultContext() => string.Format(Context());

        protected override string Context() => "<LetterBoxHide/>";
    }
    
    public sealed class TalkBalloonNode : EventNode
    {
        private readonly TextField _uniqueKeyTextField;
        private readonly TextField _uniqueGroupTextField;
        private readonly TextField _simpleTalkKeyTextField;
        
        public TalkBalloonNode(string guid) : base(guid)
        {
            title = "TalkBalloon";

            _uniqueKeyTextField = new TextField("UniqueKey");
            _uniqueGroupTextField = new TextField("UniqueGroup");
            _simpleTalkKeyTextField = new TextField("SimpleTalkKey");
            
            mainContainer.Add(_uniqueKeyTextField);
            mainContainer.Add(_uniqueGroupTextField);
            mainContainer.Add(_simpleTalkKeyTextField);
        }

        public override string GetResultContext() => string.Format(Context(), _uniqueKeyTextField.text, _uniqueGroupTextField.text, _simpleTalkKeyTextField.text);

        protected override string Context() => "<SetAction UniqueKey = \"{0}\" UniqueGroup = \"{1}\" SimpleTalkKey = \"{2}\"/>";
    }
}

