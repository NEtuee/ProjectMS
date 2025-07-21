using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneBP : ProjectorUI
{
    [SerializeField] private Image BPProgressImage;
    protected override UIDataType _dataType => UIDataType.AkaneBP;
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
    private enum AkaneBPStateType
    {
        NONE,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneBPStateType, UIState> _stateMap = new Dictionary<AkaneBPStateType, UIState>();


    //공통 메서드
    public override void Initialize()
    {
        base.PrepareInitialize();

        _receivedData = new AkaneBPData();
        _projectingData = new AkaneBPData();

        _stateMap.Add(AkaneBPStateType.NONE, new NONE(this));
        _stateMap.Add(AkaneBPStateType.Idle, new IdleState(this));
        _stateMap.Add(AkaneBPStateType.Underattacked, new UnderattackedState(this));
        _stateMap.Add(AkaneBPStateType.Lifetapping, new LifetappingState(this));
        _stateMap.Add(AkaneBPStateType.Lifestealing, new LifestealingState(this));

        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneBPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneBPStateType.NONE]);
    }


    //투영 데이터 업데이트
    private void BPProjectionUpdate()
    {
        BPProgressImage.fillAmount = ProjectingData.BPPercentage;
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
        private readonly AkaneBP _akaneBP;
        private readonly Dictionary<AkaneBPStateType, UIState> _stateMap;
        public IdleState(AkaneBP akaneBP)
        {
            _akaneBP = akaneBP;
            _stateMap = akaneBP._stateMap;
        }
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
                    return _stateMap[AkaneBPStateType.Lifestealing];
                case UIEventKey.HyperFailed:
                    return _stateMap[AkaneBPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _stateMap[AkaneBPStateType.Underattacked];
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
        private readonly AkaneBP _akaneBP;
        private readonly Dictionary<AkaneBPStateType, UIState> _stateMap;
        public UnderattackedState(AkaneBP akaneBP)
        {
            _akaneBP = akaneBP;
            _stateMap = akaneBP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = _akaneBP.DirectBPLerpCoroutine(1.0f);
            coroutineList.Add(coroutineA);

            _akaneBP._uiCoroutineManager.RegisterCoroutineList(_akaneBP, coroutineList);
            yield return _akaneBP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneBPStateType.Idle];
        }
    }
    private class LifetappingState : UIState
    {
        private readonly AkaneBP _akaneBP;
        private readonly Dictionary<AkaneBPStateType, UIState> _stateMap;
        public LifetappingState(AkaneBP akaneBP)
        {
            _akaneBP = akaneBP;
            _stateMap = akaneBP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.WaveDownPosition(_akaneBP.BPProgressImage, 0.5f, 4.0f);
            IEnumerator coroutineB = UIAnimationCommons.FlickAlpha(_akaneBP.BPProgressImage, 0.5f, 1.0f, 4);
            IEnumerator coroutineC = _akaneBP.DirectBPLerpCoroutine(1.0f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);

            _akaneBP._uiCoroutineManager.RegisterCoroutineList(_akaneBP, coroutineList);
            yield return _akaneBP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneBPStateType.Idle];
        }
    }
    private class LifestealingState : UIState
    {
        private readonly AkaneBP _akaneBP;
        private readonly Dictionary<AkaneBPStateType, UIState> _stateMap;
        public LifestealingState(AkaneBP akaneBP)
        {
            _akaneBP = akaneBP;
            _stateMap = akaneBP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.WaveUpPosition(_akaneBP.BPProgressImage, 0.3f, 1.0f);
            IEnumerator coroutineB = UIAnimationCommons.FlickAlpha(_akaneBP.BPProgressImage, 0.1f, 0.2f, 1);
            IEnumerator coroutineC = _akaneBP.NormalBPLerpCoroutine(0.25f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);

            _akaneBP._uiCoroutineManager.RegisterCoroutineList(_akaneBP, coroutineList);
            yield return _akaneBP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneBPStateType.Idle];
        }
    }
    private class NONE : UIState
    {
        private readonly AkaneBP _akaneBP;
        private readonly Dictionary<AkaneBPStateType, UIState> _stateMap;
        public NONE(AkaneBP akaneBP)
        {
            _akaneBP = akaneBP;
            _stateMap = akaneBP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.FadeOutAlpha(_akaneBP.BPProgressImage, 0.25f);
            coroutineList.Add(coroutineA);

            _akaneBP._uiCoroutineManager.RegisterCoroutineList(_akaneBP, coroutineList);
            yield return _akaneBP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override IEnumerator OnExit()
        {
            yield return null;
            UIAnimationCommons.FadeInAlpha(_akaneBP.BPProgressImage, 0.0f);

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.FloatPosition(_akaneBP.BPProgressImage, 0.5f, 24.0f);
            IEnumerator coroutineB = UIAnimationCommons.FadeInAlpha(_akaneBP.BPProgressImage, 1.0f);
            IEnumerator coroutineC = _akaneBP.OpeningBPLerpCoroutine(1.0f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);

            _akaneBP._uiCoroutineManager.RegisterCoroutineList(_akaneBP, coroutineList);
            yield return _akaneBP._uiCoroutineManager.WaitAllListedCoroutine();
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