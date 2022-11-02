using UnityEngine;
using System.Collections.Generic;

public enum ProjectileChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnHitEnd,
    ChildFrameEvent_OnEnd,
    Count,
}

public class ProjectileGraphBaseData
{
    public string                       _name;
    public ProjectileType               _projectileType;
    public ProjectileGraphShotInfoData  _defaultProjectileShotInfoData;
    public AnimationPlayDataInfo[]      _animationPlayData;

    public Dictionary<ProjectileChildFrameEventType,ChildFrameEventItem> _projectileChildFrameEvent = null;

    public int                          _penetrateCount = 1;
}

public class ProjectileGraphShotInfoData
{
    public float                    _deafaultVelocity = 0f;
    public float                    _acceleration = 0f;
    public float                    _friction = 0f;
    public float                    _defaultAngle = 0f;
    public float                    _angularAcceleration = 0f;
    public float                    _lifeTime = 0f;

}


public enum ProjectileType
{
    Count,
}