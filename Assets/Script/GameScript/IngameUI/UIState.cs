//UI의 애니메이션과 실제 데이터를 어떻게 표시할 것인지를 담당

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class UIState<TState> where TState : Enum
{
    //외부의 receivedData와 투영 컴포넌트를 가지고 있는 ProjectorUI
    protected ProjectorUI _projectorUI;
    //AnimationPreset
    public UIState(ProjectorUI projectorUI)
    {
        _projectorUI = projectorUI;
        this.Initialize();
    }
    //AnimationPreset 지정
    //ex. ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/AkaneStatusBar/DP/IdleNormal/");
    public abstract void Initialize();
    //조건에 따라 반환할 다른 TState
    public abstract TState ChangeCondition();
    //상태에 진입했을 때 실행할 코루틴, 모두 종료된 후 StateMachine이 OnUpdate 실행 가능
    public abstract IEnumerator OnEnterCoroutine();
    //외부의 receivedData를 가져와 projectingData로 변환한 후 투영 컴포넌트에 전달
    public abstract void OnUpdate();
    //상태에서 빠져나갈 때 실행할 코루틴, 모두 종료된 후 StateMachine이 다음 상태의 OnEnterCoroutine 실행 가능
    public abstract IEnumerator OnExitCoroutine();
}