using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneQTE : ProjectorUI
{
    protected override IPackedUIData _dataStruct => new AkaneQTEData();
    public struct AkaneQTEData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneQTE;
        public float CatchPercentage;
        public float StunPercentage;

        public AkaneQTEData(float catchPercentage = 0.0f, float stunPercentage = 0.0f)
        {
            this.CatchPercentage = catchPercentage;
            this.StunPercentage = stunPercentage;
        }
    }
    public new AkaneQTEData ReceivedData => (AkaneQTEData)_receivedData;
    public new AkaneQTEData ProjectingData
    {
        get => (AkaneQTEData)_projectingData;
        set => _projectingData = value;
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.NONE };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
    new[] { Frame, Progress };
    [SerializeField] private UIVisualModuleData<AkaneQTEStateType> FrameData;
    private UIVisualModule Frame;
    [SerializeField] private UIVisualModuleData<AkaneQTEStateType> HPProgressData;
    private UIVisualModule Progress;
    public enum AkaneQTEStateType
    {
        Deactivated,
        Activated,
        Idle,
    }
    private Dictionary<AkaneQTEStateType, UIState> _akaneQTEStateMap;
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneQTEStateMap;



    protected override void SetDataConstructor()
    {
        _receivedData = new AkaneQTEData();
        _projectingData = new AkaneQTEData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();
    }
    protected override void SetStateMap()
    {
        _stateMachine = new UIStateMachine(this);

        _akaneQTEStateMap = new Dictionary<AkaneQTEStateType, UIState>()
        {
            { AkaneQTEStateType.Deactivated, new DeactivatedState(this) },
            { AkaneQTEStateType.Activated, new ActivatedState(this) },
            { AkaneQTEStateType.Idle, new IdleState(this) }
        };
    }
    protected override void SetUIVisualModule()
    {
        throw new NotImplementedException();
    }
    public override void Activate()
    {
        throw new NotImplementedException();
    }
    public override void Deactivate()
    {
        throw new NotImplementedException();
    }



    private class DeactivatedState : UIState
    {
        private AkaneQTE _akaneQTE => (AkaneQTE)_projectorUI;
        public DeactivatedState(AkaneQTE akaneQTE) : base(akaneQTE) { }
        protected override IEnumerator OnEnterProjection()
        {
            yield break;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class ActivatedState : UIState
    {
        private AkaneQTE _akaneHP => (AkaneQTE)_projectorUI;
        public ActivatedState(AkaneQTE akaneQTE) : base(akaneQTE) { }
        public override void OnUpdateProjection()
        {

        }
        protected override IEnumerator OnEnterProjection()
        {
            yield break;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class IdleState : UIState
    {
        private AkaneQTE _akaneQTE => (AkaneQTE)_projectorUI;
        public IdleState(AkaneQTE akaneQTE) : base(akaneQTE) { }
        public override void OnUpdateProjection()
        {
            
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
}