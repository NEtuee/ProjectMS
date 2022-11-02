
public class CollisionInfoData
{
    private BoundBox _boundBox;
    private CollisionType _collisionType;
    private float _radius;
    private float _angle;

    public CollisionInfoData(float radius, float angle, CollisionType collisionType)
    {
        _boundBox = new BoundBox(radius,radius,UnityEngine.Vector3.zero);
        _radius = radius;
        _angle = angle;
        _collisionType = collisionType;
    }

    public bool isValid()
    {
        return _radius != 0f && _boundBox.isValid();
    }

    public BoundBox getBoundBox() {return _boundBox;}
    public CollisionType getCollisionType() {return _collisionType;}
    public float getRadius() {return _radius;}
    public float getSqrRadius() {return _radius * _radius;}

    public float getAngle() {return _angle;}
}


public enum CollisionType
{
    Default = 0,
    Character,
    Projectile,
    Attack,
    Count,
}