using System.IO;
using System.Collections.Generic;

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

    public StatusDataFloat[] _statusData;
    public StatusGraphicInterfaceData[] _graphicInterfaceData;

    public StatusInfoData()
    {
    }

    public StatusInfoData(string name, bool useHPEffect, StatusDataFloat[] statusArray, StatusGraphicInterfaceData[] graphicInterfaceArray)
    {
        _statusInfoName = name;
        _useHPEffect = useHPEffect;
        _statusData = statusArray;
        _graphicInterfaceData = graphicInterfaceArray;
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_statusInfoName);
        binaryWriter.Write(_useHPEffect);
        BinaryHelper.writeArray<StatusDataFloat>(ref binaryWriter, _statusData);
        BinaryHelper.writeArray<StatusGraphicInterfaceData>(ref binaryWriter, _graphicInterfaceData);
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _statusInfoName = binaryReader.ReadString();
        _useHPEffect = binaryReader.ReadBoolean();
        _statusData = BinaryHelper.readArray<StatusDataFloat>(ref binaryReader);
        _graphicInterfaceData = BinaryHelper.readArray<StatusGraphicInterfaceData>(ref binaryReader);
    }
}

public class StatusDataFloat : SerializableDataType
{
    public string _statusName;
    public StatusType _statusType;

    public float _maxValue;
    public float _minValue;
    public float _initialValue;

    public StatusDataFloat(){}
    public StatusDataFloat(StatusType type, string name, float min, float max, float init)
    {
        _statusType = type;
        _statusName = name;

        _maxValue = max;
        _minValue = min;
        _initialValue = init;
    }

    public bool isStatusValid()
    {
        return _statusType != StatusType.Count && _maxValue >= _minValue;
    }

    public void initStat(ref float value)
    {
        value = _initialValue;
    }

    public void variStat(ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(value + factor, _minValue, _maxValue + additionalMax);
    }

    public void setStat(ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(factor,_minValue,_maxValue + additionalMax);
    }


    public bool isMax(float value)
    {
        return value >= _maxValue;
    }

    public bool isMin(float value)
    {
        return value <= _minValue;
    }

    public StatusType getStatusType() {return _statusType;}
    public string getName() {return _statusName;}

    public float getInitValue() {return _initialValue;}
    public float getMaxValue() {return _maxValue;}
    public float getMinValue() {return _minValue;}
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_statusName);
        binaryWriter.Write((int)_statusType);
        binaryWriter.Write(_maxValue);
        binaryWriter.Write(_minValue);
        binaryWriter.Write(_initialValue);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _statusName = binaryReader.ReadString();
        _statusType = (StatusType)binaryReader.ReadInt32();
        _maxValue = binaryReader.ReadSingle();
        _minValue = binaryReader.ReadSingle();
        _initialValue = binaryReader.ReadSingle();
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