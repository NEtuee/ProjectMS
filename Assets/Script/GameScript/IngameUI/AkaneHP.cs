using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;

public class AkaneHP : ProjectorUI
{
    [SerializeField] private Image FrameImage;
    [SerializeField] private Image HPProgressImage;
    public new AkaneHPData ReceivedData => (AkaneHPData)_receivedData;
    public new AkaneHPData ProjectingData
    {
        get => (AkaneHPData)_projectingData;
        set => _projectingData = value;
    }
    protected override UIDataType _dataType => UIDataType.AkaneHP;
    public struct AkaneHPData : IPackedUIData
    {
        public UIDataType UIDataType => UIDataType.AkaneHP;
        public float HPPercentage;
        public float ChangeAmount;

        public AkaneHPData(float hpPercentage = 0.0f, float changeAmount = 0.0f)
        {
            this.HPPercentage = hpPercentage;
            this.ChangeAmount = changeAmount;
        }

        public static bool operator ==(AkaneHPData left, AkaneHPData right) =>
            Mathf.Approximately(left.HPPercentage, right.HPPercentage) &&
            Mathf.Approximately(left.ChangeAmount, right.ChangeAmount);

        public static bool operator !=(AkaneHPData left, AkaneHPData right) => !(left == right);
        public override bool Equals(object obj) => obj is AkaneHPData other && this == other;
        public override int GetHashCode() => HPPercentage.GetHashCode() ^ ChangeAmount.GetHashCode();
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



    public override void Initialize()
    {
        base.Initialize();

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
    protected override void UpdateProjection()
    {
        base.UpdateProjection();
    }



    //투영 데이터 업데이트
    private void HPProjectionUpdate()
    {
        HPProgressImage.fillAmount = ProjectingData.HPPercentage;
    }
    private void HPLerpUpdate()
    {
        float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 10.0f);
        float updatedChangeAmount = 0.0f;
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
            float easedT = t * t * t;
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
            float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 10.0f);
            float updatedChangeAmount = 0.0f;
            AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
            ProjectingData = updatedProjectingData;

            HPProjectionUpdate();
            yield return null;
        }

        ProjectingData = ReceivedData;
        HPProjectionUpdate();
    }
    //UI 애니메이션 전용 메서드 및 코루틴
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
            UIAnimationCommons.InitializePosition(_akaneHP.FrameImage);
            UIAnimationCommons.InitializePosition(_akaneHP.HPProgressImage);

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.Wave(_akaneHP.HPProgressImage, 0.25f, 2.0f);
            IEnumerator coroutineB = _akaneHP.NormalHPLerpCoroutine(0.25f);
            coroutineList.Add(coroutineA);
            coroutineList.Add(coroutineB);

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
            IEnumerator coroutineA = UIAnimationCommons.FadeOut(_akaneHP.FrameImage, 0.25f);
            IEnumerator coroutineB = UIAnimationCommons.FadeOut(_akaneHP.HPProgressImage, 0.25f);
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

            List<IEnumerator> coroutineList = new List<IEnumerator>();
            IEnumerator coroutineA = UIAnimationCommons.Float(_akaneHP.FrameImage, 0.5f, 24.0f);
            IEnumerator coroutineB = UIAnimationCommons.Float(_akaneHP.HPProgressImage, 0.5f, 24.0f);
            IEnumerator coroutineC = UIAnimationCommons.FadeIn(_akaneHP.FrameImage, 1.0f);
            IEnumerator coroutineD = UIAnimationCommons.FadeIn(_akaneHP.HPProgressImage, 1.0f);
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