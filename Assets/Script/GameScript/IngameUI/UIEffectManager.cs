using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UIState의 OnEnter, OnExit 내부의 모든 코루틴을 한 리스트로 묶어서 UICoroutineManager로 전달
//새 리스트가 들어오면 기존 리스트 정지 후 새 리스트 코루틴 실행
public class UIEffectManager
{
    private UIVisualModule _uiVisualModule;
    private List<Coroutine> _playingEffects = new List<Coroutine>();
    public UIEffectManager(UIVisualModule uiVisualModule)
    {
        _uiVisualModule = uiVisualModule;
    }
    public IEnumerator RunSequentialEffects(IEnumerator[] effectCoroutines)
    {
        StopAllRunningEffects();

        foreach (IEnumerator singleEffect in effectCoroutines)
        {
            Coroutine playingEffect = _uiVisualModule.StartCoroutine(singleEffect);
            _playingEffects.Add(playingEffect);
            yield return playingEffect;
            _playingEffects.Remove(playingEffect);
        }
    }
    public IEnumerator RunParallelEffects(IEnumerator[] effectCoroutines)
    {
        StopAllRunningEffects();

        List<Coroutine> parallelEffectList = new List<Coroutine>();
        foreach (IEnumerator singleEffect in effectCoroutines)
        {
            Coroutine playingEffect = _uiVisualModule.StartCoroutine(singleEffect);
            parallelEffectList.Add(playingEffect);
            _playingEffects.Add(playingEffect);
        }

        foreach (Coroutine playingEffect in parallelEffectList)
        {
            yield return playingEffect;
            _playingEffects.Remove(playingEffect);
        }
    }
    public void StopAllRunningEffects()
    {
        foreach (Coroutine playingEffect in _playingEffects)
        {
            if (playingEffect != null)
                _uiVisualModule.StopCoroutine(playingEffect);
        }

        _playingEffects.Clear();
    }
}