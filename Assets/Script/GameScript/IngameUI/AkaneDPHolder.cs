using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDPHolder : ProjectorUI
{
    public class AkaneDPManagedData : IManagedData
    {
        //공격 성공 여부 추가
        //자동 회복 시작 여부 추가
        public int remainingDP;
        public int maxDP;
        public AkaneDPManagedData(int remainingDP, int maxDP) { this.remainingDP = remainingDP; this.maxDP = maxDP; }
        public UIDataType uiDataType => UIDataType.AkaneDP;
    }
    private ProjectorUI[] HoldingProjectorUIList;
    private StateMachine<UIStateType> _stateMachine;
    public enum UIStateType { NONE, Idle }



    public override bool CheckLinkedComponent()
    {
        return true;
    }
    public override void Initialize()
    {

    }
    public override void Activate()
    {

    }
    public override void Deactivate()
    {

    }
    public override void UpdateProjection(IManagedData data)
    {
        if (data is AkaneDPManagedData akaneDPData)
        {
            _receivedData = akaneDPData;
        }
    }



    private class IdleState : UIState<UIStateType>
    {
        public IdleState(ProjectorUI projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override UIStateType ChangeCondition()
        {
            return UIStateType.Idle;
        }
        public override IEnumerator OnEnterCoroutine()
        {
            yield return null;
        }

        public override void OnUpdate()
        {

        }

        public override IEnumerator OnExitCoroutine()
        {
            yield return null;
        }
    }
}