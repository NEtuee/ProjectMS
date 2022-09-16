using System.Collections.Generic;
using UnityEngine;

public class MovementControl
{
    private Dictionary<System.Type, MovementBase> _movementCache = new Dictionary<System.Type, MovementBase>();
    private MovementBase _currentMovement;

    public void initialize()
    {

    }
    public bool progress(float deltaTime, Vector3 direction)
    {
        _currentMovement?.progress(deltaTime, direction);

        return true;
    }
    public void release()
    {
        _currentMovement?.release();
    }

    public bool isValid()
    {
        return _currentMovement != null;
    }
    public bool isMoving() 
    {
        return _currentMovement.isMoving();
    }

    public void setMoveScale(float moveScale){_currentMovement?.setMoveScale(moveScale);}

    public void addFrameToWorld(Transform targetTransform){_currentMovement?.AddFrameToWorldTransform(targetTransform);}
    public void addFrameToLocal(Transform targetTransform){_currentMovement?.AddFrameToLocalTransform(targetTransform);}
    public void setFrameToWorld(Transform targetTransform){_currentMovement?.SetFrameToWorldTransform(targetTransform);}
    public void setFrameToLocal(Transform targetTransform){_currentMovement?.SetFrameToLocalTransform(targetTransform);}

    public MovementBase getCurrentMovement(){return _currentMovement;}
    public MovementBase.MovementType getCurrentMovementType(){return _currentMovement.getMovementType();}

    public MovementBase changeMovement(GameEntityBase targetEntity,MovementBase.MovementType movementType)
    {
        if(_currentMovement != null && _currentMovement.getMovementType() == movementType)
        {
            _currentMovement.updateFirst(targetEntity);
            return _currentMovement;
        }

        switch(movementType)
        {
        case MovementBase.MovementType.Empty:
            setMovementEmpty();
            break;
        case MovementBase.MovementType.RootMotion:
            return changeMovement<GraphMovement>(targetEntity);
        case MovementBase.MovementType.GraphPreset:
            return changeMovement<GraphPresetMovement>(targetEntity);
        default:
            DebugUtil.assert(false,"invalid movement type: {0}",movementType);
            break;
        }

        return null;
    }

    public void setMovementEmpty()
    {
        _currentMovement?.release();
        _currentMovement = null;
    }

    public T changeMovement<T>(GameEntityBase targetEntity) where T : MovementBase, new()
    {
        _currentMovement?.release();
        
        if(_movementCache.ContainsKey(typeof(T)) == false)
            _movementCache.Add(typeof(T), new T());
        T newMovement = _movementCache[typeof(T)] as T;
        _currentMovement = newMovement;

        if(_currentMovement == null)
        {
            DebugUtil.assert(false, "movement is null");
            return null;
        }

        _currentMovement.initialize(targetEntity);
        _currentMovement.updateFirst(targetEntity);
        return newMovement;
    }
}
