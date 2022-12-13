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

        _currentDirection = direction;//(Quaternion.FromToRotation(Vector3.right,direction) * Quaternion.Euler(0f,0f,0f)) * Vector3.right;
        if(_currentDirection.sqrMagnitude >= float.Epsilon)
        {
            _currentVelocity = MathEx.convergenceTarget(
                _currentVelocity,
                _currentDirection * _movementValues[2],
                _movementValues[0] * deltaTime);
        }
        

        _currentVelocity = MathEx.convergence0(
            _currentVelocity,
            _movementValues[3] * deltaTime);

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
            _currentVelocity = _currentDirection * value;
            return;
        }

        _movementValues[valueType] = value;
    }

}