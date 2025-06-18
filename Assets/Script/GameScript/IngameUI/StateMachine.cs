using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StateMachine<TUIStateType> where TUIStateType : Enum
{
    //StateMachine 및 코루틴을 실행할 ProjectorUI
    private ProjectorUI _projectorUI;
    private bool _isChanging = false;
    private Dictionary<TUIStateType, UIState<TUIStateType>> _stateMap;
    private UIState<TUIStateType> _currentUIState;
    private TUIStateType _currentUIStateType;

    public StateMachine(ProjectorUI projectorUI, Dictionary<TUIStateType, UIState<TUIStateType>> map)
    {
        _projectorUI = projectorUI;
        _stateMap = map;
    }
    //ProjectorUI의 UpdateProjection 메소드에서 실행, 각 State의 메소드를 실행해 ProjectorUI의 receivedData를 어떻게 projectedData로 변환할지
    public void UpdateState()
    {
        if (_isChanging || _currentUIState == null) return;

        TUIStateType nextUIStateType = _currentUIState.ChangeCondition();
        if (!EqualityComparer<TUIStateType>.Default.Equals(nextUIStateType, _currentUIStateType))
            ChangeState(nextUIStateType);
        else
            _currentUIState.OnUpdate();
    }
    //StateMachine의 UpdateState에서 자동으로, 또는 ProjectorUI의 Activate, Deactivate에서 수동으로 실행, 상태를 전환한다
    public void ChangeState(TUIStateType nexTUIStateType)
    {
        if (_isChanging)
            return;
        _projectorUI.StartCoroutine(ChangeStateCoroutine(nexTUIStateType));
    }
    private IEnumerator ChangeStateCoroutine(TUIStateType nexTUIStateType)
    {
        _isChanging = true;

        if (_currentUIState != null)
            yield return _currentUIState.OnExitCoroutine();

        if (!_stateMap.TryGetValue(nexTUIStateType, out UIState<TUIStateType> nexTUIState))
            throw new Exception($"상태 {nexTUIStateType} 이 stateMap에 없습니다.");

        _currentUIState = nexTUIState;
        _currentUIStateType = nexTUIStateType;

        yield return _currentUIState.OnEnterCoroutine();

        _isChanging = false;
    }
}