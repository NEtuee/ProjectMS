using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MasterManager : MessageHub<ManagerBase>
{
    public static MasterManager     instance;

    public ObjectBase               player;

    public List<ManagerBase>        managers;

    private bool                    _update = true;

    private bool                    _frameUpdate = false;

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

        CameraControlEx.Instance().initialize();
        StageGraphManager.Instance().initialize();
    }
    public void Start()
    {
        ReceiveMessageProcessing();
    }
    public void Update()
    {
        float deltaTime = Time.deltaTime;
        ActionKeyInputManager.Instance().progress(deltaTime);

        if(_update == false && _frameUpdate == false)
        {
            ReceiveMessageProcessing();
            return;
        }
        
        if(_frameUpdate)
        {
            deltaTime = 0.01666f * 2f;
            _frameUpdate = false;
        }

        GlobalTimer.Instance().updateGlobalTime(deltaTime);
        StageGraphManager.Instance().progress(deltaTime);

        CameraControlEx.Instance().SyncPosition();

        ManagersUpdate(deltaTime);
        ManagersAfterUpdate(deltaTime);
        ManagersSendMessageProcessing();
        SendMessageProcessing();
        CallReceiveMessageProcessing();
        ManagersReceiveMessageProcessing();
        ReceiveMessageProcessing();

        CameraControlEx.Instance().progress(deltaTime);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            StageGraphManager.Instance().startStage("Assets\\Data\\StageGraph\\TestStage.xml");
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            StageGraphManager.Instance().stopStage();
        }
    }

    public void LateUpdate()
    {
        CollisionManager.Instance().collisionUpdate();
        WeightRandomManager.Instance().updateRandom();
    }

    public void FixedUpdate()
    {
        ManagersFixedUpdate(Time.fixedDeltaTime);
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
