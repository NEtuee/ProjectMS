using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

public class AkaneDPHolder : HolderUI
{
    
    protected override IPackedUIData _dataStruct => new AkaneDPData();
    public struct AkaneDPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneDP;
        public int CurrentDP;
        public int MaxDP;
        public float CooldownPercentage;
        public bool JustConsumed;
        public AkaneDPData(int currentDP = 0, int maxDP = 0, float cooldownPercentage = 0.0f, bool justConsumed = false)
        {
            this.CurrentDP = currentDP;
            this.MaxDP = maxDP;
            this.CooldownPercentage = cooldownPercentage;
            this.JustConsumed = justConsumed;
        }
    }
    public new AkaneDPData ReceivedData => (AkaneDPData)_receivedData;
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.NONE };
    protected override Vector2 _poolSize => new Vector2(4, 10);
    [SerializeField] private ProjectorUI DPPrefab;
    protected override ProjectorUI _projectorUIPrefab => DPPrefab;


    //공통 메서드
    protected override void SetInitialConstructor()
    {
        _receivedData = new AkaneDPData();
    }
    protected override void ShareUpdatedData() //SingleAkaneDP에 맞춰 데이터 가공
    {
        if (_uiHolder == null || _uiHolder.Count == 0)
            return;

        for (int i = 0; i < _uiHolder.Count; i++)
        {
            AkaneDP singleDP = (AkaneDP)_uiHolder[i];

            if (singleDP == null)
                Debug.Log("DP is NULL");

            AkaneDP.SingleAkaneDPData updatedSingleData = new AkaneDP.SingleAkaneDPData();

            if (i < ReceivedData.CurrentDP - 1) //Idle
                updatedSingleData = new AkaneDP.SingleAkaneDPData(true, false, ReceivedData.CooldownPercentage, ReceivedData.JustConsumed);
            else if (i == ReceivedData.CurrentDP - 1) //Waiting
                updatedSingleData = new AkaneDP.SingleAkaneDPData(true, true, ReceivedData.CooldownPercentage, ReceivedData.JustConsumed);
            else if (i > ReceivedData.CurrentDP - 1) //Consumed
                updatedSingleData = new AkaneDP.SingleAkaneDPData(false, false, ReceivedData.CooldownPercentage, ReceivedData.JustConsumed);

            singleDP.ReceiveUpdatedData(updatedSingleData);
        }
    }
    protected override void UpdateHolder() //데이터에 맞춰 Holder 업데이트
    {
        int difference = ReceivedData.MaxDP - _uiHolder.Count;

        if (difference == 0)
            return;

        else if (difference < 0)
        {
            for (int i = 0; i < -difference; i++)
            {
                if (_uiHolder.Count > 0)
                {
                    ProjectorUI projectorUI = _uiHolder[0];
                    _uiHolder.RemoveAt(0);
                    _objectPool.Release(projectorUI);
                }
            }
        }
        else if (difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                _uiHolder.Add(_objectPool.Get());
            }
        }

        RepositionHoldingDP();
    }


    //전용 메서드
    private void RepositionHoldingDP() //최대 DP에 맞춰서 홀더에 있는 DP 가운데 정렬 및 위치 지정
    {
        if (_uiHolder == null || _uiHolder.Count == 0)
            return;
        
        int holdingDPCount = _uiHolder.Count;
        List<int> dpXOffset = new List<int>();

        if (_uiHolder.Count % 2 == 0)
        {
            for (int i = 0; i < holdingDPCount / 2; i++)
            {
                dpXOffset.Add(-11 - (21 * i));
                dpXOffset.Add(10 + (21 * i));
            }
        }
        else
        {
            dpXOffset.Add(0);

            for (int i = 1; i <= (holdingDPCount - 1) / 2; i++)
            {
                dpXOffset.Add(0 - (21 * i));
                dpXOffset.Add(0 + (21 * i));
            }
        }

        dpXOffset.Sort();

        for (int i = 0; i < holdingDPCount; i++)
        {
            ((AkaneDP)_uiHolder[i]).DP.UpdateBasePosition(new Vector2(dpXOffset[i], 0));
        }
    }
    //1. 최초 생성
    //2. 풀에 가져옴
    //3. 풀에서 내보냄
    //4. 삭제
    //데이터 관리,, 서브 데이터 갱신될 때만 전달해도 되나? DP는 그래도 될듯
    //마지막 DP는 Next 스프라이트로 교체
}