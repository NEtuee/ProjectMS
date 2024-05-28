using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedProjectileItem
{
    public float _timer;
    public string _graphName = "";
    public ProjectileGraphShotInfoData _shotInfo;
    public AllyInfoData _allyInfo;
    public ObjectBase _executeEntity;
    public ObjectBase _targetEntity;

    public ObjectBase _summoner;

    public SetTargetType _sethTargetType;

    public bool updateTimer(float deltaTime)
    {
        _timer -= deltaTime;
        return _timer <= 0f;
    }
}

public class ProjectileManager : PoolingManagerBase<ProjectileEntityBase>
{
    public static ProjectileManager _instance;
    public string _projectileGraphPath = "";

    private Queue<ProjectileEntityBase> _projectilePool = new Queue<ProjectileEntityBase>();
    private SimplePool<DelayedProjectileItem> _delayedProjectileItemPool = new SimplePool<DelayedProjectileItem>();

    private List<DelayedProjectileItem> _currentUpdateList = new List<DelayedProjectileItem>();
    private Dictionary<string,ProjectileGraphBaseData> _projectileGraphDataList = new Dictionary<string, ProjectileGraphBaseData>();

    public override void assign()
    {
        _instance = this;

        base.assign();
        CacheUniqueID("ProjectileManager");
        RegisterRequest();
        
        _projectileGraphDataList.Clear();
        ProjectileGraphBaseData[] graphDataList = ResourceContainerEx.Instance().GetProjectileGraphBaseData(IOControl.PathForDocumentsFile(_projectileGraphPath));

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

    public override void initialize()
    {
        _projectilePool.Clear();
        for(int index = 0; index < _currentUpdateList.Count; ++index)
        {
            _delayedProjectileItemPool.enqueue(_currentUpdateList[index]);
        }

        _currentUpdateList.Clear();
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        for(int index = 0; index < _currentUpdateList.Count; ++index)
        {
            if(_currentUpdateList[index].updateTimer(deltaTime))
            {
                Vector3 spawnPosition = ActionFrameEvent_Projectile.getSpawnPosition(_currentUpdateList[index]._sethTargetType,_currentUpdateList[index]._executeEntity,_currentUpdateList[index]._targetEntity);
                spawnProjectile(_currentUpdateList[index]._graphName,ref _currentUpdateList[index]._shotInfo,spawnPosition,_currentUpdateList[index]._summoner,_currentUpdateList[index]._allyInfo);

                _delayedProjectileItemPool.enqueue(_currentUpdateList[index]);
                _currentUpdateList.RemoveAt(index);
                --index;
            }

        }
    }

    public void spawnProjectileDelayed(string name, float time, ObjectBase executeEntity, ObjectBase targetEntity, SetTargetType setTargetType, ref ProjectileGraphShotInfoData shotInfo, ObjectBase summoner, AllyInfoData allyInfo)
    {
        DelayedProjectileItem delayedProjectileItem = _delayedProjectileItemPool.dequeue();
        delayedProjectileItem._executeEntity = executeEntity;
        delayedProjectileItem._targetEntity = targetEntity;
        delayedProjectileItem._summoner = summoner;
        delayedProjectileItem._graphName = name;
        delayedProjectileItem._shotInfo = shotInfo;
        delayedProjectileItem._timer = time;
        delayedProjectileItem._sethTargetType = setTargetType;
        delayedProjectileItem._allyInfo = allyInfo;

        _currentUpdateList.Add(delayedProjectileItem);
    }

    public void spawnProjectile(string name, ref ProjectileGraphShotInfoData shotInfo, Vector3 startPosition, ObjectBase summoner, AllyInfoData allyInfo)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity.name = name;
        entity.setAllyInfo(allyInfo);
        entity.setSummonObject(summoner);
        entity.setData(getProjectileGraphData(name));
        entity.initialize();
        entity.shot(shotInfo,startPosition);
    }

    public void spawnProjectile(string name, Vector3 startPosition, ObjectBase summoner, AllyInfoData allyInfo)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity.setAllyInfo(allyInfo);
        entity.setSummonObject(summoner);
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
