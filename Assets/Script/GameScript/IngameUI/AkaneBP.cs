using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AkaneBP : ProjectorUI
{
    [SerializeField] private Image BPProgressImage;
    public struct AkaneBPManagedData : IManagedData
    {
        public UIDataType uiDataType { get { return UIDataType.AkaneBP; } }
        public float bpPercentage;
        public float changeAmount;
        public AkaneBPManagedData(float bpPercentage, float changeAmount) { this.bpPercentage = bpPercentage; this.changeAmount = changeAmount; }
    }
    public AkaneBPManagedData ReceivedData
    {
        get { return (AkaneBPManagedData)_receivedData; }
    }
    public AkaneBPManagedData ProjectingData
    {
        get { return (AkaneBPManagedData)_projectingData; }
        set { _projectingData = value; }
    }
    private StateMachine<UIStateType> _stateMachine;
    private enum UIStateType { NONE, Idle, Underattacked, LifeTapping, Lifestealing }



    public override bool CheckLinkedComponent()
    {
        if (BPProgressImage == null)
            throw new Exception($"{BPProgressImage}가 연결되지 않았습니다!");
        return true;
    }
    public override void Initialize()
    {
        var stateMap = new Dictionary<UIStateType, UIState<UIStateType>>
        {
            { UIStateType.Idle, new IdleState(this) },
            { UIStateType.Underattacked, new UnderattackedState(this) },
            { UIStateType.Lifestealing, new LifestealingState(this) }
        };

        _stateMachine = new StateMachine<UIStateType>(this, stateMap);
    }
    public override void Activate()
    {
        _stateMachine.ChangeState(UIStateType.Idle);
    }
    public override void Deactivate()
    {
        _stateMachine.ChangeState(UIStateType.NONE);
    }
    public override void UpdateProjection(IManagedData data)
    {
        if (data is AkaneBPManagedData akaneBPData)
        {
            _receivedData = akaneBPData;
        }

        _stateMachine.UpdateState();
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
    //피격
    private class UnderattackedState : UIState<UIStateType>
    {
        public UnderattackedState(ProjectorUI projectorUI) : base(projectorUI) { }
        public override void Initialize() { return; }
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
    //공격 회복
    private class LifestealingState : UIState<UIStateType>
    {
        public LifestealingState(ProjectorUI projectorUI) : base(projectorUI) { }
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