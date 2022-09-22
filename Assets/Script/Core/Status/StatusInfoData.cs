
public class StatusInfoData
{
    public StatusDataFloat[] _statusData;

    public StatusInfoData(StatusDataFloat[] statusArray)
    {
        _statusData = statusArray;
    }

}

public class StatusDataFloat
{
    private string _statusName;
    private StatusType _statusType;

    private float _maxValue;
    private float _minValue;
    private float _initialValue;

    public StatusDataFloat(){}
    public StatusDataFloat(StatusType type, string name, float min, float max, float init)
    {
        _statusType = type;
        _statusName = name;

        _maxValue = max;
        _minValue = min;
        _initialValue = init;
    }

    public bool isStatusValid()
    {
        return _statusType != StatusType.Count && _maxValue >= _minValue;
    }

    public void initStat(ref float value)
    {
        value = _initialValue;
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

    public StatusType getStatusType() {return _statusType;}
    public string getName() {return _statusName;}

    public float getInitValue() {return _initialValue;}
    public float getMaxValue() {return _maxValue;}
    public float getMinValue() {return _minValue;}

}


public enum StatusType
{
    HP,
    Stamina,
    Blood,
    Custom,
    Count,
}