using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "AnimationRotationPreset", menuName = "Scriptable Object/Animation Rotation Preset", order = int.MaxValue)]
public class AnimationRotationPreset : ScriptableObject
{
    [SerializeField]private List<AnimationRotationPresetData> _presetData = new List<AnimationRotationPresetData>();
    private Dictionary<string, AnimationRotationPresetData> _presetCache = new Dictionary<string, AnimationRotationPresetData>();
    private bool _isCacheConstructed = false;

    // private void Awake()
    // {
    //     constructPresetCache();
    // }

    public void addPresetData(AnimationRotationPresetData data)
    {
        _presetData.Add(data);
    }

    public AnimationRotationPresetData getPresetData(string targetName)
    {
        AnimationRotationPresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(AnimationRotationPresetData item in _presetData)
            {
                if(item.getName() == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil._ignoreThrowException = true;
        DebugUtil.assert(target != null,"해당 애니메이션 로테이션 프리셋 데이터가 존재하지 않습니다. 이름을 잘못 쓰지 않았나요? : {0}",targetName);

        DebugUtil._ignoreThrowException = false;
        return target;
    }


    private void constructPresetCache()
    {
        if(_isCacheConstructed == true)
            return;

        foreach(AnimationRotationPresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}


#if UNITY_EDITOR

[CustomEditor(typeof(AnimationRotationPreset))]
public class AnimationRotationPresetEditor : Editor
{
    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";
    AnimationRotationPreset controll;

	void OnEnable()
    {
        controll = (AnimationRotationPreset)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Animation Rotation Preset Search", EditorStyles.boldLabel);
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

        GUILayout.Space(10f);
        if(GUILayout.Button("Add 0 ~ 360 Linear"))
        {
            AnimationRotationPresetData presetDataButton = new AnimationRotationPresetData();
            presetDataButton._name = "NewPreset";
            presetDataButton._rotationCurve = new AnimationCurve();
            presetDataButton._rotationCurve.AddKey(0f,0f);
            presetDataButton._rotationCurve.AddKey(1f,360f);

            controll.addPresetData(presetDataButton);
            EditorUtility.SetDirty(controll);
        }
    }

}


#endif
