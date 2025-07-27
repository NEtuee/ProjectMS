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
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded, UIEventKey.Hit };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
        new[] { BPProgress };
    [SerializeField] private UIVisualModuleData<AkaneBPStateType> BPProgressData;
    private UIVisualModule BPProgress;
    public enum AkaneBPStateType
    {
        NONE,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneBPStateType, UIState> _akaneBPStateMap = new Dictionary<AkaneBPStateType, UIState>();
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneBPStateMap;


    //공통 메서드
    protected override void SetInitialConstructor()
    {
        _receivedData = new AkaneBPData();
        _projectingData = new AkaneBPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();

        BPProgress = new UIVisualModule();
    }
    protected override void SetStateMap()
    {
        _akaneBPStateMap.Add(AkaneBPStateType.NONE, new NONE(this));
        _akaneBPStateMap.Add(AkaneBPStateType.Idle, new IdleState(this));
        _akaneBPStateMap.Add(AkaneBPStateType.Underattacked, new UnderattackedState(this));
        _akaneBPStateMap.Add(AkaneBPStateType.Lifetapping, new LifetappingState(this));
        _akaneBPStateMap.Add(AkaneBPStateType.Lifestealing, new LifestealingState(this));
    }
    protected override void SetUIVisualModule()
    {
        BPProgress.SetFromData<AkaneBPStateType>(BPProgressData);

        List<IEnumerator> effectList = new List<IEnumerator>();
        effectList.Add(UIAnimationCommons.FadeOutAlpha(BPProgress, 0.0f));
        _uiEffectManager.RegisterEffectList(this, effectList);
        _uiEffectManager.WaitAllListedEffects();
    }
    public override void Activate()
    {
        if (_memorySavingCoroutine != null)
            StopCoroutine(_memorySavingCoroutine);

        gameObject.SetActive(true);
        _stateMachine.ForceStateChanging(_akaneBPStateMap[AkaneBPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_akaneBPStateMap[AkaneBPStateType.NONE]);
        _memorySavingCoroutine = StartCoroutine(MemorySavingCoroutine());
    }


    //투영 데이터 업데이트
    private void BPProjectionUpdate()
    {
        BPProgress.Image.fillAmount = ProjectingData.BPPercentage;
    }
    private void BPLerpUpdate()
    {
        float updatedBPPercentage = Mathf.Lerp(ProjectingData.BPPercentage, ReceivedData.BPPercentage, Time.deltaTime * 10.0f);
        float updatedChangeAmount = 0.0f;
        AkaneBPData updatedProjectingData = new AkaneBPData(updatedBPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;

        BPProjectionUpdate();
    }
    private IEnumerator OpeningBPLerpCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        float initialBPPercentage = ProjectingData.BPPercentage;
        float targetBPPercentage = ReceivedData.BPPercentage;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(MathEx.EaseType.EaseInCubic, 0.0f, 1.0f, t);

            float updatedBPPercentage = Mathf.Lerp(ProjectingData.BPPercentage, ReceivedData.BPPercentage, easedT);
            float updatedChangeAmount = 0.0f;
            AkaneBPData updatedProjectingData = new AkaneBPData(updatedBPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            BPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        BPProjectionUpdate();
    }
    private IEnumerator NormalBPLerpCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        float initialBPPercentage = ProjectingData.BPPercentage;
        float targetBPPercentage = ReceivedData.BPPercentage;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float updatedBPPercentage = Mathf.Lerp(ProjectingData.BPPercentage, ReceivedData.BPPercentage, Time.deltaTime * 50.0f);
            float updatedChangeAmount = 0.0f;
            AkaneBPData updatedProjectingData = new AkaneBPData(updatedBPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            BPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        BPProjectionUpdate();
    }
    private IEnumerator DirectBPLerpCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float updatedBPPercentage = ReceivedData.BPPercentage;
            float updatedChangeAmount = 0.0f;
            AkaneBPData updatedProjectingData = new AkaneBPData(updatedBPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            BPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        BPProjectionUpdate();
    }
    //UI 애니메이션 전용 메서드 및 코루틴
    private IEnumerator PrivateWait()
    {
        yield return new WaitForSeconds(1.0f);
    }

    //UIState 정의
    private class IdleState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public IdleState(AkaneBP akaneBP) : base(akaneBP) {}
        public override void OnUpdate()
        {
            _akaneBP.BPLerpUpdate();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.AttackSucceeded:
                    return _akaneBP._akaneBPStateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.HyperFailed:
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
        public UnderattackedState(AkaneBP akaneBP) : base(akaneBP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            IEnumerator coroutineA = _akaneBP.DirectBPLerpCoroutine(1.0f);
            effectList.Add(coroutineA);

            _akaneBP._uiEffectManager.RegisterEffectList(_akaneBP, effectList);
            yield return _akaneBP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];
        }
    }
    private class LifetappingState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public LifetappingState(AkaneBP akaneBP) : base(akaneBP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.WaveDownPosition(_akaneBP.BPProgress, 0.5f, 4.0f));
            effectList.Add(UIAnimationCommons.FlickAlpha(_akaneBP.BPProgress, 0.5f, 1.0f, 4));
            effectList.Add(_akaneBP.DirectBPLerpCoroutine(1.0f));

            _akaneBP._uiEffectManager.RegisterEffectList(_akaneBP, effectList);
            yield return _akaneBP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];
        }
    }
    private class LifestealingState : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public LifestealingState(AkaneBP akaneBP) : base(akaneBP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.WaveUpPosition(_akaneBP.BPProgress, 0.3f, 1.0f));
            effectList.Add(UIAnimationCommons.FlickAlpha(_akaneBP.BPProgress, 0.1f, 0.2f, 1));
            effectList.Add(_akaneBP.NormalBPLerpCoroutine(0.25f));

            _akaneBP._uiEffectManager.RegisterEffectList(_akaneBP, effectList);
            yield return _akaneBP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneBP._akaneBPStateMap[AkaneBPStateType.Idle];
        }
    }
    private class NONE : UIState
    {
        private AkaneBP _akaneBP => (AkaneBP)_projectorUI;
        public NONE(AkaneBP akaneBP) : base(akaneBP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.FadeOutAlpha(_akaneBP.BPProgress, 0.25f));

            _akaneBP._uiEffectManager.RegisterEffectList(_akaneBP, effectList);
            yield return _akaneBP._uiEffectManager.WaitAllListedEffects();
        }
        public override IEnumerator OnExit()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.FloatPosition(_akaneBP.BPProgress, 0.5f, 24.0f));
            effectList.Add(UIAnimationCommons.FadeInAlpha(_akaneBP.BPProgress, 1.0f));
            effectList.Add(_akaneBP.OpeningBPLerpCoroutine(1.0f));

            _akaneBP._uiEffectManager.RegisterEffectList(_akaneBP, effectList);
            yield return _akaneBP._uiEffectManager.WaitAllListedEffects();
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
}