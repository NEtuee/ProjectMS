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

    public Vector3 _position;
}

public class EffectItem
{
    private AnimationPlayer         _animationPlayer = new AnimationPlayer();
    private AnimationPlayDataInfo   _animationPlayData = new AnimationPlayDataInfo();

    private SpriteRenderer          _spriteRenderer;

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

        _spriteRenderer.transform.position = effectData._position;
        _spriteRenderer.transform.localRotation = Quaternion.Euler(0f,0f,effectData._angle);
        _spriteRenderer.gameObject.SetActive(true);
        
    }

    public bool progress(float deltaTime)
    {
        bool isEnd = _animationPlayer.progress(deltaTime,null);
        _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();

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