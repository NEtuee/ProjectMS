using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDPHolder : ProjectorUI<AkaneDPHolder.StateType>
{
    public class AkaneDPManagedData : IManagedData
    {
        //공격 성공 여부 추가
        //자동 회복 시작 여부 추가
        public int remainingDP;
        public int maxDP;
        public AkaneDPManagedData(int remainingDP, int maxDP) { this.remainingDP = remainingDP; this.maxDP = maxDP; }
    }
    private ProjectorUI<AkaneDP.StateType>[] HoldingProjectorUIList;
    public enum StateType { NONE, Opening, Idle, Closing }



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



    private class OpeningState : UIState<StateType>
    {
        public OpeningState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Closing;
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
    private class IdleState : UIState<StateType>
    {
        public IdleState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Closing;
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
    private class ClosingState : UIState<StateType>
    {
        public ClosingState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Closing;
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