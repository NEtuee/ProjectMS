using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UIState의 OnEnter, OnExit 내부의 모든 코루틴을 한 리스트로 묶어서 UICoroutineManager로 전달
//새 리스트가 들어오면 기존 리스트 정지 후 새 리스트 코루틴 실행
public class UIEffectManager : MonoBehaviour
{
    private List<Coroutine> _runningEffects = new List<Coroutine>();
    public void RegisterEffectList(ProjectorUI projectorUI, List<IEnumerator> newEffectList) //기존 코루틴 리스트 즉시 정지, 새 코루틴 리스트 실행
    {
        StopAllCoroutines();
        _runningEffects.Clear();

        foreach (IEnumerator effect in newEffectList)
        {
            Coroutine activeEffect = StartCoroutine(effect);
            _runningEffects.Add(activeEffect);
        }
    }
    public IEnumerator WaitAllListedEffects()
    {
        foreach (Coroutine activeEffect in _runningEffects)
        {
            yield return activeEffect;
        }
    }
}