using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;



public static class UIAnimationCommons
{
    //Easing 함수
    public static float EaseLinear(float t)
    {
        return t;
    }

    public static float EaseInQuad(float t)
    {
        return t * t;
    }

    public static float EaseOutQuad(float t)
    {
        return 1.0f - (1.0f - t) * (1.0f - t);
    }

    public static float EaseOutQuint(float t)
    {
        t -= 1.0f;
        return t * t * t * t * t + 1.0f;
    }

    public static float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        return c1 * t * t * t - (c1 - 1.0f) * t * t;
    }

    public static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        t = t - 1.0f;
        return (t * t * ((c1 + 1.0f) * t + c1) + 1.0f);
    }
    public static float EaseOutCirc(float t)
    {
        t -= 1.0f;
        return Mathf.Sqrt(1.0f - t * t);
    }
    public static float EaseInOutCirc(float t)
    {
        if (t < 0.5f)
        {
            t *= 2.0f;
            return (1.0f - Mathf.Sqrt(1.0f - t * t)) / 2.0f;
        }
        else
        {
            t = t * 2.0f - 2.0f;
            return (Mathf.Sqrt(1.0f - t * t) + 1.0f) / 2.0f;
        }
    }


    //공통 애니메이션 코루틴, 모두 최종적으로 Vector2(0.0f, 0.0f)로 초기화?
    public static IEnumerator LerpToInitialPosition(Image targetImage, float duration,
                                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuad;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = easingFunction(t);

            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator Shake(Image targetImage, float duration, float strength,
                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuad;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = easingFunction(t);

            float offsetX = (Mathf.PerlinNoise(Time.time * 20f, 0f) * 2.0f - 1.0f) * strength * (1.0f - easedT);
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 20f) * 2.0f - 1.0f) * strength * (1.0f - easedT);

            targetImage.rectTransform.anchoredPosition = startPosition + new Vector2(offsetX, offsetY);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator Float(Image targetImage, float duration, float distance,
                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuad;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition - new Vector2(0.0f, distance);
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        targetImage.rectTransform.anchoredPosition = startPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = easingFunction(t);

            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator WaveDown(Image targetImage, float duration, float strength,
                                        Func<float, float> downEasingFunction = null,
                                        Func<float, float> upEasingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (downEasingFunction == null)
            downEasingFunction = EaseOutQuint;
        if (upEasingFunction == null)
            upEasingFunction = EaseOutCirc;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        float downPhaseRatio = 0.33f;
        float upPhaseRatio = 1.0f - downPhaseRatio;

        float downDuration = duration * downPhaseRatio;
        float upDuration = duration * upPhaseRatio;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentYOffset = 0.0f;

            if (elapsedTime <= downDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / downDuration);
                float easedT = downEasingFunction(t);

                currentYOffset = Mathf.Lerp(0.0f, -strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - downDuration) / upDuration);
                float easedT = upEasingFunction(t);

                currentYOffset = Mathf.Lerp(-strength, 0.0f, easedT);
            }

            float overallT = Mathf.Clamp01(elapsedTime / duration);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, overallT) + new Vector2(0.0f, currentYOffset);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator WaveUp(Image targetImage, float duration, float strength,
                                    Func<float, float> upEasingFunction = null,
                                    Func<float, float> downEasingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (upEasingFunction == null)
            upEasingFunction = EaseOutQuint;
        if (downEasingFunction == null)
            downEasingFunction = EaseOutCirc;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        float upPhaseRatio = 0.1f;
        float downPhaseRatio = 1.0f - upPhaseRatio;

        float upDuration = duration * upPhaseRatio;
        float downDuration = duration * downPhaseRatio;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentYOffset = 0.0f;

            if (elapsedTime <= upDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / upDuration);
                float easedT = upEasingFunction(t);

                currentYOffset = Mathf.Lerp(0.0f, strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - upDuration) / downDuration);
                float easedT = downEasingFunction(t);

                currentYOffset = Mathf.Lerp(strength, 0.0f, easedT);
            }

            float overallT = Mathf.Clamp01(elapsedTime / duration);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, overallT) + new Vector2(0, currentYOffset);

            yield return null;
        }
    }
    public static IEnumerator FadeIn(Image targetImage, float duration,
                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuad;

        float startAlpha = targetImage.color.a;
        float endAlpha = 1.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = easingFunction(t);

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            Color currentColor = targetImage.color;
            currentColor.a = currentAlpha;
            targetImage.color = currentColor;

            yield return null;
        }

        Color endColor = targetImage.color;
        endColor.a = endAlpha;
        targetImage.color = endColor;
    }
    public static IEnumerator FadeOut(Image targetImage, float duration,
                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuad;

        float startAlpha = targetImage.color.a;
        float endAlpha = 0.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = easingFunction(t);

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            Color currentColor = targetImage.color;
            currentColor.a = currentAlpha;
            targetImage.color = currentColor;

            yield return null;
        }

        Color endColor = targetImage.color;
        endColor.a = endAlpha;
        targetImage.color = endColor;
    }
    public static IEnumerator Flick(Image targetImage, float duration, float strength, int frequency,
                                    Func<float, float> easingFunction = null)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == null)
            easingFunction = EaseOutQuint;

        Color startColor = targetImage.color;
        float startAlpha = startColor.a;
        float singleFlickDuration = duration / frequency;

        for (int i = 0; i < frequency; i++)
        {
            float singleFlickElapsedTime = 0.0f;
            float overallT = (float)i / frequency;
            float easedOverallT = easingFunction(overallT);

            float currentStrength = Mathf.Clamp01(strength) * (1.0f - easedOverallT);

            while (singleFlickElapsedTime < singleFlickDuration)
            {
                singleFlickElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(singleFlickElapsedTime / singleFlickDuration);

                Color currentColor = targetImage.color;
                float currentAlpha;

                if (t < 0.5f)
                {
                    currentAlpha = Mathf.Lerp(startAlpha, startAlpha - currentStrength, t * 2.0f);
                }
                else
                {
                    currentAlpha = Mathf.Lerp(startAlpha - currentStrength, startAlpha, (t - 0.5f) * 2.0f);
                }

                currentColor.a = currentAlpha;
                targetImage.color = currentColor;

                yield return null;
            }

            targetImage.color = startColor;
        }
    }
}