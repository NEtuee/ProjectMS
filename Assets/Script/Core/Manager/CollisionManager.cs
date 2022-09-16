using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CollisionDelegate(object collisionData);

public struct CollisionRequestData
{
    public CollisionDelegate _collisionDelegate;
    public CollisionInfo _collision;
}

public struct CollisionObjectData
{
    public CollisionInfo _collisionInfo;
    public object _collisionObject;
}

public class CollisionManager : Singleton<CollisionManager>
{
    private Dictionary<int, List<CollisionObjectData>>          _collisionObjectList = new Dictionary<int, List<CollisionObjectData>>();
    private Dictionary<int, Stack<CollisionRequestData>>        _collisionRequestStack = new Dictionary<int, Stack<CollisionRequestData>>();

    private int                                                 _collisionLayerCount = 0;
    private int                                                 _collisionRequestCount = 0;

    public CollisionManager()
    {
        _collisionLayerCount = (int)CollisionLayer.Count;

        buildCollisionInfoList();
    }

    private void buildCollisionInfoList()
    {
        for(int i = 0; i < _collisionLayerCount; ++i)
        {
            _collisionObjectList.Add(i, new List<CollisionObjectData>());
            _collisionRequestStack.Add(i, new Stack<CollisionRequestData>());
        }
    }

    public void registerObject(CollisionInfo collisionData, object collisionObject, CollisionLayer collisionLayer)
    {
        registerObject(new CollisionObjectData{_collisionInfo = collisionData, _collisionObject = collisionObject},collisionLayer);
    }

    public void registerObject(CollisionObjectData objectData, CollisionLayer collisionLayer)
    {
        _collisionObjectList[(int)collisionLayer].Add(objectData);
    }

    public void collisionRequest(CollisionInfo collisionData, CollisionDelegate collisionDelegate, CollisionLayer collisionLayer)
    {
        collisionRequest(new CollisionRequestData{_collisionDelegate = collisionDelegate, _collision = collisionData}, collisionLayer);
    }

    public void collisionRequest(CollisionRequestData request, CollisionLayer collisionLayer)
    {
        ++_collisionRequestCount;
        _collisionRequestStack[(int)collisionLayer].Push(request);
    }

    public void collisionUpdate()
    {
        if(_collisionRequestCount == 0)
            return;

        for(int i = 0; i < _collisionLayerCount; ++i)
        {
            while(_collisionRequestStack[i].Count != 0)
            {
                CollisionRequestData request = _collisionRequestStack[i].Pop();
                collisionCheck(i,request);
            }
            
        }

        _collisionRequestCount = 0;
    }

    private void collisionCheck(int layer, CollisionRequestData request)
    {
        List<CollisionObjectData> collisionList = _collisionObjectList[layer];
        CollisionInfo collisionInfo = request._collision;
        for(int i = 0; i < collisionList.Count; ++i)
        {
            if(collisionInfo.getUniqueID() == collisionList[i]._collisionInfo.getUniqueID())
                continue;
                
            if(collisionInfo.collisionCheck(collisionList[i]._collisionInfo) == true)
                request._collisionDelegate(collisionList[i]._collisionObject);
        }
    }
}
