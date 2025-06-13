using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StateMachine<TState> where TState : Enum
{
    //StateMachine 및 코루틴을 실행할 ProjectorUI
    private ProjectorUI<TState> _projectorUI;
    private bool _isChanging = false;
    private Dictionary<TState, UIState<TState>> _stateMap;
    private UIState<TState> _currentState;
    private TState _currentStateType;

    public StateMachine(ProjectorUI<TState> projectorUI)
    {
        _projectorUI = projectorUI;
    }

    //ProjectorUI의 Initialize 메소드에서 실행, StateMap을 전달받음, enum TState와 ProjectorUI의 모든 UIState<TState>와 매칭
    public void Initialize(Dictionary<TState, UIState<TState>> map, TState initialState)
    {
        _stateMap = map;
        ChangeState(initialState);
    }
    //ProjectorUI의 UpdateProjection 메소드에서 실행, 각 State의 메소드를 실행해 ProjectorUI의 receivedData를 어떻게 projectedData로 변환할지
    public void UpdateState()
    {
        if (_isChanging || _currentState == null) return;

        TState nextStateType = _currentState.ChangeCondition();
        if (!EqualityComparer<TState>.Default.Equals(nextStateType, _currentStateType))
        {
            ChangeState(nextStateType);
        }
        else
        {
            _currentState.OnUpdate();
        }
    }
    //StateMachine의 UpdateState에서 자동으로, 또는 ProjectorUI의 Activate, Deactivate에서 수동으로 실행, 상태를 전환한다
    public void ChangeState(TState nextStateType)
    {
        if (_isChanging) return;
        _projectorUI.StartCoroutine(ChangeStateCoroutine(nextStateType));
    }
    private IEnumerator ChangeStateCoroutine(TState nextStateType)
    {
        _isChanging = true;

        if (_currentState != null)
            yield return _currentState.OnExitCoroutine();

        if (!_stateMap.TryGetValue(nextStateType, out UIState<TState> nextState))
            throw new Exception($"상태 {nextStateType} 이 stateMap에 없습니다.");

        _currentState = nextState;
        _currentStateType = nextStateType;

        yield return _currentState.OnEnterCoroutine();

        _isChanging = false;
    }
}