using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public enum EffectUpdateType
{
    ScaledDeltaTime,
    NoneScaledDeltaTime,
}

public enum EffectType
{
    SpriteEffect,
    TimelineEffect,
}

public class EffectRequestData : MessageData
{
    public string _effectPath;

    public float _startFrame;
    public float _endFrame;
    public float _framePerSecond;

    public float _angle;

    public bool _usePhysics;
    public bool _useFlip;

    public EffectType _effectType;
    public EffectUpdateType _updateType = EffectUpdateType.ScaledDeltaTime;

    public Vector3 _position;
    public Quaternion _rotation;

    public Transform _parentTransform = null;

    public PhysicsBodyDescription _physicsBodyDesc;
}  

public abstract class EffectItemBase
{
    public EffectUpdateType        _effectUpdateType = EffectUpdateType.NoneScaledDeltaTime;
    public EffectType              _effectType = EffectType.SpriteEffect;
    public string                  _effectPath = "";

    public abstract void initialize(EffectRequestData effectData);
    public abstract bool progress(float deltaTime);
    public abstract void release();

    public abstract bool isValid();

}

public class EffectItem : EffectItemBase
{
    private AnimationPlayer         _animationPlayer = new AnimationPlayer();
    private AnimationPlayDataInfo   _animationPlayData = new AnimationPlayDataInfo();

    private PhysicsBodyEx           _physicsBody = new PhysicsBodyEx();

    private SpriteRenderer          _spriteRenderer;
    private Transform               _parentTransform = null;

    private Vector3                 _localPosition;
    private Quaternion              _rotation;
    private bool                    _usePhysics = false;
    private bool                    _useFlip = false;

    public void createItem()
    {
        GameObject gameObject = new GameObject("Effect");
        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        gameObject.SetActive(false);

        _effectType = EffectType.SpriteEffect;
    }

    public override void initialize(EffectRequestData effectData)
    {
        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _animationPlayData._path = effectData._effectPath;
        _animationPlayData._startFrame = effectData._startFrame;
        _animationPlayData._endFrame = effectData._endFrame;
        _animationPlayData._framePerSec = effectData._framePerSecond;

        _animationPlayData._frameEventData = null;
        _animationPlayData._frameEventDataCount = 0;
        _animationPlayData._hasMovementGraph = false;
        _animationPlayData._isLoop = false;
        _animationPlayData._flipState = new FlipState{xFlip = false, yFlip = false};

        _animationPlayer.initialize();
        _animationPlayer.changeAnimation(_animationPlayData);

        _parentTransform = effectData._parentTransform;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        _spriteRenderer.transform.position = effectData._position;
        _spriteRenderer.transform.localRotation = Quaternion.Euler(0f,0f,effectData._angle);
        _spriteRenderer.transform.localScale = Vector3.one;
        _spriteRenderer.gameObject.SetActive(true);

        _localPosition = _spriteRenderer.transform.position;
        if(_parentTransform != null)
            _localPosition = _parentTransform.position - _spriteRenderer.transform.position;

        _physicsBody.initialize(effectData._physicsBodyDesc);
        _usePhysics = effectData._usePhysics;

        _useFlip = effectData._useFlip;
        _rotation = effectData._rotation;
    }

    public override bool progress(float deltaTime)
    {
        bool isEnd = _animationPlayer.progress(deltaTime,null);
        _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
        _spriteRenderer.transform.localRotation *= _animationPlayer.getAnimationRotationPerFrame();
        _spriteRenderer.transform.localScale = _animationPlayer.getCurrentAnimationScale();

        if(_usePhysics)
        {
            _physicsBody.progress(deltaTime);

            Vector3 velocity = _physicsBody.getCurrentVelocity();
            float torque = _physicsBody.getCurrentTorqueValue();

            if(_useFlip)
            {
                torque *= -1f;
                velocity.x *= -1f;
            }

            _localPosition += (velocity * deltaTime);
            _spriteRenderer.transform.localRotation *= Quaternion.Euler(0f,0f,torque * deltaTime);
        }

        Vector3 worldPosition = _localPosition;
        if(_parentTransform != null)
            worldPosition = _parentTransform.position + _localPosition;

        _spriteRenderer.transform.position = worldPosition;

        return isEnd;
    }

    public override void release()
    {
        _spriteRenderer.gameObject.SetActive(false);
    }

    public override bool isValid()
    {
        return _spriteRenderer != null;
    }
}

public class TimelineEffectItem : EffectItemBase
{
    private GameObject              _effectObject;
    private PlayableDirector        _playableDirector;


    public void createItem(string prefabPath)
    {
        GameObject effectPrefab = ResourceContainerEx.Instance().GetPrefab(prefabPath);
        if(effectPrefab == null)
        {
            DebugUtil.assert(false, "invalid timeline effect prefab path : {0}", prefabPath);
            return;
        }

        _effectObject = GameObject.Instantiate(effectPrefab);
        _playableDirector = _effectObject.GetComponent<PlayableDirector>();

        if(_playableDirector == null)
        {
            DebugUtil.assert(false, "playable director is not exists in effect prefab : {0}", prefabPath);
            return;
        }

        _effectType = EffectType.TimelineEffect;
        release();
    }

    public override void initialize(EffectRequestData effectData)
    {
        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _effectObject.transform.position = effectData._position;
        _effectObject.transform.rotation = effectData._rotation;

        _effectObject.SetActive(true);
        _playableDirector.Stop();
        _playableDirector.Play();
    }

    public override bool progress(float deltaTime)
    {
        if(isValid() == false)
            return false;

        _playableDirector.playableGraph.Evaluate(deltaTime);
        return _playableDirector.state != PlayState.Playing;
    }

    public override void release()
    {
        _playableDirector.Stop();
        _effectObject.SetActive(false);
    }

    public override bool isValid()
    {
        return _effectObject != null && _playableDirector != null;
    }
}

public class EffectManager : ManagerBase
{
    private List<EffectItemBase> _processingItems = new List<EffectItemBase>();

    private SimplePool<EffectItem> _effectItemPool = new SimplePool<EffectItem>();
    private Dictionary<string, SimplePool<TimelineEffectItem>> _timelineEffectPool = new Dictionary<string, SimplePool<TimelineEffectItem>>();

    public override void assign()
    {
        base.assign();
        CacheUniqueID("EffectManager");
        RegisterRequest();

        AddAction(MessageTitles.effect_spawnEffect,receiveEffectRequest);
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

        for(int i = 0; i < _processingItems.Count;)
        {
            float targetDeltaTime = _processingItems[i]._effectUpdateType == EffectUpdateType.ScaledDeltaTime ? deltaTime : Time.deltaTime;
            if(_processingItems[i].progress(targetDeltaTime) == true)
            {
                _processingItems[i].release();

                returnEffectItemToQueue(_processingItems[i]);
                _processingItems.RemoveAt(i);

                continue;
            }
            
            ++i;
        }
    }

    private void returnEffectItemToQueue(EffectItemBase item)
    {
        switch(item._effectType)
        {
            case EffectType.SpriteEffect:
                _effectItemPool.enqueue(item as EffectItem);
            break;
            case EffectType.TimelineEffect:
                _timelineEffectPool[item._effectPath].enqueue(item as TimelineEffectItem);
            break;
        }
        
    }

    private void createEffect(EffectRequestData requestData)
    {
        EffectItemBase itemBase = null;

        if(requestData._effectType == EffectType.SpriteEffect)
        {
            EffectItem item = _effectItemPool.dequeue();
            if(item.isValid() == false)
                item.createItem();

            item.initialize(requestData);
            itemBase = item;
        }
        else if(requestData._effectType == EffectType.TimelineEffect)
        {
            if(_timelineEffectPool.ContainsKey(requestData._effectPath) == false)
                _timelineEffectPool.Add(requestData._effectPath, new SimplePool<TimelineEffectItem>());

            TimelineEffectItem item = _timelineEffectPool[requestData._effectPath].dequeue();
            if(item.isValid() == false)
                item.createItem(requestData._effectPath);

            item.initialize(requestData);
            itemBase = item;
        }
        
        if(itemBase == null)
        {
            DebugUtil.assert(false, "effect data error");
            return;
        }

        _processingItems.Add(itemBase);

    }

    public void receiveEffectRequest(Message msg)
    {
        createEffect(MessageDataPooling.CastData<EffectRequestData>(msg.data));
    }
}