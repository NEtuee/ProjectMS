using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMachine
{
    private readonly ProjectorUI _projectorUI;
    private UIState _currentState;
    private Queue<UIState> _stateQueue = new Queue<UIState>();
    private bool _isTransitioning = false;

    public UIStateMachine(ProjectorUI projectorUI) => _projectorUI = projectorUI;
    public void UpdateState()
    {
        if (_isTransitioning || _currentState == null)
            return;

        UIState nextState = _currentState.ChangeState();
        if (!ReferenceEquals(nextState, _currentState))
            RequestStateChanging(nextState);
        else
            _currentState.OnUpdate();
    }
    public void RequestStateChanging(UIState nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, _currentState))
            return;

        if (_isTransitioning)
        {
            if (_stateQueue.Count == 0 || !ReferenceEquals(_stateQueue.Peek(), nextState))
                _stateQueue.Enqueue(nextState);
            return;
        }

        _projectorUI.ConfirmSubData();
        _projectorUI.StartCoroutine(StateTransitionCoroutine(nextState));
    }
    private IEnumerator StateTransitionCoroutine(UIState nextState)
    {
        _isTransitioning = true;

        if (_currentState != null)
            yield return _projectorUI.StartCoroutine(_currentState.OnExit());

        _currentState = nextState;

        if (_currentState != null)
            yield return _projectorUI.StartCoroutine(_currentState.OnEnter());

        // 큐에 쌓인 상태 요청이 있으면 순차적으로 실행
        while (_stateQueue.Count > 0)
        {
            var queuedState = _stateQueue.Dequeue();
            if (!ReferenceEquals(queuedState, _currentState))
            {
                _projectorUI.StartCoroutine(StateTransitionCoroutine(queuedState));
                yield break;
            }
        }

        _isTransitioning = false;
    }
}