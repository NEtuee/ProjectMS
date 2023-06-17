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
                    buffItems[i]._startedTime = GlobalTimer.Instance().getScaledGlobalTime();
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

    public class BuffItem
    {
        public BuffData _buffData;
        //public BuffUpdateState _updateState;

        public ParticleEffectItem _particleEffect;
        public TimelineEffectItem _timelineEffect;

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
    private static Dictionary<string, int> _buffKeyDictinary = new Dictionary<string, int>();

    private StatusInfoData _statusInfoData;
    private ObjectBase _ownerObject;

    private Dictionary<string, Status> _statusValues = new Dictionary<string, Status>();
    private List<BuffItem> _currentlyAppliedBuffList = new List<BuffItem>();

    private SimplePool<BuffItem> _buffItemPool = new SimplePool<BuffItem>();

    private bool _isDead = false;

    public StatusInfo(){}

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

    public void initialize(ObjectBase owner, string dataName)
    {
        clearBuff();

        _ownerObject = owner;
        
        _isDead = false;
        _statusInfoData = getStatusInfoData(dataName);
        createStatusValueDictionary(_statusInfoData);

        foreach(Status item in _statusValues.Values)
        {
            _statusInfoData._statusData[item._statusIndex].initStat(ref item._value);
            item._value = item._realValue;
            item._additionalValue = 0f;
            item._regenFactor = 0f;
        }
    }

    public void initialize()
    {
        clearBuff();

        _isDead = false;
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

    public static int getBuffKeyFromName(string buffName)
    {
        if(_buffKeyDictinary.ContainsKey(buffName) == false)
            return -1;
        
        return _buffKeyDictinary[buffName];
    }

    public static void setBuffDataDictionary(Dictionary<int, BuffData> data)
    {
        _buffDataDictionary = data;

        _buffKeyDictinary.Clear();

        foreach(var item in _buffDataDictionary)
        {
            _buffKeyDictinary.Add(item.Value._buffName, item.Value._buffKey);
        }
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
        BuffItem buffItem = _buffItemPool.dequeue();
        buffItem._buffData = buff;
        buffItem._startedTime = startedTime;
        buffItem._particleEffect = null;
        buffItem._timelineEffect = null;
        
        _currentlyAppliedBuffList.Add(buffItem);

        if(buff._buffUpdateType == BuffUpdateType.DelayedContinuous || buff._buffUpdateType == BuffUpdateType.GreaterThenSet)
        {
            buffItem._startedTime -= buff._buffCustomValue0;
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
            if(_currentlyAppliedBuffList[i]._buffData._buffUpdateType == BuffUpdateType.DelayedContinuous || _currentlyAppliedBuffList[i]._buffData._buffUpdateType == BuffUpdateType.GreaterThenSet )
                getStatus((_currentlyAppliedBuffList[i]._buffData._targetStatusName)).deleteToUpdateList(_currentlyAppliedBuffList[i]._buffData._buffKey);        
            
            if(_currentlyAppliedBuffList[i]._particleEffect != null)
                _currentlyAppliedBuffList[i]._particleEffect.disableEffect();
            if(_currentlyAppliedBuffList[i]._timelineEffect != null)
                _currentlyAppliedBuffList[i]._timelineEffect.disableEffect();

            _buffItemPool.enqueue(_currentlyAppliedBuffList[i]);
        }

        _currentlyAppliedBuffList.Clear();
    }

    public void deleteBuff(int buffKey)
    {
        for(int i = 0; i < _currentlyAppliedBuffList.Count; ++i)
        {
            if(_currentlyAppliedBuffList[i]._buffData._buffKey == buffKey)
            {
                deleteBuffIndex(i);
                return;
            }
        }
    }

    public void deleteBuffIndex(int index)
    {
        BuffItem buffItem = _currentlyAppliedBuffList[index];
        if(buffItem._buffData._buffUpdateType == BuffUpdateType.DelayedContinuous || buffItem._buffData._buffUpdateType == BuffUpdateType.GreaterThenSet)
            getStatus((buffItem._buffData._targetStatusName)).deleteToUpdateList(buffItem._buffData._buffKey);
        
        _buffItemPool.enqueue(buffItem);
        _currentlyAppliedBuffList.RemoveAt(index);
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
        if(_isDead == false && hpStatus != null && hpStatus._value <= 0f)
            setDead(true);
    }

    public void setDead(bool value)
    {
        _isDead = value;
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
                {
                    Status targetStatus = getStatus(buffData._targetStatusName);

                    canApply = globalTime - buffItem._startedTime >= buffData._buffCustomValue0;
                }
                break;
                case BuffUpdateType.ButtonHit:
                {
                    if(buffData._buffCustomValue2 != null)
                    {
                        for(int index = 0; index < buffData._buffCustomValue2.Length; ++index)
                        {
                            canApply = ActionKeyInputManager.Instance().keyCheck(buffData._buffCustomValue2[index]);
                            if(canApply)
                                break;
                        }
                    }
                }
                break;
                case BuffUpdateType.GreaterThenSet:
                {
                    Status customTargetStatus = getStatus(buffData._buffCustomStatusName);
                    Status targetStatus = getStatus(buffData._targetStatusName);

                    canApply = globalTime - buffItem._startedTime >= 1f && customTargetStatus._realValue > targetStatus._realValue;
                    if(canApply == false)
                        buffItem._startedTime = globalTime - 1f;
                }
                break;
            }

            if(canApply == true)
            {
                if(updateBuffXXX(buffData) == false)
                {
                    DebugUtil.assert(false,"failed to update buff: {0}",buffData._buffName);
                }
                else
                {
                    if(buffData._particleEffect != "" && buffItem._particleEffect == null && _ownerObject != null)
                    {
                        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                        requestData.clearRequestData();
                        requestData._parentTransform = _ownerObject.transform;
                        requestData._position = _ownerObject.transform.position;
                        requestData._effectPath = buffData._particleEffect;
                        requestData._effectType = EffectType.ParticleEffect;
                        buffItem._particleEffect = EffectManager._instance.createEffect(requestData) as ParticleEffectItem;
                        requestData.isUsing = true;
                    }
                    
                    if(buffData._timelineEffect != "" && buffItem._timelineEffect == null && _ownerObject != null)
                    {
                        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                        requestData.clearRequestData();
                        requestData._parentTransform = _ownerObject.transform;
                        requestData._position = _ownerObject.transform.position;
                        requestData._effectPath = buffData._timelineEffect;
                        requestData._effectType = EffectType.TimelineEffect;
                        buffItem._timelineEffect = EffectManager._instance.createEffect(requestData) as TimelineEffectItem;
                        requestData.isUsing = true;
                    }
                }

                
            }
            else
            {
                if(buffItem._particleEffect != null)
                {
                    buffItem._particleEffect.stopEffect();
                    buffItem._particleEffect = null;
                }
                if(buffItem._timelineEffect != null)
                {
                    buffItem._timelineEffect.stopEffect();
                    buffItem._timelineEffect = null;
                }
            }
                

            if(deleteBuff == true)
            {
                deleteBuffIndex(i);
            }
            else
            {
                ++i;
            }

        }
    }

    public float getCurrentStatusPercentage(string targetName)
    {
        Status status = getStatus(targetName);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", targetName,_statusInfoData._statusInfoName);
            return 0f;
        }
            
        return status._realValue / _statusInfoData._statusData[status._statusIndex].getMaxValue();
    }

    public float getCurrentStatus(string targetName)
    {
        Status status = getStatus(targetName);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", targetName,_statusInfoData._statusInfoName);
            return 0f;
        }
            
        return status._realValue;
    }

    private bool updateBuffXXX(BuffData buff)
    {
        if(buff.isBuffValid() == false)
            return false;

        if(buff._targetStatusName != null && getStatus(buff._targetStatusName) == null)
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", buff._targetStatusName,_statusInfoData._statusInfoName);

        if(buff._buffUpdateType == BuffUpdateType.GreaterThenSet)
            return setStat(buff._targetStatusName,getStatus(buff._buffCustomStatusName)._realValue);

        switch(buff._buffApplyType)
        {
            case BuffApplyType.Direct:
            {
                Status status = getStatus(buff._targetStatusName);
                variRegenStat(buff._targetStatusName, -status._regenFactor);

                status.updateBuffList(_currentlyAppliedBuffList);
                return variStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
            case BuffApplyType.Additional:
            {
                return variAddtionalStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
            case BuffApplyType.DirectDelta:
            {
                return variRegenStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
            case BuffApplyType.DirectSet:
            {
                getStatus(buff._targetStatusName).updateBuffList(_currentlyAppliedBuffList);
                return setStat(buff._targetStatusName, buff._buffVaryStatFactor);
            }
            case BuffApplyType.Empty:
            {
                return true;
            }
        }

        DebugUtil.assert(false, "invalid buff apply type: {0}",buff._buffApplyType);
        return false;
    }

    public bool checkBuffApplyStatus(int buffKey, string status)
    {
        if(_buffDataDictionary.ContainsKey(buffKey) == false)
        {
            DebugUtil.assert(false, "target buff is not exists: [targetKey: {0}]", buffKey);
            return false;
        }

        BuffData buff = _buffDataDictionary[buffKey];
        if(buff.isBuffValid() == false)
            return false;

        return buff._targetStatusName == status;
    }

    public float buffValuePrediction(float deltaTime, int buffKey)
    {
        if(_buffDataDictionary.ContainsKey(buffKey) == false)
        {
            DebugUtil.assert(false, "target buff is not exists: [targetKey: {0}]", buffKey);
            return 0f;
        }

        BuffData buff = _buffDataDictionary[buffKey];
        if(buff.isBuffValid() == false)
            return 0f;

        if(getStatus(buff._targetStatusName) == null)
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", buff._targetStatusName,_statusInfoData._statusInfoName);

        if(buff._buffUpdateType == BuffUpdateType.GreaterThenSet)
        {
            Status customTargetStatus = getStatus(buff._buffCustomStatusName);
            Status targetStatus = getStatus(buff._targetStatusName);

            return customTargetStatus._realValue > targetStatus._realValue ? customTargetStatus._realValue - targetStatus._realValue : 0f;
        }

        switch(buff._buffApplyType)
        {
            case BuffApplyType.Direct:
            {
                return buff._buffVaryStatFactor;
            }
            case BuffApplyType.Additional:
            {
                Status status = getStatus(buff._targetStatusName);
                return status._realValue + buff._buffVaryStatFactor;
            }
            case BuffApplyType.DirectDelta:
            {
                Status status = getStatus(buff._targetStatusName);
                return (status._regenFactor + buff._buffVaryStatFactor) * deltaTime;
            }
            case BuffApplyType.DirectSet:
            {
                return buff._buffVaryStatFactor;
            }
            case BuffApplyType.Empty:
            {
                return 0f;
            }
        }

        DebugUtil.assert(false, "invalid buff apply type: {0}",buff._buffApplyType);
        return 0f;
    }


    public bool variRegenStat(string name, float value)
    {
        Status status = getStatus(name);
        if(status == null)
        {
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", name, _statusInfoData._statusInfoName);
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
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", name, _statusInfoData._statusInfoName);
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
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", name, _statusInfoData._statusInfoName);
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
            DebugUtil.assert(false, "target status is not exists: [targetName: {0}] [currentStatusInfo: {1}]", name, _statusInfoData._statusInfoName);
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

    public StatusInfoData getStatusInfoData()
    {
        return _statusInfoData;
    }
}