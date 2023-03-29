
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AnimationCustomPreset", menuName = "Scriptable Object/Animation Custom Preset", order = int.MaxValue)]
public class AnimationCustomPreset : ScriptableObject
{
    public AnimationCustomPresetData _animationCustomPresetData;
}

#if UNITY_EDITOR

[CustomEditor(typeof(AnimationCustomPreset))]
public class AnimationCustomPresetEditor : Editor
{
    AnimationCustomPreset controll;

    private float _fps = 0f;

	void OnEnable()
    {
        controll = (AnimationCustomPreset)target;
    }

    public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

        GUILayout.Space(10f);
        if(GUILayout.Button("Calculate Total Duration"))
        {
            float totalDuration = 0f;
            for(int index = 0; index < controll._animationCustomPresetData._duration.Length; ++index)
            {
                totalDuration += controll._animationCustomPresetData._duration[index];
            }

            controll._animationCustomPresetData._totalDuration = totalDuration;

            EditorUtility.SetDirty(controll);
        }

        EditorGUILayout.BeginHorizontal();

        _fps = EditorGUILayout.FloatField(_fps);
        if(GUILayout.Button("Set Duration From FPS"))
        {
            float perFrame = 1f / _fps;
            for(int index = 0; index < controll._animationCustomPresetData._duration.Length; ++index)
            {
                controll._animationCustomPresetData._duration[index] = perFrame;
            }

            controll._animationCustomPresetData._totalDuration = (float)controll._animationCustomPresetData._duration.Length * perFrame;
        }

        EditorGUILayout.EndHorizontal();

        
    }
}


#endif
