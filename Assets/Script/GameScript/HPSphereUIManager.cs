using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPSphereUIManager : Singleton<HPSphereUIManager>
{
    private SimplePool<HPSphereUI> _hpSpherePool = new SimplePool<HPSphereUI>();
    private List<HPSphereUI> _enabledSpheres = new List<HPSphereUI>();

    public GameEntityBase _targetEntity;
    private Transform _targetEntityTransform;

    public string _gaugeTargetStatusName = "HP";
    public int _gaugeStackCount = 5;
    public float _sphereXOffset = -0.7f;
    public float _sphereYOffset = 0.37f;

    public float _sphereXGap = 0.05f;
    public float _sphereYGap = 0.2f;

    public int _beforeStackCount = 0;

    public bool _xFlip = false;

    public void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _targetEntityTransform = targetEntity.transform;

        for(int i = 0; i < _gaugeStackCount; ++i)
        {
            HPSphereUI sphere = _hpSpherePool.dequeue();
            sphere.initialize(_targetEntityTransform.position);
            _enabledSpheres.Add(sphere);
        }
    }

    public void release()
    {
        for(int i = 0; i < _enabledSpheres.Count; ++i)
        {
            _enabledSpheres[i].release();
            _hpSpherePool.enqueue(_enabledSpheres[i]);
        }

        _enabledSpheres.Clear();
    }

    public void progress(float deltaTime)
    {
        if(_targetEntity == null)
            return;
            
        float percentage = _targetEntity.getStatusPercentage(_gaugeTargetStatusName);
        float currentGaugeStackRate = (((float)_gaugeStackCount) * percentage);
        int currentGagueStackCount = (int)Mathf.Ceil(currentGaugeStackRate);

        int currentIndex = 0;
        float xOffset = 0f;

        _xFlip = _targetEntity.getDirection().x < 0f;

        for(int index = 0; index < _gaugeStackCount; ++index)
        {
            float gauge = currentGaugeStackRate - (float)(_gaugeStackCount - index - 1);
            gauge = gauge >= 0f ? gauge : 0f;
            _enabledSpheres[index].updateGauge(gauge);
            if(_gaugeStackCount - currentGagueStackCount > index)
                continue;

            xOffset += (_sphereXGap * ((currentIndex) <= ((float)currentGagueStackCount * 0.5f) ? -1f : 1f));

            Vector3 gaugePosition = Vector3.down * (((float)currentIndex * _sphereYGap) - _sphereYOffset);
            gaugePosition.x = _sphereXOffset + xOffset;// * (_targetEntity.getFlipState().xFlip ? 1f : -1f);
            gaugePosition += new Vector3(Random.Range(-0.07f,0.07f),Random.Range(-0.07f,0.07f),0f);

            if(currentGagueStackCount > _beforeStackCount && _gaugeStackCount - _beforeStackCount > index)
                _enabledSpheres[index].setPosition(_targetEntityTransform.position + gaugePosition);

            _enabledSpheres[index].progress(_targetEntityTransform.position + gaugePosition,deltaTime);
            ++currentIndex;
        }

        _beforeStackCount = currentGagueStackCount;
    }
}