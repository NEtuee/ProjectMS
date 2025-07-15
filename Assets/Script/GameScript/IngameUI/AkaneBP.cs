using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneBP : ProjectorUI
{
    [SerializeField] private Image BPProgressImage;

    public new AkaneBPData ReceivedData => (AkaneBPData)_receivedData;
    public new AkaneBPData ProjectingData
    {
        get => (AkaneBPData)_projectingData;
        set => _projectingData = value;
    }
    protected override UIDataType _dataType => UIDataType.AkaneBP;
    public struct AkaneBPData : IPackedUIData
    {
        public UIDataType UIDataType => UIDataType.AkaneBP;
        public float BPPercentage;
        public float ChangeAmount;

        public AkaneBPData(float bpPercentage = 0.0f, float changeAmount = 0.0f)
        {
            this.BPPercentage = bpPercentage;
            this.ChangeAmount = changeAmount;
        }

        public static bool operator ==(AkaneBPData left, AkaneBPData right) =>
            Mathf.Approximately(left.BPPercentage, right.BPPercentage) &&
            Mathf.Approximately(left.ChangeAmount, right.ChangeAmount);

        public static bool operator !=(AkaneBPData left, AkaneBPData right) => !(left == right);
        public override bool Equals(object obj) => obj is AkaneBPData other && this == other;
        public override int GetHashCode() => BPPercentage.GetHashCode() ^ ChangeAmount.GetHashCode();
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneBPStateType
    {
        NONE,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneBPStateType, UIState> _stateMap = new Dictionary<AkaneBPStateType, UIState>();



    public override void Initialize()
    {
        base.Initialize();

        _receivedData = new AkaneBPData();
        _projectingData = new AkaneBPData();

        //_stateMap.Add(AkaneBPStateType.NONE, new NONE(this));

        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneBPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneBPStateType.NONE]);
    }
    protected override void UpdateProjection()
    {
        base.UpdateProjection();
        BPProgressImage.fillAmount = ProjectingData.BPPercentage;
    }
}