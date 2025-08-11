using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneBP : ProjectorUI
{
    protected override IPackedUIData _dataStruct => new AkaneBPData();
    public struct AkaneBPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneBP;
        public float BPPercentage;
        public float ChangeAmount;

        public AkaneBPData(float bpPercentage = 0.0f, float changeAmount = 0.0f)
        {
            this.BPPercentage = bpPercentage;
            this.ChangeAmount = changeAmount;
        }
    }
    public new AkaneBPData ReceivedData => (AkaneBPData)_receivedData;
    public new AkaneBPData ProjectingData
    {
        get => (AkaneBPData)_projectingData;
        set => _projectingData = value;
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.Hyperfailed, UIEventKey.Attacked, UIEventKey.Hit };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
        new[] { Progress };
    [SerializeField] private UIVisualModuleData<AkaneBPStateType> BPProgressData;
    private UIVisualModule Progress;
    public enum AkaneBPStateType
    {
        Deactivated,
        Activated,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneBPStateType, UIState> _akaneBPStateMap;
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneBPStateMap;


    //공통 메서드
    protected override void SetDataConstructor()
    {
        _receivedData = new AkaneBPData();
        _projectingData = new AkaneBPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();
    }
    protected override void SetStateMap()
    {
        _stateMachine = new UIStateMachine(this);

        _akaneBPStateMap = new Dictionary<AkaneBPStateType, UIState>()
        {
            { AkaneBPStateType.Deactivated, new DeactivatedState(this) },
            { AkaneBPStateType.Activated, new ActivatedState(this) },
            { AkaneBPStateType.Idle, new IdleState(this) },
            { AkaneBPStateType.Underattacked, new UnderattackedState(this) },
            { AkaneBPStateType.Lifetapping, new LifetappingState(this) },
            { AkaneBPStateType.Lifestealing, new LifestealingState(this) }
        };
    }
    protected override void SetUIVisualModule()
    {
        _projectingCoroutineList = new List<Coroutine>();

        Progress = gameObject.AddComponent<UIVisualModule>();

        foreach (UIVisualModule uiVisualModule in UIVisualModules)
            uiVisualModule.Initialize();
        
        Progress.SetFromData<AkaneBPStateType>(BPProgressData);
    }
    public override void Activate()
    {
        gameObject.SetActive(true);
        StopAllProjectionCoroutine();
        _stateMachine.ForceStateChanging(_akaneBPStateMap[AkaneBPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_akaneBPStateMap[AkaneBPStateType.Deactivated]);
        StopAllProjectionCoroutine();
        gameObject.SetActive(false);
    }


    //데이터 투영    
    private void UpdateAkaneBPProjection()
    {
        if (Progress != null)
            Progress.Image.fillAmount = ProjectingData.BPPercentage;
    }
    //데이터 수정
    private void NormalBPDataUpdate()
    {
        float updatedBPPercentage = Mathf.Lerp(ProjectingData.BPPercentage, ReceivedData.BPPercentage, Time.deltaTime * 50.0f);
        float updatedChangeAmount = 0.0f;
        AkaneBPData updatedProjectingData = new AkaneBPData(updatedBPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;

        UpdateAkaneBPProjection();
    }
    private void DirectBPDataUpdate()
    {
        ProjectingData = ReceivedData;

        UpdateAkaneBPProjection();
    }
    //UI 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class DeactivatedState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public DeactivatedState(AkaneBP akaneBP) : base(akaneBP) { }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] bpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.FadeOutAlpha(_akaneBP.Progress, 0.25f)
            };

            _akaneBP._projectingCoroutineList.Add(_akaneBP.StartCoroutine(_akaneBP.Progress.ApplyEffectsInParallel(bpProgressEffects)));

            foreach (Coroutine coroutine in _akaneBP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class ActivatedState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public ActivatedState(AkaneBP akaneBP) : base(akaneBP) { }
        public override void OnUpdateProjection()
        {
            _akaneBP.DirectBPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] bpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneBP.Progress, 0.5f, 24.0f),
                UIAnimationCommons.FadeInAlpha(_akaneBP.Progress, 1.0f)
            };

            _akaneBP._projectingCoroutineList.Add(_akaneBP.StartCoroutine(_akaneBP.Progress.ApplyEffectsInParallel(bpProgressEffects)));

            foreach (Coroutine coroutine in _akaneBP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];

            return null;
        }
    }
    private class IdleState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public IdleState(AkaneBP akaneBP) : base(akaneBP) { }
        public override void OnUpdateProjection()
        {
            _akaneBP.NormalBPDataUpdate();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            return null;
        }
    }
    private class UnderattackedState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public UnderattackedState(AkaneBP akaneBP) : base(akaneBP) { }
        public override void OnUpdateProjection()
        {
            _akaneBP.DirectBPDataUpdate();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];

            return null;
        }
    }
    private class LifetappingState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public LifetappingState(AkaneBP akaneBP) : base(akaneBP) { }
        public override void OnUpdateProjection()
        {
            _akaneBP.DirectBPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] bpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveDownOffsetToPosition(_akaneBP.Progress, 0.5f, 4.0f),
                UIAnimationCommons.FlickAlpha(_akaneBP.Progress, 0.5f, 1.0f, 4)
            };

            _akaneBP._projectingCoroutineList.Add(_akaneBP.StartCoroutine(_akaneBP.Progress.ApplyEffectsInParallel(bpProgressEffects)));

            foreach (Coroutine coroutine in _akaneBP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];

            return null;
        }
    }
    private class LifestealingState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public LifestealingState(AkaneBP akaneBP) : base(akaneBP) { }
        public override void OnUpdateProjection()
        {
            _akaneBP.DirectBPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] bpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveUpOffsetToPosition(_akaneBP.Progress, 0.3f, 1.0f),
                UIAnimationCommons.FlickAlpha(_akaneBP.Progress, 0.1f, 0.2f, 1)
            };

            _akaneBP._projectingCoroutineList.Add(_akaneBP.StartCoroutine(_akaneBP.Progress.ApplyEffectsInParallel(bpProgressEffects)));

            foreach (Coroutine coroutine in _akaneBP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];

            return null;
        }
    }
}