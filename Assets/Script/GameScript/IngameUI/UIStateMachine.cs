using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMachine
{
    private readonly ProjectorUI _projectorUI;
    private UIState _previousState;
    private UIState _currentState;
    private bool _isStateChanging = false;
    private Coroutine _stateChangingCoroutine = null;

    public UIStateMachine(ProjectorUI projectorUI) => _projectorUI = projectorUI;

    public void ProjectingCurrentState() //매 프레임 실행됨, _currentState의 ChangeState를 실행하고 null을 반환했으면 _currentState의 OnUpdate를, UIState를 반환했으면 RequestStateChanging을 실행
    {
        if (_currentState == null)
            return;

        if (!_isStateChanging)
            _currentState.OnUpdate();
    }
    public void RequestStateBySubData(SubUIData subData) //subData 업데이트 될 때마다 호출,, 겹침 문제 고민해봐야할듯 UpdateState로 바로 다른 UIState로 전환하는 데 그 때 SubData가 끼어들어서 null 반환하면?
                                                         //기다렸다가 실행하기 같은거 되는지
    {
        UIState nextState = _currentState.ChangeState(subData);

        if (nextState != null)
        {
            _previousState = _currentState;
            _currentState = nextState;

            StateChanging();
        }
    }
    public void RequestStateByUpdate() //매 프레임마다 호출, subData 업데이트랑 겹치지 않게 조심해야할듯
    {
        UIState nextState = _currentState.UpdateState();

        if (nextState != null)
        {
            _previousState = _currentState;
            _currentState = nextState;

            StateChanging();
        }
    }
    public void ForceStateChanging(UIState nextState) //주어진 UIState로 상태 전이, _prevState를 _currentState로, _currentState를 nextState로 변경, 외부에서 호출해서 사용
    {
        if (nextState == null)
            return;

        _previousState = _currentState;
        _currentState = nextState;

        StateChanging();
    }
    private void StateChanging() 
    {
        if (!_isStateChanging) //상태 전환 중이 아니라면 Normal
        {
            _stateChangingCoroutine = _projectorUI.StartCoroutine(NormalChangingCoroutine());
        }
        else //상태 전환 중이라면 Overwrite
        {
            if (_stateChangingCoroutine != null) //정지시켜야 되니 null 체크
            {
                if (_currentState.OnEnter().MoveNext()) //currentState의 OnEnter가 비어있지 않다면 진행중이던 ChangingCoroutine 계속 진행, 여기서 코루틴의 첫줄을 실행하고 넘어가는듯
                {
                    _projectorUI.StopCoroutine(_stateChangingCoroutine);
                    _stateChangingCoroutine = _projectorUI.StartCoroutine(OverwriteChangingCoroutine());
                }
                else
                {
                    //Debug.Log($"Skipped StateChangingCoroutine");
                }
            }
        }
    }
    private IEnumerator NormalChangingCoroutine() //일반 전환, 이전 상태의 OnExit과 현재 상태의 OnEnter 실행
    {
        //Debug.Log($"Normal Changing Coroutine: {_prevState} to {_currentState}");
        _isStateChanging = true;

        yield return _projectorUI.StartCoroutine(WaitForOnExit(_previousState));
        yield return _projectorUI.StartCoroutine(WaitForOnEnter(_currentState));

        _isStateChanging = false;
        _stateChangingCoroutine = null;
    }
    private IEnumerator OverwriteChangingCoroutine() //강제 전환, 현재 상태의 OnEnter만 실행
    {
        //Debug.Log($"Overwrite Changing Coroutine: {_prevState} to {_currentState}");
        _isStateChanging = true;

        yield return _projectorUI.StartCoroutine(WaitForOnEnter(_currentState));

        _isStateChanging = false;
        _stateChangingCoroutine = null;
    }

    private IEnumerator WaitForOnExit(UIState state)
    {
        yield return _projectorUI.StartCoroutine(state.OnExit());
    }
    private IEnumerator WaitForOnEnter(UIState state)
    {
        yield return _projectorUI.StartCoroutine(state.OnEnter());
    }
}