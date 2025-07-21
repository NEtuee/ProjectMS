using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDPHolder : HolderUI
{
    [SerializeField] private ProjectorUI DPPrefab;
    public new AkaneDPData ReceivedData => (AkaneDPData)_receivedData;
    protected override UIDataType _dataType => UIDataType.AkaneDP;
    public struct AkaneDPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneDP;
        public float CurrentDP;
        public float MaxDP;
        public AkaneDPData(float currentDP = 0.0f, float maxDP = 0.0f)
        {
            this.CurrentDP = currentDP;
            this.MaxDP = maxDP;
        }
    }
    protected override ProjectorUI _projectorUIPrefab => DPPrefab;

    //공통 메서드
    public override void Initialize()
    {
        base.PrepareInitialize();

        _receivedData = new AkaneDPData();
    }
    public override void Activate()
    {

    }
    public override void Deactivate()
    {
        
    }
}