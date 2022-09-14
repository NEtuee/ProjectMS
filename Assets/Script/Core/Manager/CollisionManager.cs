using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct CollisionRequest
{
    public delegate void CollisionDelegate(object[] collisionData);
    public CollisionDelegate _collisionDelegate;
    public CollisionInfo _collision;
}

public class CollisionManager : Singleton<CollisionManager>
{
    private Dictionary<int, CollisionInfo> _collisionInfoList = new Dictionary<int, CollisionInfo>();
    
    


}
