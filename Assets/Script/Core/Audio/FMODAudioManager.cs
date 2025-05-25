using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FMODAudioManager : Singleton<FMODAudioManager>
{
    public struct AudioSwitchItem
    {
        public int _id;
        public FMODUnity.StudioEventEmitter _emitter;
    }
    
    private AudioInfoItem                                           _infoItem;

    private Dictionary<int, AudioInfoItem.AudioInfo>                _audioMap;
    private Dictionary<int, Queue<FMODUnity.StudioEventEmitter>>    _cacheMap;

    private Dictionary<int, List<FMODUnity.StudioEventEmitter>>     _activeMap;
    private Dictionary<int, FMOD.Studio.PARAMETER_DESCRIPTION>      _globalCache;

    private Dictionary<ObjectBase, List<AudioSwitchItem>>           _audioSwitchMap;

    private GameObject                                              _audioObject;
    private GameObject                                              _listener;

    public void initialize()
    {
        if(_audioObject == null)
        {
            _audioObject = new GameObject("Audio");
        }

        if(_listener == null)
        {
            _listener = new GameObject("Listener");
            _listener.AddComponent<FMODUnity.StudioListener>();
        }

        _infoItem = ResourceContainerEx.Instance().GetScriptableObject("Audio/AudioInfo/AudioInfo") as AudioInfoItem;

        CreateAudioMap();

        _cacheMap = new Dictionary<int, Queue<FMODUnity.StudioEventEmitter>>();
        _activeMap = new Dictionary<int, List<FMODUnity.StudioEventEmitter>>();
        _globalCache = new Dictionary<int, FMOD.Studio.PARAMETER_DESCRIPTION>();
        _audioSwitchMap = new Dictionary<ObjectBase, List<AudioSwitchItem>>();

        CreateCachedGlobalParams();
    }

    public void updateAudio()
    {
        if(_activeMap != null)
        {
            foreach(var pair in _activeMap)
            {
                var value = pair.Value;
                for(int i = 0; i < value.Count;)
                {
                    if(value[i]._manageSelf == false && !value[i].IsPlaying())
                    {
                        //_cacheMap[value[i].DataCode].Enqueue(value[i]);
                        ReturnCache(pair.Key,value[i]);
                        value.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
    }

    public void setListener(Transform target)
    {
        _listener.transform.SetParent(target);
        _listener.transform.localPosition = Vector3.zero;
    }

    public FMODUnity.StudioEventEmitter playSwitch(ObjectBase playObject, int id, Vector3 position, Transform parent)
    {
        if(_audioSwitchMap.ContainsKey(playObject) == false)
            _audioSwitchMap.Add(playObject, new List<AudioSwitchItem>());

        List<AudioSwitchItem> audioSwitchList = _audioSwitchMap[playObject];

        for(int index = 0; index < audioSwitchList.Count; ++index)
        {
            if(audioSwitchList[index]._id != id)
                continue;

            audioSwitchList[index]._emitter.Stop();
            ReturnCache(audioSwitchList[index]._id, audioSwitchList[index]._emitter);

            audioSwitchList.RemoveAt(index);
            break;
        }

        AudioSwitchItem item = new AudioSwitchItem();
        item._emitter = Play(id,position,parent);
        item._id = id;

        audioSwitchList.Add(item);
        return item._emitter;
    }

    public void stopSwitch(ObjectBase playObject, int id)
    {
        if(_audioSwitchMap.ContainsKey(playObject) == false)
            return;

        List<AudioSwitchItem> audioSwitchList = _audioSwitchMap[playObject];
        for(int index = 0; index < audioSwitchList.Count; ++index)
        {
            if(audioSwitchList[index]._id != id)
                continue;

            audioSwitchList[index]._emitter.Stop();
            ReturnCache(audioSwitchList[index]._id, audioSwitchList[index]._emitter);

            audioSwitchList.RemoveAt(index);
            break;
        }
    }

    public FMODUnity.StudioEventEmitter Play(int id, Vector3 localPosition,Transform parent)
    {
        var emitter = GetCache(id);

        emitter.transform.SetParent(parent);
        emitter.transform.localPosition = localPosition;
        emitter.gameObject.SetActive(true);

        emitter.Play();

        AddActiveMap(id,emitter);
        
        return emitter;
    }

    public void killSwitchAll(ObjectBase playObject)
    {
        if(_audioSwitchMap.ContainsKey(playObject) == false)
            return;
        
        List<AudioSwitchItem> audioSwitchList = _audioSwitchMap[playObject];
        for(int index = 0; index < audioSwitchList.Count; ++index)
        {
            audioSwitchList[index]._emitter.Stop();
        }

        audioSwitchList.Clear();
    }

    public void ReturnAllCache()
    {
        if(_activeMap != null)
        {
            foreach(var pair in _activeMap)
            {
                var value = pair.Value;
                for(int i = 0; i < value.Count; ++i)
                {
                    value[i].Stop();
                    value[i].transform.SetParent(_audioObject.transform);
                    ReturnCache(pair.Key,value[i]);

                }
                
                value.Clear();
            }
        }

        if(_audioSwitchMap != null)
        {
            foreach(var item in _audioSwitchMap)
            {
                if(item.Value != null)
                    item.Value.Clear();
            }
        }
    }

    public FMODUnity.StudioEventEmitter Play(int id, Vector3 position)
    {
        var emitter = GetCache(id);
        
        emitter.transform.SetParent(null);
        emitter.transform.SetPositionAndRotation(position,Quaternion.identity);
        emitter.gameObject.SetActive(true);

        emitter.Play();
        
        AddActiveMap(id,emitter);

        return emitter;
    }

    private void AddActiveMap(int id, FMODUnity.StudioEventEmitter emitter)
    {
        if(_activeMap.ContainsKey(id))
        {
            _activeMap[id].Add(emitter);
        }
        else
        {
            var list = new List<FMODUnity.StudioEventEmitter>
            {
                emitter
            };
            _activeMap.Add(id,list);
        }
    }

    public void setParam(ref FMODUnity.StudioEventEmitter eventEmitter, int audioID, int[] parameterID, float[] value)
    {
        AudioInfoItem.AudioInfo audioInfo = FindAudioInfo(audioID);
        for(int index = 0; index < parameterID.Length; ++index)
        {
            AudioInfoItem.AudioParameter parameter = audioInfo.FindParameter(parameterID[index]);
            if(parameter == null)
                return;

            float resultValue = Mathf.Clamp(value[index],parameter.min,parameter.max);
            eventEmitter.SetParameter(parameter.name, resultValue);
        }
    }

    public void setParam(ref FMODUnity.StudioEventEmitter eventEmitter, int audioID, int[] parameterID, float value)
    {
        AudioInfoItem.AudioInfo audioInfo = FindAudioInfo(audioID);
        for(int index = 0; index < parameterID.Length; ++index)
        {
            AudioInfoItem.AudioParameter parameter = audioInfo.FindParameter(parameterID[index]);
            if(parameter == null)
                return;

            float resultValue = Mathf.Clamp(value,parameter.min,parameter.max);
            eventEmitter.SetParameter(parameter.name, resultValue);
        }
    }

    public void SetParam(int audioID, int parameterID, float value)
    {
        var n = FindAudioInfo(audioID).FindParameter(parameterID);
        value = Mathf.Clamp(value,n.min,n.max);

        if(_activeMap != null && _activeMap.ContainsKey(audioID))
        {
            foreach(var list in _activeMap[audioID])
            {
                list.SetParameter(n.name,value);
            }
        }

        if(_cacheMap != null && _cacheMap.ContainsKey(audioID))
        {
            foreach(var list in _cacheMap[audioID])
            {
                list.SetParameter(n.name,value);
            }
        }
    }

    public void SetGlobalParam(int id, float value)
    {
        var desc = FindGlobalParamDesc(id);
        var result = FMODUnity.RuntimeManager.StudioSystem.setParameterByID(desc.id, value);
        if(result != FMOD.RESULT.OK)
            Debug.Log("global parameter not found : " + id + ", value:" + value);
        
    }
    
    public float GetGlobalParam(int id)
    {
        var desc = FindGlobalParamDesc(id);
        RESULT result = FMODUnity.RuntimeManager.StudioSystem.getParameterByID(desc.id, out var value);
        if(result != FMOD.RESULT.OK)
            Debug.Log("global parameter not found");
            
        return value;
    }

    private void ReturnCache(int id, FMODUnity.StudioEventEmitter emitter)
    {
        if(_cacheMap == null)
        {
            initialize();
        }

        emitter.gameObject.SetActive(false);
        emitter.transform.SetParent(_audioObject.transform);
        _cacheMap[id].Enqueue(emitter);
    }

    public void clearAll()
    {
        ReturnAllCache();

        foreach(var item in _infoItem.audioData)
        {
            bool isGlobal = item.id == 0;
            foreach(var param in item.parameters)
            {
                if(isGlobal)
                    continue;
                
                SetParam(item.id,param.id,param.min);
            }
        }
    }

    private FMODUnity.StudioEventEmitter GetCache(int id)
    {
        if (_cacheMap == null)
        {
            initialize();
        }
        
        if(!_cacheMap.ContainsKey(id) || _cacheMap[id].Count == 0)
        {
            CreateAudioCacheItem(id,1);
        }

        if(_cacheMap.ContainsKey(id) == false)
        {
            DebugUtil.assert(false,"존재하지 않는 Sound ID {0}",id);
            return null;
        }

        return _cacheMap[id].Dequeue();
    }

    private void CreateAudioCacheItem(int id,int count,bool active = false)
    {
        var audio = FindAudioInfo(id);
        if(audio == null)
            return;
        
        for(int i = 0; i < count; ++i)
        {
            var comp = new GameObject("Audio").AddComponent<FMODUnity.StudioEventEmitter>();
            
            comp.EventReference = audio.eventReference;
            comp.Preload = true;
            comp.gameObject.SetActive(active);
            comp.transform.SetParent(_audioObject.transform);
            comp._audioEventKey = id;

            if(_cacheMap.ContainsKey(id))
            {
                _cacheMap[id].Enqueue(comp);
            }
            else
            {
                var queue = new Queue<FMODUnity.StudioEventEmitter>();
                queue.Enqueue(comp);
                _cacheMap.Add(id,queue);
            }
            
        }
    }

    private FMOD.Studio.PARAMETER_DESCRIPTION FindGlobalParamDesc(int id)
    {
        if(_globalCache.ContainsKey(id))
        {
            return _globalCache[id];
        }
        else
        {
            Debug.Log("global parameter does not exists : " + id);
            return default(FMOD.Studio.PARAMETER_DESCRIPTION);
        }
    }

    private AudioInfoItem.AudioInfo FindAudioInfo(int id)
    {
        if(_audioMap.ContainsKey(id))
        {
            return _audioMap[id];
        }
        else
        {
            Debug.LogError("Audio id not found");
            return null;
        }
    }

    private void CreateCachedGlobalParams()
    {
        var global = _infoItem.FindAudio(0);
        
        foreach(var item in global.parameters)
        {
            var result = FMODUnity.RuntimeManager.StudioSystem.getParameterDescriptionByName(item.name, out var desc);

            if(result != FMOD.RESULT.OK)
            {
                Debug.Log("global Parameter does not exists : " + item.name);
                return;
            }

            _globalCache.Add(item.id,desc);
        }
    }

    private void CreateAudioMap()
    {
        if(_audioMap != null)
        {
            _audioMap.Clear();
        }

        _audioMap = new Dictionary<int, AudioInfoItem.AudioInfo>();

        var data = _infoItem.audioData;

        foreach(var d in data)
        {
            _audioMap.Add(d.id,d);
        }
    }
}