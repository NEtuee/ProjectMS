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
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded, UIEventKey.Hit };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
    new[] { Frame, HPProgress };
    [SerializeField] private UIVisualModuleData<AkaneHPStateType> FrameData;
    private UIVisualModule Frame;
    [SerializeField] private UIVisualModuleData<AkaneHPStateType> HPProgressData;
    private UIVisualModule HPProgress;
    public enum AkaneHPStateType
    {
        NONE,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneHPStateType, UIState> _akaneHPStateMap = new Dictionary<AkaneHPStateType, UIState>();
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneHPStateMap;


    //공통 메서드    
    protected override void SetInitialConstructor()
    {
        _receivedData = new AkaneHPData();
        _projectingData = new AkaneHPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();

        Frame = new UIVisualModule();
        HPProgress = new UIVisualModule();
    }
    protected override void SetStateMap()
    {
        _akaneHPStateMap.Add(AkaneHPStateType.NONE, new NONE(this));
        _akaneHPStateMap.Add(AkaneHPStateType.Idle, new IdleState(this));
        _akaneHPStateMap.Add(AkaneHPStateType.Underattacked, new UnderattackedState(this));
        _akaneHPStateMap.Add(AkaneHPStateType.Lifetapping, new LifetappingState(this));
        _akaneHPStateMap.Add(AkaneHPStateType.Lifestealing, new LifestealingState(this));
    }
    protected override void SetUIVisualModule()
    {
        Frame.SetFromData<AkaneHPStateType>(FrameData);
        HPProgress.SetFromData<AkaneHPStateType>(HPProgressData);

        List<IEnumerator> effectList = new List<IEnumerator>();
        effectList.Add(UIAnimationCommons.FadeOutAlpha(Frame, 0.0f));
        effectList.Add(UIAnimationCommons.FadeOutAlpha(HPProgress, 0.0f));
        _uiEffectManager.RegisterEffectList(this, effectList);
        _uiEffectManager.WaitAllListedEffects();
    }
    public override void Activate()
    {
        if (_memorySavingCoroutine != null)
            StopCoroutine(_memorySavingCoroutine);

        gameObject.SetActive(true);
        _stateMachine.ForceStateChanging(_akaneHPStateMap[AkaneHPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_akaneHPStateMap[AkaneHPStateType.NONE]);
        _memorySavingCoroutine = StartCoroutine(MemorySavingCoroutine());
    }


    //투영 데이터 업데이트 메서드 및 코루틴
    private void HPProjectionUpdate()
    {
        HPProgress.Image.fillAmount = ProjectingData.HPPercentage;
    }
    private void HPLerpUpdate()
    {
        float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 50.0f);
        float updatedChangeAmount = ReceivedData.ChangeAmount;
        AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;

        HPProjectionUpdate();
    }
    private IEnumerator OpeningHPLerpCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        float initialHPPercentage = ProjectingData.HPPercentage;
        float targetHPPercentage = ReceivedData.HPPercentage;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(MathEx.EaseType.EaseInCubic, 0.0f, 1.0f, t);

            float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, easedT);
            float updatedChangeAmount = 0.0f;
            AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            HPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        HPProjectionUpdate();
    }
    private IEnumerator NormalHPLerpCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        float initialHPPercentage = ProjectingData.HPPercentage;
        float targetHPPercentage = ReceivedData.HPPercentage;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 50.0f);
            float updatedChangeAmount = 0.0f;
            AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            HPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        HPProjectionUpdate();
    }
    private IEnumerator HPBackLerpCoroutine(float duration)
    {
        yield return null;
    }
    //전용 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class IdleState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public IdleState(AkaneHP akaneHP) : base(akaneHP) {}
        public override void OnUpdate()
        {
            _akaneHP.HPLerpUpdate();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.AttackSucceeded:
                    return _akaneHP._akaneHPStateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.HyperFailed:
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
        public UnderattackedState(AkaneHP akaneHP) : base(akaneHP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.WaveDownPosition(_akaneHP.Frame, 0.45f, 2.0f));
            effectList.Add(UIAnimationCommons.ShakePosition(_akaneHP.HPProgress, 0.5f, 4.0f));
            effectList.Add(UIAnimationCommons.FlickAlpha(_akaneHP.HPProgress, 0.5f, 1.0f, 4));
            effectList.Add(_akaneHP.NormalHPLerpCoroutine(0.1f));

            _akaneHP._uiEffectManager.RegisterEffectList(_akaneHP, effectList);
            yield return _akaneHP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];
        }
    }
    private class LifetappingState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public LifetappingState(AkaneHP akaneHP) : base(akaneHP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.WaveDownPosition(_akaneHP.Frame, 0.45f, 4.0f));
            effectList.Add(UIAnimationCommons.WaveDownPosition(_akaneHP.HPProgress, 0.5f, 4.0f));
            effectList.Add(UIAnimationCommons.FlickAlpha(_akaneHP.HPProgress, 0.25f, 0.5f, 2));
            effectList.Add(_akaneHP.NormalHPLerpCoroutine(0.1f));

            _akaneHP._uiEffectManager.RegisterEffectList(_akaneHP, effectList);
            yield return _akaneHP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];
        }
    }
    private class LifestealingState : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public LifestealingState(AkaneHP akaneHP) : base(akaneHP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.WaveUpPosition(_akaneHP.Frame, 0.3f, 1.0f));
            effectList.Add(UIAnimationCommons.WaveUpPosition(_akaneHP.HPProgress, 0.27f, 1.0f));
            effectList.Add(UIAnimationCommons.FlickAlpha(_akaneHP.HPProgress, 0.1f, 0.2f, 1));
            effectList.Add(_akaneHP.NormalHPLerpCoroutine(0.25f));

            _akaneHP._uiEffectManager.RegisterEffectList(_akaneHP, effectList);
            yield return _akaneHP._uiEffectManager.WaitAllListedEffects();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _akaneHP._akaneHPStateMap[AkaneHPStateType.Idle];
        }
    }
    private class NONE : UIState
    {
        private AkaneHP _akaneHP => (AkaneHP)_projectorUI;
        public NONE(AkaneHP akaneHP) : base(akaneHP) {}
        public override IEnumerator OnEnter()
        {
            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.FadeOutAlpha(_akaneHP.Frame, 0.25f));
            effectList.Add(UIAnimationCommons.FadeOutAlpha(_akaneHP.HPProgress, 0.25f));

            _akaneHP._uiEffectManager.RegisterEffectList(_akaneHP, effectList);
            yield return _akaneHP._uiEffectManager.WaitAllListedEffects();
        }
        public override IEnumerator OnExit()
        {
            UIAnimationCommons.FadeInAlpha(_akaneHP.Frame, 0.0f);
            UIAnimationCommons.FadeInAlpha(_akaneHP.HPProgress, 0.0f);

            List<IEnumerator> effectList = new List<IEnumerator>();
            effectList.Add(UIAnimationCommons.FloatPosition(_akaneHP.Frame, 0.5f, 24.0f));
            effectList.Add(UIAnimationCommons.FadeInAlpha(_akaneHP.Frame, 1.0f));
            effectList.Add(UIAnimationCommons.FloatPosition(_akaneHP.HPProgress, 0.5f, 24.0f));
            effectList.Add(UIAnimationCommons.FadeInAlpha(_akaneHP.HPProgress, 1.0f));
            effectList.Add(_akaneHP.OpeningHPLerpCoroutine(1.0f));

            _akaneHP._uiEffectManager.RegisterEffectList(_akaneHP, effectList);
            yield return _akaneHP._uiEffectManager.WaitAllListedEffects();
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