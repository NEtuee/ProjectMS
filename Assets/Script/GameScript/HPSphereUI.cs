using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPSphereUI
{
    private GameObject _spriteObject;
    private SpriteRenderer _spriteRenderer;

    public void createSpriteRenderObject()
    {
        _spriteObject = new GameObject("SpriteObject");
        _spriteObject.transform.position = Vector3.zero;

        _spriteRenderer = _spriteObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.material = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/Material_SpriteDefaultWithPixelSnap"));
    }

    public void initialize(Vector3 position)
    {
        if(_spriteObject == null)
            createSpriteRenderObject();
        
        _spriteObject.transform.position = position;
        _spriteObject.transform.localScale = Vector3.one;
        _spriteObject.SetActive(true);

        _spriteRenderer.sprite = ResourceContainerEx.Instance().GetSprite("Sprites/UI/HPSphere/Gauge/hpsphere_full");
    }

    public void release()
    {
        _spriteObject.SetActive(false);
    }

    public void progress(Vector3 followPosition, float deltaTime)
    {
        _spriteObject.transform.position = MathEx.damp(_spriteObject.transform.position,followPosition,8f,deltaTime);
    }

    public void setPosition(Vector3 position)
    {
        _spriteObject.transform.position = position;
    }

    public void updateGauge(float percentage)
    {
        if(percentage <= 0f)
        {
            _spriteObject.SetActive(false);
        }
        else
        {
            if(percentage > 1f)
                percentage = 1f;

            _spriteObject.SetActive(true);
            _spriteObject.transform.localScale = Vector3.one * percentage;
        }
    }
}
