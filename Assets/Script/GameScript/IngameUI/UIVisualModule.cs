using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class UIVisualModule
{
    public Image Image;
    public Vector2 BasePosition;
    public class UIAnimationPack
    {
        public AnimationCustomPreset animationCustomPreset;
        public string spriteFolderPath;
        public UIAnimationPack(AnimationCustomPreset animationCustomPreset, string spriteFolderPath)
        {
            this.animationCustomPreset = animationCustomPreset;
            this.spriteFolderPath = spriteFolderPath;
        }
    }
    private Dictionary<int, UIAnimationPack> _uiAnimationData = new Dictionary<int, UIAnimationPack>();
    private AnimationPlayer _animationPlayer = new AnimationPlayer();



    public void SetFromData<TUIStateType>(UIVisualModuleData<TUIStateType> data) where TUIStateType : Enum
    {
        if (data == null)
        {
            Debug.LogWarning($"데이터 없음!");
            return;
        }
        this.Image = data.Image;
        this.BasePosition = data.Image.rectTransform.anchoredPosition;

        Debug.Log($"{Image}");

        foreach (var singleData in data.UIAnimationPackDataList)
        {
            int key = Convert.ToInt32(singleData.StateType);
            if (!_uiAnimationData.ContainsKey(key))
                _uiAnimationData.Add(key, singleData.UIAnimationPack);
            else
                Debug.LogWarning($"중복 UI 비주얼 모듈 데이터 감지");
        }
    }
    public void PlayAnimation()
    {

    }
}


[System.Serializable]
public class UIVisualModuleData<TUIStateType> where TUIStateType : Enum
{
    public Image Image;
    [System.Serializable]
    public class UIAnimationPackData
    {
        public TUIStateType StateType;
        public UIVisualModule.UIAnimationPack UIAnimationPack;
    }
    public List<UIAnimationPackData> UIAnimationPackDataList = new List<UIAnimationPackData>();
}