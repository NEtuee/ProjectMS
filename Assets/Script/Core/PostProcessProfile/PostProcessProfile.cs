using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PostProcessProfileData
{
    public bool _useSunAngle = true;
    public float _sunAngle = 0f;

    public bool _useShadowDistance = true;
    public float _shadowDistance = 0f;

    public bool _useShadowDistanceRatio = true;
    public float _shadowDistanceRatio = 0f;

    
    public bool _useScreenSize = true;
    public Vector2 _screenSize = new Vector2();


    public bool _useShadowDistanceOffset = true;
    public float _shadowDistanceOffset = 0f;

    public bool _useShadowColor = true;
    public Color _shadowColor = Color.white;


    public bool _useImpactFrame = true;
    public float _impactFrame = 0f;

    public bool _useBrightness = true;
    public float _brightness = 0f;

    public bool _useSaturation = true;
    public float _saturation = 0f;

    public bool _useBloom = true;
    public float _bloom = 0f;

    public bool _useVignette = true;
    public float _vignette = 0f;

    public bool _useContrast = true;
    public float _contrast = 1f;

    public bool _useContrastTarget = true;
    public float _contrastTarget = 0.5f;

    public bool _useColorTint = true;
    public Color _colorTint = Color.white;

    public bool _useBackgroundColorTint = true;
    public Color _backgroundColorTint = Color.white;

    public bool _useForwardScreenColorTint = true;
    public Color _forwardScreenColorTint = Color.white;

    public bool _useBlurSize = true;
    public float _blurSize = 0f;

    public bool _useMultiSampleDistance = true;
    public float _multiSampleDistance = 0f;

    public bool _useMultiSampleColorTintRight = true;
    public Color _multiSampleColorTintRight = Color.white;

    public bool _useMultiSampleColorTintLeft = true;
    public Color _multiSampleColorTintLeft = Color.white;


    public bool _useFogRate = true;
    public float _fogRate = 0f;

    public bool _useFogStrength = true;
    public float _fogStrength = 0f;

    public bool _useFogColor = true;
    public Color _fogColor = Color.white;

    public float _backgroundTransitionRate = 0f;
    public bool _useBackgroundTransitionRate = false;

    public float _curvature = 0f;
    public bool _useCurvature = true;

    public float _scanLine = 0f;
    public bool _useScanLine = true;

    public bool _pixelSnap = false;

    public void syncShadowValueToMaterial(Material targetMaterial)
    {
        if(_useSunAngle)
            targetMaterial.SetFloat("_SunAngle",_sunAngle);
        if(_useShadowDistance)
            targetMaterial.SetFloat("_ShadowDistance",_shadowDistance);
        if(_useShadowDistanceRatio)
            targetMaterial.SetFloat("_ShadowDistanceRatio",_shadowDistanceRatio);

        if(_useScreenSize)
            targetMaterial.SetVector("_ScreenSize",new Vector4(_screenSize.x,_screenSize.y,0f,0f));
        if(_useShadowDistanceOffset)
            targetMaterial.SetFloat("_ShadowDistanceOffset",_shadowDistanceOffset);
        if(_useShadowColor)
            targetMaterial.SetColor("_ShadowColor",_shadowColor);
    }

    public void syncValueToMaterial(Material targetMaterial)
    {
        if(_useSunAngle)
            targetMaterial.SetFloat("_SunAngle",_sunAngle);
        if(_useShadowDistance)
            targetMaterial.SetFloat("_ShadowDistance",_shadowDistance);
        if(_useShadowDistanceRatio)
            targetMaterial.SetFloat("_ShadowDistanceRatio",_shadowDistanceRatio);

        if(_useScreenSize)
            targetMaterial.SetVector("_ScreenSize",new Vector4(_screenSize.x,_screenSize.y,0f,0f));
        if(_useShadowDistanceOffset)
            targetMaterial.SetFloat("_ShadowDistanceOffset",_shadowDistanceOffset);
        if(_useShadowColor)
            targetMaterial.SetColor("_ShadowColor",_shadowColor);

        if(_useImpactFrame)
            targetMaterial.SetFloat("_ImpactFrame",_impactFrame);
        if(_useBrightness)
            targetMaterial.SetFloat("_Brightness",_brightness);
        if(_useSaturation)
            targetMaterial.SetFloat("_Saturation",_saturation);
        if(_useBloom)
            targetMaterial.SetFloat("_Bloom",_bloom);
        if(_useVignette)
            targetMaterial.SetFloat("_Vignette",_vignette);
        if (_useContrast)
            targetMaterial.SetFloat("_Contrast", _contrast);
        if (_useContrastTarget)
            targetMaterial.SetFloat("_ContrastTarget", _contrastTarget);
        if (_useColorTint)
            targetMaterial.SetColor("_ColorTint",_colorTint);
        if(_useBackgroundColorTint)
            targetMaterial.SetColor("_BackgroundColorTint",_backgroundColorTint);
        if(_useForwardScreenColorTint)
            targetMaterial.SetColor("_ForwardScreenColorTint",_forwardScreenColorTint);
        
        if(_useBlurSize)
            targetMaterial.SetFloat("_BlurSize",_blurSize);
        if(_useMultiSampleDistance)
            targetMaterial.SetFloat("_MultiSampleDistance",_multiSampleDistance);
        if(_useMultiSampleColorTintRight)
            targetMaterial.SetColor("_MultiSampleColorTintRight",_multiSampleColorTintRight);
        if(_useMultiSampleColorTintLeft)
            targetMaterial.SetColor("_MultiSampleColorTintLeft",_multiSampleColorTintLeft);

        if(_useFogRate)
            targetMaterial.SetFloat("_FogRate",_fogRate);
        if(_useFogStrength)
            targetMaterial.SetFloat("_FogStrength",_fogStrength);
        if(_useFogColor)
            targetMaterial.SetColor("_FogColor",_fogColor);
        if(_useBackgroundTransitionRate)
            targetMaterial.SetFloat("_CrossFillFactor", _backgroundTransitionRate);

        if(_useCurvature)
            targetMaterial.SetFloat("_Curvature", _curvature);
        if(_useScanLine)
            targetMaterial.SetFloat("_Scanline", _scanLine);

        targetMaterial.SetFloat("PixelSnap",_pixelSnap ? 1f : 0f);
    }

    public void syncValueFromMaterial(Material targetMaterial)
    {
        _sunAngle = targetMaterial.GetFloat("_SunAngle");
        _shadowDistance = targetMaterial.GetFloat("_ShadowDistance");
        _shadowDistanceRatio = targetMaterial.GetFloat("_ShadowDistanceRatio");

        _screenSize = targetMaterial.GetVector("_ScreenSize");
        _shadowDistanceOffset = targetMaterial.GetFloat("_ShadowDistanceOffset");
        _shadowColor = targetMaterial.GetColor("_ShadowColor");

        _brightness = targetMaterial.GetFloat("_Brightness");
        _saturation = targetMaterial.GetFloat("_Saturation");
        _bloom = targetMaterial.GetFloat("_Bloom");
        _vignette = targetMaterial.GetFloat("_Vignette");
        _contrast = targetMaterial.GetFloat("_contrast");
        _contrastTarget = targetMaterial.GetFloat("_ContrastTarget");
        _colorTint = targetMaterial.GetColor("_ColorTint");
        _backgroundColorTint = targetMaterial.GetColor("_BackgroundColorTint");
        _forwardScreenColorTint = targetMaterial.GetColor("_ForwardScreenColorTint");

        _blurSize = targetMaterial.GetFloat("_BlurSize");
        _multiSampleDistance = targetMaterial.GetFloat("_MultiSampleDistance");
        _multiSampleColorTintRight = targetMaterial.GetColor("_MultiSampleColorTintRight");
        _multiSampleColorTintLeft = targetMaterial.GetColor("_MultiSampleColorTintLeft");

        _fogRate = targetMaterial.GetFloat("_FogRate");
        _fogStrength = targetMaterial.GetFloat("_FogStrength");
        _fogColor = targetMaterial.GetColor("_FogColor");

        _curvature = targetMaterial.GetFloat("_Curvature");
        _scanLine = targetMaterial.GetFloat("_ScanLine");

        _pixelSnap = targetMaterial.GetFloat("PixelSnap") == 1f;
    }

    public void blend(PostProcessProfile destination, float ratio)
    {
        if(destination._profileData._useSunAngle)
            _sunAngle                   = Mathf.LerpAngle(_sunAngle, destination._profileData._sunAngle, ratio);
        if(destination._profileData._useShadowDistance)
            _shadowDistance             = Mathf.Lerp(_shadowDistance, destination._profileData._shadowDistance, ratio);
        if(destination._profileData._useShadowDistanceRatio)
            _shadowDistanceRatio        = Mathf.Lerp(_shadowDistanceRatio, destination._profileData._shadowDistanceRatio, ratio);
        if(destination._profileData._useScreenSize)
            _screenSize                 = Vector2.Lerp(_screenSize, destination._profileData._screenSize, ratio);
        if(destination._profileData._useShadowDistanceOffset)
            _shadowDistanceOffset       = Mathf.Lerp(_shadowDistanceOffset, destination._profileData._shadowDistanceOffset, ratio);
        if(destination._profileData._useShadowColor)
            _shadowColor                = Color.Lerp(_shadowColor, destination._profileData._shadowColor, ratio);
        if(destination._profileData._useImpactFrame)
            _impactFrame                = Mathf.Lerp(_impactFrame, destination._profileData._impactFrame, ratio);//ratio >= 0.5f ? destination._profileData._impactFrame : _impactFrame;
        if(destination._profileData._useBrightness)
            _brightness                 = Mathf.Lerp(_brightness, destination._profileData._brightness, ratio);
        if(destination._profileData._useSaturation)
            _saturation                 = Mathf.Lerp(_saturation, destination._profileData._saturation, ratio);
        if(destination._profileData._useBloom)
            _bloom                      = Mathf.Lerp(_bloom, destination._profileData._bloom, ratio);
        if(destination._profileData._useVignette)
            _vignette                   = Mathf.Lerp(_vignette, destination._profileData._vignette, ratio);
        if (destination._profileData._useContrast)
            _contrast = Mathf.Lerp(_contrast, destination._profileData._contrast, ratio);
        if (destination._profileData._useContrastTarget)
            _contrastTarget = Mathf.Lerp(_contrastTarget, destination._profileData._contrastTarget, ratio);
        if (destination._profileData._useColorTint)
            _colorTint                  = Color.Lerp(_colorTint, destination._profileData._colorTint, ratio);
        if(destination._profileData._useBackgroundColorTint)
            _backgroundColorTint        = Color.Lerp(_backgroundColorTint, destination._profileData._backgroundColorTint, ratio);
        if(destination._profileData._useForwardScreenColorTint)
            _forwardScreenColorTint     = Color.Lerp(_forwardScreenColorTint, destination._profileData._forwardScreenColorTint, ratio);
        if(destination._profileData._useBlurSize)
            _blurSize                   = Mathf.Lerp(_blurSize, destination._profileData._blurSize, ratio);
        if(destination._profileData._useMultiSampleDistance)
            _multiSampleDistance        = Mathf.Lerp(_multiSampleDistance, destination._profileData._multiSampleDistance, ratio);
        if(destination._profileData._useMultiSampleColorTintRight)
            _multiSampleColorTintRight  = Color.Lerp(_multiSampleColorTintRight, destination._profileData._multiSampleColorTintRight, ratio);
        if(destination._profileData._useMultiSampleColorTintLeft)
            _multiSampleColorTintLeft   = Color.Lerp(_multiSampleColorTintLeft, destination._profileData._multiSampleColorTintLeft, ratio);
        if(destination._profileData._useFogRate)
            _fogRate                    = Mathf.Lerp(_fogRate, destination._profileData._fogRate, ratio);
        if(destination._profileData._useFogStrength)
            _fogStrength                = Mathf.Lerp(_fogStrength, destination._profileData._fogStrength, ratio);
        if(destination._profileData._useFogColor)
            _fogColor                   = Color.Lerp(_fogColor, destination._profileData._fogColor, ratio);
        if(destination._profileData._useBackgroundTransitionRate)
            _backgroundTransitionRate   = Mathf.Lerp(_backgroundTransitionRate, destination._profileData._backgroundTransitionRate, ratio);
        if(destination._profileData._useCurvature)
            _curvature                  = Mathf.Lerp(_curvature, destination._profileData._curvature, ratio);
        if(destination._profileData._useScanLine)
            _scanLine                   = Mathf.Lerp(_scanLine, destination._profileData._scanLine, ratio);

        _pixelSnap                  = ratio >= 0.5f ? destination._profileData._pixelSnap : _pixelSnap;
    }

    public void blendCopy(PostProcessProfileData source, PostProcessProfileData destination, float ratio)
    {
        if(destination._useSunAngle)
            _sunAngle                   = Mathf.LerpAngle(source._sunAngle, destination._sunAngle, ratio);
        if(destination._useShadowDistance)
            _shadowDistance             = Mathf.Lerp(source._shadowDistance, destination._shadowDistance, ratio);
        if(destination._useShadowDistanceRatio)
            _shadowDistanceRatio        = Mathf.Lerp(source._shadowDistanceRatio, destination._shadowDistanceRatio, ratio);
        if(destination._useScreenSize)
            _screenSize                 = Vector2.Lerp(source._screenSize, destination._screenSize, ratio);
        if(destination._useShadowDistanceOffset)
            _shadowDistanceOffset       = Mathf.Lerp(source._shadowDistanceOffset, destination._shadowDistanceOffset, ratio);
        if(destination._useShadowColor)
            _shadowColor                = Color.Lerp(source._shadowColor, destination._shadowColor, ratio);
        if(destination._useImpactFrame)
            _impactFrame                = Mathf.Lerp(_impactFrame, destination._impactFrame, ratio);//ratio >= 0.5f ? destination._impactFrame : source._impactFrame;
        if(destination._useBrightness)
            _brightness                 = Mathf.Lerp(source._brightness, destination._brightness, ratio);
        if(destination._useSaturation)
            _saturation                 = Mathf.Lerp(source._saturation, destination._saturation, ratio);
        if(destination._useBloom)
            _bloom                      = Mathf.Lerp(source._bloom, destination._bloom, ratio);
        if(destination._useVignette)
            _vignette                   = Mathf.Lerp(source._vignette, destination._vignette, ratio);
        if (destination._useContrast)
            _contrast = Mathf.Lerp(source._contrast, destination._contrast, ratio);
        if (destination._useContrastTarget)
            _contrastTarget = Mathf.Lerp(source._contrastTarget, destination._contrastTarget, ratio);
        if (destination._useColorTint)
            _colorTint                  = Color.Lerp(source._colorTint, destination._colorTint, ratio);
        if(destination._useBackgroundColorTint)
            _backgroundColorTint        = Color.Lerp(source._backgroundColorTint, destination._backgroundColorTint, ratio);
        if(destination._useForwardScreenColorTint)
            _forwardScreenColorTint     = Color.Lerp(source._forwardScreenColorTint, destination._forwardScreenColorTint, ratio);
        if(destination._useBlurSize)
            _blurSize                   = Mathf.Lerp(source._blurSize, destination._blurSize, ratio);
        if(destination._useMultiSampleDistance)
            _multiSampleDistance        = Mathf.Lerp(source._multiSampleDistance, destination._multiSampleDistance, ratio);
        if(destination._useMultiSampleColorTintRight)
            _multiSampleColorTintRight  = Color.Lerp(source._multiSampleColorTintRight, destination._multiSampleColorTintRight, ratio);
        if(destination._useMultiSampleColorTintLeft)
            _multiSampleColorTintLeft   = Color.Lerp(source._multiSampleColorTintLeft, destination._multiSampleColorTintLeft, ratio);
        if(destination._useFogRate)
            _fogRate                    = Mathf.Lerp(source._fogRate, destination._fogRate, ratio);
        if(destination._useFogStrength)
            _fogStrength                = Mathf.Lerp(source._fogStrength, destination._fogStrength, ratio);
        if(destination._useFogColor)
            _fogColor                   = Color.Lerp(source._fogColor, destination._fogColor, ratio);
        if(destination._useBackgroundTransitionRate)
            _backgroundTransitionRate   = Mathf.Lerp(source._backgroundTransitionRate, destination._backgroundTransitionRate, ratio);
        if(destination._useCurvature)
            _curvature                  = Mathf.Lerp(source._curvature, destination._curvature, ratio);
        if(destination._useScanLine)
            _scanLine                   = Mathf.Lerp(source._scanLine, destination._scanLine, ratio);

        _pixelSnap                      = ratio >= 0.5f ? destination._pixelSnap : source._pixelSnap;
    }

    public void copy(PostProcessProfile profile)
    {
        _useSunAngle                    = profile._profileData._useSunAngle;
        _useShadowDistance              = profile._profileData._useShadowDistance;
        _useShadowDistanceRatio         = profile._profileData._useShadowDistanceRatio;
        _useScreenSize                  = profile._profileData._useScreenSize;
        _useShadowDistanceOffset        = profile._profileData._useShadowDistanceOffset;
        _useShadowColor                 = profile._profileData._useShadowColor;
        _useImpactFrame                 = profile._profileData._useImpactFrame;
        _useBrightness                  = profile._profileData._useBrightness;
        _useSaturation                  = profile._profileData._useSaturation;
        _useColorTint                   = profile._profileData._useColorTint;
        _useBackgroundColorTint         = profile._profileData._useBackgroundColorTint;
        _useForwardScreenColorTint      = profile._profileData._useForwardScreenColorTint;
        _useBlurSize                    = profile._profileData._useBlurSize;
        _useMultiSampleDistance         = profile._profileData._useMultiSampleDistance;
        _useMultiSampleColorTintRight   = profile._profileData._useMultiSampleColorTintRight;
        _useMultiSampleColorTintLeft    = profile._profileData._useMultiSampleColorTintLeft;
        _useFogRate                     = profile._profileData._useFogRate;
        _useFogStrength                 = profile._profileData._useFogStrength;
        _useFogColor                    = profile._profileData._useFogColor;
        _useBackgroundTransitionRate    = profile._profileData._useBackgroundTransitionRate;

        if(_useSunAngle)
            _sunAngle                   = profile._profileData._sunAngle;
        if(_useShadowDistance)
            _shadowDistance             = profile._profileData._shadowDistance ;
        if(_useShadowDistanceRatio)
            _shadowDistanceRatio        = profile._profileData._shadowDistanceRatio;
        if(_useScreenSize)
            _screenSize                 = profile._profileData._screenSize;
        if(_useShadowDistanceOffset)
            _shadowDistanceOffset       = profile._profileData._shadowDistanceOffset;
        if(_useShadowColor)
            _shadowColor                = profile._profileData._shadowColor;
        if(_useImpactFrame)
            _impactFrame                = profile._profileData._impactFrame;
        if(_useBrightness)
            _brightness                 = profile._profileData._brightness;
        if(_useSaturation)
            _saturation                 = profile._profileData._saturation;
        if(_useBloom)
            _bloom                      = profile._profileData._bloom;
        if(_useVignette)
            _vignette                   = profile._profileData._vignette;
        if (_useContrast)
            _contrast = profile._profileData._contrast;
        if (_useContrastTarget)
            _contrastTarget = profile._profileData._contrastTarget;
        if (_useColorTint)
            _colorTint                  = profile._profileData._colorTint;
        if(_useBackgroundColorTint)
            _backgroundColorTint        = profile._profileData._backgroundColorTint;
        if(_useForwardScreenColorTint)
            _forwardScreenColorTint     = profile._profileData._forwardScreenColorTint;
        if(_useBlurSize)
            _blurSize                   = profile._profileData._blurSize;
        if(_useMultiSampleDistance)
            _multiSampleDistance        = profile._profileData._multiSampleDistance;
        if(_useMultiSampleColorTintRight)
            _multiSampleColorTintRight  = profile._profileData._multiSampleColorTintRight;
        if(_useMultiSampleColorTintLeft)
            _multiSampleColorTintLeft   = profile._profileData._multiSampleColorTintLeft;
        if(_useFogRate)
            _fogRate                    = profile._profileData._fogRate;
        if(_useFogStrength)
            _fogStrength                = profile._profileData._fogStrength;
        if(_useFogColor)
            _fogColor                   = profile._profileData._fogColor;
        if(_useBackgroundTransitionRate)
            _backgroundTransitionRate   = profile._profileData._backgroundTransitionRate;
        if(_useCurvature)
            _curvature                  = profile._profileData._curvature;
        if(_useScanLine)
            _scanLine                   = profile._profileData._scanLine;

        _pixelSnap                  = profile._profileData._pixelSnap;
    }
}

[CreateAssetMenu(fileName = "PostProcessProfile", menuName = "Scriptable Object/PostProcessProfile", order = 1)]
public class PostProcessProfile : ScriptableObject
{
    public PostProcessProfileData _profileData = new PostProcessProfileData();
}


#if UNITY_EDITOR

[CustomEditor(typeof(PostProcessProfile))]
public class PostProcessProfileEditor : Editor
{
    private PostProcessProfile controll;
    private bool _editMode = false;
    private Material _targetMaterial = null;

    void OnEnable()
    {
        controll = (PostProcessProfile)target;
    }

    public override void OnInspectorGUI()
    {
        bool isChange = false;
        isChange |= floatSlider("Sun Angle", ref controll._profileData._sunAngle, 0f, 360f,ref controll._profileData._useSunAngle);
        isChange |= floatSlider("Shadow Distance", ref controll._profileData._shadowDistance, 0.1f, 3f,ref controll._profileData._useShadowDistance);
        isChange |= floatSlider("Shadow Distance Ratio", ref controll._profileData._shadowDistanceRatio, 0f, 10f,ref controll._profileData._useShadowDistanceRatio);

        GUILayout.Space(20f);
        isChange |= vector2Field("Screen Size",ref controll._profileData._screenSize,ref controll._profileData._useScreenSize);
        isChange |= floatSlider("Shadow Distance Offset",ref controll._profileData._shadowDistanceOffset, -100f, 100f,ref controll._profileData._useShadowDistanceOffset);
        isChange |= colorPicker("Shadow Color",ref controll._profileData._shadowColor,ref controll._profileData._useShadowColor);

        GUILayout.Space(20f);
        isChange |= floatSlider("ImpactFrame",ref controll._profileData._impactFrame, 0f, 1f,ref controll._profileData._useImpactFrame);
        isChange |= floatSlider("Brightness",ref controll._profileData._brightness, 0f, 5f,ref controll._profileData._useBrightness);
        isChange |= floatSlider("Saturation",ref controll._profileData._saturation, 0f, 1f,ref controll._profileData._useSaturation);
        isChange |= floatSlider("Vignette",ref controll._profileData._vignette, 0f, 1f,ref controll._profileData._useVignette);
        isChange |= floatSlider("Background Bloom",ref controll._profileData._bloom, 0f, 2f,ref controll._profileData._useBloom);
        isChange |= floatSlider("Background Contrast", ref controll._profileData._contrast, 0f, 1f, ref controll._profileData._useContrast);
        isChange |= floatSlider("Background Contrast Target", ref controll._profileData._contrastTarget, 0f, 1f, ref controll._profileData._useContrastTarget);
        isChange |= colorPicker("Character Color Tint",ref controll._profileData._colorTint,ref controll._profileData._useColorTint);
        isChange |= colorPicker("Background Color Tint",ref controll._profileData._backgroundColorTint,ref controll._profileData._useBackgroundColorTint);
        isChange |= colorPicker("Forward Screen Color Tint",ref controll._profileData._forwardScreenColorTint,ref controll._profileData._useForwardScreenColorTint);

        GUILayout.Space(20f);
        isChange |= floatSlider("Blur Size",ref controll._profileData._blurSize, 0f, 2f,ref controll._profileData._useBlurSize);
        isChange |= floatSlider("MultiSample Distance",ref controll._profileData._multiSampleDistance, 0f, 5f,ref controll._profileData._useMultiSampleDistance);
        isChange |= colorPicker("MultiSample Color Tint Right",ref controll._profileData._multiSampleColorTintRight,ref controll._profileData._useMultiSampleColorTintRight);
        isChange |= colorPicker("MultiSample Color Tint Left",ref controll._profileData._multiSampleColorTintLeft,ref controll._profileData._useMultiSampleColorTintLeft);

        GUILayout.Space(20f);
        isChange |= floatSlider("Fog Rate",ref controll._profileData._fogRate, 0f, 1f,ref controll._profileData._useFogRate);
        isChange |= floatSlider("Fog Strength",ref controll._profileData._fogStrength, 0f, 1f,ref controll._profileData._useFogStrength);
        isChange |= colorPicker("Fog Color",ref controll._profileData._fogColor,ref controll._profileData._useFogColor);

        GUILayout.Space(20f);
        isChange |= floatField("Background Transition Rate", ref controll._profileData._backgroundTransitionRate, ref controll._profileData._useBackgroundTransitionRate);

        GUILayout.Space(20f);
        isChange |= floatSlider("Curvature", ref controll._profileData._curvature,0f, 2f, ref controll._profileData._useCurvature);
        isChange |= floatSlider("ScanLine", ref controll._profileData._scanLine, 0f, 1f, ref controll._profileData._useScanLine);

        if(isChange)
        {
            syncValueToMaterial();
            EditorUtility.SetDirty(controll);
        }

        GUILayout.Space(20f);

        Color guiColor = GUI.color;
        GUI.color = _editMode ? Color.green : Color.red;
        if(GUILayout.Button(_editMode ? "Exit Edit Mode" :"Edit PostProcess"))
        {
            _editMode = !_editMode;
            syncValueToMaterial();
        }
        GUI.color = guiColor;

        if(GUILayout.Button("Get Value From Material"))
        {
            controll._profileData.syncValueFromMaterial(PostProcessProfileControl.getPostProcessMaterial(true));
            EditorUtility.SetDirty(controll);
        }
    }

    public bool floatSlider(string title, ref float value, float min, float max, ref bool active)
    {
        GUILayout.BeginHorizontal();
        bool activeDiff = EditorGUILayout.Toggle(active,GUILayout.Width(20f));
        GUI.enabled = active;
        float beforeValue = EditorGUILayout.Slider(title, value,min,max);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        if(beforeValue != value)
        {
            value = beforeValue;
            return true;
        }

        if(activeDiff != active)
        {
            active = activeDiff;
            return true;
        }

        return false;
    }

    public bool floatField(string title, ref float value, ref bool active)
    {
        GUILayout.BeginHorizontal();
        bool activeDiff = EditorGUILayout.Toggle(active,GUILayout.Width(20f));
        GUI.enabled = active;
        float beforeValue = EditorGUILayout.FloatField(title, value);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        if(beforeValue != value)
        {
            value = beforeValue;
            return true;
        }

        if(activeDiff != active)
        {
            active = activeDiff;
            return true;
        }

        return false;
    }

    public bool vector2Field(string title, ref Vector2 value, ref bool active)
    {
        GUILayout.BeginHorizontal();
        bool activeDiff = EditorGUILayout.Toggle(active,GUILayout.Width(20f));
        GUI.enabled = active;
        Vector2 afterValue = EditorGUILayout.Vector2Field(title,value);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        if(value != afterValue)
        {
            value = afterValue;
            return true;
        }

        if(activeDiff != active)
        {
            active = activeDiff;
            return true;
        }

        return false;
    }

    public bool colorPicker(string title, ref Color color, ref bool active)
    {
        GUILayout.BeginHorizontal();
        bool activeDiff = EditorGUILayout.Toggle(active,GUILayout.Width(20f));
        GUI.enabled = activeDiff;
        Color beforeColor = EditorGUILayout.ColorField(title, color);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        
        if(activeDiff != active)
        {
            active = activeDiff;
            return true;
        }
        
        if(MathEx.equals(beforeColor,color,0f) == false)
        {
            color = beforeColor;
            return true;
        }

        return false;
    }

    public bool materialToggle(string title, ref float toggle, ref bool active)
    {
        GUILayout.BeginHorizontal();
        bool activeDiff = EditorGUILayout.Toggle(active,GUILayout.Width(20f));
        GUI.enabled = active;
        bool realToggle = toggle == 1f;
        bool beforeToggle = EditorGUILayout.Toggle(title,realToggle);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        if(realToggle != beforeToggle)
        {
            toggle = beforeToggle ? 1f : 0f;
            return true;
        }

        if(activeDiff != active)
        {
            active = activeDiff;
            return true;
        }

        return false;
    }

    public void syncValueToMaterial()
    {
        if(_editMode == false)
            return;

        if(_targetMaterial == null)
        {
            GameObject targetGameObject = GameObject.FindGameObjectWithTag("ScreenResultMesh");
            if(targetGameObject == null)
                return;

            MeshRenderer targetMeshRenderer = targetGameObject.GetComponent<MeshRenderer>();
            if(targetMeshRenderer == null)
                return;

            List<Material> sharedMaterial = new List<Material>();
            targetMeshRenderer.GetSharedMaterials(sharedMaterial);
            _targetMaterial = sharedMaterial[0];
        }

        controll._profileData.syncValueToMaterial(_targetMaterial);
    }
}

#endif