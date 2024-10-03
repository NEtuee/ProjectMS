using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

public enum ProjectileChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnHitEnd,
    ChildFrameEvent_OnEnd,
    Count,
}

public class ProjectileGraphBaseDataList : SerializableDataType
{
    public ProjectileGraphBaseData[] _projectileGraphBaseDataList = null;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeArray<ProjectileGraphBaseData>(ref binaryWriter, _projectileGraphBaseDataList);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _projectileGraphBaseDataList = BinaryHelper.readArray<ProjectileGraphBaseData>(ref binaryReader);
    }
}

public class ProjectileGraphBaseData : SerializableDataType
{
    public string                       _name = "";
    public ProjectileGraphShotInfoData  _defaultProjectileShotInfoData = new ProjectileGraphShotInfoData();
    public AnimationPlayDataInfo[]      _animationPlayData = null;

    public Dictionary<ProjectileChildFrameEventType,ChildFrameEventItem> _projectileChildFrameEvent = new Dictionary<ProjectileChildFrameEventType, ChildFrameEventItem>();

    public float                        _collisionRadius = 0.1f;
    public float                        _collisionAngle = 0f;
    public float                        _collisionStartDistance = 0f;
    public float                        _gravity = 0f;

    public bool                         _useSpriteRotation = false;
    public bool                         _castShadow = false;

    public bool                         _executeBySummoner = false;
    public bool                         _cameraBound = false;

    public int                          _penetrateCount = 1;

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        _defaultProjectileShotInfoData.serialize(ref binaryWriter);
        BinaryHelper.writeArray<AnimationPlayDataInfo>(ref binaryWriter, _animationPlayData);
        binaryWriter.Write(_projectileChildFrameEvent == null ? 0 : _projectileChildFrameEvent.Count);
        if(_projectileChildFrameEvent != null)
        {
            foreach(var item in _projectileChildFrameEvent)
            {
                binaryWriter.Write((int)item.Key);
                item.Value.serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_collisionRadius);
        binaryWriter.Write(_collisionAngle);
        binaryWriter.Write(_collisionStartDistance);
        binaryWriter.Write(_gravity);
        binaryWriter.Write(_useSpriteRotation);
        binaryWriter.Write(_castShadow);
        binaryWriter.Write(_executeBySummoner);
        binaryWriter.Write(_penetrateCount );
        binaryWriter.Write(_cameraBound);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _defaultProjectileShotInfoData = new ProjectileGraphShotInfoData();
        _defaultProjectileShotInfoData.deserialize(ref binaryReader);
        _animationPlayData = BinaryHelper.readArray<AnimationPlayDataInfo>(ref binaryReader);
        int childFrameEventCount = binaryReader.ReadInt32();
        for(int i =0; i < childFrameEventCount; ++i)
        {
            ProjectileChildFrameEventType eventType = (ProjectileChildFrameEventType)binaryReader.ReadInt32();
            ChildFrameEventItem childFrameEventItem = new ChildFrameEventItem();
            childFrameEventItem.deserialize(ref binaryReader);

            _projectileChildFrameEvent.Add(eventType, childFrameEventItem);
        }

        _collisionRadius = binaryReader.ReadSingle();
        _collisionAngle = binaryReader.ReadSingle();
        _collisionStartDistance = binaryReader.ReadSingle();
        _gravity = binaryReader.ReadSingle();
        _useSpriteRotation = binaryReader.ReadBoolean();
        _castShadow = binaryReader.ReadBoolean();
        _executeBySummoner = binaryReader.ReadBoolean();
        _penetrateCount = binaryReader.ReadInt32();
        _cameraBound = binaryReader.ReadBoolean();
    }
}

public struct ProjectileGraphShotInfoData : SerializableStructure
{
    public float                    _deafaultVelocity;
    public float                    _acceleration;
    public float                    _friction;
    public float                    _defaultAngle;
    public float                    _angularAcceleration;
    public float                    _lifeTime;
    public UnityEngine.Vector3      _offset;

    public bool                     _useRandomAngle;
    public UnityEngine.Vector2      _randomAngle;

    public ProjectileGraphShotInfoData(float defaultVelocity, float acceleration, float friction, float defaultAngle, float angularAcceleration, float lifeTime, UnityEngine.Vector3 offset, bool useRandomAngle, UnityEngine.Vector2 randomAngle)
    {
        _deafaultVelocity = defaultVelocity;
        _acceleration = acceleration;
        _friction = friction;
        _defaultAngle = defaultAngle;
        _angularAcceleration = angularAcceleration;
        _lifeTime = lifeTime;
        _offset = offset;

        _useRandomAngle = useRandomAngle;
        _randomAngle = randomAngle;
    }

    public static ProjectileGraphShotInfoData operator +(ProjectileGraphShotInfoData a, ProjectileGraphShotInfoData b)
    {
        return new ProjectileGraphShotInfoData{
            _acceleration = a._acceleration + b._acceleration
            , _angularAcceleration = a._angularAcceleration + b._angularAcceleration
            , _deafaultVelocity = a._deafaultVelocity + b._deafaultVelocity
            , _defaultAngle = a._defaultAngle + b._defaultAngle
            , _friction = a._friction + b._friction
            , _lifeTime = a._lifeTime + b._lifeTime
            , _offset = a._offset + b._offset
            , _useRandomAngle = a._useRandomAngle | b._useRandomAngle
            , _randomAngle = a._randomAngle + b._randomAngle};
    }

    public void clear()
    {
        _deafaultVelocity = 0f;
        _acceleration = 0f;
        _friction = 0f;
        _defaultAngle = 0f;
        _angularAcceleration = 0f;
        _lifeTime = 0f;
        _offset = UnityEngine.Vector3.zero;

        _useRandomAngle = false;
        _randomAngle = UnityEngine.Vector2.zero;
    }
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_deafaultVelocity);
        binaryWriter.Write(_acceleration);
        binaryWriter.Write(_friction);
        binaryWriter.Write(_defaultAngle);
        binaryWriter.Write(_angularAcceleration);
        binaryWriter.Write(_lifeTime);
        BinaryHelper.writeVector3(ref binaryWriter, _offset);
        binaryWriter.Write(_useRandomAngle);
        BinaryHelper.writeVector2(ref binaryWriter, _randomAngle);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _deafaultVelocity = binaryReader.ReadSingle();
        _acceleration = binaryReader.ReadSingle();
        _friction = binaryReader.ReadSingle();
        _defaultAngle = binaryReader.ReadSingle();
        _angularAcceleration = binaryReader.ReadSingle();
        _lifeTime = binaryReader.ReadSingle();
        _offset = BinaryHelper.readVector3(ref binaryReader);
        _useRandomAngle = binaryReader.ReadBoolean();
        _randomAngle = BinaryHelper.readVector2(ref binaryReader);
    }
}

