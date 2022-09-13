
public class CollisionInfoData
{
    private BoundBox _boundBox;
    private float _radius;
    private float _angle;

    public CollisionInfoData(float radius, float angle)
    {
        _boundBox = new BoundBox(radius,radius,UnityEngine.Vector3.zero);
        _radius = radius;
        _angle = angle;
    }

    
}
