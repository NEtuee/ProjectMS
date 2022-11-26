using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRequestData : MessageData
{
    public string _effectPath;

    public float _startFrame;
    public float _endFrame;
    public float _framePerSecond;

    public float _angle;

    public bool _usePhysics;
    public bool _useFlip;

    public Vector3 _position;
    public Quaternion _rotation;

    public Transform _parentTransform = null;

    public PhysicsBodyDescription _physicsBodyDesc;
}

public class EffectItem
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
    }

    public void initialize(EffectRequestData effectData)
    {
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

    public bool progress(float deltaTime)
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

    public void release()
    {
        _spriteRenderer.gameObject.SetActive(false);
    }
}

public class EffectManager : ManagerBase
{
    private List<EffectItem> _processingItems = new List<EffectItem>();
    //private List<EffectRequestData> _effect
    private Queue<EffectItem> _effectQueue = new Queue<EffectItem>();

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
            if(_processingItems[i].progress(deltaTime) == true)
            {
                _processingItems[i].release();

                returnEffectItemToQueue(_processingItems[i]);
                _processingItems.RemoveAt(i);

                continue;
            }
            
            ++i;
        }
    }

    private void returnEffectItemToQueue(EffectItem item)
    {
        _effectQueue.Enqueue(item);
    }

    private EffectItem getEffectItem()
    {
        if(_effectQueue.Count == 0)
        {
            EffectItem item = new EffectItem();
            item.createItem();

            return item;
        }
        
        return _effectQueue.Dequeue();
    }

    private void createEffect(EffectRequestData requestData)
    {
        EffectItem item = getEffectItem();
        item.initialize(requestData);

        _processingItems.Add(item);
    }

    public void receiveEffectRequest(Message msg)
    {
        createEffect(MessageDataPooling.CastData<EffectRequestData>(msg.data));
    }
}