using System.Collections;
using UnityEngine;

public static class UIAnimationCommons
{
    public static IEnumerator Shake(RectTransform target)
    {
        Vector2 basePosition = target.anchoredPosition;
        float timer = 0.0f;
        float duration = 0.5f;
        float strength = 1.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            target.anchoredPosition = basePosition + new Vector2(x, y);
            yield return null;
        }

        target.anchoredPosition = basePosition;
    }
    public static IEnumerator Float(RectTransform target, AnimationCurve curve = null)
    {
        Vector2 basePosition = target.anchoredPosition;
        float timer = 0.0f;
        float duration = 0.5f;
        float distance = 24.0f;

        Vector2 startPosition = target.anchoredPosition - new Vector2(0.0f, distance);
        target.anchoredPosition = startPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float evalT = curve != null ? curve.Evaluate(t) : t;
            target.anchoredPosition = Vector2.Lerp(startPosition, basePosition, evalT);
            yield return null;
        }
        target.anchoredPosition = basePosition;
    }
    public static IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
    }
}