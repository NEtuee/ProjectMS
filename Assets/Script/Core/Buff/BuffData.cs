using System.Collections.Generic;
using System.IO;

public class BuffDataList : SerializableDataType
{
    public Dictionary<int, BuffData> _buffDataList = new Dictionary<int, BuffData>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_buffDataList.Count);
        foreach(var item in _buffDataList)
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
            int key = binaryReader.ReadInt32();
            BuffData buffData = new BuffData();
            buffData.deserialize(ref binaryReader);

            _buffDataList.Add(key,buffData);
        }
    }
}

public class BuffData : SerializableDataType
{
    public string              _buffName = "";
    public int                 _buffKey = -1;
    public string              _targetStatusName = "";

    public BuffType            _buffType = BuffType.Count;
    public BuffUpdateType      _buffUpdateType = BuffUpdateType.Count;
    public BuffApplyType       _buffApplyType = BuffApplyType.Count;

    public DefenceType         _defenceType = DefenceType.Count;

    public bool                _allowOverlap = false;

    public float               _buffVaryStatFactor = 0f;
    public float               _buffCustomValue0 = 0f;
    public float               _buffCustomValue1 = 0f;
    public string[]            _buffCustomValue2 = null;

    public string              _buffCustomStatusName = "";

    public string              _buffStartEffectPreset = "";
    public string              _buffEndEffectPreset = "";

    public string              _particleEffect = "";
    public string              _timelineEffect = "";
    public string              _effectPreset = "";

    public int                 _audioID = -1;
    public int[]               _audioParameter = null;

    public BuffData()
    {
        _buffName = "";
        _buffKey = -1;
        _targetStatusName = "";
        _buffType = BuffType.Status;
        _buffUpdateType = BuffUpdateType.Count;
        _buffApplyType = BuffApplyType.Count;
        _defenceType = DefenceType.Count;
        _allowOverlap = false;

        _buffVaryStatFactor = 0f;
        _buffCustomValue0 = 0f;
        _buffCustomValue1 = 0f;
        _buffCustomValue2 = null;

        _buffStartEffectPreset = "";
        _buffEndEffectPreset = "";

        _particleEffect = "";
        _timelineEffect = "";
        _effectPreset = "";
        _audioID = -1;
        _audioParameter = null;
    }

    public void copy(BuffData target)
    {
        _buffKey = target._buffKey;
        _targetStatusName = target._targetStatusName;
        _buffType = target._buffType;
        _buffUpdateType = target._buffUpdateType;
        _buffApplyType = target._buffApplyType;
        _defenceType = target._defenceType;
        _allowOverlap = target._allowOverlap;
        _buffVaryStatFactor = target._buffVaryStatFactor;
        _buffCustomValue0 = target._buffCustomValue0;
        _buffCustomValue1 = target._buffCustomValue1;
        _buffCustomValue2 = target._buffCustomValue2;
        _buffStartEffectPreset = target._buffStartEffectPreset;
        _buffEndEffectPreset = target._buffEndEffectPreset;
        _particleEffect = target._particleEffect;
        _timelineEffect = target._timelineEffect;
        _effectPreset = target._effectPreset;
        _audioID = target._audioID;
        _audioParameter = target._audioParameter;
    }


    public bool isStatusBuffValid()
    {
        return _buffKey >= 0 && _buffUpdateType != BuffUpdateType.Count && _buffApplyType != BuffApplyType.Count && _buffType != BuffType.Count;
    }

    public bool isDefenceBuffValid()
    {
        return _buffKey >= 0 && _buffUpdateType != BuffUpdateType.Count && _defenceType != DefenceType.Count && _buffType != BuffType.Count;
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_buffName);
        binaryWriter.Write(_buffKey);
        binaryWriter.Write(_targetStatusName);
        BinaryHelper.writeEnum<BuffType>(ref binaryWriter, _buffType);
        BinaryHelper.writeEnum<BuffUpdateType>(ref binaryWriter, _buffUpdateType);
        BinaryHelper.writeEnum<BuffApplyType>(ref binaryWriter, _buffApplyType);
        BinaryHelper.writeEnum<DefenceType>(ref binaryWriter, _defenceType);
        binaryWriter.Write(_allowOverlap);
        binaryWriter.Write(_buffVaryStatFactor);
        binaryWriter.Write(_buffCustomValue0);
        binaryWriter.Write(_buffCustomValue1);
        BinaryHelper.writeArray(ref binaryWriter, _buffCustomValue2);
        binaryWriter.Write(_buffCustomStatusName);
        binaryWriter.Write(_buffStartEffectPreset);
        binaryWriter.Write(_buffEndEffectPreset);
        binaryWriter.Write(_particleEffect);
        binaryWriter.Write(_timelineEffect);
        binaryWriter.Write(_effectPreset);
        binaryWriter.Write(_audioID);
        BinaryHelper.writeArray(ref binaryWriter, _audioParameter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _buffName= binaryReader.ReadString();
        _buffKey = binaryReader.ReadInt32();
        _targetStatusName = binaryReader.ReadString();
        _buffType = BinaryHelper.readEnum<BuffType>(ref binaryReader);
        _buffUpdateType = BinaryHelper.readEnum<BuffUpdateType>(ref binaryReader);
        _buffApplyType = BinaryHelper.readEnum<BuffApplyType>(ref binaryReader);
        _defenceType = BinaryHelper.readEnum<DefenceType>(ref binaryReader);
        _allowOverlap = binaryReader.ReadBoolean();
        _buffVaryStatFactor = binaryReader.ReadSingle();
        _buffCustomValue0 = binaryReader.ReadSingle();
        _buffCustomValue1 = binaryReader.ReadSingle();
        _buffCustomValue2 = BinaryHelper.readArrayString(ref binaryReader);
        _buffCustomStatusName = binaryReader.ReadString();
        _buffStartEffectPreset = binaryReader.ReadString();
        _buffEndEffectPreset = binaryReader.ReadString();
        _particleEffect = binaryReader.ReadString();
        _timelineEffect = binaryReader.ReadString();
        _effectPreset = binaryReader.ReadString();
        _audioID = binaryReader.ReadInt32();
        _audioParameter = BinaryHelper.readArrayInt(ref binaryReader);
    }
}

public enum BuffApplyType
{
    Direct = 0,
    DirectDelta,
    Additional,
    DirectSet,
    Empty,
    Count
}

public enum BuffUpdateType
{
    OneShot = 0,
    Time,
    StatSection,
    Continuous,
    DelayedContinuous,
    ButtonHit,
    GreaterThenSet,
    Count,
};

public enum BuffType
{
    Status,
    Defence,
    Count,
}