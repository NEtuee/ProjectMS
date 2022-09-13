
public class StatusInfoData
{
    private StatusDataFloat[] _statusData;

    public StatusInfoData(StatusDataFloat[] statusArray)
    {
        _statusData = statusArray;
    }

    public StatusDataFloat getStatusData(StatusType type)
    {
        if(type == StatusType.Count)
        {
            DebugUtil.assert(false, "invalid status type: {0}", type);
            return null;
        }

        return _statusData[(int)type];
    }
}

public class StatusDataFloat
{
    private StatusType _statusType;

    private float _maxValue;
    private float _minValue;

    private float _autoRegenTime;
    private float _autoRegenFactor;

    public StatusDataFloat(){}
    public StatusDataFloat(StatusType type, float min, float max, float regenTime, float regenFactor)
    {
        _statusType = type;

        _maxValue = max;
        _minValue = min;
        _autoRegenTime = regenTime;
        _autoRegenFactor = regenFactor;
    }

    public void variStat(ref float value, float factor )
    {
        value += factor;
        MathEx.clampf(value, _minValue, _maxValue);
    }

    public bool isMax(float value)
    {
        return value >= _maxValue;
    }

    public bool isMin(float value)
    {
        return value <= _minValue;
    }

    public StatusType getStatustype() {return _statusType;}

    public float getMaxValue() {return _maxValue;}
    public float getMinValue() {return _minValue;}

    public float getAutoRegenTime() {return _autoRegenTime;}
    public float getAutoRegenFactor() {return _autoRegenFactor;}


}


public enum StatusType
{
    HP,
    Stamina,
    Blood,
    Count,
}