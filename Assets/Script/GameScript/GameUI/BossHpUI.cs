using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BossHpUI : IUIElement
{
    public class HPItem
    {
        private Image _uiImage;
        private PhysicsBodyEx _physicsBody = new PhysicsBodyEx();
        private PhysicsBodyDescription _bodyDesc = new PhysicsBodyDescription(null);

        private const float kLifeTime = 1f;
        private float _lifeTime = 0f;

        private Color kColor = new Color(1f,0f,0f);

        public HPItem()
        {
            GameObject imageObject = new GameObject();
            _uiImage = imageObject.AddComponent<Image>();
            _bodyDesc._angularFriction = 10f;
            _bodyDesc._friction = 1f;
            _bodyDesc._torque.setValue( -30f, 30f );
            _bodyDesc._velocity.setValue(-25f,25f,55f,85f,0f,0f);
            _bodyDesc._useGravity = true;
            _bodyDesc._gravityRatio = 10f;

            imageObject.SetActive(false);
        }

        public void initialize(Transform canvas, Vector2 position, float width, float height)
        {
            _uiImage.rectTransform.SetParent(canvas);
            _uiImage.rectTransform.anchoredPosition = position;
            _uiImage.rectTransform.localScale = Vector3.one;
            _uiImage.rectTransform.sizeDelta = new Vector2(width,height);
            _uiImage.rectTransform.rotation = quaternion.identity;
            _physicsBody.initialize(_bodyDesc);
            _lifeTime = kLifeTime;

            _uiImage.gameObject.SetActive(true);

            updateImage();
        }

        public void updateImage()
        {
            Color color = kColor;
            color.a = _lifeTime * (1f / kLifeTime);

            _uiImage.color = color;
        }

        public void disable()
        {
            _uiImage.gameObject.SetActive(false);
        }

        public bool isActive()
        {
            return _uiImage ? _uiImage.gameObject.activeSelf : false;
        }

        public void progress(float deltaTime)
        {
            if(deltaTime == 0f)
                return;

            _lifeTime -= deltaTime;
            if(_lifeTime <= 0f)
            {
                disable();
                return;
            }

            updateImage();

            _physicsBody.progress(deltaTime);

            Vector2 velocity = _physicsBody.getCurrentVelocity();
            float torque = _physicsBody.getCurrentTorqueValue();

            _uiImage.rectTransform.anchoredPosition += velocity * deltaTime;
            _uiImage.rectTransform.localRotation *= Quaternion.Euler(0f,0f,torque * deltaTime);
        }
    }

    private BossHpUIBinder _binder;

    private bool _active = false;
    private GameEntityBase _target;

    private UIActionLerpTimer _appearTimer;
    private UIActionLerpTimer _hpFillTimer;

    private float _prevPercentage = 0.0f;
    private float _fromPercentage = 0.0f;
    private float _toPercentage = 0.0f;
    private UIActionLerpTimer _hitTimer;

    private UIActionLerpTimer _shakeTimer;

    private SimplePool<HPItem> _hpItemPool = new SimplePool<HPItem>();

    private List<HPItem> _activeHPItem = new List<HPItem>(3);
    
    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "보스 hp ui 바인더가 없음";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as BossHpUIBinder;
    }

    public void Initialize()
    {
        _binder.Root.gameObject.SetActive(false);
        _binder._decoRoot.gameObject.SetActive(false);
        _binder.InGauge.fillAmount = 0.0f;
        _binder.OutGauge.fillAmount = 0.0f;
        
        foreach(var item in _activeHPItem)
        {
            item.disable();
            _hpItemPool.enqueue(item);
        }
        _activeHPItem.Clear();

        _appearTimer = new UIActionLerpTimer(UpdateAppearHpBar, PlayFillHpBar);
        _hpFillTimer = new UIActionLerpTimer(UpdateFillHpBar);

        _hitTimer = new UIActionLerpTimer(UpdateHitTimer);

        _shakeTimer = new UIActionLerpTimer(UpdateShake, EndShake);
    }

    public void UpdateByManager()
    {
        if (_active == false || _target == null)
        {
            return;
        }
        
        _appearTimer.Update();
        _hpFillTimer.Update();

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();
        for(int i = 0; i < _activeHPItem.Count;)
        {
            _activeHPItem[i].progress(deltaTime);

            if(_activeHPItem[i].isActive() == false)
            {
                _hpItemPool.enqueue(_activeHPItem[i]);
                _activeHPItem.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        if (_appearTimer.IsPlay == false && _hpFillTimer.IsPlay == false)
        {
            var curHpPercentage = _target.getStatusPercentage("HP");
            float diff = _prevPercentage - curHpPercentage;

            if (Mathf.Abs(diff) > Mathf.Epsilon)
            {
                _fromPercentage = _prevPercentage;
                _toPercentage = curHpPercentage;
                _prevPercentage = curHpPercentage;
                _binder.OutGauge.fillAmount = curHpPercentage;
                PlayHitTimer();

                if (Mathf.Abs(_toPercentage - 1.0f) > Mathf.Epsilon)
                {
                    PlayShake();
                }

                Vector2 hpLeft = _binder.InGauge.rectTransform.anchoredPosition;
                float width = _binder.InGauge.rectTransform.rect.width - _binder._inGaugePadding;
                float height = _binder.InGauge.rectTransform.rect.height;
                hpLeft.x = hpLeft.x - (width * 0.5f);

                float hitBarWidth = width * diff;
                Vector2 hitBarSpawnPosition = new Vector2(hpLeft.x + (width * curHpPercentage) + (hitBarWidth * 0.5f),hpLeft.y);

                activeHitBar(hitBarSpawnPosition, hitBarWidth, height);
            }
            
            _hitTimer.Update();
            _shakeTimer.Update();
        }
    }

    public void Active(GameEntityBase target, string localizationKey, Sprite portrait)
    {
        _active = true;
        _binder.Root.gameObject.SetActive(true);
        _binder._decoRoot.gameObject.SetActive(true);
        _target = target;

        _binder._localizationText.updateString(localizationKey);
        _binder._portrait.sprite = portrait;
        _binder._portrait.gameObject.SetActive(portrait != null);

        var curHpPercentage = _target.getStatusPercentage("HP");
        _fromPercentage = curHpPercentage;
        _toPercentage = curHpPercentage;
        _prevPercentage = curHpPercentage;

        if(portrait != null)
            _binder._portrait.SetNativeSize();            
        
        PlayAppearHpBar();
    }

    public void Disable()
    {
        _active = false;
        _target = null;
        _binder.Root.gameObject.SetActive(false);
        _binder._decoRoot.gameObject.SetActive(false);

        foreach(var item in _activeHPItem)
        {
            item.disable();
            _hpItemPool.enqueue(item);
        }
        _activeHPItem.Clear();
        
        _appearTimer.Stop();
        _hpFillTimer.Stop();
        _hitTimer.Stop();
    }

    private void PlayAppearHpBar()
    {
        //Debug.Log("PlayAppearHp");
        _binder.CanvasGroup.alpha = 0.0f;
        _appearTimer.Play(0.5f);
    }

    private void UpdateAppearHpBar(double t)
    {
        //Debug.Log("UpdateAppearHp");
        _binder.CanvasGroup.alpha = (float)t;
    }

    private void PlayFillHpBar()
    {
        //Debug.Log("PlayFillHp");
        _binder.InGauge.fillAmount = 0.0f;
        _binder.OutGauge.fillAmount = 0.0f;
        _hpFillTimer.Play(0.5f);
    }

    private void UpdateFillHpBar(double t)
    {
        //Debug.Log("UpdateFillHp");
        var value = MathEx.easeOutExpo(0, 1, (float)t);
        _binder.InGauge.fillAmount = value;
        _binder.OutGauge.fillAmount = value;
    }

    public void activeHitBar(Vector2 position, float width, float height)
    {
        HPItem item = _hpItemPool.dequeue();
        item.initialize(_binder._gaugeRoot,position,width,height);

        _activeHPItem.Add(item);
    }

    private void PlayHitTimer()
    {
        _hitTimer.Play(0.8f);
    }

    private void UpdateHitTimer(double t)
    {
        var value = MathEx.easeOutExpo(_fromPercentage, _toPercentage, (float)t);
        _binder.InGauge.fillAmount = value;
    }

    private void PlayShake()
    {
        _shakeTimer.Play(0.15f);
    }
    
    private void UpdateShake(double t)
    {
        _binder.Root.localPosition = (Vector2)(Random.insideUnitCircle * 7.0f);
    }

    private void EndShake()
    {
        _binder.Root.localPosition = Vector3.zero;
    }
}
