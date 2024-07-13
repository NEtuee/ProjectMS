using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharacterInfoData : SerializableDataType
{
    public string       _displayName;
    public string       _actionGraphPath;
    public string       _aiGraphPath;
    public string       _statusName;

    public float        _characterRadius;
    public float        _headUpOffset;

    public bool         _indicatorVisible = true;
    public bool         _useCameraBoundLock = true;
    public bool         _useHpInterface = false;
    public bool         _selfCollision = true;
    public bool         _immortalCharacter = false;

    public string       _allyInfoKey = "";
    
    public CommonMaterial   _defaultMaterial = CommonMaterial.Skin;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_displayName);
        binaryWriter.Write(_actionGraphPath);
        binaryWriter.Write(_aiGraphPath);
        binaryWriter.Write(_statusName);
        binaryWriter.Write(_characterRadius);
        binaryWriter.Write(_headUpOffset);
        binaryWriter.Write(_indicatorVisible);
        binaryWriter.Write(_useCameraBoundLock);
        binaryWriter.Write(_useHpInterface);
        binaryWriter.Write(_selfCollision);
        binaryWriter.Write(_immortalCharacter);
        binaryWriter.Write(_allyInfoKey);
        binaryWriter.Write((int)_defaultMaterial);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _displayName = binaryReader.ReadString();
        _actionGraphPath = binaryReader.ReadString();
        _aiGraphPath = binaryReader.ReadString();
        _statusName = binaryReader.ReadString();
        _characterRadius = binaryReader.ReadSingle();
        _headUpOffset = binaryReader.ReadSingle();
        _indicatorVisible = binaryReader.ReadBoolean();
        _useCameraBoundLock = binaryReader.ReadBoolean();
        _useHpInterface = binaryReader.ReadBoolean();
        _selfCollision = binaryReader.ReadBoolean();
        _immortalCharacter = binaryReader.ReadBoolean();
        _allyInfoKey = binaryReader.ReadString();
        _defaultMaterial = (CommonMaterial)binaryReader.ReadInt32();
    }
}

public class CharacterInfoDataList : SerializableDataType
{
    public Dictionary<string, CharacterInfoData> _characterInfoDataDic = new Dictionary<string, CharacterInfoData>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_characterInfoDataDic.Count);
        foreach(var item in _characterInfoDataDic)
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
            CharacterInfoData characterInfo = new CharacterInfoData();
            characterInfo.deserialize(ref binaryReader);

            _characterInfoDataDic.Add(key, characterInfo);
        }
    }
}