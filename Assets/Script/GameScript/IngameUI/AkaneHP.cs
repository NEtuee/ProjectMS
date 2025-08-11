using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneHP : ProjectorUI
{
    protected override IPackedUIData _dataStruct => new AkaneHPData();
    public struct AkaneHPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneHP;
        public float HPPercentage;
        public float ChangeAmount;

        public AkaneHPData(float hpPercentage = 0.0f, float changeAmount = 0.0f)
        {
            this.HPPercentage = hpPercentage;
            this.ChangeAmount = changeAmount;
        }
    }
    public new AkaneHPData ReceivedData => (AkaneHPData)_receivedData;
    public new AkaneHPData ProjectingData
    {
        get => (AkaneHPData)_projectingData;
        set => _projectingData = value;
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.Hyperfailed, UIEventKey.Attacked, UIEventKey.Hit };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
    new[] { Frame, Progress };
    [SerializeField] private UIVisualModuleData<AkaneHPStateType> FrameData;
    private UIVisualModule Frame;
    [SerializeField] private UIVisualModuleData<AkaneHPStateType> HPProgressData;
    private UIVisualModule Progress;
    public enum AkaneHPStateType
    {
        Deactivated,
        Activated,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneHPStateType, UIState> _akaneHPStateMap;
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneHPStateMap;


    //공통 메서드    
    protected override void SetDataConstructor()
    {
        _receivedData = new AkaneHPData();
        _projectingData = new AkaneHPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();
    }
    protected override void SetStateMap()
    {
        _stateMachine = new UIStateMachine(this);

        _akaneHPStateMap = new Dictionary<AkaneHPStateType, UIState>()
        {
            { AkaneHPStateType.Deactivated, new DeactivatedState(this) },
            { AkaneHPStateType.Activated, new ActivatedState(this) },
            { AkaneHPStateType.Idle, new IdleState(this) },
            { AkaneHPStateType.Underattacked, new UnderattackedState(this) },
            { AkaneHPStateType.Lifetapping, new LifetappingState(this) },
            { AkaneHPStateType.Lifestealing, new LifestealingState(this) }
        };
    }
    protected override void SetUIVisualModule()
    {
        _projectingCoroutineList = new List<Coroutine>();

        Frame = gameObject.AddComponent<UIVisualModule>();
        Progress = gameObject.AddComponent<UIVisualModule>();

        foreach (UIVisualModule uiVisualModule in UIVisualModules)
            uiVisualModule.Initialize();

        Frame.SetFromData<AkaneHPStateType>(FrameData);
        Progress.SetFromData<AkaneHPStateType>(HPProgressData);
    }
    public override void Activate()
    {
        gameObject.SetActive(true);
        StopAllProjectionCoroutine();
        _stateMachine.ForceStateChanging(_akaneHPStateMap[AkaneHPStateType.Activated]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_akaneHPStateMap[AkaneHPStateType.Deactivated]);
        StopAllProjectionCoroutine();
        gameObject.SetActive(false);
    }


    //데이터 투영
    private void UpdateAkaneHPProjection()
    {
        if (Progress != null)
            Progress.Image.fillAmount = ProjectingData.HPPercentage;
    }
    //데이터 수정
    // private IEnumerator HPLerpCoroutine()
    // {
    //     if (Progress.Image == null)
    //         yield break;

    //     float elapsedTime = 0.0f;
    //     float duration = 0.2f;
    //     MathEx.EaseType easingFunction = MathEx.EaseType.EaseOutCubic;

    //     while (elapsedTime < duration)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float t = Mathf.Clamp01(elapsedTime / duration);
    //         float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

    //         float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, easedT);
    //         float updatedChangeAmount = updatedHPPercentage - ProjectingData.HPPercentage;
    //         AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
    //         ProjectingData = updatedProjectingData;

    //         yield return null;
    //     }

    //     ProjectingData = ReceivedData;
    // }
    private void NormalHPDataUpdate()
    {
        float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 50.0f);
        float updatedChangeAmount = ReceivedData.ChangeAmount;
        AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;

        UpdateAkaneHPProjection();
    }
    private void SlowHPDataUpdate()
    {
        float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 2.0f);
        float updatedChangeAmount = ReceivedData.ChangeAmount;
        AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;

        UpdateAkaneHPProjection();
    }
    //UI 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class DeactivatedState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public DeactivatedState(AkaneHP akaneHP) : base(akaneHP) { }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] frameEffects = new IEnumerator[]
            {
                UIAnimationCommons.FadeOutAlpha(_akaneHP.Frame, 0.25f)
            };
            IEnumerator[] hpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.FadeOutAlpha(_akaneHP.Progress, 0.25f)
            };

            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Frame.ApplyEffectsInParallel(frameEffects)));
            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Progress.ApplyEffectsInParallel(hpProgressEffects)));

            foreach (Coroutine coroutine in _akaneHP._projectingCoroutineList)
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
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public ActivatedState(AkaneHP akaneHP) : base(akaneHP) { }
        public override void OnUpdateProjection()
        {
            _akaneHP.NormalHPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] frameEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneHP.Frame, 0.5f, 24.0f),
                UIAnimationCommons.FadeInAlpha(_akaneHP.Frame, 1.0f)
            };
            IEnumerator[] hpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneHP.Progress, 0.5f, 24.0f),
                UIAnimationCommons.FadeInAlpha(_akaneHP.Progress, 1.0f)
            };

            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Frame.ApplyEffectsInParallel(frameEffects)));
            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Progress.ApplyEffectsInParallel(hpProgressEffects)));

            foreach (Coroutine coroutine in _akaneHP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];

            return null;
        }
    }
    private class IdleState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public IdleState(AkaneHP akaneHP) : base(akaneHP) { }
        public override void OnUpdateProjection()
        {
            _akaneHP.NormalHPDataUpdate();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Underattacked];
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
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public UnderattackedState(AkaneHP akaneHP) : base(akaneHP) { }
        public override void OnUpdateProjection()
        {
            _akaneHP.NormalHPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] frameEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveDownOffsetToPosition(_akaneHP.Frame, 0.45f, 2.0f)
            };
            IEnumerator[] hpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddShakeOffsetToPosition(_akaneHP.Progress, 0.5f, 4.0f),
                UIAnimationCommons.FlickAlpha(_akaneHP.Progress, 0.5f, 1.0f, 4)
            };

            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Frame.ApplyEffectsInParallel(frameEffects)));
            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Progress.ApplyEffectsInParallel(hpProgressEffects)));

            foreach (Coroutine coroutine in _akaneHP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];

            return null;
        }
    }
    private class LifetappingState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public LifetappingState(AkaneHP akaneHP) : base(akaneHP) { }
        public override void OnUpdateProjection()
        {
            _akaneHP.NormalHPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] frameEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveDownOffsetToPosition(_akaneHP.Frame, 0.45f, 4.0f)
            };
            IEnumerator[] hpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveDownOffsetToPosition(_akaneHP.Progress, 0.5f, 4.0f),
                UIAnimationCommons.FlickAlpha(_akaneHP.Progress, 0.25f, 0.5f, 2)
            };

            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Frame.ApplyEffectsInParallel(frameEffects)));
            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Progress.ApplyEffectsInParallel(hpProgressEffects)));

            foreach (Coroutine coroutine in _akaneHP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];

            return null;
        }
    }
    private class LifestealingState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public LifestealingState(AkaneHP akaneHP) : base(akaneHP) { }
        public override void OnUpdateProjection()
        {
            _akaneHP.NormalHPDataUpdate();
        }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] frameEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveUpOffsetToPosition(_akaneHP.Frame, 0.3f, 1.0f)
            };
            IEnumerator[] hpProgressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddWaveUpOffsetToPosition(_akaneHP.Progress, 0.27f, 1.0f),
                UIAnimationCommons.FlickAlpha(_akaneHP.Progress, 0.1f, 0.2f, 1)
            };

            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Frame.ApplyEffectsInParallel(frameEffects)));
            _akaneHP._projectingCoroutineList.Add(_akaneHP.StartCoroutine(_akaneHP.Progress.ApplyEffectsInParallel(hpProgressEffects)));

            foreach (Coroutine coroutine in _akaneHP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.Hyperfailed:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Underattacked];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];

            return null;
        }
    }
}