using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    protected override UIDataType DataType => UIDataType.AkaneHP;
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
    protected override IReadOnlyCollection<UIEventKey> ValidEventKeys { get; } =
        new[] { UIEventKey.HyperFailed, UIEventKey.AttackSucceeded };
    private enum AkaneHPStateType
    {
        NONE,
        Idle,
        UnderAttacked,
        Lifetapping,
        Lifestealing
    }
    private Dictionary<AkaneHPStateType, UIState> _stateMap = new Dictionary<AkaneHPStateType, UIState>();



    public override void Initialize()
    {
        base.Initialize();

        _receivedData = new AkaneHPData();
        _receivedSubData = new SubUIData();
        _projectingData = new AkaneHPData();
        _projectingSubData = new SubUIData();

        _stateMap.Add(AkaneHPStateType.NONE, new NONE(this));
        _stateMap.Add(AkaneHPStateType.Idle, new IdleState(this));
        _stateMap.Add(AkaneHPStateType.UnderAttacked, new UnderAttackedState(this));
        _stateMap.Add(AkaneHPStateType.Lifetapping, new LifetappingState(this));
        _stateMap.Add(AkaneHPStateType.Lifestealing, new LifestealingState(this));
        
        Deactivate();
    }
    public override void Activate()
    {
        _stateMachine.RequestStateChanging(_stateMap[AkaneHPStateType.Idle]);
    }
    public override void Deactivate()
    {
        _stateMachine.RequestStateChanging(_stateMap[AkaneHPStateType.NONE]);
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


    //UIState
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
        public override UIState ChangeState()
        {
            var eventKey = UIEventKey.NONE; //_akaneHP.ReceivedSubData.UIEventKey;
            switch (eventKey)
            {
                case UIEventKey.AttackSucceeded:
                    return _stateMap[AkaneHPStateType.Lifestealing];
                case UIEventKey.HyperFailed:
                    return _stateMap[AkaneHPStateType.Lifetapping];
                default:
                    break;
            }
            //if (_akaneHP.ReceivedData.ChangeAmount < 0.0f)
            //    return _stateMap[AkaneHPStateType.UnderAttacked];

            return this;
        }
    }
    private class UnderAttackedState : UIState
    {
        private readonly AkaneHP _akaneHP;
        private readonly Dictionary<AkaneHPStateType, UIState> _stateMap;
        public UnderAttackedState(AkaneHP akaneHP)
        {
            _akaneHP = akaneHP;
            _stateMap = akaneHP._stateMap;
        }
        public override IEnumerator OnEnter()
        {
            _akaneHP.StartCoroutine(UIAnimationCommons.Shake(_akaneHP.HPProgressImage.rectTransform));
            yield break;
        }
        public override void OnUpdate()
        {
            
        }
        public override UIState ChangeState()
        {
            return this;
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
        public override UIState ChangeState()
        {
            return this;
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
        public override UIState ChangeState()
        {
            return this;
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
            _akaneHP.StartCoroutine(UIAnimationCommons.Float(_akaneHP.FrameImage.rectTransform));
            _akaneHP.StartCoroutine(UIAnimationCommons.Float(_akaneHP.HPProgressImage.rectTransform));
            yield break;
        }
        public override UIState ChangeState()
        {
            return this;
        }
    }
}