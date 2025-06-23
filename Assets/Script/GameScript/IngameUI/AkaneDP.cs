using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AkaneDP : ProjectorUI
{
    [SerializeField] private Image DPImage;
    public class SingleDPManagedData : IManagedData
    {
        public bool isCharged;
        public bool isReady;
        public SingleDPManagedData(bool isCharged, bool isReady) { this.isCharged = isCharged; this.isReady = isReady; }
        public UIDataType uiDataType => UIDataType.AkaneDP;
    }
    private StateMachine<UIStateType> _stateMachine;
    public enum UIStateType { NONE, Idle, AutoRecovering, AttackRecovering, Consumed }



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



    private class IdleState : UIState<UIStateType>
    {
        private AnimationCustomPreset _idleNormalInfo;
        private AnimationCustomPreset _idleNextInfo;
        public IdleState(ProjectorUI projectorUI) : base(projectorUI) {}
        public override void Initialize()
        {
            _idleNormalInfo = ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/IngameUI/AkaneStatus/DP/IdleNormal/");
            _idleNextInfo = ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/IngameUI/AkaneStatus/DP/IdleNext/");
        }
        public override UIStateType ChangeCondition()
        {
            return UIStateType.Idle;
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