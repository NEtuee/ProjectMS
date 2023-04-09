

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
    public string[]            _buffCustomValue2 = null;

    public string              _buffCustomStatusName;

    public string              _particleEffect = "";

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
        _buffCustomValue2 = null;

        _particleEffect = "";
    }

    public void copy(BuffData target)
    {
        _buffKey = target._buffKey;
        _targetStatusName = target._targetStatusName;
        _buffUpdateType = target._buffUpdateType;
        _buffApplyType = target._buffApplyType;
        _buffVaryStatFactor = target._buffVaryStatFactor;
        _buffCustomValue0 = target._buffCustomValue0;
        _buffCustomValue1 = target._buffCustomValue1;
        _buffCustomValue2 = target._buffCustomValue2;
        _particleEffect = target._particleEffect;
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