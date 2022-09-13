using UnityEngine;


struct BoundBox
{
    private float _width;
    private float _height;
    private Vector3 _worldCenter;
    private Vector3 _localPivot;

    private float _l,_r,_b,_t;

    public BoundBox(float width, float height, Vector3 localPivot)
    {
        _width = width;
        _height = height;

        _worldCenter = Vector3.zero;
        _localPivot = localPivot;

        _l = _r = _b = _t = 0f;
    }

    public void setPivot(Vector3 pivot)
    {
        _localPivot = pivot;
    }

    public void updateBoundBox(Vector3 worldPosition)
    {
        _worldCenter = worldPosition;

        Vector3 pivotCenter = worldPosition + _localPivot;

        _l = pivotCenter.x - _width * 0.5f;
        _r = pivotCenter.x + _width * 0.5f;

        _b = pivotCenter.y - _height * 0.5f;
        _t = pivotCenter.y + _height * 0.5f;
    }

    public bool intersector(Vector3 point)
    {
        return (_l < point.x && _r > point.x) && (_b < point.y && _t > point.y);
    }
    public bool intersection(BoundBox target)
    {
        return (_l < target._r && _r > target._l && _t < target._b && _b > target._t);
    }

}