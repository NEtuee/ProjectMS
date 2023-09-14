
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public enum AnimationPresetFrameEventType
{
    TimelineEffect,
    ParticleEffect,
    SpriteEffect,

}

[System.Serializable]
public class AnimationPresetFrameEventData_ParticleEffect
{
    [Tooltip("파티클 이펙트 프리팹 경로 (프리팹 이름 포함)")]
    public string              _effectPrefabPath = "";

    [Tooltip("생성 프레임")]
    public float               _startFrame = 0f;

    [Tooltip("대상(공격 대상 등)을 기준으로 이펙트 생성")]
    public bool                _toTarget = false;

    [Tooltip("이펙트 기준이 되는 오브젝트의 자식으로 Attach")]
    public bool                _attach = false;

    [Tooltip("위치 오프셋")]
    public Vector3             _spawnOffset = Vector3.zero;

    [Tooltip("방향 지정 타입 사용 안하고 캐릭터 방향으로 돌리기")]
    public bool                _followDirection = false;

    [Tooltip("방향 지정 타입. Direction은 캐릭터 방향, AttackPoint는 공격당했을 때 공격 시작 지점에서 바라보는 방향")]
    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;
}

[System.Serializable]
public class AnimationPresetFrameEventData_SpriteEffect
{
    [Tooltip("이펙트 프리셋 경로")]
    public string               _effectPresetPath = "";

    [Tooltip("생성 프레임")]
    public float               _startFrame = 0f;

    [Tooltip("각도")]
    public float                _spawnAngle = 0f;

    [Tooltip("이펙트를 소환하는 오브젝트의 각도를 따라간다")]
    public bool                 _followEntityAngle = false;

    [Tooltip("대상(공격 대상 등)을 기준으로 이펙트 생성")]
    public bool                 _toTarget = false;

    [Tooltip("이펙트 기준이 되는 오브젝트의 자식으로 Attach")]
    public bool                 _attach = false;

    [Tooltip("물리 바디를 사용한다. 설정은 PhysicsBodyDesc에서")]
    public bool                 _usePhysics = false;

    [Tooltip("이펙트를 소환하는 오브젝트의 Flip Type에 따라 물리 방향이 바뀐다.")]
    public bool                 _useFlipForPhysics = false;

    [Tooltip("이펙트의 그림자를 그린다. (레이어가 바뀌기 때문에 특정 쉐이더 적용 안될 수 있음)")]
    public bool                 _castShadow = false;

    [Tooltip("랜덤 각도 적용 여부")]
    public bool                 _randomAngleEnable = false;

    [Tooltip("랜덤 각도 min max")]
    public Vector2              _randomAngle = Vector2.zero;

    [Tooltip("위치 오프셋")]
    public Vector3              _spawnOffset = Vector3.zero;

    [Tooltip("물리 바디 셋팅")]
    public PhysicsBodyDescription _physicsBodyDesc = new PhysicsBodyDescription(null);

    [Tooltip("시간 느려지는거에 영향 받을지 여부 (ScaledDeltaTime이면 영향 받음")]
    public EffectUpdateType     _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

}

[System.Serializable]
public class AnimationPresetFrameEventData_TimelineEffect
{
    [Tooltip("타임라인 이펙트 프리펩 경로 (프리팹 이름 포함)")]
    public string               _effectPrefabPath = "";

    [Tooltip("생성 프레임")]
    public float               _startFrame = 0f;

    [Tooltip("대상(공격 대상 등)을 기준으로 이펙트 생성")]
    public bool                 _toTarget = false;

    [Tooltip("이펙트 기준이 되는 오브젝트의 자식으로 Attach")]
    public bool                 _attach = false;

    [Tooltip("이펙트 플레이 시간 (0이면 적용 안함, 이펙트 길이를 강제로 LifeTime으로 맞춘다)")]
    public float                _lifeTime = 0f;

    [Tooltip("위치 오프셋")]
    public Vector3              _spawnOffset = Vector3.zero;

    [Tooltip("시간 느려지는거에 영향 받을지 여부 (ScaledDeltaTime이면 영향 받음, 파티클은 적용 안됨")]
    public EffectUpdateType     _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    [Tooltip("방향 지정 타입 사용 안하고 캐릭터 방향으로 돌리기")]
    public bool                _followDirection = false;

    [Tooltip("방향 지정 타입. Direction은 캐릭터 방향, AttackPoint는 공격당했을 때 공격 시작 지점에서 바라보는 방향")]
    public AngleDirectionType   _angleDirectionType = AngleDirectionType.identity;

}


[CreateAssetMenu(fileName = "AnimationCustomPreset", menuName = "Scriptable Object/Animation Custom Preset", order = int.MaxValue)]
public class AnimationCustomPreset : ScriptableObject
{
    public AnimationCustomPresetData _animationCustomPresetData;
    public string _translationPresetName = "";
    public string _rotationPresetName="";
    public string _scalePresetName = "";

    public AnimationPresetFrameEventData_SpriteEffect[] _spriteEffects = null;
    public AnimationPresetFrameEventData_ParticleEffect[] _particleEffects = null;
    public AnimationPresetFrameEventData_TimelineEffect[] _timelineEffects = null;
}

#if UNITY_EDITOR

[CustomEditor(typeof(AnimationCustomPreset))]
public class AnimationCustomPresetEditor : Editor
{
    AnimationCustomPreset controll;

    private AnimationPlayer _animationPlayer = new AnimationPlayer();

    private float _fps = 0f;
    private bool _isLoaded = false;
    private bool _update = false;
    private bool _playSecond = false;

    private double previousTime;

    public void Update()
    {
        double currentTime = EditorApplication.timeSinceStartup;

        if (previousTime == 0)
        {
            previousTime = currentTime;
            return;
        }

        double deltaTime = currentTime - previousTime;
        previousTime = currentTime;

        if(_playSecond)
        {
            deltaTime = 0.02f;
        }

        if((_isLoaded && _update) || _playSecond)
        {
            _animationPlayer.progress((float)deltaTime, null);
            Repaint();

            _playSecond = false;
        }

        EditorApplication.update -= Update;
    }

	void OnEnable()
    {
        controll = (AnimationCustomPreset)target;
    }

    public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

        GUILayout.Space(10f);

        if(controll._animationCustomPresetData != null)
            GUILayout.Label("Total Duration : " + controll._animationCustomPresetData.getTotalDuration());

        EditorGUILayout.BeginHorizontal();

        _fps = EditorGUILayout.FloatField(_fps);
        if(GUILayout.Button("Set Duration From FPS"))
        {
            float perFrame = 1f / _fps;
            for(int index = 0; index < controll._animationCustomPresetData._duration.Length; ++index)
            {
                controll._animationCustomPresetData._duration[index] = perFrame;
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30f);

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("TestPlay");
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Play Begin", GUILayout.Width(100f)))
        {
            _animationPlayer.initialize();
            _animationPlayer.changeAnimation(loadAnimation());

            _isLoaded = true;
            _update = true;

            Repaint();
        }

        GUI.enabled = _isLoaded;

        if(GUILayout.Button(">", GUILayout.Width(30f)))
        {
            _update = false;
            _playSecond = true;

            Repaint();
        }

        Color currentColor = GUI.color;
        GUI.color = _isLoaded == false ? currentColor : (_update ? Color.green : Color.red);
        if(GUILayout.Button(_update ? "Playing" : "Paused"))
        {
            _update = !_update;
            Repaint();
        }

        GUI.color = currentColor;
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        if(_isLoaded)
        {
            EditorApplication.update += Update;
        }

        Texture2D currentTexture = _isLoaded  ? _animationPlayer.getCurrentSprite().texture : null;

        if(_isLoaded)
        {
            GUILayout.Label("Animation Time : (" + _animationPlayer.getCurrentAnimationTime() + " : " + _animationPlayer.getCurrentAnimationDuration() + ")");
            GUILayout.Label("Index : (" + (_animationPlayer.getCurrentIndex() + 1) + " : " + _animationPlayer.getEndIndex() + ")");
        }

        if(currentTexture != null)
        {
            Rect rect = GUILayoutUtility.GetRect(currentTexture.width, currentTexture.height);
            Vector3 translation = Vector3.zero;
            _animationPlayer.getCurrentAnimationTranslation(out translation);
            rect.center += new Vector2(translation.x, translation.y) * 100f;

            Vector3 scale = Vector3.one;
            _animationPlayer.getCurrentAnimationScale(out scale);

            EditorGUIUtility.ScaleAroundPivot(scale,rect.center);
            EditorGUIUtility.RotateAroundPivot(_animationPlayer.getCurrentAnimationRotation().eulerAngles.z, rect.center);
            
            GUI.DrawTexture(rect, currentTexture,ScaleMode.ScaleToFit);
            EditorGUIUtility.RotateAroundPivot(-_animationPlayer.getCurrentAnimationRotation().eulerAngles.z, rect.center);
            EditorGUIUtility.ScaleAroundPivot(Vector3.one,rect.center);
        }



        EditorGUILayout.EndVertical();
    }

    public AnimationPlayDataInfo loadAnimation()
    {
        AnimationCustomPreset animationCustomPreset = controll;

        AnimationPlayDataInfo playData = new AnimationPlayDataInfo();
        playData._actionTime = controll._animationCustomPresetData.getTotalDuration();
        playData._customPresetData = animationCustomPreset._animationCustomPresetData;
        playData._customPreset = animationCustomPreset;
        playData._path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(controll));

        if(animationCustomPreset._rotationPresetName != "")
        {
            AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
            playData._rotationPresetData = rotationPreset.getPresetData(animationCustomPreset._rotationPresetName);
        }
        
        if(animationCustomPreset._scalePresetName != "")
        {
            AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
            playData._scalePresetData = scalePreset.getPresetData(animationCustomPreset._scalePresetName);
        }

        if (animationCustomPreset._translationPresetName != "")
        {
            AnimationTranslationPreset translationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationTranslationPreset") as AnimationTranslationPreset;
            playData._translationPresetData = translationPreset.getPresetData(animationCustomPreset._translationPresetName);
        }

        return playData;
    }
}


#endif
