using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneDPHolder : ProjectorUI
{
    public new AkaneDPData ReceivedData => (AkaneDPData)_receivedData;
    public new AkaneDPData ProjectingData
    {
        get => (AkaneDPData)_projectingData;
        set => _projectingData = value;
    }
    protected override UIDataType DataType => UIDataType.AkaneDP;
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
    protected override IReadOnlyCollection<UIEventKey> ValidEventKeys { get; } =
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneDPStateType
    {
        NONE,
        Idle,
        UnderAttacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneDPStateType, UIState> _stateMap = new Dictionary<AkaneDPStateType, UIState>();



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
        _stateMachine.RequestStateChanging(_stateMap[AkaneDPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.RequestStateChanging(_stateMap[AkaneDPStateType.NONE]);
    }
    protected override void UpdateProjection()
    {
        base.UpdateProjection();
    }
}