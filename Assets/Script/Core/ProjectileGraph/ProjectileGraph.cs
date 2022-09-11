
using System.Collections.Generic;

//todo : animation start, ed frame, 
public class ProjectileGraph
{
    private ProjectileGraphBaseData _projectileGraphBaseData;
    private ProjectileGraphShotInfoData _projectileGraphShotInfoData;
    private AnimationPlayer _animationPlayer = new AnimationPlayer();

    public ProjectileGraph(){}
    public ProjectileGraph(ProjectileGraphBaseData baseData){_projectileGraphBaseData = baseData;}

    private UnityEngine.Vector3 _movementOfFrame = UnityEngine.Vector3.zero;

    private bool _isEnd = false;

    private float _currentVelocity = 0f;
    private float _currentAngle = 0f;
    private float _currentLifeTime = 0f;

    public void initialize()
    {
        initialize(_projectileGraphBaseData._defaultProjectileShotInfoData);
    }

    public void initialize(ProjectileGraphShotInfoData shotInfoData)
    {
        setShotInfo(shotInfoData);

        _animationPlayer.initialize();
        _animationPlayer.changeAnimation(_projectileGraphBaseData._animationPlayData[0]);

        _movementOfFrame = UnityEngine.Vector3.zero;

        _isEnd = false;
    }

    private void setShotInfo(ProjectileGraphShotInfoData shotInfoData)
    {
        _projectileGraphShotInfoData = shotInfoData;

        _currentVelocity = _projectileGraphShotInfoData._deafaultVelocity;
        _currentAngle = _projectileGraphShotInfoData._defaultAngle;
        _currentLifeTime = _projectileGraphShotInfoData._lifeTime;
    }

    public bool progress(float deltaTime, ObjectBase targetEntity)
    {
        if(isEnd(deltaTime) == true)
            return true;

        if(isOutOfBound() == true)
            return true;

        _animationPlayer.progress(deltaTime,targetEntity);

        _currentVelocity += _projectileGraphShotInfoData._acceleration * deltaTime;

        if(_projectileGraphShotInfoData._friction != 0f)
            _currentVelocity = MathEx.convergence0(_currentVelocity,_projectileGraphShotInfoData._friction * deltaTime);
        if(_projectileGraphShotInfoData._angularAcceleration != 0f)
            _currentAngle += _projectileGraphShotInfoData._angularAcceleration * deltaTime;
            
        _movementOfFrame += (_currentVelocity * deltaTime) * (UnityEngine.Quaternion.Euler(0f,0f,_currentAngle) * UnityEngine.Vector3.right);

        return isEnd(deltaTime);
    }

    public void release()
    {
        _animationPlayer.Release();
    }

    public UnityEngine.Vector3 getMovementOfFrame() 
    {
        UnityEngine.Vector3 movement = _movementOfFrame;
        _movementOfFrame = UnityEngine.Vector3.zero;
        return movement;
    }
    public UnityEngine.Sprite getCurrentSprite() {return _animationPlayer.getCurrentSprite();}

    public bool isOutOfBound()
    {
        return false;
    }

    public bool isEnd(float deltaTime)
    {
        if(_isEnd == true)
            return true;

        _currentLifeTime -= deltaTime;
        _isEnd = _currentLifeTime <= 0f;

        return _isEnd;
    }
};