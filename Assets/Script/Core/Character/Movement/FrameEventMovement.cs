using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameEventMovement : MovementBase
{
    private GameEntityBase _targetEntity;

    public enum FrameEventMovementValueType
    {
        Speed = 0,
        Velocity,
        MaxVelocity,
        Friction,
        Count,
    };

    private float[] _movementValues = new float[(int)FrameEventMovementValueType.Count];
    private Vector3 _currentVelocity = Vector3.zero;

    public override MovementType getMovementType(){return MovementType.FrameEvent;}

    public override void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _currentDirection = Vector3.zero;
        
        int numMovementValue = (int)FrameEventMovementValueType.Count;
        for(int i = 0; i < numMovementValue; ++i)
        {
            _movementValues[i] = 0f;
        }

        _currentVelocity = Vector3.zero;
    }

    public override void updateFirst(GameEntityBase targetEntity)
    {
        
    }

    public override bool progress(float deltaTime, Vector3 direction)
    {
        if(_targetEntity == null)
        {
            DebugUtil.assert(false, "invalid movement update call");
            return false;
        }

        _currentDirection = direction;
        float resultSpeed = _movementValues[0] + (_movementValues[0] >= 0 ? -_movementValues[3] : _movementValues[3]);
        
        _currentVelocity += (_currentDirection * _movementValues[0]) * deltaTime;
        Vector3 velocityDirection = _currentVelocity.normalized;

        _currentVelocity -= _currentVelocity.normalized * _movementValues[3] * deltaTime;

        if(Vector3.Angle(_currentVelocity.normalized, velocityDirection) > 100f)
            _currentVelocity = Vector3.zero;
        else if(_currentVelocity.sqrMagnitude > _movementValues[2] * _movementValues[2])
            _currentVelocity = _currentVelocity.normalized * _movementValues[2];

        movementOfFrame += _currentVelocity * deltaTime;

        return true;
    }

    public override void release()
    {
        
    }

    public void setMovementValue(float value, int valueType)
    {
        if(valueType == 1)
        {
            if(MathEx.equals(_targetEntity.getDirection().sqrMagnitude, 0f, float.Epsilon) == false)
                _currentVelocity = _targetEntity.getDirection() * value;

            return;
        }

        _movementValues[valueType] = value;
    }

}