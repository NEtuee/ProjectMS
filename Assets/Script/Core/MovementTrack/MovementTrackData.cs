using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MovementTrackPointData
{
    public MathEx.EaseType _easeType = MathEx.EaseType.Linear;
    public Vector2     _point = Vector2.zero;
    public Vector2     _bezierPoint = Vector2.zero;

    public Vector2 getInverseBezierPoint()
    {
        return _point - (_bezierPoint - _point);
    }

    public Vector2 convertInverseBezierPointToBezierPoint(Vector2 bezierPointInv)
    {
        return _point - (bezierPointInv - _point);
    }
}

[System.Serializable]
public class MovementTrackData
{
    public string       _name;
    public List<MovementTrackPointData> _trackPointData = new List<MovementTrackPointData>();
    public float[] _pointLengthArray;
    public float _trackTotalLength;

    public void calculateTrackLength()
    {
        _pointLengthArray = new float[_trackPointData.Count];
        _trackTotalLength = 0f;

        for(int index = 0; index < _pointLengthArray.Length; ++index)
        {
            MovementTrackPointData p0 = _trackPointData[index];
            MovementTrackPointData p1 = _trackPointData[index + 1];
            _pointLengthArray[index] = MathEx.approximateBezierCurveLength(p0._point,p0._bezierPoint,p1._bezierPoint,p1._point);

            _trackTotalLength += _pointLengthArray[index];
        }
    }
}
