using System.IO;
using System.Collections.Generic;
using UnityEngine.Animations;

public class StatusInfoDataList : SerializableDataType
{
    public Dictionary<string, StatusInfoData> _statusInfoList = new Dictionary<string, StatusInfoData>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_statusInfoList.Count);
        foreach(var item in _statusInfoList)
        {
            binaryWriter.Write(item.Key);
            item.Value.serialize(ref binaryWriter);
        }
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        for(int i = 0; i < count; ++i)
        {
            string key = binaryReader.ReadString();
            StatusInfoData statusInfoData = new StatusInfoData();
            statusInfoData.deserialize(ref binaryReader);

            _statusInfoList.Add(key,statusInfoData);
        }
    }
}

public class StatusInfoData : SerializableDataType
{
    public string _statusInfoName;
    public bool _useHPEffect;
    public uint _defaultLevel = 0;

    public StatusDataFloat[] _statusData;
    public StatusGraphicInterfaceData[] _graphicInterfaceData;

    public StatusInfoData()
    {
    }

    public StatusInfoData(string name, bool useHPEffect, uint defaultLevel, StatusDataFloat[] statusArray, StatusGraphicInterfaceData[] graphicInterfaceArray)
    {
        _statusInfoName = name;
        _useHPEffect = useHPEffect;
        _statusData = statusArray;
        _defaultLevel = defaultLevel;
        _graphicInterfaceData = graphicInterfaceArray;
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_statusInfoName);
        binaryWriter.Write(_useHPEffect);
        binaryWriter.Write(_defaultLevel);
        BinaryHelper.writeArray<StatusDataFloat>(ref binaryWriter, _statusData);
        BinaryHelper.writeArray<StatusGraphicInterfaceData>(ref binaryWriter, _graphicInterfaceData);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _statusInfoName = binaryReader.ReadString();
        _useHPEffect = binaryReader.ReadBoolean();
        _defaultLevel = binaryReader.ReadUInt32();
        _statusData = BinaryHelper.readArray<StatusDataFloat>(ref binaryReader);
        _graphicInterfaceData = BinaryHelper.readArray<StatusGraphicInterfaceData>(ref binaryReader);
    }
}

public class StatusDataFloat : SerializableDataType
{
    public struct LevelData : SerializableStructure
    {
        public LevelData(uint level, float min, float max, float init)
        {
            _level = level;
            _maxValue = max;
            _minValue = min;
            _initialValue = init;
        }

        public uint     _level;
        public float    _maxValue;
        public float    _minValue;
        public float    _initialValue;

#if UNITY_EDITOR
        public void serialize(ref BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_level);
            binaryWriter.Write(_maxValue);
            binaryWriter.Write(_minValue);
            binaryWriter.Write(_initialValue);
        }
#endif
        public void deserialize(ref BinaryReader binaryReader)
        {
            _level = binaryReader.ReadUInt32();
            _maxValue = binaryReader.ReadSingle();
            _minValue = binaryReader.ReadSingle();
            _initialValue = binaryReader.ReadSingle();
        }
    }

    public string _statusName;
    public StatusType _statusType;

    public LevelData[] _statusLevelData = null;


    public StatusDataFloat(){}


    public bool getLevelData(uint level, ref LevelData levelData)
    {
        if(_statusLevelData == null)
        {
            DebugUtil.assert(false,"???");
            return false;
        }

        foreach(var item in _statusLevelData)
        {
            if(item._level == level)
            {
                levelData = item;
                return true;
            }
        }

        return false;
    }

    public void initStat(ref LevelData levelData, ref float value)
    {
        value = levelData._initialValue;
    }

    public void initLessStat(ref LevelData levelData, ref float value)
    {
        if(value < levelData._initialValue)
            value = levelData._initialValue;
    }

    public void variStat(ref LevelData levelData, ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(value + factor, levelData._minValue, levelData._maxValue + additionalMax);
    }

    public void setStat(ref LevelData levelData, ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(factor,levelData._minValue,levelData._maxValue + additionalMax);
    }


    public StatusType getStatusType() {return _statusType;}
    public string getName() {return _statusName;}

    
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_statusName);
        binaryWriter.Write((int)_statusType);
        BinaryHelper.writeArrayStructure<LevelData>(ref binaryWriter, _statusLevelData);
        
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _statusName = binaryReader.ReadString();
        _statusType = (StatusType)binaryReader.ReadInt32();
        _statusLevelData = BinaryHelper.readArrayStructure<LevelData>(ref binaryReader);
    }
}

public class StatusGraphicInterfaceData : SerializableDataType
{
    public UnityEngine.Color    _interfaceColor;
    public string               _targetStatus;
    public float                _horizontalGap;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_interfaceColor.r);
        binaryWriter.Write(_interfaceColor.g);
        binaryWriter.Write(_interfaceColor.b);
        binaryWriter.Write(_interfaceColor.a);

        binaryWriter.Write(_targetStatus);
        binaryWriter.Write(_horizontalGap);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        float r,g,b,a;

        r = binaryReader.ReadSingle();
        g = binaryReader.ReadSingle();
        b = binaryReader.ReadSingle();
        a = binaryReader.ReadSingle();

        _interfaceColor = new UnityEngine.Color(r,g,b,a);

        _targetStatus = binaryReader.ReadString();
        _horizontalGap = binaryReader.ReadSingle();
    }
}


public enum StatusVariType
{
    Fixed,
    List
};


public enum StatusType
{
    HP,
    Stamina,
    Blood,
    Battle,
    HitCount,
    GuardCount,
    Custom,
    Count,
}