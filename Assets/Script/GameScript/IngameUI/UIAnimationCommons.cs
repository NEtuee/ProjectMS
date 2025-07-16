using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimationCommons
{
    public static void InitializePosition(Image targetImage)
    {
        targetImage.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
    }
    public static IEnumerator Shake(Image targetImage, float duration, float strength) //지속 시간동안 흔들림, 갈수록 약해짐
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float x = Random.Range(-1f, 1f) * strength * easedT;
            float y = Random.Range(-1f, 1f) * strength * easedT;
            targetImage.rectTransform.anchoredPosition = new Vector2(x, y);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
    }
    public static IEnumerator Float(Image targetImage, float duration, float distance) //지속 시간동안 떠오름, 갈수록 약해짐
    {
        float elapsedTime = 0.0f;

        Vector2 startPosition = new Vector2(0.0f, - distance);
        targetImage.rectTransform.anchoredPosition = startPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, new Vector2(0.0f, 0.0f), easedT);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
    }
    public static IEnumerator Wave(Image targetImage, float duration, float distance)
    {
        float elapsedTime = 0.0f;

        Vector2 startPosition = new Vector2(0.0f, 0.0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Mathf.Sin(t * Mathf.PI);
            targetImage.rectTransform.anchoredPosition = startPosition - new Vector2(0.0f, (distance * easedT));
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
    }
    public static IEnumerator Flick(Image targetImage, float duration) //지속시간 동안 깜빡임
    {
        yield return null;
    }
    public static IEnumerator FadeIn(Image targetImage, float duration) //지속시간 동안 투명도 1로
    {
        float elapsedTime = 0.0f;
        float initialAlpha = targetImage.color.a;
        Color currentColor = targetImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            currentColor.a = Mathf.Lerp(initialAlpha, 1.0f, easedT);
            targetImage.color = currentColor;
            yield return null;
        }

        currentColor.a = 1.0f;
        targetImage.color = currentColor;
    }
    public static IEnumerator FadeOut(Image targetImage, float duration) //지속시간 동안 투명도 0으로
    {
        float elapsedTime = 0.0f;
        float initialAlpha = targetImage.color.a;
        Color currentColor = targetImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            currentColor.a = Mathf.Lerp(initialAlpha, 0.0f, easedT);
            targetImage.color = currentColor;
            yield return null;
        }

        currentColor.a = 0.0f;
        targetImage.color = currentColor;
    }
    public static IEnumerator CommonWait()
    {
        yield return new WaitForSeconds(2.0f);
    }
}