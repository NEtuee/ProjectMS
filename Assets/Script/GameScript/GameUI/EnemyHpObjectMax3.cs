using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyHpObjectMax3 : MonoBehaviour
{
    struct AnimationPresetInfo
    {
        public AnimationCustomPreset _customPreset;
        public string _path;

        public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
        {
            _customPreset = customPreset;
            _path = path;
        }
    }

    public SpriteRenderer _spriteRenderer;

    [SerializeField] private GameEntityBase _target;
    private Action<EnemyHpObjectMax3> _onDead;
    
    private AnimationPlayer _animationPlayer = new AnimationPlayer();

    private AnimationPresetInfo[][] _rankBadgeAnimationSet = new AnimationPresetInfo[4][];
    private AnimationPresetInfo[] _rank2 = new AnimationPresetInfo[2];
    private AnimationPresetInfo[] _rank3 = new AnimationPresetInfo[3];
    private AnimationPresetInfo[] _rankElite = new AnimationPresetInfo[3];

    private Vector3 _offset;

    private int _currentBadgeType = 0;
    private int _currentSegmentIndex = 0;
    private float _segmentLength = 0f;

    public void Init()
    {
        _rankBadgeAnimationSet[0] = new AnimationPresetInfo[1];
        _rankBadgeAnimationSet[0][0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/1/"), "Sprites/UI/StatusSticker/RankBadge/1");

        _rankBadgeAnimationSet[1] = new AnimationPresetInfo[2];
        _rankBadgeAnimationSet[1][0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/2/1/"), "Sprites/UI/StatusSticker/RankBadge/2/1/");
        _rankBadgeAnimationSet[1][1] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/2/2/"), "Sprites/UI/StatusSticker/RankBadge/2/2/");

        _rankBadgeAnimationSet[2] = new AnimationPresetInfo[3];
        _rankBadgeAnimationSet[2][0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/3/1/"), "Sprites/UI/StatusSticker/RankBadge/3/1/");
        _rankBadgeAnimationSet[2][1] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/3/2/"), "Sprites/UI/StatusSticker/RankBadge/3/2/");
        _rankBadgeAnimationSet[2][2] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/3/3/"), "Sprites/UI/StatusSticker/RankBadge/3/3/");

        _rankBadgeAnimationSet[3] = new AnimationPresetInfo[3];
        _rankBadgeAnimationSet[3][0] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/Elite/1/"), "Sprites/UI/StatusSticker/RankBadge/Elite/1/");
        _rankBadgeAnimationSet[3][1] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/Elite/2/"), "Sprites/UI/StatusSticker/RankBadge/Elite/2/");
        _rankBadgeAnimationSet[3][2] = new AnimationPresetInfo(ResourceContainerEx.Instance().GetAnimationCustomPreset("Sprites/UI/StatusSticker/RankBadge/Elite/3/"), "Sprites/UI/StatusSticker/RankBadge/Elite/3/");

        _animationPlayer.initialize();
    }
    
    public void Active(GameEntityBase target, int badgeType, Action<EnemyHpObjectMax3> onDead)
    {
        if (target == null)
        {
            return;
        }
        _animationPlayer.initialize();
        
        _currentBadgeType = badgeType;
        _currentSegmentIndex = _rankBadgeAnimationSet[_currentBadgeType].Length - 1; 
        _segmentLength = 1f / (float)_rankBadgeAnimationSet[_currentBadgeType].Length;

        AnimationPresetInfo presetInfo = _rankBadgeAnimationSet[_currentBadgeType][_currentSegmentIndex];

        _animationPlayer.changeAnimationByCustomPreset(presetInfo._path, presetInfo._customPreset);

        _offset = Vector3.up * target.getHeadUpOffset();

        _target = target;
        _onDead = onDead;

        updatePosition();
        gameObject.SetActive(target.isSpriteRendererActive());
    }
    
    public void UpdateByManager(float deltaTime)
    {
        if (_target == null)
        {
            return;
        }

        gameObject.SetActive(_target.isSpriteRendererActive());

        if (_target.isDead())
        {
            gameObject.SetActive(false);
            _target = null;
            _onDead?.Invoke(this);
            return;
        }

        float percentage = _target.getStatusPercentage("HP");

        int newSegmentIndex = _currentSegmentIndex;
        if (percentage == 1f)
        {
            newSegmentIndex = _rankBadgeAnimationSet[_currentBadgeType].Length - 1;
        }
        else
        {
            float realValue = percentage / _segmentLength;
            newSegmentIndex = (int)(MathEx.floorNoSign(percentage, 2) / MathEx.floorNoSign(_segmentLength,2));
            newSegmentIndex = MathEx.equals(realValue, (float)newSegmentIndex, float.Epsilon) ? newSegmentIndex - 1 : newSegmentIndex;
        }

        if(_currentSegmentIndex != newSegmentIndex)
        {
            _currentSegmentIndex = newSegmentIndex;
            AnimationPresetInfo presetInfo = _rankBadgeAnimationSet[_currentBadgeType][_currentSegmentIndex];

            _animationPlayer.changeAnimationByCustomPreset(presetInfo._path, presetInfo._customPreset);
        }

        updatePosition();
        
        _animationPlayer.progress(deltaTime,null);
        _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
    }

    public void updatePosition()
    {
        transform.position = _target.transform.position + _offset;
    }

    public void Stop()
    {
        gameObject.SetActive(false);
        
        if (_target == null)
        {
            return;
        }

        _target = null;
        _onDead?.Invoke(this);
    }

}
