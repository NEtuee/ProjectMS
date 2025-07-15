using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimationCommons
{
    public static IEnumerator Shake(Image targetImage) //지속 시간동안 흔들림, 갈수록 약해짐
    {
        Vector2 basePosition = targetImage.rectTransform.anchoredPosition;
        float timer = 0.0f;
        float duration = 0.5f;
        float strength = 1.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
            float x = Random.Range(-1f, 1f) * strength * eased;
            float y = Random.Range(-1f, 1f) * strength * eased;
            targetImage.rectTransform.anchoredPosition = basePosition + new Vector2(x, y);

            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = basePosition;
    }
    public static IEnumerator Float(Image targetImage) //지속 시간동안 떠오름, 갈수록 약해짐
    {
        Vector2 basePosition = targetImage.rectTransform.anchoredPosition;
        float timer = 0.0f;
        float duration = 0.5f;
        float distance = 24.0f;

        Vector2 startPosition = basePosition - new Vector2(0.0f, distance);
        targetImage.rectTransform.anchoredPosition = startPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
            targetImage.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, basePosition, eased);
            yield return null;
        }

        targetImage.rectTransform.anchoredPosition = basePosition;
    }
    public static IEnumerator WaitA()
    {
        int timer = 0;
        while (timer < 120)
        {
            timer += 1;
            Debug.Log($"{timer}");
            yield return null;
        }
        Debug.Log("WaitingA Done");
    }
    public static IEnumerator WaitB()
    {
        yield return new WaitForSeconds(2.0f);
        //Debug.Log("WaitingB Done");
    }
}