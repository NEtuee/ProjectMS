using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public enum ActionKeyPressType
{
    KeyDown = 0,
    KeyPressed,
    KeyUp,
    Count
};

public enum ActionKeyMultiInputType
{
    Single = 0,
    SameTime,
    Count
}

public class ActionKeyPresetDataList : SerializableDataType
{
    public ActionKeyPresetData[] _actionKeyPresetList = null;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeArray(ref binaryWriter, _actionKeyPresetList);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _actionKeyPresetList = BinaryHelper.readArray<ActionKeyPresetData>(ref binaryReader);
    }
}

[System.Serializable]
public class ActionKeyPresetData : SerializableDataType
{
    const int kInputSystemCount = 3;

    public string                      _actionKeyName = "";
    public ActionKeyMultiInputType     _multiInputType = ActionKeyMultiInputType.Single;
    public ActionKeyPressType          _pressType = ActionKeyPressType.KeyPressed;
    public string[][]                  _keys = new string[kInputSystemCount][];
    public int[]                       _keyCount = new int[kInputSystemCount];

    public float                       _multiInputThreshold = 0f;

    public string getKey(int controller, int index)
    {
        if(_keys == null || _keys.Length <= controller || _keys[controller] == null || _keys[controller].Length <= index)
            return "";

        return _keys[controller][index];
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_actionKeyName);
        BinaryHelper.writeEnum<ActionKeyMultiInputType>(ref binaryWriter, _multiInputType);
        BinaryHelper.writeEnum<ActionKeyPressType>(ref binaryWriter, _pressType);
        for(int i = 0; i < kInputSystemCount; ++i)
        {
            BinaryHelper.writeArray(ref binaryWriter, _keys[i]);
        }
        binaryWriter.Write(_multiInputThreshold);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _actionKeyName = binaryReader.ReadString();
        _multiInputType = BinaryHelper.readEnum<ActionKeyMultiInputType>(ref binaryReader);
        _pressType = BinaryHelper.readEnum<ActionKeyPressType>(ref binaryReader);
        for(int i = 0; i < kInputSystemCount; ++i)
        {
            _keys[i] = BinaryHelper.readArrayString(ref binaryReader);
            _keyCount[i] = _keys[i] == null ? 0 : _keys[i].Length;
        }

        _multiInputThreshold = binaryReader.ReadSingle();
    }
}