using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterManager : MessageHub<ManagerBase>
{
    public static MasterManager     instance;

    public List<ManagerBase>        managers;

    private bool                    _update = true;
    private bool                    _prevUpdate = true;

    private bool                    _frameUpdate = false;

    private float                   _updateStopTimer = 0f;

    private float                   _timeScaleTimer = 0f;
    private float                   _targetTimeScale = 0f;

    private float                   _timeScaleBlendTimer = 0f;
    private float                   _timeScaleBlendFactor = 0f;

    public StageProcessor           _stageProcessor = null;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        _unknownMessageProcess = (msg)=>
        {
            MessagePool.ReturnMessage(msg);
        };
        AddAction(MessageTitles.system_registerRequest,(msg)=>
        {
            RegisterReceiver(msg);
        });
        AddAction(MessageTitles.system_deregisterRequest,(msg)=>
        {
            DeregisteReceiver(msg);
        });

        AddAction(MessageTitles.system_pauseUpdate,(msg)=>
        {
            _update = false;
        });
        AddAction(MessageTitles.system_playUpdate,(msg)=>
        {
            _update = true;
        });
        AddAction(MessageTitles.system_updateFrame,(msg)=>
        {
            _frameUpdate = true;
        });
        AddAction(MessageTitles.entity_stopUpdate, (msg)=>{
            StopUpdateSecond(MessageDataPooling.CastData<FloatData>(msg.data).value);
        });
        AddAction(MessageTitles.entity_setTimeScale, (msg)=>{
            Vector3 timeScaleData = MessageDataPooling.CastData<Vector3Data>(msg.data).value;
            setTimeScaleSecond(timeScaleData.x,timeScaleData.y,timeScaleData.z);
        });

        StaticDataLoader.loadStaticData();

        foreach(var m in managers)
        {
            if(m == null)
                continue;

            m.assign();
            RegisterReceiver(m);
        }
        foreach(var m in managers)
        {
            if(m == null)
                continue;
                
            m.initialize();
        }

        FMODAudioManager.Instance().initialize();
        CollisionManager.Instance().initialize();
        CameraControlEx.Instance().initialize();
        DanmakuManager.Instance().initialize();
        UIRepeater.Instance().clear();

        _stageProcessor = new StageProcessor();
        _stageProcessor.setTargetCameraControl(CameraControlEx.Instance());
    }
    public void Start()
    {
        ReceiveMessageProcessing();

#if UNITY_EDITOR
#else
        ActiveTitleMenu();
#endif
    }
    public void Update()
    {
        if(Input.GetKey(KeyCode.F1))
        {
            Screen.SetResolution(800,600,FullScreenMode.Windowed);
        }
        else if(Input.GetKey(KeyCode.F2))
        {
            Screen.SetResolution(1600,1200,FullScreenMode.Windowed);
        }
        else if(Input.GetKey(KeyCode.F3))
        {
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height,FullScreenMode.FullScreenWindow);
        }
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            if(Input.GetKey(KeyCode.Return))
            {
                activePauseUI();
            }
            else
            {
                _stageProcessor.stopStage(true);
                HPSphereUIManager.Instance().setActive(true);
                GameUI.Instance.SetActiveCrossHair(true);
                GameUI.Instance.SetEntity(null);
                GameUI.Instance.activeOptionUI(false);
                GameUI.Instance.ActivePauseUI(false);
                ScreenDirector._instance.setActiveMainHud(true);
                ScreenDirector._instance._screenFader.clear();
                LetterBox._instance.clear();
                FMODAudioManager.Instance().clearAll();
                EffectManager._instance.killSwitchAll();
            }
#else
            activePauseUI();
#endif
        }

        float deltaTimeMultiflier = 1f;

#if UNITY_EDITOR
        if(Input.GetKey(KeyCode.LeftBracket))
            deltaTimeMultiflier = 10f;

        GizmoHelper.instance.updateGizmoVisibleState();
#endif

        float deltaTime = Time.deltaTime * deltaTimeMultiflier;
        GlobalTimer.Instance().setUpdateProcessing(true);
        ActionKeyInputManager.Instance().progress(deltaTime);

        CameraControlEx.Instance().updateDebugMode();

        if(_prevUpdate != _update)
            ActionKeyInputManager.Instance().clearInputData();
        
        _prevUpdate = _update;

        if(_update == false && _frameUpdate == false)
        {
            GlobalTimer.Instance().setUpdateProcessing(false);
            ReceiveMessageProcessing();

            return;
        }
        
        if(_frameUpdate)
        {
            deltaTime = 0.01666f * 2f;
            _frameUpdate = false;
        }

        if(_updateStopTimer > 0f)
        {
            _updateStopTimer -= deltaTime;
            return;
        }

        if(_timeScaleTimer > 0f)
        {
            _timeScaleTimer -= deltaTime;
            deltaTime *= _targetTimeScale;
        }
        else if(_timeScaleBlendTimer > 0f)
        {
            _timeScaleBlendTimer -= deltaTime;
            deltaTime *= Mathf.Lerp(1f, _targetTimeScale, (_timeScaleBlendTimer / _timeScaleBlendFactor));
        }

        GlobalTimer.Instance().setScaledDeltaTime(deltaTime);
        GlobalTimer.Instance().updateGlobalTime(deltaTime);

        deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();
        
        _stageProcessor.processStage(deltaTime);

        DanmakuManager.Instance().process(deltaTime);

        ManagersUpdate(deltaTime);
        ManagersAfterUpdate(deltaTime);
        ManagersSendMessageProcessing();
        SendMessageProcessing();
        CallReceiveMessageProcessing();
        ManagersReceiveMessageProcessing();
        ReceiveMessageProcessing();

        UIRepeater.Instance().updateUIRepeater();
        HPSphereUIManager.Instance().progress(deltaTime);
        TalkBalloonManager.Instance().updateTalkBalloonManager(deltaTime);

        CameraControlEx.Instance().progress(deltaTime);

        FMODAudioManager.Instance().updateAudio();
    }

    public void LateUpdate()
    {
        if(_update == false)
            return;
            
        CollisionManager.Instance().collisionUpdate();
        WeightRandomManager.Instance().updateRandom();

        CameraControlEx.Instance().SyncPosition();

        _stageProcessor.cameraProcess(GlobalTimer.Instance().getSclaedDeltaTime());
        _stageProcessor.updateBackgroundLayer();

    }

    public void FixedUpdate()
    {
        ManagersFixedUpdate(Time.fixedDeltaTime);
    }

    public void resetAll()
    {
        SceneManager.LoadScene(0);
    }

    public void activePauseUI()
    {
        if(_stageProcessor.isValid() && _stageProcessor._stageData._isMenuStage)
            return;

        if(_update)
        {
            ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject("PostProcessProfile/CRTImpact");
            CameraControlEx.Instance().getPostProcessProfileControl().setOneShotEffectProfile(profile as PostProcessProfile,MathEx.EaseType.EaseInCubic,999,0.7f);
        }
        else
        {
            ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject("PostProcessProfile/TitleStart");
            CameraControlEx.Instance().getPostProcessProfileControl().setOneShotEffectProfile(profile as PostProcessProfile,MathEx.EaseType.EaseInCubic,999,0.5f);
        }
        GameUI.Instance.ActivePauseUI(_update);

        _update = !_update;
    }

    public void ActiveTitleMenu()
    {
        LetterBox._instance.clear();
        ScreenDirector._instance._screenFader.clear();
        ActionKeyInputManager.setCursorVisible(true);

        var logoStageData = ResourceContainerEx.Instance().GetStageData("StageData/0_LogoStage");
        if (logoStageData == null)
            return;
        
        MasterManager.instance._stageProcessor.stopStage(true);
        MasterManager.instance._stageProcessor.startStage(logoStageData,Vector3.zero,Vector3.zero);
    }

    public void StopUpdateSecond(float time)
    {
        if(_updateStopTimer < time)
            _updateStopTimer = time;
    }

    public bool isGameUpdate() {return _update;}

    public void setTimeScaleSecond(float timeScale, float time, float blendTime)
    {
        _targetTimeScale = timeScale;
        _timeScaleTimer = time;
        _timeScaleBlendFactor = blendTime;
        _timeScaleBlendTimer = blendTime;
    }

    public void ManagersUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.progress(deltaTime);
        }
    }
    public void ManagersAfterUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.afterProgress(deltaTime);
        }
    }
    public void ManagersFixedUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.fixedProgress(deltaTime);
        }
    }
    public void ManagersSendMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.SendMessageProcessing();
        }
    }
    public void ManagersReceiveMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.CallReceiveMessageProcessing();
        }
    }
    public void SendMessageDirectInMaster(ushort title, int target, System.Object data)
    {
        var msg = MessagePack(title,target,data);
        SendMessageDirectInMaster(msg);
    }
    public void SendMessageDirectInMaster(Message msg)
    {
        HandleMessage(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void SendMessageDirectInMasterQuick(ushort title, int target, System.Object data)
    {
        var msg = MessagePack(title,target,data);
        SendMessageDirectInMasterQuick(msg);
    }
    public void SendMessageDirectInMasterQuick(Message msg)
    {
        HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void HandleMessageQuick(Message msg)
    {
        if(IsInReceivers(msg.target) && _receivers[msg.target].CanHandleMessage(msg))
        {
#if UNITY_EDITOR
            _receivers[msg.target].Debug_AddReceivedQueue(msg);
#endif
            _receivers[msg.target].MessageProcessing(msg);
            MessagePool.ReturnMessage(msg);
        }
        else if((msg.target == 0 || GetUniqueID() == msg.target) && CanHandleMessage(msg))
        {
#if UNITY_EDITOR
            Debug_AddReceivedQueue(msg);
#endif
            MessageProcessing(msg);
            MessagePool.ReturnMessage(msg);
        }
        else
        {
            bool find = false;
            foreach(var other in _receivers.Values)
            {            
                if(other.IsInReceivers(msg.target) && other.GetReciever(msg.target).CanHandleMessage(msg))
                {
#if UNITY_EDITOR
                    other.GetReciever(msg.target).Debug_AddReceivedQueue(msg);
#endif
                    Debug.Log("End");
                    other.GetReciever(msg.target).MessageProcessing(msg);
                    MessagePool.ReturnMessage(msg);
                    find = true;
                    break;
                }
            }
            if(!find)
                _unknownMessageProcess(msg);
        }
    }
    public override void HandleMessage(Message msg)
    {
        if(IsInReceivers(msg.target))
        {
            _receivers[msg.target].ReceiveMessage(msg);
        }
        else if(msg.target == 0 || GetUniqueID() == msg.target)
        {
            ReceiveMessage(msg);
        }
        else
        {
            bool find = false;
            foreach(var other in _receivers.Values)
            {
                if(other.IsInReceivers(msg.target))
                {
                    other.GetReciever(msg.target).ReceiveMessage(msg);
                    find = true;
                    break;
                }
            }
            if(!find)
                _unknownMessageProcess(msg);
        }
    }
    private void RegisterReceiver(Message msg)
    {
        if(msg.target == GetUniqueID() || msg.target == 0)
        {
            var manager = (ManagerBase)msg.sender;
            RegisterReceiver(manager);
        }
        else if(IsInReceivers(msg.target))
        {
            _receivers[msg.target].RegisterReceiver((ObjectBase)msg.sender);
        }
    }
    private void DeregisteReceiver(Message msg)
    {
        if(msg.target == GetUniqueID() || msg.target == 0)
        {
            var manager = (int)msg.data;
            DeregisteReceiver(manager);
        }
        else if(IsInReceivers(msg.target))
        {
            var target = (int)msg.data;
            _receivers[msg.target].DeregisteReceiver(target);
        }
    }
    protected virtual void OnDestroy()
    {
        dispose(true);
        ClearUniqueIDCache();
    }
}
