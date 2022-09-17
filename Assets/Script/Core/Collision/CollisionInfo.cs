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
            DebugUtil.assert(false,"collision data is invalid");
            return false;
        }

        if(_boundBox.intersection(target.getBoundBox()) == false)
            return false;


        if(_collisionInfoData.getAngle() != 0f)
        {
            Vector3 result = Vector3.zero;
            float nearDistance = 0f;
            if(MathEx.findNearestPointOnTriangle(target._centerPosition,_triangle.get(0),_triangle.get(1),_triangle.get(2),out result,out nearDistance) == false)
                return true;
            
            return nearDistance < target.getRadius();
        }


        float distance = Vector3.Distance(_centerPosition, target.getCenterPosition());
        return distance < getRadius() + target.getRadius();
    }

    public void drawCollosionArea(Color color)
    {
        if(_collisionInfoData.getAngle() == 0f)
            drawCircle(color);
        else
            drawTriangle(color);
    }

    public void drawCircle(Color color)
    {
        GizmoHelper.instance.drawCircle(_centerPosition,getRadius(),36,color);
    }

    public void drawTriangle(Color color)
    {
        GizmoHelper.instance.drawPolygon(_triangle.getVertices(),color);
    }

    public void drawBoundBox(Color color)
    {
        GizmoHelper.instance.drawPolygon(_boundBox.getVertices(),color);
    }

    public void updateCollisionInfo(Vector3 position, Vector3 direction)
    {
        _centerPosition = position;
        _direction = direction;

        if(_collisionInfoData.getAngle() != 0f)
        {
            _triangle.makeTriangle(position,_collisionInfoData.getRadius(), _collisionInfoData.getAngle(), MathEx.clampDegree(Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.z));
            _boundBox.updateBoundBox(_triangle);

        }
        else
        {
            _boundBox.updateBoundBox(position);
        }
    }

    public int getUniqueID() {return _uniqueID;}
    public float getRadius() {return _collisionInfoData.getRadius();}
    public float getSqrRadius() {return _collisionInfoData.getSqrRadius();}
    public Vector3 getCenterPosition() {return _centerPosition;}
    public Vector3 getDirection() {return _direction;}
    public CollisionInfoData getCollisionInfoData() {return _collisionInfoData;}
    public BoundBox getBoundBox() {return _boundBox;}
}