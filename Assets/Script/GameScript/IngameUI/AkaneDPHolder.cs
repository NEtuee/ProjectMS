using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneDPHolder : HolderUI
{
    public new AkaneDPData ReceivedData => (AkaneDPData)_receivedData;
    public new AkaneDPData ProjectingData
    {
        get => (AkaneDPData)_projectingData;
        set => _projectingData = value;
    }
    protected override UIDataType _dataType => UIDataType.AkaneDP;
    public struct AkaneDPData : IPackedUIData
    {
        public UIDataType UIDataType => UIDataType.AkaneDP;
        public float CurrentDP;

        public AkaneDPData(float currentDP = 0.0f)
        {
            this.CurrentDP = currentDP;
        }

        public static bool operator ==(AkaneDPData left, AkaneDPData right) =>
            Mathf.Approximately(left.CurrentDP, right.CurrentDP);

        public static bool operator !=(AkaneDPData left, AkaneDPData right) => !(left == right);
        public override bool Equals(object obj) => obj is AkaneDPData other && this == other;
        public override int GetHashCode() => CurrentDP.GetHashCode();
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneDPHolderStateType //개별 DP에 전달? 여기는 Holder의 상태만,,
    {
        NONE,
        Idle,
        Underattacked,
    }
    private Dictionary<AkaneDPHolderStateType, UIState> _stateMap = new Dictionary<AkaneDPHolderStateType, UIState>();



    public override void Initialize()
    {
        base.Initialize();

        _receivedData = new AkaneDPData();
        _projectingData = new AkaneDPData();

        //_stateMap.Add(AkaneDPStateType.NONE, new NONE(this));

        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneDPHolderStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneDPHolderStateType.NONE]);
    }
    protected override void UpdateProjection()
    {
        base.UpdateProjection();
    }
}