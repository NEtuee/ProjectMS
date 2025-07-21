using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDP : ProjectorUI
{
    [SerializeField] private Image DPImage;
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
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.Dash, UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneDPStateType
    {
        NONE,
        Idle,
        Consumed,
        AutoRecovering,
        AttackRecovering
    }
    private Dictionary<AkaneDPStateType, UIState> _stateMap = new Dictionary<AkaneDPStateType, UIState>();


    //공통 메서드
    public override void Initialize()
    {
        base.PrepareInitialize();

        _stateMap.Add(AkaneDPStateType.NONE, new NONE(this));
        _stateMap.Add(AkaneDPStateType.Idle, new IdleState(this));
        _stateMap.Add(AkaneDPStateType.Consumed, new ConsumedState(this));
        _stateMap.Add(AkaneDPStateType.AutoRecovering, new AutoRecoveringState(this));
        _stateMap.Add(AkaneDPStateType.AttackRecovering, new AttackRecoveringState(this));

        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneDPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneDPStateType.NONE]);
    }

    //투영 데이터 업데이트 메서드 및 코루틴

    //전용 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class IdleState : UIState
    {
        private readonly AkaneDP _akaneDP;
        private readonly Dictionary<AkaneDPStateType, UIState> _stateMap;
        public IdleState(AkaneDP akaneDP)
        {
            _akaneDP = akaneDP;
            _stateMap = akaneDP._stateMap;
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
    private class ConsumedState : UIState
    {
        private readonly AkaneDP _akaneDP;
        private readonly Dictionary<AkaneDPStateType, UIState> _stateMap;
        public ConsumedState(AkaneDP akaneDP)
        {
            _akaneDP = akaneDP;
            _stateMap = akaneDP._stateMap;
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
    private class AutoRecoveringState : UIState
    {
        private readonly AkaneDP _akaneDP;
        private readonly Dictionary<AkaneDPStateType, UIState> _stateMap;
        public AutoRecoveringState(AkaneDP akaneDP)
        {
            _akaneDP = akaneDP;
            _stateMap = akaneDP._stateMap;
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
    private class AttackRecoveringState : UIState
    {
        private readonly AkaneDP _akaneDP;
        private readonly Dictionary<AkaneDPStateType, UIState> _stateMap;
        public AttackRecoveringState(AkaneDP akaneDP)
        {
            _akaneDP = akaneDP;
            _stateMap = akaneDP._stateMap;
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
    private class NONE : UIState
    {
        private readonly AkaneDP _akaneDP;
        private readonly Dictionary<AkaneDPStateType, UIState> _stateMap;
        public NONE(AkaneDP akaneDP)
        {
            _akaneDP = akaneDP;
            _stateMap = akaneDP._stateMap;
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