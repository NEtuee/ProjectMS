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
    protected override Vector2 _poolSize => new Vector2(4, 10);
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


    //전용 메서드
    private void CenteringHoldingDP() //홀더에 있는 DP 가운데 정렬 및 위치 지정
    {

    }
    //1. 최초 생성
    //2. 풀에 가져옴
    //3. 풀에서 내보냄
    //4. 삭제
    //데이터 관리,, 서브 데이터 갱신될 때만 전달해도 되나? DP는 그래도 될듯
    //마지막 DP는 Next 스프라이트로 교체
}