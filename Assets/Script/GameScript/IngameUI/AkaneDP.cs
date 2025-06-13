using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDP : ProjectorUI<AkaneDP.StateType>
{
    [SerializeField] private Image DPImage;
    public class SingleDPManagedData : IManagedData
    {
        public bool isCharged;
        public bool isReady;
        public SingleDPManagedData(bool isCharged, bool isReady) { this.isCharged = isCharged; this.isReady = isReady; }
    }
    public enum StateType { NONE, Opening, Idle, AutoRecovering, AttackRecovering, Consumed, Closing }



    public override bool CheckLinkedComponent()
    {
        return true;
    }
    public override void Initialize()
    {

    }
    public override void Activate()
    {
        
    }
    public override void Deactivate()
    {
        
    }
    public override void UpdateProjection(IManagedData data)
    {

    }



    private class OpeningState : UIState<StateType>
    {
        private AnimationCustomPreset _openingInfo;
        public OpeningState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Opening;
        }
        public override IEnumerator OnEnterCoroutine()
        {
            yield return null;
        }

        public override void OnUpdate()
        {

        }

        public override IEnumerator OnExitCoroutine()
        {
            yield return null;
        }
    }
    private class IdleState : UIState<StateType>
    {
        private AnimationCustomPreset _idleNormalInfo;
        private AnimationCustomPreset _idleNextInfo;
        public IdleState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {
            _idleNormalInfo = ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/IngameUI/AkaneStatus/DP/IdleNormal/");
            _idleNextInfo = ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/IngameUI/AkaneStatus/DP/IdleNext/");
        }
        public override StateType ChangeCondition()
        {
            return StateType.Idle;
        }
        public override IEnumerator OnEnterCoroutine()
        {
            yield return null;
        }

        public override void OnUpdate()
        {

        }

        public override IEnumerator OnExitCoroutine()
        {
            yield return null;
        }
    }
    private class ClosingState : UIState<StateType>
    {
        public ClosingState(ProjectorUI<StateType> projectorUI) : base(projectorUI) {}
        private AnimationCustomPreset _closingInfo;

        public override void Initialize()
        {

        }
        public override StateType ChangeCondition()
        {
            return StateType.Closing;
        }
        public override IEnumerator OnEnterCoroutine()
        {
            yield return null;
        }

        public override void OnUpdate()
        {

        }

        public override IEnumerator OnExitCoroutine()
        {
            yield return null;
        }
    }
}