using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PostProcessProfile", menuName = "Scriptable Object/PostProcessProfile", order = 1)]
public class PostProcessProfile : ScriptableObject
{
    public float _sunAngle = 0f;
    public float _shadowDistance = 0f;
    public float _shadowDistanceRatio = 0f;

    public Vector2 _screenSize = new Vector2();
    public float _shadowDistanceOffset = 0f;
    public Color _shadowColor = Color.white;

    public float _brightness = 0f;
    public float _saturation = 0f;
    public Color _colorTint = Color.white;
    public Color _backgroundColorTint = Color.white;

    public float _blurSize = 0f;
    public float _multiSampleDistance = 0f;
    public Color _multiSampleColorTintRight = Color.white;
    public Color _multiSampleColorTintLeft = Color.white;

    public float _fogRate = 0f;
    public float _fogStrength = 0f;
    public Color _fogColor = Color.white;

    public bool _pixelSnap = false;

    public void syncValueToMaterial(bool editMode)
    {
        GameObject targetGameObject = GameObject.FindGameObjectWithTag("ScreenResultMesh");
        if(targetGameObject == null)
            return;

        MeshRenderer targetMeshRenderer = targetGameObject.GetComponent<MeshRenderer>();
        if(targetMeshRenderer == null)
            return;

        Material targetMaterial = null;
        
        if(editMode)
        {
            List<Material> sharedMaterial = new List<Material>();
            targetMeshRenderer.GetSharedMaterials(sharedMaterial);
            targetMaterial = sharedMaterial[0];
        }
        else
        {
            targetMaterial = targetMeshRenderer.material;
        }
        
        syncValueToMaterial(targetMaterial);
    }

    public void syncValueToMaterial(Material targetMaterial)
    {
        targetMaterial.SetFloat("_SunAngle",_sunAngle);
        targetMaterial.SetFloat("_ShadowDistance",_shadowDistance);
        targetMaterial.SetFloat("_ShadowDistanceRatio",_shadowDistanceRatio);

        targetMaterial.SetVector("_ScreenSize",new Vector4(_screenSize.x,_screenSize.y,0f,0f));
        targetMaterial.SetFloat("_ShadowDistanceOffset",_shadowDistanceOffset);
        targetMaterial.SetColor("_ShadowColor",_shadowColor);

        targetMaterial.SetFloat("_Brightness",_brightness);
        targetMaterial.SetFloat("_Saturation",_saturation);
        targetMaterial.SetColor("_ColorTint",_colorTint);
        targetMaterial.SetColor("_BackgroundColorTint",_backgroundColorTint);
        
        targetMaterial.SetFloat("_BlurSize",_blurSize);
        targetMaterial.SetFloat("_MultiSampleDistance",_multiSampleDistance);
        targetMaterial.SetColor("_MultiSampleColorTintRight",_multiSampleColorTintRight);
        targetMaterial.SetColor("_MultiSampleColorTintLeft",_multiSampleColorTintLeft);

        targetMaterial.SetFloat("_FogRate",_fogRate);
        targetMaterial.SetFloat("_FogStrength",_fogStrength);
        targetMaterial.SetColor("_FogColor",_fogColor);

        targetMaterial.SetFloat("PixelSnap",_pixelSnap ? 1f : 0f);
    }
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
        isChange |= floatSlider("Sun Angle", ref controll._sunAngle, 0f, 360f);
        isChange |= floatSlider("Shadow Distance", ref controll._shadowDistance, 0.1f, 3f);
        isChange |= floatSlider("Shadow Distance Ratio", ref controll._shadowDistanceRatio, 0f, 10f);

        GUILayout.Space(20f);
        isChange |= vector2Field("Screen Size",ref controll._screenSize);
        isChange |= floatSlider("Shadow Distance Offset",ref controll._shadowDistanceOffset, 0f, 10f);
        isChange |= colorPicker("Shadow Color",ref controll._shadowColor);

        GUILayout.Space(20f);
        isChange |= floatSlider("Brightness",ref controll._brightness, 0f, 10f);
        isChange |= floatSlider("Saturation",ref controll._saturation, 0f, 10f);
        isChange |= colorPicker("Color Tint",ref controll._colorTint);
        isChange |= colorPicker("Background Color Tint",ref controll._backgroundColorTint);

        GUILayout.Space(20f);
        isChange |= floatSlider("Blur Size",ref controll._blurSize, 0f, 10f);
        isChange |= floatSlider("MultiSample Distance",ref controll._multiSampleDistance, 0f, 10f);
        isChange |= colorPicker("MultiSample Color Tint Right",ref controll._multiSampleColorTintRight);
        isChange |= colorPicker("MultiSample Color Tint Left",ref controll._multiSampleColorTintLeft);

        GUILayout.Space(20f);
        isChange |= floatSlider("Fog Rate",ref controll._fogRate, 0f, 10f);
        isChange |= floatSlider("Fog Strength",ref controll._fogStrength, 0f, 10f);
        isChange |= colorPicker("Fog Color",ref controll._fogColor);

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
    }

    public bool floatSlider(string title, ref float value, float min, float max)
    {
        float beforeValue = EditorGUILayout.Slider(title, value,min,max);
        if(beforeValue != value)
        {
            value = beforeValue;
            return true;
        }

        return false;
    }

    public bool vector2Field(string title, ref Vector2 value)
    {
        Vector2 afterValue = EditorGUILayout.Vector2Field(title,value);
        if(value != afterValue)
        {
            value = afterValue;
            return true;
        }

        return false;
    }

    public bool colorPicker(string title, ref Color color)
    {
        Color beforeColor = EditorGUILayout.ColorField(title, color);
        if(MathEx.equals(beforeColor,color,0f) == false)
        {
            color = beforeColor;
            return true;
        }

        return false;
    }

    public bool materialToggle(string title, ref float toggle)
    {
        bool realToggle = toggle == 1f;
        bool beforeToggle = EditorGUILayout.Toggle(title,realToggle);
        if(realToggle != beforeToggle)
        {
            toggle = beforeToggle ? 1f : 0f;
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

        controll.syncValueToMaterial(_targetMaterial);
    }
}

#endif