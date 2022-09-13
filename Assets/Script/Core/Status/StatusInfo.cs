

public class StatusInfo
{
    private StatusInfoData _statusInfoData;

    private float[] _statusValues;
    //private float[] _autoRegenTimer;

    public StatusInfo(StatusInfoData data)
    {
        _statusInfoData = data;
        _statusValues = new float[(int)StatusType.Count];
    }

    public bool isValid()
    {
        return _statusValues != null && _statusValues.Length < (int)StatusType.Count;
    }

    public void variStat(StatusType type, float value)
    {
        if(type == StatusType.Count)
        {
            DebugUtil.assert(false, "invalid status type: {0}", type);
            return;
        }

        int index = (int)type;
        StatusDataFloat data = _statusInfoData.getStatusData(type);

        data.variStat(ref _statusValues[index],value);
    }

    public bool isDead()
    {
        StatusDataFloat data = _statusInfoData.getStatusData(StatusType.HP);
        return data.isMin(_statusValues[(int)StatusType.HP]);
    }
}