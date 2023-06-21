using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageProcessor : Singleton<StageProcessor>
{
    public StageData _stageData = null;

    private int _currentPoint = 0;
    private bool _isEnd = false;

    private Transform _targetTransform;

    private Vector3 _clampTargetPosition;
    private Vector3 _smoothDampVelocity;

    public void setTargetTransform(Transform target)
    {
        _targetTransform = target;
    }

    public void initializeStage(StageData data)
    {
        _stageData = data;

        _currentPoint = 0;
        _isEnd = false;

        if(_stageData._stagePointData.Count != 0)
            _clampTargetPosition = _stageData._stagePointData[0]._stagePoint;
    }

    public void processStage(float deltaTime)
    {
        if(isValid() == false)
            return;
        
        Vector3 resultPoint;
        float resultDistance;
        float fraction = getLimitedFractionOnLine(_targetTransform.position, out resultPoint, out resultDistance);
        if(_isEnd == false && fraction >= 1f)
        {
            ++_currentPoint;
            if(_currentPoint >= _stageData._stagePointData.Count)
            {
                _currentPoint = _stageData._stagePointData.Count - 1;
                _isEnd = true;
            }
        }

        resultPoint.z = -20f;
        _targetTransform.position = resultPoint;
        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            Color targetColor = index < _currentPoint ? Color.green : ( index == _currentPoint ? Color.magenta : Color.red);
            GizmoHelper.instance.drawCircle(_stageData._stagePointData[index]._stagePoint, 0.3f, 12, targetColor);
        }
    }

    public bool isValid()
    {
        return _targetTransform != null && _stageData != null;
    }

    private float getLimitedFractionOnLine(Vector3 targetPosition, out Vector3 resultPoint, out float distance)
    {
        Vector3 targetPositionWithoutZ = MathEx.deleteZ(targetPosition);
        resultPoint = targetPosition;
        distance = 0f;

        if(_stageData._stagePointData.Count == 0)
            return 0f;

        bool isEndPoint = _currentPoint == _stageData._stagePointData.Count - 1;
        StagePointData startPoint = _stageData._stagePointData[_currentPoint];
        StagePointData nextPoint = isEndPoint ? startPoint : _stageData._stagePointData[_currentPoint + 1];

        Vector3 perpendicular = isEndPoint ? startPoint._stagePoint : MathEx.getPerpendicularPointOnLineSegment(startPoint._stagePoint, nextPoint._stagePoint,targetPositionWithoutZ);
        
        float resultFraction = isEndPoint ? 1f : (perpendicular - startPoint._stagePoint).magnitude * (1f / (nextPoint._stagePoint - startPoint._stagePoint).magnitude);
        float limitedDistance = Mathf.Lerp(startPoint._maxLimitedDistance, nextPoint._maxLimitedDistance, resultFraction);

        distance = Vector3.Distance(targetPositionWithoutZ, perpendicular);

        if(distance > limitedDistance)
            resultPoint = perpendicular + (targetPositionWithoutZ - perpendicular).normalized * limitedDistance;

        return resultFraction;
    }

}

