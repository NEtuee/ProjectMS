using UnityEngine;

public class CollisionInfo
{
    private CollisionInfoData _collisionInfoData;
    private Vector3 _centerPosition;
    private Vector3 _direction;
    private BoundBox _boundBox;

    public CollisionInfo(CollisionInfoData data)
    {
        _collisionInfoData = data;
        _centerPosition = Vector3.zero;
        
        _boundBox = data.getBoundBox();
    }

    public bool isValid()
    {
        return _collisionInfoData != null && _collisionInfoData.isValid();
    }

    public bool collisionCheck(CollisionInfo target)
    {
        if(isValid() == false || target.isValid() == false)
        {
            DebugUtil.assert(false,"collision data is invalid");
            return false;
        }

        if(_boundBox.intersection(target.getBoundBox()) == false)
            return false;

        float distanceSq = (_centerPosition - target.getCenterPosition()).sqrMagnitude;
        if(distanceSq >= getSqrRadius() + target.getSqrRadius())
            return false;

        return true;
    }

    public void updateCollisionInfo(Vector3 position, Vector3 direction)
    {
        _centerPosition = position;
        _direction = direction;
        _boundBox.updateBoundBox(position);
    }

    public float getSqrRadius() {return _collisionInfoData.getSqrRadius();}
    public Vector3 getCenterPosition() {return _centerPosition;}
    public Vector3 getDirection() {return _direction;}
    public CollisionInfoData getCollisionInfoData() {return _collisionInfoData;}
    public BoundBox getBoundBox() {return _boundBox;}
}