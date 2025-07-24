using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDP : ProjectorUI
{
    protected override UIDataType _dataType => UIDataType.AkaneDP;
    public struct SingleAkaneDPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneDP;
        public bool IsActivated;
        public float RecoveryRatio;
        public SingleAkaneDPData(bool isActivated = true, float recoveryRatio = 0.0f)
        {
            this.IsActivated = isActivated;
            this.RecoveryRatio = recoveryRatio;
        }
    }
    public new SingleAkaneDPData ReceivedData => (SingleAkaneDPData)_receivedData;
    public new SingleAkaneDPData ProjectingData
    {
        get => (SingleAkaneDPData)_projectingData;
        set => _projectingData = value;
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.Dash, UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    public enum AkaneDPStateType
    {
        NONE,
        Idle,
        Consumed,
        Autorecovering,
        Attackrecovering
    }
    private Dictionary<AkaneDPStateType, UIState> _akaneDPStateMap = new Dictionary<AkaneDPStateType, UIState>();
    protected override IDictionary<TUIStateType, UIState> _stateMap<TUIStateType>()
        { return (IDictionary<TUIStateType, UIState>)(object)_akaneDPStateMap; }
    public IDictionary<AkaneDPStateType, UIState> StateMap => _stateMap<AkaneDPStateType>();
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPData;
    private UIVisualModule DP;
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPProgressData;
    private UIVisualModule DPProgress;
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPProgressFrameData;
    private UIVisualModule DPProgressFrame;


    //공통 메서드
    protected override void SetInitialConstructor()
    {
        _receivedData = new SingleAkaneDPData();
        _projectingData = new SingleAkaneDPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();

        DP = new UIVisualModule();
        DPProgress = new UIVisualModule();
        DPProgressFrame = new UIVisualModule();
    }
    protected override void SetStateMap()
    {
        _akaneDPStateMap.Add(AkaneDPStateType.NONE, new NONE(this));
        _akaneDPStateMap.Add(AkaneDPStateType.Idle, new IdleState(this));
        _akaneDPStateMap.Add(AkaneDPStateType.Consumed, new ConsumedState(this));
        _akaneDPStateMap.Add(AkaneDPStateType.Autorecovering, new AutorecoveringState(this));
        _akaneDPStateMap.Add(AkaneDPStateType.Attackrecovering, new AttackrecoveringState(this));
    }
    protected override void SetUIVisualModule()
    {
        DP.SetFromData<AkaneDPStateType>(DPData);
        DPProgress.SetFromData<AkaneDPStateType>(DPProgressData);
        DPProgressFrame.SetFromData<AkaneDPStateType>(DPProgressFrameData);
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(StateMap[AkaneDPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(StateMap[AkaneDPStateType.NONE]);
    }

    //투영 데이터 업데이트 메서드 및 코루틴
    private void DPProjectionUpdate()
    {

    }
    private IEnumerator DPAutoRecoveringCoroutine()
    {
        yield return null;
    }
    //전용 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class IdleState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public IdleState(AkaneDP akaneDP) : base(akaneDP) {}
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class ConsumedState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public ConsumedState(AkaneDP akaneDP) : base(akaneDP) {}
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class AutorecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public AutorecoveringState(AkaneDP akaneDP) : base(akaneDP) {}
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class AttackrecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public AttackrecoveringState(AkaneDP akaneDP) : base(akaneDP) {}
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class NONE : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public NONE(AkaneDP akaneDP) : base(akaneDP) {}
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