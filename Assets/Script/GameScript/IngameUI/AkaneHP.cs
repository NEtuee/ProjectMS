using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AkaneHP : ProjectorUI<AkaneHP.StateType>
{
    [SerializeField] private Image HPProgressImage;
    public class AkaneHPManagedData : IManagedData
    {
        public float hpPercentage;
        public AkaneHPManagedData(float hpPercentage) { this.hpPercentage = hpPercentage; }
    }
    public enum StateType { NONE, Opening, Idle, Closing }



    public override bool CheckLinkedComponent()
    {
        if (HPProgressImage == null)
            throw new Exception($"{HPProgressImage}가 연결되지 않았습니다!");
        return true;
    }
    public override void Initialize()
    {
        var stateMap = new Dictionary<StateType, UIState<StateType>>
        {
            { StateType.Opening, new OpeningState(this) },
            { StateType.Idle, new IdleState(this) },
            { StateType.Closing, new ClosingState(this) }
        };

        _stateMachine = new StateMachine<StateType>(this);
        _stateMachine.Initialize(stateMap, StateType.Opening);
    }
    public override void Activate()
    {
        _stateMachine.ChangeState(StateType.Opening);
    }
    public override void Deactivate()
    {
        _stateMachine.ChangeState(StateType.Closing);
    }
    public override void UpdateProjection(IManagedData data)
    {
        if (data is AkaneHPManagedData akaneHPData)
        {
            _receivedData = akaneHPData;
        }

        _stateMachine.UpdateState();
    }



    private class OpeningState : UIState<StateType>
    {
        public OpeningState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Opening;
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
            return StateType.Idle;
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