using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostProcessProfileApplyType
{
    BaseBlend,
    OneShot,
    OneShotAdditional,
}

public class PostProcessProfileControl
{   
    private class ProfileBlender
    {
        public PostProcessProfile _sourceLayer;
        public PostProcessProfileData _tempData = new PostProcessProfileData();

        public float _blendTime = 0f;
        public MathEx.EaseType _easeType = MathEx.EaseType.Linear;
        private float _timer = 0f;

        public bool blend(ref PostProcessProfileData postProcessProfileData, float deltaTime, bool reverse)
        {
            _timer += deltaTime;
            float blendRate = _timer * (1f / _blendTime);
            blendRate = MathEx.clamp01f(blendRate);

            if(reverse)
                blendRate = 1f - blendRate;

            postProcessProfileData.blend(_sourceLayer,MathEx.getEaseFormula(_easeType, 0f, 1f, blendRate));
            return isEnd();
        }

        public bool addBlend(ref PostProcessProfileData postProcessProfileData, float deltaTime, bool reverse)
        {
            _timer += deltaTime;
            float blendRate = _timer * (1f / _blendTime);
            blendRate = MathEx.clamp01f(blendRate);

            if(reverse)
                blendRate = 1f - blendRate;

            _tempData.copy(postProcessProfileData);
            _tempData.add(_sourceLayer);

            postProcessProfileData.blend(_tempData,MathEx.getEaseFormula(_easeType, 0f, 1f, blendRate));
            return isEnd();
        }

        public bool isEnd()
        {
            return _timer >= _blendTime;
        }

        public void setProfileData(PostProcessProfile profile, MathEx.EaseType easeType, float blendTime)
        {
            _sourceLayer = profile;
            _easeType = easeType;
            _blendTime = blendTime;
            _timer = 0f;
        }
    }
    
    private SimplePool<ProfileBlender>  _profileBlenderPool = new SimplePool<ProfileBlender>();
    private List<ProfileBlender>        _baseBlendingProfileList = new List<ProfileBlender>();
    private ProfileBlender              _oneShotEffectProfile = new ProfileBlender();
    private ProfileBlender              _oneShotAdditionalEffectProfile = new ProfileBlender();

    private PostProcessProfileData      _resultData = new PostProcessProfileData();

    private Material                    _targetMaterial;

    private bool                        _isBlending = false;
    private int                         _currentOneShotBlendingOrder = 0;
    private int                         _currentOneShotAdditionalBlendingOrder = 0;

    static public Material getPostProcessMaterial(bool editMode)
    {
        GameObject targetGameObject = GameObject.FindGameObjectWithTag("ScreenResultMesh");
        if(targetGameObject == null)
            return null;

        MeshRenderer targetMeshRenderer = targetGameObject.GetComponent<MeshRenderer>();
        if(targetMeshRenderer == null)
            return null;

        if(editMode)
        {
            List<Material> sharedMaterial = new List<Material>();
            targetMeshRenderer.GetSharedMaterials(sharedMaterial);
            return sharedMaterial[0];
        }
        else
        {
            return targetMeshRenderer.material;
        }
    }

    public void syncShadowValueToMaterial(Material material)
    {
        _resultData.syncShadowValueToMaterial(material);
    }

    public void updateMaterial(bool editMode)
    {
        _targetMaterial = getPostProcessMaterial(editMode);
    }

    public void processBlend(float deltaTime)
    {
        if(_baseBlendingProfileList.Count == 0)
            return;

        if(_isBlending == false && _oneShotEffectProfile.isEnd() && _oneShotAdditionalEffectProfile.isEnd())
            return;

        _resultData.copy(_baseBlendingProfileList[0]._sourceLayer);
        int resultIndex = 0;
        for(int baseBlendIndex = 1; baseBlendIndex < _baseBlendingProfileList.Count; ++ baseBlendIndex)
        {
            if(_baseBlendingProfileList[baseBlendIndex].blend(ref _resultData, deltaTime, false))
                resultIndex = baseBlendIndex;
        }

        for(int index = 0; index < resultIndex; ++index)
        {
            _baseBlendingProfileList.RemoveAt(0);
        }

        if(_oneShotAdditionalEffectProfile.isEnd() == false)
            _oneShotAdditionalEffectProfile.addBlend(ref _resultData, deltaTime, true);

        if(_oneShotEffectProfile.isEnd() == false)
            _oneShotEffectProfile.blend(ref _resultData, deltaTime, true);

        if(_isBlending)
            _resultData.syncValueToMaterial(_targetMaterial);

        _isBlending = _baseBlendingProfileList.Count > 1 || _oneShotEffectProfile.isEnd() == false || _oneShotAdditionalEffectProfile.isEnd() == false;
    }

    public void applyCenterUVPosition(Vector2 centerUV)
    {
        _targetMaterial.SetVector("_CenterUV", centerUV);
    }

    public void setOneShotEffectProfile(PostProcessProfile profile, MathEx.EaseType easeType, int order, float blendTime)
    {
        if(_oneShotEffectProfile.isEnd() == false && _currentOneShotBlendingOrder > order)
            return;
        
        if(MasterManager.instance._stageProcessor._stageData == null)
            return;

        if(_baseBlendingProfileList.Count == 0)
        {
            DebugUtil.assert(false,"Base Blend PPP가 없습니다. 스테이지 시작 시퀀스에 추가해 주세요 [Stage: ]" + MasterManager.instance._stageProcessor._stageData._stageName);
            return;
        }

        _oneShotEffectProfile.setProfileData(profile,easeType,blendTime);
        _currentOneShotBlendingOrder = order;
        _isBlending = true;

        processBlend(0f);
    }

    public void setOneShotAdditionalEffectProfile(PostProcessProfile profile, MathEx.EaseType easeType, int order, float blendTime)
    {
        if(_oneShotAdditionalEffectProfile.isEnd() == false && _currentOneShotAdditionalBlendingOrder > order)
            return;
        
        if(MasterManager.instance._stageProcessor._stageData == null)
            return;

        if(_baseBlendingProfileList.Count == 0)
        {
            DebugUtil.assert(false,"Base Blend PPP가 없습니다. 스테이지 시작 시퀀스에 추가해 주세요 [Stage: ]" + MasterManager.instance._stageProcessor._stageData._stageName);
            return;
        }

        _oneShotAdditionalEffectProfile.setProfileData(profile,easeType,blendTime);
        _currentOneShotAdditionalBlendingOrder = order;
        _isBlending = true;

        processBlend(0f);
    }

    public void addBaseBlendProfile(PostProcessProfile profile, MathEx.EaseType easeType, float blendTime)
    {
        ProfileBlender blender = _profileBlenderPool.dequeue();
        blender.setProfileData(profile,easeType,blendTime);

        _baseBlendingProfileList.Add(blender);
        _isBlending = true;
    }
}
