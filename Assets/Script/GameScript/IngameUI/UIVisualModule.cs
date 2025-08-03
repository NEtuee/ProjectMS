using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisualModule : MonoBehaviour
{
    public Image Image;
    public Vector2 BasePosition;
    private Dictionary<int, UIAnimationPack> _uiAnimationData = new Dictionary<int, UIAnimationPack>();
    private AnimationPlayer _animationPlayer;
    private UIEffectManager _uiEffectManager;



    public void Initialize()
    {
        _animationPlayer = new AnimationPlayer();
        _uiEffectManager = new UIEffectManager(this);
    }
    public void SetFromData<TUIStateType>(UIVisualModuleData<TUIStateType> data) where TUIStateType : Enum
    {
        if (data == null)
        {
            Debug.LogWarning($"데이터 없음!");
            return;
        }
        this.Image = data.Image;
        this.BasePosition = data.Image.rectTransform.anchoredPosition;

        foreach (var singleData in data.UIAnimationPackDataList)
        {
            int key = Convert.ToInt32(singleData.StateType);
            if (!_uiAnimationData.ContainsKey(key))
                _uiAnimationData.Add(key, singleData.UIAnimationPack);
            else
                Debug.LogWarning($"중복 UI 비주얼 모듈 데이터 감지");
        }
    }
    public void ShowImage()
    {
        if (Image == null)
            return;

        Color currentColor = Image.color;
        currentColor.a = 1.0f;
        Image.color = currentColor;
    }
    public void HideImage()
    {
        if (Image == null)
            return;

        Color currentColor = Image.color;
        currentColor.a = 0.0f;
        Image.color = currentColor;
    }
    public void BackToBasePosition()
    {
        Image.rectTransform.anchoredPosition = BasePosition;
    }
    public void UpdateBasePosition(Vector2 updatedPosition) //BasePosition을 바꾸는 경우에만 적용, UI애니메이션의 offset엔 적용 X
    {
        Image.rectTransform.anchoredPosition = updatedPosition;
        this.BasePosition = updatedPosition;
    }
    public void ChangeAnimation(int key)
    {
        _animationPlayer.changeAnimationByCustomPreset(_uiAnimationData[key].SpriteFolderPath, _uiAnimationData[key].AnimationCustomPreset);
    }
    public void UpdateSprite(float deltaTime)
    {
        if (_animationPlayer.isEnd() == false)
        {
            _animationPlayer.progress(deltaTime, null);
            Sprite updatedSprite = _animationPlayer.getCurrentSprite();

            if (updatedSprite != null)
                Image.sprite = updatedSprite;
        }
    }
    public IEnumerator ApplyEffectsInSequence(IEnumerator[] effectCoroutines)
    {
        if (Image == null)
            yield break;

        yield return _uiEffectManager.RunSequentialEffects(effectCoroutines);
    }
    public IEnumerator ApplyEffectsInParallel(IEnumerator[] effectCoroutines)
    {
        if (Image == null)
            yield break;

        yield return _uiEffectManager.RunParallelEffects(effectCoroutines);
    }
    public void StopAllEffects()
    {
        _uiEffectManager.StopAllRunningEffects();
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
        public UIAnimationPack UIAnimationPack;
    }
    public List<UIAnimationPackData> UIAnimationPackDataList = new List<UIAnimationPackData>();
}
[System.Serializable]
public class UIAnimationPack
{
    public AnimationCustomPreset AnimationCustomPreset;
    public string SpriteFolderPath;
    public UIAnimationPack(AnimationCustomPreset animationCustomPreset, string spriteFolderPath)
    {
        this.AnimationCustomPreset = animationCustomPreset;
        this.SpriteFolderPath = spriteFolderPath;
    }
}