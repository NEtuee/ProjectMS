using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneDP : ProjectorUI
{
    protected override UIDataType _dataType => UIDataType.AkaneDP;
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneDPStateType
    {
        NONE,
        Idle,
        Consumed,
        AutoRecovering,
        AttackRecovering
    }
    private Dictionary<AkaneDPStateType, UIState> _stateMap = new Dictionary<AkaneDPStateType, UIState>();
    public override void Initialize()
    {
        base.Initialize();

        //_stateMap.Add(AkaneDPStateType.NONE, new NONE(this));

        Deactivate();
    }
    public override void Activate()
    {
        
    }
    public override void Deactivate()
    {
        
    }
}