using System.Collections.Generic;

public class StatusInfo
{
    public class Status
    {
        public int _statusIndex = -1;
        public float _value = 0f;
        public float _additionalValue = 0f;
        public float _regenFactor = 0f;
    }

    public struct BuffItem
    {
        public BuffData _buffData;
        public BuffUpdateState _updateState;
    }

    public enum BuffUpdateState
    {
        Compelete,
        Ing,
        Delayed,
    };

    private StatusInfoData _statusInfoData;

    private Dictionary<StatusType, Status> _typeStatusValues = new Dictionary<StatusType, Status>();
    private Dictionary<string, Status> _customStatusValues = new Dictionary<string, Status>();
    //private float[] _autoRegenTimer;

    public StatusInfo(StatusInfoData data)
    {
        _statusInfoData = data;
        createStatusValueDictionary(_statusInfoData);
    }

    private void createStatusValueDictionary(StatusInfoData data)
    {
        _typeStatusValues.Clear();
        _customStatusValues.Clear();

        for(int i = 0; i < data._statusData.Length; ++i)
        {
            StatusDataFloat statusDataFloat = data._statusData[i];
            Status stat = new Status();
            stat._statusIndex = i;
            statusDataFloat.initStat(ref stat._value);

            if(statusDataFloat.getStatusType() == StatusType.Custom)
                _customStatusValues.Add(statusDataFloat.getName(), stat);
            else
                _typeStatusValues.Add(statusDataFloat.getStatusType(),stat);
        }
    }

    public bool isValid()
    {
        return _statusInfoData != null;
    }

    public bool applyBuff(BuffData buff)
    {
        if(buff.isBuffValid() == false)
            return false;

        switch(buff._buffApplyType)
        {
            case BuffApplyType.Direct:
                variStat(buff._targetStatusType, buff._targetStatusName, buff._buffVaryStatFactor);
                return true;
            case BuffApplyType.Additional:
                variAddtionalStat(buff._targetStatusType, buff._targetStatusName, buff._buffVaryStatFactor);
                return true;
            case BuffApplyType.Regen:
                variRegenStat(buff._targetStatusType, buff._targetStatusName, buff._buffVaryStatFactor);
                return true;
        }

        return false;
    }


    public void variRegenStat(StatusType type, string name, float value)
    {
        if(type == StatusType.Count)
        {
            DebugUtil.assert(false, "invalid status type: {0}", type);
            return;
        }

        int index = (int)type;
        Status status = getStatus(type,name);
        status._regenFactor += value;
    }

    public void variAddtionalStat(StatusType type, string name, float value)
    {
        if(type == StatusType.Count)
        {
            DebugUtil.assert(false, "invalid status type: {0}", type);
            return;
        }

        int index = (int)type;
        Status status = getStatus(type,name);
        status._additionalValue += value;
    }

    public void variStat(StatusType type, string name, float value)
    {
        if(type == StatusType.Count)
        {
            DebugUtil.assert(false, "invalid status type: {0}", type);
            return;
        }

        int index = (int)type;
        Status status = getStatus(type,name);

        _statusInfoData._statusData[status._statusIndex].variStat(ref status._value, value);

    }

    private Status getStatus(StatusType type, string name = null)
    {
        if(type == StatusType.Custom)
            return _customStatusValues[name];
        else
            return _typeStatusValues[type];
    }

    public bool isDead()
    {
        return getStatus(StatusType.HP)._value <= 0;
    }
}