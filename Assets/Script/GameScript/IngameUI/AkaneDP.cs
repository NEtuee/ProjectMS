using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDP : ProjectorUI
{
    protected override IPackedUIData _dataStruct => new SingleAkaneDPData();
    public struct SingleAkaneDPData : IPackedUIData
    {
        public readonly UIDataType UIDataType => UIDataType.AkaneDP;
        public bool IsActivated;
        public bool IsWaiting;
        public float CooldownPercentage;
        public bool JustConsumed; //줄어드는 순간 잠깐 true로 바뀜
        public SingleAkaneDPData(bool isActivated = true, bool isWaiting = true, float cooldownPercentage = -1.0f, bool justConsumed = false)
        {
            this.IsActivated = isActivated;
            this.IsWaiting = isWaiting;
            this.CooldownPercentage = cooldownPercentage;
            this.JustConsumed = justConsumed;
        }
    }
    public new SingleAkaneDPData ReceivedData => (SingleAkaneDPData)_receivedData;
    public new SingleAkaneDPData ProjectingData
    {
        get => (SingleAkaneDPData)_projectingData;
        set => _projectingData = value;
    }
    protected override IReadOnlyCollection<UIEventKey> _validEventKeys =>
        new[] { UIEventKey.Attacked, UIEventKey.Kicked, UIEventKey.Evaded };
    public override IReadOnlyCollection<UIVisualModule> UIVisualModules =>
        new[] { DP, Progress, ProgressFrame };
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPData;
    public UIVisualModule DP; //프리팹의 최상위는 public으로,,
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPProgressData;
    private UIVisualModule Progress;
    [SerializeField] private UIVisualModuleData<AkaneDPStateType> DPProgressFrameData;
    private UIVisualModule ProgressFrame;
    public enum AkaneDPStateType
    {
        Deactivated,
        Activated,
        Idle,
        Waiting,
        Consumed,
        Autorecovering,
        Attackrecovering,
        Kickrecovering,
        Evaderecovering
    }
    private Dictionary<AkaneDPStateType, UIState> _akaneDPStateMap;
    protected override IDictionary<Enum, UIState> _stateMap =>
        (IDictionary<Enum, UIState>)_akaneDPStateMap;


    //공통 메서드
    protected override void SetDataConstructor()
    {
        _receivedData = new SingleAkaneDPData();
        _projectingData = new SingleAkaneDPData();
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();
    }
    protected override void SetStateMap()
    {
        _stateMachine = new UIStateMachine(this);

        _akaneDPStateMap = new Dictionary<AkaneDPStateType, UIState>()
        {
            { AkaneDPStateType.Deactivated, new DeactivatedState(this) },
            { AkaneDPStateType.Activated, new ActivatedState(this) },
            { AkaneDPStateType.Idle, new IdleState(this) },
            { AkaneDPStateType.Waiting, new WaitingState(this) },
            { AkaneDPStateType.Consumed, new ConsumedState(this) },
            { AkaneDPStateType.Autorecovering, new AutorecoveringState(this) },
            { AkaneDPStateType.Attackrecovering, new AttackrecoveringState(this) },
            { AkaneDPStateType.Kickrecovering, new KickrecoveringState(this) },
            { AkaneDPStateType.Evaderecovering, new EvaderecoveringState(this) }
        };
    }
    protected override void SetUIVisualModule()
    {
        _projectingCoroutineList = new List<Coroutine>();

        DP = gameObject.AddComponent<UIVisualModule>();
        Progress = gameObject.AddComponent<UIVisualModule>();
        ProgressFrame = gameObject.AddComponent<UIVisualModule>();

        foreach (UIVisualModule uiVisualModule in UIVisualModules)
        {
            uiVisualModule.Initialize();
        }

        DP.SetFromData<AkaneDPStateType>(DPData);
        Progress.SetFromData<AkaneDPStateType>(DPProgressData);
        ProgressFrame.SetFromData<AkaneDPStateType>(DPProgressFrameData);
    }
    public override void Activate()
    {
        gameObject.SetActive(true);
        _stateMachine.ForceStateChanging(_akaneDPStateMap[AkaneDPStateType.Activated]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_akaneDPStateMap[AkaneDPStateType.Deactivated]);
        gameObject.SetActive(false);
    }


    //데이터 투영
    private void UpdateAkaneDPProjection()
    {
        Progress.Image.fillAmount = ReceivedData.CooldownPercentage;
    }
    //데이터 수정

    //UI 애니메이션 메서드 및 코루틴

    //UIState 정의
    private class DeactivatedState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public DeactivatedState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            foreach (UIVisualModule uiVisualModule in _akaneDP.UIVisualModules)
            {
                uiVisualModule.HideImage();
                uiVisualModule.BackToBasePosition();
            }

            yield break;
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
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public ActivatedState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            IEnumerator[] dpEffects = new IEnumerator[]
            {
                    UIAnimationCommons.FadeInAlpha(_akaneDP.DP, 0.5f)
            };

            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.DP.ApplyEffectsInParallel(dpEffects)));

            foreach (Coroutine coroutine in _akaneDP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];

            return null;
        }
    }
    private class IdleState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public IdleState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            _akaneDP.DP.ShowImage();
            _akaneDP.Progress.HideImage();
            _akaneDP.ProgressFrame.HideImage();

            _akaneDP.DP.ChangeAnimation(Convert.ToInt32(AkaneDPStateType.Idle));

            IEnumerator[] dpEffects = new IEnumerator[]
            {
                UIAnimationCommons.LerpToBasePosition(_akaneDP.DP, 0.1f),
                UIAnimationCommons.FlickAlpha(_akaneDP.DP, 0.1f, 0.2f, 1)
            };

            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.DP.ApplyEffectsInParallel(dpEffects)));

            foreach (Coroutine coroutine in _akaneDP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            //Idle도 UIEventKey 적용? (상황별 Recovering)
            return null;
        }
        public override UIState UpdateState()
        {
            if (_akaneDP.ReceivedData.IsActivated == false)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Consumed];
            else if (_akaneDP.ReceivedData.IsWaiting == true)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Waiting];

            return null;
        }
    }
    private class WaitingState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public WaitingState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            _akaneDP.DP.ChangeAnimation(Convert.ToInt32(AkaneDPStateType.Waiting));

            IEnumerator[] dpEffects = new IEnumerator[]
            {
                    UIAnimationCommons.FlickAlpha(_akaneDP.DP, 0.05f, 0.8f, 1)
            };

            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.DP.ApplyEffectsInParallel(dpEffects)));

            foreach (Coroutine coroutine in _akaneDP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_akaneDP.ReceivedData.IsActivated == false)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Consumed];
            else if (_akaneDP.ReceivedData.IsWaiting == false)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];

            return null;
        }
    }
    private class ConsumedState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public ConsumedState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            //DP 위치가 변하면 Progress와 Frame도 변하므로 Progress와 Frame만 따로 offset 추가
            _akaneDP.DP.ChangeAnimation(Convert.ToInt32(AkaneDPStateType.Idle));

            IEnumerator[] dpEffects = new IEnumerator[]
            {
                UIAnimationCommons.FadeOutAlpha(_akaneDP.DP, 0.1f),
            };
            IEnumerator[] progressEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneDP.Progress, 0.5f, 8.0f),
                UIAnimationCommons.FadeOutAlpha(_akaneDP.Progress, 0.15f)
            };
            IEnumerator[] progressFrameEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneDP.ProgressFrame, 0.5f, 8.0f),
                UIAnimationCommons.FadeOutAlpha(_akaneDP.ProgressFrame, 0.15f)
            };

            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.DP.ApplyEffectsInParallel(dpEffects)));
            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.Progress.ApplyEffectsInParallel(progressEffects)));
            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.ProgressFrame.ApplyEffectsInParallel(progressFrameEffects)));

            foreach (Coroutine coroutine in _akaneDP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Autorecovering];
            else if (!_onEntered && _akaneDP.ReceivedData.JustConsumed)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Consumed];

            return null;
        }
    }
    private class AutorecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public AutorecoveringState(AkaneDP akaneDP) : base(akaneDP) { }
        public override void OnUpdateProjection()
        {
            _akaneDP.UpdateAkaneDPProjection();
        }
        protected override IEnumerator OnEnterProjection()
        {
            _akaneDP.Progress.BackToBasePosition();
            _akaneDP.ProgressFrame.BackToBasePosition();
            _akaneDP.Progress.ShowImage();
            
            IEnumerator[] dpEffects = new IEnumerator[]
            {
                UIAnimationCommons.AddFloatOffsetToPosition(_akaneDP.DP, 0.75f, 8.0f)
            };
            IEnumerator[] progressEffects = new IEnumerator[]
            {
                    UIAnimationCommons.FlickAlpha(_akaneDP.Progress, 0.5f, 1.0f, 4)
            };
            IEnumerator[] progressFrameEffects = new IEnumerator[]
            {
                    UIAnimationCommons.FadeInAlpha(_akaneDP.ProgressFrame, 0.25f)
            };

            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.DP.ApplyEffectsInParallel(dpEffects)));
            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.Progress.ApplyEffectsInParallel(progressEffects)));
            _akaneDP._projectingCoroutineList.Add(_akaneDP.StartCoroutine(_akaneDP.ProgressFrame.ApplyEffectsInParallel(progressFrameEffects)));

            foreach (Coroutine coroutine in _akaneDP._projectingCoroutineList)
                yield return coroutine;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_akaneDP.ReceivedData.IsActivated)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];
            else
                if (_akaneDP.ReceivedData.JustConsumed)
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Consumed];

            return null;
        }
    }
    private class AttackrecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public AttackrecoveringState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            //IsWaiting에 따라 다른 애니메이션 재생
            foreach (UIVisualModule uiVisualModule in _akaneDP.UIVisualModules)
            {
                uiVisualModule.HideImage();
                uiVisualModule.BackToBasePosition();
            }
            _akaneDP.DP.ShowImage();

            yield return null;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];

            return null;
        }
    }
    private class KickrecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public KickrecoveringState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            foreach (UIVisualModule uiVisualModule in _akaneDP.UIVisualModules)
            {
                uiVisualModule.HideImage();
                uiVisualModule.BackToBasePosition();
            }
            _akaneDP.DP.ShowImage();

            yield break;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];

            return null;
        }
    }
    private class EvaderecoveringState : UIState
    {
        private AkaneDP _akaneDP => (AkaneDP)_projectorUI;
        public EvaderecoveringState(AkaneDP akaneDP) : base(akaneDP) { }
        protected override IEnumerator OnEnterProjection()
        {
            foreach (UIVisualModule uiVisualModule in _akaneDP.UIVisualModules)
            {
                uiVisualModule.HideImage();
                uiVisualModule.BackToBasePosition();
            }
            _akaneDP.DP.ShowImage();

            yield break;
        }
        public override UIState ChangeState(SubUIData subData)
        {
            var eventKey = subData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.Attacked or UIEventKey.Hyperattacked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Attackrecovering];
                case UIEventKey.Kicked:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Kickrecovering];
                case UIEventKey.Evaded:
                    return _akaneDP._akaneDPStateMap[AkaneDPStateType.Evaderecovering];
                default:
                    break;
            }

            return null;
        }
        public override UIState UpdateState()
        {
            if (_onEntered)
                return _akaneDP._akaneDPStateMap[AkaneDPStateType.Idle];

            return null;
        }
    }
}