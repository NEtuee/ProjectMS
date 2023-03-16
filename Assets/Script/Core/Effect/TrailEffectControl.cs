using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TrailEffectDescription
{
    public float _time;
    public float _width;
    public LineTextureMode _textureMode;
    public string _sortingLayerName;
    public int _sortingOrder;
}

public class TrailEffectControl : MonoBehaviour
{
    public TrailRenderer _trailRenderer;

    public void setPositions(Vector3[] positionArray)
    {
        _trailRenderer.SetPositions(positionArray);
    }

    public void setMaterial(Material material)
    {
        _trailRenderer.material = material;
    }

    public void setDescription(ref TrailEffectDescription desc)
    {
        _trailRenderer.Clear();

        _trailRenderer.time = desc._time;
        _trailRenderer.startWidth = desc._width;
        _trailRenderer.endWidth = desc._width;
        _trailRenderer.textureMode = desc._textureMode;
        _trailRenderer.sortingLayerName = desc._sortingLayerName;
        _trailRenderer.sortingOrder = desc._sortingOrder;
    }
}
