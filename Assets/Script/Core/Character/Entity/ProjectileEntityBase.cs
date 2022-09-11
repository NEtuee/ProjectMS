using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEntityBase : ObjectBase
{
    private ProjectileGraph _projectileGraph;

    public override void initialize()
    {
        base.initialize();
        RegisterRequest(QueryUniqueID("ProjectileManager"));

        _projectileGraph = new ProjectileGraph(ProjectileGraphLoader.readFromXML(IOControl.PathForDocumentsFile("Assets\\Data\\Example\\ProjectileGraphExample.xml"))[0]);
        _projectileGraph.initialize();
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
    }

    public override void fixedProgress(float deltaTime)
    {
        base.fixedProgress(deltaTime);
        transform.position += _projectileGraph.getMovementOfFrame();
    }
}
