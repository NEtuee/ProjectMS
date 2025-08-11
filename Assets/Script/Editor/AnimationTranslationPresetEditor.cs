using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationTranslationPreset))]
public class AnimationTranslationPresetEditor : Editor
{
    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Animation Translation Preset Search", EditorStyles.boldLabel);
        _searchString = EditorGUILayout.TextField("Search", _searchString);

        if (_searchStringCompare != _searchString)
        {
            if (string.IsNullOrEmpty(_searchString))
                _searchStringList = null;
            else
                _searchStringList = _searchString.ToLower().Split(' ');

            _searchStringCompare = _searchString;
        }

        EditorGUILayout.Space();

        SerializedProperty presetData = serializedObject.FindProperty("_presetData");

        if (_searchStringList == null)
        {
            EditorGUILayout.PropertyField(presetData, true);
        }
        else
        {
            EditorGUILayout.PropertyField(presetData.FindPropertyRelative("Array.size"));

            for (int i = 0; i < presetData.arraySize; i++)
            {
                SerializedProperty element = presetData.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = element.FindPropertyRelative("_name");
                string presetName = nameProperty.stringValue;

                if (string.IsNullOrEmpty(presetName))
                {
                    EditorGUILayout.PropertyField(element, new GUIContent("Element " + i), true);
                    continue;
                }

                string lowerName = presetName.ToLower();
                bool shouldShow = true;
                foreach (var searchItem in _searchStringList)
                {
                    if (!lowerName.Contains(searchItem))
                    {
                        shouldShow = false;
                        break;
                    }
                }

                if (shouldShow)
                {
                    EditorGUILayout.PropertyField(element, new GUIContent(presetName), true);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
