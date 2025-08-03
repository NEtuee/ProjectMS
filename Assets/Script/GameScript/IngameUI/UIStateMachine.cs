using System.Collections;
using UnityEngine;

public class UIStateMachine
{
    private readonly ProjectorUI _projectorUI;
    private UIState _previousState;
    private UIState _currentState;
    private Coroutine _stateChangingProjection = null;

    public UIStateMachine(ProjectorUI projectorUI) => _projectorUI = projectorUI;
    public void Initialize()
    {
        _previousState = null;
        _currentState = null;
        _stateChangingProjection = null;
    }
    public void ProjectingCurrentState() //매 프레임 실행됨, UIState의 OnUpdateProjection()을 실행
    {
        if (_currentState == null)
            return;

        _currentState.OnUpdateProjection();
    }
    public void RequestStateByUpdate() //매 프레임마다 호출, ReceivedData를 조건으로 UIState 변경
    {
        if (_currentState == null)
            return;
        
        UIState nextState = _currentState.UpdateState();
        if (nextState != null)
        {
            _previousState = _currentState;
            _currentState = nextState;

            ProjectingStateChanging();
        }
    }
    public void RequestStateBySubData(SubUIData subData) //SubUIData가 변경될 때마다 RequestStateByUpdate를 대신해 호출, UIEventKey를 조건으로 UIState 변경 
    {
        if (_currentState == null)
            return;
        
        UIState nextState = _currentState.ChangeState(subData);
        if (nextState != null)
        {
            _previousState = _currentState;
            _currentState = nextState;

            ProjectingStateChanging();
        }
    }
    public void ForceStateChanging(UIState nextState) //조건을 무시하고 주어진 UIState로 강제 상태 전이, UIState 외부에서 호출해서 사용
    {
        if (nextState != null)
        {
            _previousState = _currentState;
            _currentState = nextState;

            ProjectingStateChanging();
        }
    }
    private void ProjectingStateChanging()
    {
        if (_stateChangingProjection != null)
        {
            _projectorUI.StopCoroutine(_stateChangingProjection);
            _stateChangingProjection = _projectorUI.StartCoroutine(OverwriteChangingCoroutine());

            return;
        }

        _stateChangingProjection = _projectorUI.StartCoroutine(NormalChangingCoroutine());
    }
    private IEnumerator NormalChangingCoroutine() //_stateChangingCoroutine, 일반 상태 전환, 이전 상태의 OnExit과 현재 상태의 OnEnter를 실행
    {
        // if (_projectorUI is )     
        //     Debug.Log($"Normal Changing Coroutine: {_previousState} to {_currentState}");

        if (_previousState == null)
            yield break;
        
        _projectorUI.StopAllProjectionCoroutine();

        _previousState.StopExitingState();
        yield return _projectorUI.StartCoroutine(_previousState.StartExitingState());
        _currentState.StopEnteringState();
        yield return _projectorUI.StartCoroutine(_currentState.StartEnteringState());

        _stateChangingProjection = null;
    }
    private IEnumerator OverwriteChangingCoroutine() //_stateChangingCoroutine, 강제 상태 전환, 현재 상태의 OnEnter만 실행
    {
        // if (_projectorUI is )     
        //     Debug.Log($"Overwrite Changing Coroutine: {_previousState} to {_currentState}");

        if (_previousState == null)
            yield break;
        
        _projectorUI.StopAllProjectionCoroutine();

        _previousState.StopExitingState();

        _currentState.StopEnteringState();
        yield return _projectorUI.StartCoroutine(_currentState.StartEnteringState());

        _stateChangingProjection = null;
    }
}