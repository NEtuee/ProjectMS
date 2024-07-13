using System.Collections.Generic;
using System.IO;

public class WeightGroupDataList : SerializableDataType
{
    public Dictionary<string, WeightGroupData> _weightGroupDataList = new Dictionary<string, WeightGroupData>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_weightGroupDataList.Count);
        foreach(var item in _weightGroupDataList)
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
            WeightGroupData weightGroupData = new WeightGroupData();
            weightGroupData.deserialize(ref binaryReader);

            _weightGroupDataList.Add(key,weightGroupData);
        }
    }
}

public struct WeightGroupData : SerializableStructure
{
    public string _groupKey;
    public int _weightCount;
    public WeightData[] _weights;

    public bool isValid()
    {
        return _weights != null && _weightCount != 0 && _groupKey != "";
    }
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_groupKey);
        binaryWriter.Write(_weightCount);
        BinaryHelper.writeArrayStructure<WeightData>(ref binaryWriter, _weights);

    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _groupKey = binaryReader.ReadString();
        _weightCount = binaryReader.ReadInt32();
        _weights = BinaryHelper.readArrayStructure<WeightData>(ref binaryReader);
    }
}

public struct WeightData : SerializableStructure
{
    public string _key;
    public float _weight;
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_key);
        binaryWriter.Write(_weight);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _key = binaryReader.ReadString();
        _weight = binaryReader.ReadSingle();
    }
}