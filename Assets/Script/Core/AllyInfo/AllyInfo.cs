using System.Collections.Generic;
using System.IO;

public class AllyInfoDataList : SerializableDataType
{
    public Dictionary<string, AllyInfoData> _allyInfoDataDic = new Dictionary<string, AllyInfoData>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_allyInfoDataDic.Count);
        foreach(var item in _allyInfoDataDic)
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
            AllyInfoData allyInfoData = new AllyInfoData();
            allyInfoData.deserialize(ref binaryReader);

            _allyInfoDataDic.Add(key, allyInfoData);
        }
    }
}

[System.Serializable]
public class AllyInfoData : SerializableDataType
{
    public string _key = "";
    public int _index;
    public int[] _allyGroup = null;
    public int[] _enemyGroup = null;
    public int[] _neutralGroup = null;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_key);
        binaryWriter.Write(_index);
        BinaryHelper.writeArray(ref binaryWriter, _allyGroup);
        BinaryHelper.writeArray(ref binaryWriter, _enemyGroup);
        BinaryHelper.writeArray(ref binaryWriter, _neutralGroup);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _key = binaryReader.ReadString();
        _index = binaryReader.ReadInt32();
        _allyGroup = BinaryHelper.readArrayInt(ref binaryReader);
        _enemyGroup = BinaryHelper.readArrayInt(ref binaryReader);
        _neutralGroup = BinaryHelper.readArrayInt(ref binaryReader);
    }
}