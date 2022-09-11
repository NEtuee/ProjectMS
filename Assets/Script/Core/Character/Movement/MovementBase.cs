using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase
{
    public enum MovementType
    {
        Empty = 0,
        RootMotion,
        Count,
    }

    protected bool      _isMoving = false;
    protected Vector2   _currentPosition = Vector2.zero;
    protected Vector2   _currentDirection = Vector2.right;

    public abstract MovementType getMovementType();

    public abstract void initialize(GameEntityBase targetEntity);
    public abstract bool progress(float deltaTime, Vector3 direction);
    public abstract void release();
    public bool isMoving() {return _isMoving;}
    public virtual void AddFrameToLocalTransform(Transform target)
    {
        target.localPosition += movementOfFrame;
        movementOfFrame = Vector3.zero;
    }
    public virtual void AddFrameToWorldTransform(Transform target)
    {
        target.position += movementOfFrame;
        movementOfFrame = Vector3.zero;
    }

    public virtual void SetFrameToWorldTransform(Transform target)
    {
        target.position = movementOfFrame;
        movementOfFrame = Vector3.zero;
    }
    public virtual void SetFrameToLocalTransform(Transform target)
    {
        target.localPosition = movementOfFrame;
        movementOfFrame = Vector3.zero;
    }
    
    public virtual void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    public void UpdatePosition(Vector3 currentPosition)
    {
        _currentPosition = currentPosition;
    }
    protected float moveSpeed;
    protected Vector3 movementOfFrame;
}
