using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;

public class FlowAnimation : MonoBehaviour
{
    public Vector2          _moveSpeed;

    private SpriteRenderer  _spriteRenderer;
    private Material        _material;

    private int             _propertyIDX;
    private int             _propertyIDY;

    private Vector2         _scroll = Vector2.zero;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = Material.Instantiate( _spriteRenderer.material );

        _spriteRenderer.material = _material;
        _propertyIDX = Shader.PropertyToID("_UVX");
        _propertyIDY = Shader.PropertyToID("_UVY");
    }

    private void Update()
    {
        _material.SetFloat(_propertyIDX, _scroll.x);
        _material.SetFloat(_propertyIDY, _scroll.y);
        _scroll += _moveSpeed * GlobalTimer.Instance().getSclaedDeltaTime();
    }
}
