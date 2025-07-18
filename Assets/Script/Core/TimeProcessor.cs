using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TimeProcessor
{
    public class TimeProcessItem
    {
        private float _time = 0f;
        private float _targetTime = 0f;
        private bool _ignoreDeltaTimeScale = false;

        public void update(float deltaTime) {_time += deltaTime;}
        public void initialize(bool clear = true) {_time = clear ? 0f : _targetTime;}
        public void set(float targetTime) {_time = 0f; _targetTime = targetTime;}
        public void setIgnoreDeltaTimeScale(bool ignore) {_ignoreDeltaTimeScale = ignore;}

        public bool isTriggered() {return _time >= _targetTime;}
        public bool isIgnoreDeltaTimeScale() {return _ignoreDeltaTimeScale;}

        public float getTime() {return _time;}
        public float getRate() {return _time * (1f / _targetTime);}
    }

    private Dictionary<string,TimeProcessItem> _timeProcessItemList = new Dictionary<string, TimeProcessItem>();

    public void updateProcessor(float deltaTime)
    {
        foreach(var item in _timeProcessItemList.Values)
        {
            if(item.isTriggered())
                continue;

            float updateDeltaTime = item.isIgnoreDeltaTimeScale() ? Time.deltaTime : deltaTime;
            item.update(updateDeltaTime);
        }
    }

    public TimeProcessItem addTimer(string key, float targetTime)
    {
        if(_timeProcessItemList.ContainsKey(key))
        {
            DebugUtil.assert(false, "이미 존재하는 Timer {0}", key);
            return null;
        }

        var item = new TimeProcessItem();
        item.set(targetTime);
        _timeProcessItemList.Add(key, item);

        return item;
    }

    public void initialize(string key, bool clear = true)
    {
        _timeProcessItemList[key].initialize(clear);
    }

    public TimeProcessItem getTimeProcessItem(string key)
    {
        return _timeProcessItemList[key];
    }

    public float getRate(string key) 
    {
        return _timeProcessItemList[key].getRate();
    }

    public float getTime(string key) 
    {
        return _timeProcessItemList[key].getTime();
    }

    public bool isTriggered(string key)
    {
        return _timeProcessItemList[key].isTriggered();
    }
}
