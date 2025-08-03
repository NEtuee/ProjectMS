using System.Collections;
using UnityEngine;

public static class UIAnimationCommons
{
    public static IEnumerator Wait(float duration)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    public static IEnumerator LerpToBasePosition(UIVisualModule uiVisualModule, float duration,
                                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        Vector2 startPosition = uiVisualModule.Image.rectTransform.anchoredPosition;
        Vector2 endPosition = uiVisualModule.BasePosition;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            uiVisualModule.Image.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        uiVisualModule.Image.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator AddShakeOffsetToPosition(UIVisualModule uiVisualModule, float duration, float strength,
                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        Vector2 startPosition = uiVisualModule.Image.rectTransform.anchoredPosition;
        Vector2 endPosition = uiVisualModule.BasePosition;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            float offsetX = (Mathf.PerlinNoise(Time.time * 20f, 0f) * 2.0f - 1.0f) * strength * (1.0f - easedT);
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 20f) * 2.0f - 1.0f) * strength * (1.0f - easedT);

            uiVisualModule.Image.rectTransform.anchoredPosition = startPosition + new Vector2(offsetX, offsetY);

            yield return null;
        }

        uiVisualModule.Image.rectTransform.anchoredPosition = endPosition;
    }
    public static IEnumerator AddFloatOffsetToPosition(UIVisualModule uiVisualModule, float duration, float distance,
                                    MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            float offsetY = -distance * (1.0f - easedT);

            uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition + new Vector2(0.0f, offsetY);
            yield return null;
        }

        uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition;
    }
    public static IEnumerator AddWaveDownOffsetToPosition(UIVisualModule uiVisualModule, float duration, float strength,
                                        MathEx.EaseType downEasingFunction = MathEx.EaseType.End,
                                        MathEx.EaseType upEasingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (downEasingFunction == MathEx.EaseType.End)
            downEasingFunction = MathEx.EaseType.EaseInCubic;
        if (upEasingFunction == MathEx.EaseType.End)
            upEasingFunction = MathEx.EaseType.EaseInCubic;

        float elapsedTime = 0.0f;

        float downPhaseRatio = 0.33f;
        float upPhaseRatio = 1.0f - downPhaseRatio;

        float downDuration = duration * downPhaseRatio;
        float upDuration = duration * upPhaseRatio;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float offsetY = 0.0f;

            if (elapsedTime <= downDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / downDuration);
                float easedT = MathEx.getEaseFormula(downEasingFunction, 0.0f, 1.0f, t);

                offsetY = Mathf.Lerp(0.0f, -strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - downDuration) / upDuration);
                float easedT = MathEx.getEaseFormula(upEasingFunction, 0.0f, 1.0f, t);

                offsetY = Mathf.Lerp(-strength, 0.0f, easedT);
            }

            uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition + new Vector2(0.0f, offsetY);

            yield return null;
        }

        uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition;
    }
    public static IEnumerator AddWaveUpOffsetToPosition(UIVisualModule uiVisualModule, float duration, float strength,
                                    MathEx.EaseType upEasingFunction = MathEx.EaseType.End,
                                    MathEx.EaseType downEasingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (upEasingFunction == MathEx.EaseType.End)
            upEasingFunction = MathEx.EaseType.EaseOutCubic;
        if (downEasingFunction == MathEx.EaseType.End)
            downEasingFunction = MathEx.EaseType.EaseOutCubic;

        float elapsedTime = 0.0f;

        float upPhaseRatio = 0.1f;
        float downPhaseRatio = 1.0f - upPhaseRatio;

        float upDuration = duration * upPhaseRatio;
        float downDuration = duration * downPhaseRatio;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float offsetY = 0.0f;

            if (elapsedTime <= upDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / upDuration);
                float easedT = MathEx.getEaseFormula(upEasingFunction, 0.0f, 1.0f, t);

                offsetY = Mathf.Lerp(0.0f, strength, easedT);
            }
            else
            {
                float t = Mathf.Clamp01((elapsedTime - upDuration) / downDuration);
                float easedT = MathEx.getEaseFormula(downEasingFunction, 0.0f, 1.0f, t);

                offsetY = Mathf.Lerp(strength, 0.0f, easedT);
            }

            uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition + new Vector2(0.0f, offsetY);

            yield return null;
        }

        uiVisualModule.Image.rectTransform.anchoredPosition = uiVisualModule.BasePosition;
    }
    public static IEnumerator FadeInAlpha(UIVisualModule uiVisualModule, float duration,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        float startAlpha = uiVisualModule.Image.color.a;
        float endAlpha = 1.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            Color currentColor = uiVisualModule.Image.color;
            currentColor.a = currentAlpha;
            uiVisualModule.Image.color = currentColor;

            yield return null;
        }

        Color endColor = uiVisualModule.Image.color;
        endColor.a = endAlpha;
        uiVisualModule.Image.color = endColor;
    }
    public static IEnumerator FadeOutAlpha(UIVisualModule uiVisualModule, float duration,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.EaseOutCubic;

        float startAlpha = uiVisualModule.Image.color.a;
        float endAlpha = 0.0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = MathEx.getEaseFormula(easingFunction, 0.0f, 1.0f, t);

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            Color currentColor = uiVisualModule.Image.color;
            currentColor.a = currentAlpha;
            uiVisualModule.Image.color = currentColor;

            yield return null;
        }

        Color endColor = uiVisualModule.Image.color;
        endColor.a = endAlpha;
        uiVisualModule.Image.color = endColor;
    }
    public static IEnumerator FlickAlpha(UIVisualModule uiVisualModule, float duration, float strength, int frequency,
                                        MathEx.EaseType easingFunction = MathEx.EaseType.End)
    {
        if (uiVisualModule.Image == null)
            yield break;

        if (easingFunction == MathEx.EaseType.End)
            easingFunction = MathEx.EaseType.Linear;

        float startAlpha = uiVisualModule.Image.color.a;
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

                Color currentColor = uiVisualModule.Image.color;
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
                uiVisualModule.Image.color = currentColor;

                yield return null;
            }

            Color endColor = uiVisualModule.Image.color;
            endColor.a = endAlpha;
            uiVisualModule.Image.color = endColor;
        }
    }
}