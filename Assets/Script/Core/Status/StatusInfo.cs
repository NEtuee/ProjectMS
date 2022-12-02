using System.Collections.Generic;

public class StatusInfo
{
    public class Status
    {
        public string _statusName = "";
        public int _statusIndex = -1;
        public float _value = 0f;
        public float _realValue = 0f;
        public float _additionalValue = 0f;
        public float _regenFactor = 0f;

        public HashSet<int> _updateListWhenValueChange = new HashSet<int>();

        public bool _buffListUpdated = false;

        public void updateBuffList(List<BuffItem> buffItems)
        {
            for(int i = 0; i < buffItems.Count; ++i)
            {
                if(_updateListWhenValueChange.Contains(buffItems[i]._buffData._buffKey))
                {
                    BuffItem itemCopy = buffItems[i];
                    itemCopy._startedTime = GlobalTimer.Instance().getScaledGlobalTime();

                    buffItems[i] = itemCopy;
                }
            }
        }

        public void addToUpdateList(int buffKey)
        {
            _updateListWhenValueChange.Add(buffKey);
        }

        public void deleteToUpdateList(int buffKey)
        {
            if(_updateListWhenValueChange.Contains(buffKey) == false)
            {
                DebugUtil.assert(false,"target buff key is not exists, must fix: {0}",buffKey);
                return;
            }

            _updateListWhenValueChange.Remove(buffKey);
        }
    }

    public struct BuffItem
    {
        public BuffData _buffData;
        //public BuffUpdateState _updateState;

        public float _startedTime;

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
    private static Dictionary<int, BuffData> _buffDataDictionary;

    private StatusInfoData _statusInfoData;

    private Dictionary<string, Status> _statusValues = new Dictionary<string, Status>();
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

    public void initialize()
    {
        clearBuff();
        foreach(Status item in _statusValues.Values)
        {
            _statusInfoData._statusData[item._statusIndex].initStat(ref item._value);
            item._value = item._realValue;
            item._additionalValue = 0f;
            item._regenFactor = 0f;
        }
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

    private BuffData getBuffData(int buffKey)
    {
        if(_buffDataDictionary.ContainsKey(buffKey) == false)
        {
            DebugUtil.assert(false, "target buff data not exists: {0}",buffKey);
            return null;
        }

        return _buffDataDictionary[buffKey];
    }

    public static void setBuffDataDictionary(Dictionary<int, BuffData> data)
    {
        _buffDataDictionary = data;
    }

    public static void setStatusInfoDataDictionary(Dictionary<string, StatusInfoData> data)
    {
        _statusInfoDataDictionary = data;
    }

    private void createStatusValueDictionary(StatusInfoData data)
    {
        _statusValues.Clear();

        for(int i = 0; i < data._statusData.Length; ++i)
        {
            StatusDataFloat statusDataFloat = data._statusData[i];
            Status stat = new Status();
            stat._statusIndex = i;
            statusDataFloat.initStat(ref stat._realValue);

            stat._statusName = statusDataFloat.getName();
            _statusValues.Add(statusDataFloat.getName(), stat);
        }
    }

    public bool isValid()
    {
        return _statusInfoData != null;
    }

    public void updateActionConditionData(GameEntityBase target)
    {
        foreach(Status item in _statusValues.Values)
        {
            target.updateStatusConditionData(item._statusName,item._value);
        }

    }

    public void applyBuff(int buffKey)
    {
        if(_buffDataDictionary.ContainsKey(buffKey) == false)
        {
            DebugUtil.assert(false, "invalid buff key: {0}", buffKey);
            return;
        }

        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            if(_currentlyAppliedBuffList[i]._buffData._buffKey == buffKey)
                return;
        }

        applyBuff(_buffDataDictionary[buffKey], GlobalTimer.Instance().getScaledGlobalTime());
    }

    private void applyBuff(BuffData buff, float startedTime)
    {
        BuffItem buffItem = new BuffItem();
        buffItem._buffData = buff;
        buffItem._startedTime = startedTime;
        
        _currentlyAppliedBuffList.Add(buffItem);

        if(buff._buffUpdateType == BuffUpdateType.DelayedContinuous)
        {
            getStatus(buff._targetStatusName).addToUpdateList(buff._buffKey);
        }
    }

    // public void deleteBuff(int buffKey)
    // {
    //     if(_currentlyAppliedBuffList.ContainsKey(buffKey) == true)
    //         _currentlyAppliedBuffList.Remove(buffKey);
    // }

    public void clearBuff()
    {
        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            if(_currentlyAppliedBuffList[i]._buffData._buffUpdateType == BuffUpdateType.DelayedContinuous)
                getStatus((_currentlyAppliedBuffList[i]._buffData._targetStatusName)).deleteToUpdateList(_currentlyAppliedBuffList[i]._buffData._buffKey);        
        }

        _currentlyAppliedBuffList.Clear();
    }

    public void deleteBuff(int buffKey)
    {
        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            if(_currentlyAppliedBuffList[i]._buffData._buffKey == buffKey)
            {
                if(_currentlyAppliedBuffList[i]._buffData._buffUpdateType == BuffUpdateType.DelayedContinuous)
                    getStatus((_currentlyAppliedBuffList[i]._buffData._targetStatusName)).deleteToUpdateList(buffKey);

                _currentlyAppliedBuffList.RemoveAt(i);
                return;
            }
        }
    }

    public void updateStatus(float deltaTime)
    {
        updateBuff();

        foreach(Status item in _statusValues.Values)
        {
            item._buffListUpdated = false;

            if(item._regenFactor == 0f && item._additionalValue == 0f)
            {
                item._value = item._realValue;
                continue;
            }

            _statusInfoData._statusData[item._statusIndex].variStat(ref item._realValue, item._additionalValue, item._regenFactor * deltaTime);
            item._value = item._realValue;

            item._additionalValue = 0f;
            item._regenFactor = 0f;
        }

        Status hpStatus = getStatus("HP");
        if(hpStatus != null && hpStatus._value <= 0f)
            _isDead = true;
    }

    private void updateBuff()
    {
        float globalTime = GlobalTimer.Instance().getScaledGlobalTime();

        for(int i = 0; i < _currentlyAppliedBuffList.Count;)
        {
            bool deleteBuff = false;
            bool canApply = true;

            BuffItem buffItem = _currentlyAppliedBuffList[i];
            BuffData buffData = buffItem._buffData;

            switch(buffData._buffUpdateType)
            {
                case BuffUpdateType.OneShot:
                    deleteBuff = true;
                    break;
                case BuffUpdateType.Time:
                    deleteBuff = (globalTime - buffItem._startedTime) > buffData._buffCustomValue0;
                    break;
                case BuffUpdateType.StatSection:
                    Status stat = getStatus(buffData._buffCustomStatusName);
                    if(stat == null)
                    {
                        DebugUtil.assert(false,"statSection target status not found: {0}",buffData._buffCustomStatusName);
                        break;
                    }

                    canApply = (stat._value >= buffData._buffCustomValue0) && (stat._value <= buffData._buffCustomValue1);

                    break;
                case BuffUpdateType.Continuous:
                    break;
                case BuffUpdateType.DelayedContinuous:
                    Status targetStatus = getStatus(buffData._targetStatusName);
                    canApply = globalTime - buffItem._startedTime > buffData._buffCustomValue0;
                    break;
            }

            if(canApply == true)
            {
                if(updateBuffXXX(buffData) == false)
                    DebugUtil.assert(false,"failed to update buff: {0}",buffData._buffName);
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
            {
                getStatus(buff._targetStatusName).updateBuffList(_currentlyAppliedBuffList);
                return variStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
            case BuffApplyType.Additional:
                return variAddtionalStat(buff._targetStatusName, buff._buffVaryStatFactor);
            case BuffApplyType.DirectDelta:
                return variRegenStat(buff._targetStatusName, buff._buffVaryStatFactor);
            case BuffApplyType.DirectSet:
            {
                getStatus(buff._targetStatusName).updateBuffList(_currentlyAppliedBuffList);
                return setStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
        }

        DebugUtil.assert(false, "invalid buff apply type: {0}",buff._buffApplyType);
        return false;
    }


    public bool variRegenStat(string name, float value)
    {
        Status status = getStatus(name);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: {0}", name);
            return false;
        }

        status._regenFactor += value;
        return true;
    }

    public bool variAddtionalStat(string name, float value)
    {
        Status status = getStatus(name);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: {0}", name);
            return false;
        }
            
        status._additionalValue += value;
        return true;
    }

    public bool variStat(string name, float value)
    {
        Status status = getStatus(name);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: {0}", name);
            return false;
        }
            
        _statusInfoData._statusData[status._statusIndex].variStat(ref status._realValue, 0f, value);
        return true;
    }

    public bool setStat(string name, float value)
    {
        Status status = getStatus(name);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: {0}", name);
            return false;
        }
            
        _statusInfoData._statusData[status._statusIndex].setStat(ref status._realValue, 0f, value);
        return true;
    }

    private Status getStatus(string name)
    {
        if(_statusValues.ContainsKey(name) == false)
            return null;

        return _statusValues[name];
        
    }

    public void updateDebugTextXXX(DebugTextManager debugTextManager)
    {
        for(int i = 0; i < _statusInfoData._statusData.Length; ++i)
        {
            StatusDataFloat statusDataFloat = _statusInfoData._statusData[i];
            string debugText = "[" + (statusDataFloat._statusType == StatusType.Custom ? "Custom/" : "") + statusDataFloat._statusName + "]";
            Status stat = getStatus(statusDataFloat._statusName);
            string statText = ": " + stat._value;

            debugTextManager.updateDebugText(debugText, debugText + statText);
        }

        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            BuffData data = _currentlyAppliedBuffList[i]._buffData;
            string debugText = data._buffName;

            debugTextManager.updateDebugText(debugText, "[Buff] " + debugText);
        }
    }

    public bool isDead()
    {
        return _isDead;
    }
}