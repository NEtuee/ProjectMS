using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UICoroutineManager : MonoBehaviour //상태 내의 개별 코루틴을 실행할 때 여기에 등록, 중복 실행 방지?, 이름으로 등록하지 말고 같은 OnEnter, OnExit 내부에 있는걸 묶어서,, (공통 코루틴 존재하니까)
{
    private readonly ProjectorUI _projectorUI;
    public UICoroutineManager(ProjectorUI projectorUI) => _projectorUI = projectorUI;
    private Dictionary<IEnumerator, Coroutine> _runningCoroutines = new Dictionary<IEnumerator, Coroutine>();

    public bool IsCoroutineRunning(IEnumerator coroutine)
    {
        return _runningCoroutines.ContainsKey(coroutine);
    }
    public void RegisterCoroutine(IEnumerator coroutine) //코루틴을 등록,, 같은 이름의 코루틴이 있다면 중지 후 다시 시작?
    {
        if (_runningCoroutines.ContainsKey(coroutine))
            RestartCoroutine(coroutine);

        Coroutine newCoroutine = _projectorUI.StartCoroutine(ObserveCoroutine(coroutine));
        _runningCoroutines[coroutine] = newCoroutine;
    }
    private void RestartCoroutine(IEnumerator coroutine)
    {
        if (!_runningCoroutines.ContainsKey(coroutine))
            return;

        _projectorUI.StopCoroutine(_runningCoroutines[coroutine]);
        _runningCoroutines.Remove(coroutine);

        Coroutine restartingCoroutine = _projectorUI.StartCoroutine(ObserveCoroutine(coroutine));
        _runningCoroutines[coroutine] = restartingCoroutine;
    }
    private IEnumerator ObserveCoroutine(IEnumerator coroutine) //끝나면 리스트에서 삭제
    {
        yield return coroutine;
        _runningCoroutines.Remove(coroutine);
    }
}