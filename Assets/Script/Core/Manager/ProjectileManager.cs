using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : PoolingManagerBase<ProjectileEntityBase>
{
    public static ProjectileManager _instance;
    public string _projectileGraphPath = "";

    private Queue<ProjectileEntityBase> _projectilePool = new Queue<ProjectileEntityBase>();

    private Dictionary<string,ProjectileGraphBaseData> _projectileGraphDataList = new Dictionary<string, ProjectileGraphBaseData>();

    public override void assign()
    {
        _instance = this;

        base.assign();
        CacheUniqueID("ProjectileManager");
        RegisterRequest();
        
        _projectileGraphDataList.Clear();
        ProjectileGraphBaseData[] graphDataList = ProjectileGraphLoader.readFromXML(IOControl.PathForDocumentsFile(_projectileGraphPath));

        if(graphDataList == null)
        {
            DebugUtil.assert(false, "projectileGraphBaseData load fail: {0}",_projectileGraphPath);
            return;
        }

        for(int i = 0; i < graphDataList.Length; ++i)
        {
            _projectileGraphDataList.Add(graphDataList[i]._name, graphDataList[i]);
        }
        
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
    }

    public void spawnProjectile(string name, ref ProjectileGraphShotInfoData shotInfo, Vector3 startPosition, SearchIdentifier searchIdentifier)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity._searchIdentifier = searchIdentifier;
        entity.setData(getProjectileGraphData(name));
        entity.initialize();
        entity.shot(shotInfo,startPosition);
    }

    public void spawnProjectile(string name, Vector3 startPosition, SearchIdentifier searchIdentifier)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity._searchIdentifier = searchIdentifier;
        entity.setData(getProjectileGraphData(name));
        entity.initialize();
        entity.shot(startPosition);
    }

    public ProjectileGraphBaseData getProjectileGraphData(string name)
    {
        if(_projectileGraphDataList.ContainsKey(name) == false)
        {
            DebugUtil.assert(false, "projectileGraphBaseData is not exists : {0}",name);
            return null;
        }

        return _projectileGraphDataList[name];
    }
}
