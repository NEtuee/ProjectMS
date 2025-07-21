using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneHP : ProjectorUI
{
    [SerializeField] private Image FrameImage;
    [SerializeField] private Image HPProgressImage;
    [SerializeField] private Image HPBackProgressImage;
    protected override UIDataType _dataType => UIDataType.AkaneHP;
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
    private enum AkaneHPStateType
    {
        NONE,
        Idle,
        Underattacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneHPStateType, UIState> _stateMap = new Dictionary<AkaneHPStateType, UIState>();


    //공통 메서드    
    public override void Initialize()
    {
        base.PrepareInitialize();

        _receivedData = new AkaneHPData();
        _projectingData = new AkaneHPData();

        _stateMap.Add(AkaneHPStateType.NONE, new NONE(this));
        _stateMap.Add(AkaneHPStateType.Idle, new IdleState(this));
        _stateMap.Add(AkaneHPStateType.Underattacked, new UnderattackedState(this));
        _stateMap.Add(AkaneHPStateType.Lifetapping, new LifetappingState(this));
        _stateMap.Add(AkaneHPStateType.Lifestealing, new LifestealingState(this));

        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneHPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.ForceStateChanging(_stateMap[AkaneHPStateType.NONE]);
    }


    //투영 데이터 업데이트 메서드 및 코루틴
    private void HPProjectionUpdate()
    {
        HPProgressImage.fillAmount = ProjectingData.HPPercentage;
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
    private IEnumerator PrivateWait()
    {
        yield return new WaitForSeconds(1.0f);
    }

    //UIState 정의
    private class IdleState : UIState
    {
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public IdleState(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
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
                    return _stateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.HyperFailed:
                    return _stateMap[AkaneHPStateType.Lifetapping];
                case UIEventKey.Hit:
                    return _stateMap[AkaneHPStateType.Underattacked];
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
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public UnderattackedState(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.WaveDownPosition(_akaneHP.FrameImage, 0.45f, 2.0f);
            IEnumerator coroutineB = UIAnimationCommons.ShakePosition(_akaneHP.HPProgressImage, 0.5f, 4.0f);
            IEnumerator coroutineC = UIAnimationCommons.FlickAlpha(_akaneHP.HPProgressImage, 0.5f, 1.0f, 4);
            IEnumerator coroutineD = _akaneHP.NormalHPLerpCoroutine(0.1f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);
            coroutineList.Add(coroutineD);

            _akaneHP._uiCoroutineManager.RegisterCoroutineList(_akaneHP, coroutineList);
            yield return _akaneHP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneHPStateType.Idle];
        }
    }
    private class LifetappingState : UIState
    {
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public LifetappingState(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.WaveDownPosition(_akaneHP.FrameImage, 0.45f, 4.0f);
            IEnumerator coroutineB = UIAnimationCommons.WaveDownPosition(_akaneHP.HPProgressImage, 0.5f, 4.0f);
            IEnumerator coroutineC = UIAnimationCommons.FlickAlpha(_akaneHP.HPProgressImage, 0.25f, 0.5f, 2);
            IEnumerator coroutineD = _akaneHP.NormalHPLerpCoroutine(0.1f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);
            coroutineList.Add(coroutineD);

            _akaneHP._uiCoroutineManager.RegisterCoroutineList(_akaneHP, coroutineList);
            yield return _akaneHP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneHPStateType.Idle];
        }
    }
    private class LifestealingState : UIState
    {
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public LifestealingState(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.WaveUpPosition(_akaneHP.FrameImage, 0.3f, 1.0f);
            IEnumerator coroutineB = UIAnimationCommons.WaveUpPosition(_akaneHP.HPProgressImage, 0.27f, 1.0f);
            IEnumerator coroutineC = UIAnimationCommons.FlickAlpha(_akaneHP.HPProgressImage, 0.1f, 0.2f, 1);
            
            IEnumerator coroutineD = _akaneHP.NormalHPLerpCoroutine(0.25f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);
            coroutineList.Add(coroutineD);

            _akaneHP._uiCoroutineManager.RegisterCoroutineList(_akaneHP, coroutineList);
            yield return _akaneHP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override UIState ChangeState(SubUIData subData)
        {
            return null;
        }
        public override UIState UpdateState()
        {
            return _stateMap[AkaneHPStateType.Idle];
        }
    }
    private class NONE : UIState
    {
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public NONE(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            //투명도 0으로
            yield return null;

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.FadeOutAlpha(_akaneHP.FrameImage, 0.25f);
            IEnumerator coroutineB = UIAnimationCommons.FadeOutAlpha(_akaneHP.HPProgressImage, 0.25f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);

            _akaneHP._uiCoroutineManager.RegisterCoroutineList(_akaneHP, coroutineList);
            yield return _akaneHP._uiCoroutineManager.WaitAllListedCoroutine();
        }
        public override IEnumerator OnExit()
        {
            //위로 떠올라서 원래 위치로
            //투명도 0에서 1로
            yield return null;
            UIAnimationCommons.FadeInAlpha(_akaneHP.FrameImage, 0.0f);
            UIAnimationCommons.FadeInAlpha(_akaneHP.HPProgressImage, 0.0f);

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.FloatPosition(_akaneHP.FrameImage, 0.5f, 24.0f);
            IEnumerator coroutineB = UIAnimationCommons.FadeInAlpha(_akaneHP.FrameImage, 1.0f);
            IEnumerator coroutineC = UIAnimationCommons.FloatPosition(_akaneHP.HPProgressImage, 0.5f, 24.0f);
            IEnumerator coroutineD = UIAnimationCommons.FadeInAlpha(_akaneHP.HPProgressImage, 1.0f);
            IEnumerator coroutineE = _akaneHP.OpeningHPLerpCoroutine(1.0f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);
            coroutineList.Add(coroutineC);
            coroutineList.Add(coroutineD);
            coroutineList.Add(coroutineE);

            _akaneHP._uiCoroutineManager.RegisterCoroutineList(_akaneHP, coroutineList);
            yield return _akaneHP._uiCoroutineManager.WaitAllListedCoroutine();
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