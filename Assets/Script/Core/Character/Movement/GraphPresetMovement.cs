using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPresetMovement : MovementBase
{
    private MovementGraphPresetData _presetData = null;
    private GameEntityBase _targetEntity;

    public override MovementType getMovementType(){return MovementType.GraphPreset;}

    public override void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _currentDirection = Vector3.right;
    }

    public override void updateFirst(GameEntityBase targetEntity)
    {
        setGraphPresetData(targetEntity.getCurrentMovementGraphPreset());
    }

    public void setGraphPresetData(MovementGraphPresetData preset)
    {
         if(preset == null || preset.isValid() == false)
        {
            DebugUtil.assert(false, "movement graph preset data is not valid {0}", preset == null);
            return;
        }

        _presetData = preset;
    }

    public override bool progress(float deltaTime, Vector3 direction)
    {
        if(_presetData == null || _presetData.isValid() == false || _targetEntity == null)
        {
            DebugUtil.assert(false, "movement graph data is not valid");
            return false;
        }

        if(direction == Vector3.zero)
            return false;

        movementOfFrame += Quaternion.FromToRotation(Vector3.right,direction) * (Vector3.right * _presetData.getMoveValuePerFrameFromTime(_targetEntity.getMoveValuePerFrameFromTimeDesc()));
        return true;
    }

    public override void release()
    {
        _presetData = null;
    }

}