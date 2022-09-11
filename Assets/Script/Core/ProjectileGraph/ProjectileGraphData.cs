using UnityEngine;

public class ProjectileGraphBaseData
{
    public string                       _name;
    public ProjectileType               _projectileType;
    public ProjectileGraphShotInfoData  _defaultProjectileShotInfoData;
    public AnimationPlayDataInfo[]      _animationPlayData;
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