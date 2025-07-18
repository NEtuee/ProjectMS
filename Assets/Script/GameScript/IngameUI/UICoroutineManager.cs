using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UIState의 OnEnter, OnExit 내부의 모든 코루틴을 한 리스트로 묶어서 UICoroutineManager로 전달
//새 리스트가 들어오면 기존 리스트 정지 후 새 리스트 코루틴 실행
public class UICoroutineManager : MonoBehaviour
{
    private List<Coroutine> _runningCoroutines = new List<Coroutine>();
    public void RegisterCoroutineList(ProjectorUI projectorUI, List<IEnumerator> newCoroutineList)
    {
        StopAllCoroutines();
        _runningCoroutines.Clear();

        foreach (IEnumerator coroutine in newCoroutineList)
        {
            Coroutine activeCoroutine = StartCoroutine(coroutine);
            _runningCoroutines.Add(activeCoroutine);
        }
    }
    public IEnumerator WaitAllListedCoroutine()
    {
        foreach (Coroutine activeCoroutine in _runningCoroutines)
        {
            yield return activeCoroutine;
        }
    }
}