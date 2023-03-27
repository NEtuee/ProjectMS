
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
    }
}


#endif
