using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVariable : MonoBehaviour
{
    public static GlobalVariable _instance;
    public static GlobalVariable Instance()
    {
        return _instance;
    }

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public static string _shakeTranslationPreset = "ShakeSmall";
    public static string _shakeRotationPreset = "Shake";
    public static float _shakeDuration = 0.3f;
}
