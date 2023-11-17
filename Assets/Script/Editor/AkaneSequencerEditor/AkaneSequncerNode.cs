using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace AkaneSequencerGraph
{
    [Serializable]
    public class AkaneSequenceNode : Node
    {
        public AkaneSequenceNode()
        {
        }
    }

    public abstract class ReservedPhaseNode : AkaneSequenceNode
    {
        public Port NextPort { get; private set; }

        public abstract string GetOpenContext();
        public abstract string GetCloseContext();

        protected ReservedPhaseNode()
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

        public InitializePhaseNode() : base()
        {
            title = "InitializePhase";
        }
    }

    public sealed class UpdatePhaseNode : ReservedPhaseNode
    {
        public override string GetOpenContext() => "<UpdatePhase>";
        public override string GetCloseContext() => "</UpdatePhase>";

        public UpdatePhaseNode() : base()
        {
            title = "UpdatePhase";
        }
    }

    public sealed class EndPhaseNode : ReservedPhaseNode
    {
        public override string GetOpenContext() => "<EndPhase>";
        public override string GetCloseContext() => "</EndPhase>";

        public EndPhaseNode() : base()
        {
            title = "EndPhase";
        }
    }

    public abstract class EventNode : AkaneSequenceNode
    {
        public Port PrevPort { get; private set; }
        public Port NextPort { get; private set; }

        protected EventNode()
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
    }

    public sealed class CallAIEventNode : EventNode
    {
        public override string GetResultContext() =>
            string.Format(Context(), _eventNameTextField.text, _uniqueKeyTextField.text);

        protected override string Context() => "<CallAIEvent EventName = \"{0}\" UniqueKey = \"{1}\"/>";

        private readonly TextField _eventNameTextField;
        private readonly TextField _uniqueKeyTextField;

        public CallAIEventNode() : base()
        {
            title = "CallAIEvent";

            _eventNameTextField = new TextField("EventName");
            _uniqueKeyTextField = new TextField("UniqueKey");

            _eventNameTextField.style.minWidth = new StyleLength(20f);
            _uniqueKeyTextField.style.minWidth = new StyleLength(20f);

            mainContainer.Add(_eventNameTextField);
            mainContainer.Add(_uniqueKeyTextField);
        }
    }

    public sealed class SetCameraZoomNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _sizeFloatField.value);

        protected override string Context() => "<SetCameraZoom Size = \"{0}\"/>";

        private readonly FloatField _sizeFloatField;

        public SetCameraZoomNode() : base()
        {
            title = "SetCameraZoom";

            _sizeFloatField = new FloatField("Size");

            mainContainer.Add(_sizeFloatField);
        }
    }

    public sealed class ApplyPostProcessProfileNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _pathTextField.text,
            _blendTimeFloatField.value, _applyTypeField.text);

        protected override string Context() => "<ApplyPostProcessProfile Path = \"{0}\" BlendTime = \"{1}\" ApplyType = \"{2}\"/>";

        private readonly TextField _pathTextField;
        private readonly FloatField _blendTimeFloatField;
        private readonly TextField _applyTypeField;

        public ApplyPostProcessProfileNode() : base()
        {
            title = "ApplyPostProcessProfile";

            _pathTextField = new TextField("Path");
            _blendTimeFloatField = new FloatField("BlendTime");
            _applyTypeField = new TextField("ApplyType");

            mainContainer.Add(_pathTextField);
            mainContainer.Add(_blendTimeFloatField);
            mainContainer.Add(_applyTypeField);
        }
    }

    public sealed class WaitSignalNode : EventNode
    {
        public override string GetResultContext() => string.Format(Context(), _signalTextField.text);

        protected override string Context() => "<WaitSignal Signal = \"{0}\"/>";

        private readonly TextField _signalTextField;

        public WaitSignalNode() : base()
        {
            title = "WaitSignal";

            _signalTextField = new TextField("Signal");

            mainContainer.Add(_signalTextField);
        }
    }
}

