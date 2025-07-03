using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneDP : ProjectorUI
{
    protected override UIDataType DataType => UIDataType.AkaneDP;
    protected override IReadOnlyCollection<UIEventKey> ValidEventKeys { get; } =
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
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