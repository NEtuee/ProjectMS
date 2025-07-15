using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AkaneHP : ProjectorUI
{
    [SerializeField] private Image FrameImage;
    [SerializeField] private Image HPProgressImage;
    //위의 이미지마다 basePosition?
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
        HPProgressImage.fillAmount = ProjectingData.HPPercentage;
    }


    // 투영 데이터 업데이트
    private void HPLerpUpdate()
    {
        float updatedHPPercentage = Mathf.Lerp(ProjectingData.HPPercentage, ReceivedData.HPPercentage, Time.deltaTime * 10.0f);
        float updatedChangeAmount = 0.0f;
        AkaneHPData updatedProjectingData = new AkaneHPData(updatedHPPercentage, updatedChangeAmount);
        ProjectingData = updatedProjectingData;
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
        public override UIState UpdateState()
        {
            return null;
        }
        public override UIState ChangeState(SubUIData subData) //외부에서 실행? 아카네와 관련없는건 매 프레임 체크해야되니 패스,, 아닌가? OnUpdate에서도 처리 가능하지 않은지
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
            yield return _akaneHP.StartCoroutine(UIAnimationCommons.WaitB());
            //Debug.Log("Lifetapping Enter Entered");
            //코루틴 리스트를 한꺼번에 묶기?
            //List<string, IEnumerator> onEnterCoroutines
            //Debug.Log("Lifetapping Enter Ended");
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
            yield return _akaneHP.StartCoroutine(UIAnimationCommons.WaitA());
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
        public override IEnumerator OnExit()
        {
            yield return _akaneHP.StartCoroutine(UIAnimationCommons.Float(_akaneHP.HPProgressImage));
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