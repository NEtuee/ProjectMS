using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimationCommons
{
    //공통 애니메이션 코루틴, 모두 최종적으로 Vector2(0.0f, 0.0f)로 초기화?
    public static IEnumerator LerpToInitialPosition(Image targetImage, float duration,
                                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator ShakePosition(Image targetImage, float duration, float strength,
                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            float offsetX = (Mathf.PerlinNoise(Time.time * 20f, 0f) * 2.0f - 1.0f) * strength * (1.0f - easedT);
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 20f) * 2.0f - 1.0f) * strength * (1.0f - easedT);

            targetImage.rectTransform.anchoredPosition = startPosition + new Vector2(offsetX, offsetY);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator FloatPosition(Image targetImage, float duration, float distance,
                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        Vector2 startPosition = targetImage.rectTransform.anchoredPosition - new Vector2(0.0f, distance);
        Vector2 endPosition = new Vector2(0.0f, 0.0f);
        float elapsedTime = 0.0f;

        targetImage.rectTransform.anchoredPosition = startPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator WaveDownPosition(Image targetImage, float duration, float strength,
                                        MathEx.EaseType downEasingFunction = MathEx.EaseType.End,
                                        MathEx.EaseType upEasingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (downEasingFunction == MathEx.EaseType.End)
            downEasingFunction = MathEx.EaseType.EaseInCubic;
        if (upEasingFunction == MathEx.EaseType.End)
            upEasingFunction = MathEx.EaseType.EaseInCubic;

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
                float easedT = MathEx.getEaseFormula(downEasingFunction, 0.0f, 1.0f, t);

                currentYOffset = Mathf.Lerp(0.0f, -strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - downDuration) / upDuration);
                float easedT = MathEx.getEaseFormula(upEasingFunction, 0.0f, 1.0f, t);

                currentYOffset = Mathf.Lerp(-strength, 0.0f, easedT);
            }

            float overallT = Mathf.Clamp01(elapsedTime / duration);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, overallT) + new Vector2(0.0f, currentYOffset);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator WaveUpPosition(Image targetImage, float duration, float strength,
                                    MathEx.EaseType upEasingFunction = MathEx.EaseType.End,
                                    MathEx.EaseType downEasingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (upEasingFunction == MathEx.EaseType.End)
            upEasingFunction = MathEx.EaseType.EaseOutCubic;
        if (downEasingFunction == MathEx.EaseType.End)
            downEasingFunction = MathEx.EaseType.EaseOutCubic;

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
                float easedT = MathEx.getEaseFormula(upEasingFunction, 0.0f, 1.0f, t);

                currentYOffset = Mathf.Lerp(0.0f, strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - upDuration) / downDuration);
                float easedT = MathEx.getEaseFormula(downEasingFunction, 0.0f, 1.0f, t);

                currentYOffset = Mathf.Lerp(strength, 0.0f, easedT);
            }

            float overallT = Mathf.Clamp01(elapsedTime / duration);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, overallT) + new Vector2(0, currentYOffset);

            yield return null;
        }
    }
    public static IEnumerator FadeInAlpha(Image targetImage, float duration,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        float startAlpha = targetImage.color.a;
        float endAlpha = 1.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

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
    public static IEnumerator FadeOutAlpha(Image targetImage, float duration,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        float startAlpha = targetImage.color.a;
        float endAlpha = 0.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

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
    public static IEnumerator FlickAlpha(Image targetImage, float duration, float strength, int frequency,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (targetImage == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.Linear;

        float startAlpha = targetImage.color.a;
        float endAlpha = 1.0f;
        float singleFlickDuration = duration / frequency;

        for (int i = 0; i < frequency; i++)
        {
            float singleFlickElapsedTime = 0.0f;
            float overallT = (float)i / frequency;
            float easedOverallT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, overallT);

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

            Color endColor = targetImage.color;
            endColor.a = endAlpha;
            targetImage.color = endColor;
        }
    }
}