using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharacterInfoData : SerializableDataType
{
    public string       _displayName = "";
    public string       _actionGraphPath = "";
    public string       _aiGraphPath = "";
    public string       _statusName = "";

    public string       _portraitPath = "";

    public float        _characterRadius = 0f;
    public float        _headUpOffset = 0f;

    public int          _rankBadgeID = -1;

    public bool         _indicatorVisible = true;
    public bool         _useCameraBoundLock = true;
    public bool         _selfCollision = true;
    public bool         _immortalCharacter = false;

    public bool         _useInpuBuffer = false;

    public string       _allyInfoKey = "";
    
    public CommonMaterial   _defaultMaterial = CommonMaterial.Skin;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_displayName);
        binaryWriter.Write(_actionGraphPath);
        binaryWriter.Write(_aiGraphPath);
        binaryWriter.Write(_statusName);
        binaryWriter.Write(_portraitPath);
        binaryWriter.Write(_characterRadius);
        binaryWriter.Write(_headUpOffset);
        binaryWriter.Write(_indicatorVisible);
        binaryWriter.Write(_rankBadgeID);
        binaryWriter.Write(_useCameraBoundLock);
        binaryWriter.Write(_selfCollision);
        binaryWriter.Write(_immortalCharacter);
        binaryWriter.Write(_allyInfoKey);
        binaryWriter.Write((int)_defaultMaterial);
        binaryWriter.Write(_useInpuBuffer);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _displayName = binaryReader.ReadString();
        _actionGraphPath = binaryReader.ReadString();
        _aiGraphPath = binaryReader.ReadString();
        _statusName = binaryReader.ReadString();
        _portraitPath = binaryReader.ReadString();
        _characterRadius = binaryReader.ReadSingle();
        _headUpOffset = binaryReader.ReadSingle();
        _indicatorVisible = binaryReader.ReadBoolean();
        _rankBadgeID = binaryReader.ReadInt32();
        _useCameraBoundLock = binaryReader.ReadBoolean();
        _selfCollision = binaryReader.ReadBoolean();
        _immortalCharacter = binaryReader.ReadBoolean();
        _allyInfoKey = binaryReader.ReadString();
        _defaultMaterial = (CommonMaterial)binaryReader.ReadInt32();
        _useInpuBuffer = binaryReader.ReadBoolean();
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