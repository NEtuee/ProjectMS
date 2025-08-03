using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class UIState
{
    protected bool _onEntered = false;
    private Coroutine _onEnterCoroutine;
    private Coroutine _onExitCoroutine;
    protected readonly ProjectorUI _projectorUI;
    public UIState(ProjectorUI projectorUI)
    {
        _projectorUI = projectorUI;
    }
    public virtual void OnUpdateProjection() { }
    public abstract UIState ChangeState(SubUIData subData);
    public abstract UIState UpdateState();
    public IEnumerator StartEnteringState()
    {
        StopEnteringState();

        _onEnterCoroutine = _projectorUI.StartCoroutine(CheckOnEntered());
        yield return _onEnterCoroutine;
    }
    public void StopEnteringState()
    {
        if (_onEnterCoroutine != null)
        {
            _projectorUI.StopCoroutine(_onEnterCoroutine);
            _onEnterCoroutine = null;
        }

        _onEntered = false;
    }
    private IEnumerator CheckOnEntered() //_onEnterCoroutine
    {
        yield return OnEnterProjection();

        _onEntered = true;
        _onEnterCoroutine = null;
    }
    protected virtual IEnumerator OnEnterProjection() { yield break; }
    public IEnumerator StartExitingState()
    {
        StopExitingState();

        _onExitCoroutine = _projectorUI.StartCoroutine(CheckOnExited());
        yield return _onExitCoroutine;
    }
    public void StopExitingState()
    {
        if (_onExitCoroutine != null)
        {
            _projectorUI.StopCoroutine(_onExitCoroutine);
            _onExitCoroutine = null;
        }
    }
    private IEnumerator CheckOnExited() //_onExitCoroutine
    {
        yield return OnExitProjection();

        _onExitCoroutine = null;
    }
    protected virtual IEnumerator OnExitProjection() { yield break; }
}