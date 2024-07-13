using System.IO;

public enum DanmakuVariableEventType
{
    Add = 0,
    Set,
    Mul,
    Div,
    Count
};

public enum DanmakuVariableType
{
    Velocity = 0,
    Acceleration,
    Friction,
    Angle,
    AngularAccel,
    LifeTime,
    OffsetX,
    OffsetY,
    OffsetZ,
    Count
};

public enum DanmakuEventType
{
    VariableEvent,
    ProjectileEvent,
    LoopEvent,
    WaitEvent,
    Count,
}

[System.Serializable]
public abstract class DanmakuEventBase : SerializableDataType
{
    public abstract DanmakuEventType getEventType();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeEnum<DanmakuEventType>(ref binaryWriter, getEventType());
    }
#endif
    public static DanmakuEventBase buildDanmakuEvent(ref BinaryReader binaryReader)
    {
        DanmakuEventType eventType = BinaryHelper.readEnum<DanmakuEventType>(ref binaryReader);
        DanmakuEventBase danmakuEvent = getDanmakuEventBase(eventType);
        danmakuEvent.deserialize(ref binaryReader);

        return danmakuEvent;
    }

    public static DanmakuEventBase getDanmakuEventBase(DanmakuEventType danmakuEventType)
    {
        switch(danmakuEventType)
        {
            case DanmakuEventType.VariableEvent:
                return new DanmakuVariableEventData();
            case DanmakuEventType.LoopEvent:
                return new DanmakuLoopEventData();
            case DanmakuEventType.ProjectileEvent:
                return new DanmakuProjectileEventData();
            case DanmakuEventType.WaitEvent:
                return new DanmakuWaitEventData();
        }

        return null;
    }
}

[System.Serializable]
public class DanmakuVariableEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.VariableEvent;

    public DanmakuVariableType _type;
    public DanmakuVariableEventType[] _eventType;

    public int _eventCount;

    public FloatEx[] _value;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        BinaryHelper.writeEnum<DanmakuVariableType>(ref binaryWriter, _type);
        BinaryHelper.writeEnumArray<DanmakuVariableEventType>(ref binaryWriter, _eventType);
        BinaryHelper.writeArray<FloatEx>(ref binaryWriter, _value);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _type = BinaryHelper.readEnum<DanmakuVariableType>(ref binaryReader);
        _eventType = BinaryHelper.readArrayEnum<DanmakuVariableEventType>(ref binaryReader);
        _eventCount = _eventType == null ? 0 : _eventType.Length;
        _value = BinaryHelper.readArray<FloatEx>(ref binaryReader);
    }
};

[System.Serializable]
public class DanmakuProjectileEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.ProjectileEvent;
    
    public string _projectileName;
    public DirectionType _directionType;
    public ActionFrameEvent_Projectile.ShotInfoUseType _shotInfoUseType;

    public ActionFrameEvent_Projectile.PredictionType _predictionType = ActionFrameEvent_Projectile.PredictionType.Path;
    public UnityEngine.Vector3[]           _pathPredictionArray = null;
    public SetTargetType                   _setTargetType = SetTargetType.SetTargetType_Self;
    public int                             _predictionAccuracy = 0;
    public float                           _startTerm = 0f;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        binaryWriter.Write(_projectileName);
        BinaryHelper.writeEnum<DirectionType>(ref binaryWriter, _directionType);
        BinaryHelper.writeEnum<ActionFrameEvent_Projectile.ShotInfoUseType>(ref binaryWriter, _shotInfoUseType);
        BinaryHelper.writeEnum<ActionFrameEvent_Projectile.PredictionType>(ref binaryWriter, _predictionType);
        BinaryHelper.writeArray(ref binaryWriter, _pathPredictionArray);
        BinaryHelper.writeEnum<SetTargetType>(ref binaryWriter, _setTargetType);
        binaryWriter.Write(_predictionAccuracy);
        binaryWriter.Write(_startTerm);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _projectileName = binaryReader.ReadString();
        _directionType = BinaryHelper.readEnum<DirectionType>(ref binaryReader);
        _shotInfoUseType = BinaryHelper.readEnum<ActionFrameEvent_Projectile.ShotInfoUseType>(ref binaryReader);
        _predictionType = BinaryHelper.readEnum<ActionFrameEvent_Projectile.PredictionType>(ref binaryReader);
        _pathPredictionArray = BinaryHelper.readArrayVector3(ref binaryReader);
        _setTargetType = BinaryHelper.readEnum<SetTargetType>(ref binaryReader);
        _predictionAccuracy = binaryReader.ReadInt32();
        _startTerm = binaryReader.ReadSingle();
    }
    
};

[System.Serializable]
public class DanmakuLoopEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.LoopEvent;

    public int _loopCount = 0;
    public float _term = 0f;

    public DanmakuEventBase[] _events = null;
    public int _eventCount = 0;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        binaryWriter.Write(_loopCount);
        binaryWriter.Write(_term);
        BinaryHelper.writeArray<DanmakuEventBase>(ref binaryWriter, _events);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _loopCount = binaryReader.ReadInt32();
        _term = binaryReader.ReadSingle();
        _eventCount = binaryReader.ReadInt32();
        _events = _eventCount == 0 ? null : new DanmakuEventBase[_eventCount];
        for(int i = 0; i < _eventCount; ++i)
        {
            _events[i] = DanmakuEventBase.buildDanmakuEvent(ref binaryReader);
        }
    }
};

[System.Serializable]
public class DanmakuWaitEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.WaitEvent;

    public float _waitTime = 0f;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        binaryWriter.Write(_waitTime);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _waitTime = binaryReader.ReadSingle();
    }
}

[System.Serializable]
public class DanmakuGraphBaseData : SerializableDataType
{
    public string _name;

    public DanmakuEventBase[] _danamkuEventList;
    public int _danamkuEventCount;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_name);
        BinaryHelper.writeArray<DanmakuEventBase>(ref binaryWriter, _danamkuEventList);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _name = binaryReader.ReadString();
        _danamkuEventCount = binaryReader.ReadInt32();
        _danamkuEventList = _danamkuEventCount == 0 ? null : new DanmakuEventBase[_danamkuEventCount];
        for(int i = 0; i < _danamkuEventCount; ++i)
        {
            _danamkuEventList[i] = DanmakuEventBase.buildDanmakuEvent(ref binaryReader);
        }
    }
}