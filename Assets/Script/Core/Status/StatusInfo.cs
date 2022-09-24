using System.Collections.Generic;

public class StatusInfo
{
    public class Status
    {
        public int _statusIndex = -1;
        public float _value = 0f;
        public float _realValue = 0f;
        public float _additionalValue = 0f;
        public float _regenFactor = 0f;
    }

    public struct BuffItem
    {
        public BuffData _buffData;
        //public BuffUpdateState _updateState;

        public float _startedTime;
        public float _time;

        public void updateStartTime(float startedTime)
        {
            _startedTime = startedTime;
        }
    }

    public enum BuffUpdateState
    {
        Compelete,
        Ing,
        Delayed,
    };

    private static Dictionary<string, StatusInfoData> _statusInfoDataDictionary;

    private StatusInfoData _statusInfoData;

    private Dictionary<StatusType, Status> _typeStatusValues = new Dictionary<StatusType, Status>();
    private Dictionary<string, Status> _customStatusValues = new Dictionary<string, Status>();
    private List<BuffItem> _currentlyAppliedBuffList = new List<BuffItem>();

    private bool _isDead = false;

    public StatusInfo(string dataName)
    {
        _statusInfoData = getStatusInfoData(dataName);
        createStatusValueDictionary(_statusInfoData);
    }

    public StatusInfo(StatusInfoData data)
    {
        _statusInfoData = data;
        createStatusValueDictionary(_statusInfoData);
    }

    private StatusInfoData getStatusInfoData(string targetName)
    {
        if(_statusInfoDataDictionary.ContainsKey(targetName) == false)
        {
            DebugUtil.assert(false, "target status info data not exists: {0}",targetName);
            return null;
        }

        return _statusInfoDataDictionary[targetName];
    }

    public static void setStatusInfoDataDictionary(Dictionary<string, StatusInfoData> data)
    {
        _statusInfoDataDictionary = data;
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
            statusDataFloat.initStat(ref stat._realValue);

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

    public void applyBuff(BuffData buff, float startedTime)
    {
        BuffItem buffItem = new BuffItem();
        buffItem._buffData = buff;
        buffItem._startedTime = startedTime;
        buffItem._time = buff._buffCustomValue0;

        _currentlyAppliedBuffList[buff._buffKey] = buffItem;
    }

    // public void deleteBuff(int buffKey)
    // {
    //     if(_currentlyAppliedBuffList.ContainsKey(buffKey) == true)
    //         _currentlyAppliedBuffList.Remove(buffKey);
    // }

    public void deleteBuff(int buffKey)
    {
        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            if(_currentlyAppliedBuffList[i]._buffData._buffKey == buffKey)
            {
                _currentlyAppliedBuffList.RemoveAt(i);
                return;
            }
        }
    }

    public void updateStatus(float deltaTime)
    {
        updateBuff();

        foreach(Status item in _typeStatusValues.Values)
        {
            _statusInfoData._statusData[item._statusIndex].variStat(ref item._realValue, item._regenFactor * deltaTime);
            item._value = item._realValue + item._additionalValue;

            item._additionalValue = 0f;
            item._regenFactor = 0f;
        }

        Status hpStatus = getStatus(StatusType.HP);
        if(hpStatus != null && hpStatus._value <= 0f)
            _isDead = true;
    }

    private void updateBuff()
    {
        float globalTime = GlobalTimer.Instance().getScaledGlobalTime();

        for(int i = 0; i < _currentlyAppliedBuffList.Count;)
        {
            bool deleteBuff = false;
            // bool canApply = true;
            switch(_currentlyAppliedBuffList[i]._buffData._buffUpdateType)
            {
                case BuffUpdateType.OneShot:
                    deleteBuff = true;
                    break;
                case BuffUpdateType.Section:
                    deleteBuff = globalTime - _currentlyAppliedBuffList[i]._startedTime > _currentlyAppliedBuffList[i]._time;
                    break;
                case BuffUpdateType.Continuous:
                    break;
                // case BuffUpdateType.DelayedContinuous:
                //     canApply = globalTime - _currentlyAppliedBuffList[i]._startedTime > _currentlyAppliedBuffList[i]._time;
                //     break;
            }

            if(updateBuffXXX(_currentlyAppliedBuffList[i]._buffData))
            {
                DebugUtil.assert(false,"failed to update buff");
            }

            if(deleteBuff == true)
                _currentlyAppliedBuffList.RemoveAt(i);
            else
                ++i;

        }
    }

    private bool updateBuffXXX(BuffData buff)
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

        DebugUtil.assert(false, "invalid buff apply type");
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
        if(status == null)
            return;

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
        if(status == null)
            return;
            
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
        if(status == null)
            return;
            
        _statusInfoData._statusData[status._statusIndex].variStat(ref status._realValue, value);

    }

    private Status getStatus(StatusType type, string name = null)
    {
        if(type == StatusType.Custom)
        {
            if(_customStatusValues.ContainsKey(name) == false)
                return null;

            return _customStatusValues[name];
        }
        else
        {
            if(_typeStatusValues.ContainsKey(type) == false)
                return null;

            return _typeStatusValues[type];
        }
    }

    public bool isDead()
    {
        return _isDead;
    }
}