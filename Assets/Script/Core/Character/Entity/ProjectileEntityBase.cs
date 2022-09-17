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
        GizmoHelper.instance.drawCircle(transform.position,_collisionInfo.getRadius(),36,_debugColor);
    }
}
