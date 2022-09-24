

public class BuffData
{
    public string              _buffName;
    public int                 _buffKey;
    public string              _targetStatusName;
    public BuffUpdateType      _buffUpdateType;
    public BuffApplyType       _buffApplyType;

    public float               _buffVaryStatFactor;
    public float               _buffCustomValue0;
    public float               _buffCustomValue1;

    public string              _buffCustomStatusName;

    public BuffData()
    {
        _buffName = null;
        _buffKey = -1;
        _targetStatusName = null;
        _buffUpdateType = BuffUpdateType.Count;
        _buffApplyType = BuffApplyType.Count;

        _buffVaryStatFactor = 0f;
        _buffCustomValue0 = 0f;
        _buffCustomValue1 = 0f;

    }

    public bool isBuffValid()
    {
        return _buffKey >= 0 && _buffUpdateType != BuffUpdateType.Count && _buffApplyType != BuffApplyType.Count;
    }
}

public struct BuffSet
{
    public int[] _buffKeys;
}

public enum BuffApplyType
{
    Direct = 0,
    DirectDelta,
    Additional,
    Count
}

public enum BuffUpdateType
{
    OneShot = 0,
    Time,
    StatSection,
    Continuous,
    DelayedContinuous,
    Count,
};