using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProjectileEntityBase : ObjectBase
{
    private ProjectileGraph _projectileGraph;

    private CollisionInfo _collisionInfo;

    private Color _debugColor = Color.red;

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("ProjectileManager"));

        _projectileGraph = new ProjectileGraph(ProjectileGraphLoader.readFromXML(IOControl.PathForDocumentsFile("Assets\\Data\\Example\\ProjectileGraphExample.xml"))[0]);
        _projectileGraph.initialize();

        CollisionInfoData data = new CollisionInfoData(.1f,0f);
        _collisionInfo = new CollisionInfo(data);

        CollisionManager.Instance().registerObject(_collisionInfo, this, CollisionLayer.Default);
    }

    public void shot(ProjectileGraphShotInfoData shotInfoData, Vector3 startPosition)
    {
        transform.position = startPosition;
        _projectileGraph.initialize(shotInfoData);
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        _projectileGraph.progress(deltaTime, this);
        transform.position += _projectileGraph.getMovementOfFrame();

        _collisionInfo.updateCollisionInfo(transform.position,Vector3.right);
    }

    private void OnDrawGizmos()
    {
        if(_collisionInfo == null)
            return;
        
        Color color = Gizmos.color;

        Gizmos.color = _debugColor;
        for(int i = 0; i < 36; ++i)
        {
            float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
            float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

            float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
            float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

            Gizmos.DrawLine(new Vector3(x,y) * _collisionInfo.getRadius() + transform.position,new Vector3(x2,y2) * _collisionInfo.getRadius() + transform.position);
        }
        
        _debugColor = Color.red;
        Gizmos.color = color;
    }
}
