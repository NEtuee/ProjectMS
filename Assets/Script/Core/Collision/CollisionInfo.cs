using UnityEngine;

public class CollisionInfo
{
    private static int _uniqueIDPointer;
    private CollisionInfoData _collisionInfoData;
    private Vector3 _centerPosition;
    private Vector3 _direction;
    private BoundBox _boundBox;
    private Triangle _triangle;

    private int _uniqueID = 0;

    public CollisionInfo(CollisionInfoData data)
    {
        _collisionInfoData = data;
        _centerPosition = Vector3.zero;
        
        _boundBox = data.getBoundBox();
        _triangle = new Triangle(true);

        _uniqueID = _uniqueIDPointer++;
    }

    public bool isValid()
    {
        return _collisionInfoData != null && _collisionInfoData.isValid();
    }

    public bool collisionCheck(CollisionInfo target)
    {
        if(isValid() == false || target.isValid() == false)
        {
            DebugUtil.assert(false,"collision data is invalid : [{0}/{1}], [{2}/{3}]",getCollisionType(),isValid(),target.getCollisionType(), target.isValid());
            return false;
        }

        if(_boundBox.intersection(target.getBoundBox()) == false)
            return false;

        float circleDistance = Vector3.Distance(_centerPosition, target.getCenterPosition());
        if((circleDistance + target.getRadius()) < _collisionInfoData.getStartDistance() || (circleDistance + getRadius()) < target.getCollisionInfoData().getStartDistance())
            return false;

        bool circleCollision = circleDistance < getRadius() + target.getRadius();

        if(_collisionInfoData.getAngle() != 0f)
        {
            if(circleCollision == false)
                return false;

            Vector3 direction = (target.getCenterPosition() - _centerPosition).normalized;
            if(_collisionInfoData.getAngle() * 0.5f > Vector3.Angle(direction,_direction))
                return true;

            Vector3 result = Vector3.zero;
            float nearDistance = 0f;
            if(MathEx.findNearestPointOnTriangle(target._centerPosition,_triangle.get(0),_triangle.get(1),_triangle.get(2),out result,out nearDistance) == false)
                return true;
            
            if(nearDistance < target.getRadius())
                return true;

            return false;
        }

        return circleCollision;
    }

    public void drawCollosionArea(Color color, float time = 0f)
    {
        if(_collisionInfoData.getAngle() == 0f)
            drawCircle(color,Color.red, time);
        else
            drawSection(color,Color.red, time);
    }

    public void drawCircle(Color mainColor, Color startDistanceColor, float time = 0f)
    {
        GizmoHelper.instance.drawCircle(_centerPosition,getRadius(),36,mainColor, time);
        if(_collisionInfoData.getStartDistance() != 0f)
            GizmoHelper.instance.drawCircle(_centerPosition,_collisionInfoData.getStartDistance(),36,startDistanceColor, time);
    }

    public void drawSection(Color mainColor, Color startDistanceColor, float time = 0f)
    {
        //GizmoHelper.instance.drawPolygon(_triangle.getVertices(),color, time); 
        GizmoHelper.instance.drawArc(_centerPosition,getRadius(),_collisionInfoData.getAngle(),_direction,mainColor,time);

        if(_collisionInfoData.getStartDistance() != 0f)
            GizmoHelper.instance.drawArc(_centerPosition,_collisionInfoData.getStartDistance(),_collisionInfoData.getAngle(),_direction,startDistanceColor,time);
    }

    public void drawBoundBox(Color color)
    {
        GizmoHelper.instance.drawPolygon(_boundBox.getVertices(),color);
    }

    public void setCollisionInfo(float radius, float angle, float startDistance)
    {
        _collisionInfoData.setCollisionInfoData(radius,angle,startDistance,CollisionType.Projectile);
    }

    public void updateCollisionInfo(Vector3 position, Vector3 direction)
    {
        _centerPosition = position;
        _direction = direction;

        if(_collisionInfoData.getAngle() != 0f)
        {
            _triangle.makeTriangle(position,_collisionInfoData.getRadius(), _collisionInfoData.getAngle(), MathEx.clampDegree(Vector3.SignedAngle(Vector3.right, direction, Vector3.forward)));
            //_boundBox.updateBoundBox(_triangle);

        }


        _boundBox.updateBoundBox(position);
    }

    public int getUniqueID() {return _uniqueID;}
    public float getRadius() {return _collisionInfoData.getRadius();}
    public float getSqrRadius() {return _collisionInfoData.getSqrRadius();}
    public Vector3 getCenterPosition() {return _centerPosition;}
    public Vector3 getDirection() {return _direction;}
    public CollisionType getCollisionType() {return _collisionInfoData.getCollisionType();}
    public CollisionInfoData getCollisionInfoData() {return _collisionInfoData;}
    public BoundBox getBoundBox() {return _boundBox;}
}