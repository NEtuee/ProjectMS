using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase
{
    public enum MovementType
    {
        Empty = 0,
        RootMotion,
        GraphPreset,
        FrameEvent,
        Count,
    }

    protected bool      _isMoving = false;

    protected float     _moveScale = 1f;

    protected Vector2   _currentPosition = Vector2.zero;
    protected Vector3   _currentDirection = Vector2.right;

    protected Vector3   movementOfFrame;


    public abstract MovementType getMovementType();

    public abstract void initialize(GameEntityBase targetEntity);
    public abstract void updateFirst(GameEntityBase targetEntity);
    public abstract bool progress(float deltaTime, Vector3 direction);
    public abstract void release();
    public bool isMoving() {return _isMoving;}
    public virtual void AddFrameToLocalTransform(Transform target)
    {
        target.localPosition += movementOfFrame * _moveScale;
        movementOfFrame = Vector3.zero;
    }
    public virtual void AddFrameToWorldTransform(Transform target)
    {
        target.position += movementOfFrame * _moveScale;
        movementOfFrame = Vector3.zero;
    }

    public virtual void SetFrameToWorldTransform(Transform target)
    {
        target.position = movementOfFrame * _moveScale;
        movementOfFrame = Vector3.zero;
    }
    public virtual void SetFrameToLocalTransform(Transform target)
    {
        target.localPosition = movementOfFrame * _moveScale;
        movementOfFrame = Vector3.zero;
    }
    
    public void UpdatePosition(Vector3 currentPosition)
    {
        _currentPosition = currentPosition;
    }

    public void setMoveScale(float moveScale)
    {
        _moveScale = moveScale;
    }

    public Vector3 getCurrentDirection() {return _currentDirection;}

}
