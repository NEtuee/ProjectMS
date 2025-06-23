using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneHP : ProjectorUI
{
    [SerializeField] private Image FrameImage;
    [SerializeField] private Image HPProgressImage;
    public struct AkaneHPManagedData : IManagedData
    {
        public UIDataType uiDataType { get { return UIDataType.AkaneHP; } }
        public float hpPercentage;
        public float changeAmount;
        public AkaneHPManagedData(float hpPercentage = 0.0f, float changeAmount = 0.0f) { this.hpPercentage = hpPercentage; this.changeAmount = changeAmount; }
    }
    public AkaneHPManagedData ReceivedData
    {
        get { return (AkaneHPManagedData)_receivedData; }
    }
    public AkaneHPManagedData ProjectingData
    {
        get { return (AkaneHPManagedData)_projectingData; }
        set { _projectingData = value; }
    }
    private enum UIStateType { NONE, Idle, Underattacked, LifeTapping, Lifestealing }
    private StateMachine<UIStateType> _stateMachine;



    public override bool CheckLinkedComponent()
    {
        if (HPProgressImage == null)
            throw new Exception("AkaneHP UI의 프로젝터 컴포넌트가 연결되지 않았습니다!");

        return true;
    }
    public override void Initialize()
    {
        _receivedData = new AkaneHPManagedData(0.0f, 0.0f);
        _projectingData = new AkaneHPManagedData(0.0f, 0.0f);

        var stateMap = new Dictionary<UIStateType, UIState<UIStateType>>
        {
            { UIStateType.Idle, new IdleState(this) },
            { UIStateType.Underattacked, new UnderattackedState(this) },
            { UIStateType.LifeTapping, new LifeTappingState(this) },
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
        if (data is AkaneHPManagedData akaneHPData)
        {
            _receivedData = akaneHPData;
        }

        _stateMachine.UpdateState();
    }



    private class IdleState : UIState<UIStateType>
    {
        private AkaneHP _akaneHP { get { return _projectorUI as AkaneHP; } }
        private Vector3 _basePosition;
        private float chasingSpeed = 5.0f;

        public IdleState(ProjectorUI projectorUI) : base(projectorUI) { }
        public override void Initialize()
        {
            _basePosition = _akaneHP.HPProgressImage.rectTransform.anchoredPosition;
        }
        public override UIStateType ChangeCondition()
        {
            return UIStateType.Idle;
        }
        public override IEnumerator OnEnterCoroutine() { yield return null; }
        public override void OnUpdate()
        {
            if (_akaneHP == null)
                return;

            float updatedHPPercentage = Mathf.Lerp(_akaneHP.ProjectingData.hpPercentage, _akaneHP.ReceivedData.hpPercentage, Time.deltaTime * chasingSpeed);
            float updatedChangeAmount = 0.0f;
            _akaneHP.HPProgressImage.fillAmount = _akaneHP.ProjectingData.hpPercentage;

            AkaneHPManagedData updatedProjectingData = new AkaneHPManagedData(updatedHPPercentage, updatedChangeAmount);
            _akaneHP.ProjectingData = updatedProjectingData;
        }
        public override IEnumerator OnExitCoroutine() { yield return null; }
    }

    private class UnderattackedState : UIState<UIStateType>
    {
        public UnderattackedState(ProjectorUI projectorUI) : base(projectorUI) { }

        public override void Initialize() { }
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

        private class LifeTappingState : UIState<UIStateType>
    {
        public LifeTappingState(ProjectorUI projectorUI) : base(projectorUI) { }

        public override void Initialize() { }
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

    private class LifestealingState : UIState<UIStateType>
    {
        public LifestealingState(ProjectorUI projectorUI) : base(projectorUI) { }

        public override void Initialize() { }
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