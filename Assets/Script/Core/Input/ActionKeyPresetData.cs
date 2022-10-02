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

[System.Serializable]
public class ActionKeyPresetData
{
    public string                      _actionKeyName = "";
    public ActionKeyMultiInputType     _multiInputType = ActionKeyMultiInputType.Single;
    public ActionKeyPressType          _pressType = ActionKeyPressType.KeyPressed;
    public string[][]                  _keys = new string[3][];
    public int[]                       _keyCount = new int[3];

    public float                       _multiInputThreshold = 0f;
}