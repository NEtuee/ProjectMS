

public class BuffData
{
    public string              _buffName;
    public int                 _buffKey;
    public string              _targetStatusName;

    public BuffType            _buffType;
    public BuffUpdateType      _buffUpdateType;
    public BuffApplyType       _buffApplyType;

    public DefenceType         _defenceType;

    public bool                _allowOverlap = false;

    public float               _buffVaryStatFactor;
    public float               _buffCustomValue0;
    public float               _buffCustomValue1;
    public string[]            _buffCustomValue2 = null;

    public string              _buffCustomStatusName;

    public string              _buffStartEffectPreset = "";
    public string              _buffEndEffectPreset = "";

    public string              _particleEffect = "";
    public string              _timelineEffect = "";
    public string              _effectPreset = "";

    public int                 _audioID = -1;
    public int[]               _audioParameter = null;

    public BuffData()
    {
        _buffName = null;
        _buffKey = -1;
        _targetStatusName = null;
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

public enum BuffType
{
    Status,
    Defence,
    Count,
}