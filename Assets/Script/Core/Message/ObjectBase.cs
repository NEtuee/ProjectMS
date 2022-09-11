using UnityEngine;


public abstract class ObjectBase : MessageReceiver, IProgress
{
    public System.Action    whenDeactive = ()=>{};
    protected int           _currentManagerNumber = -1;
    protected override void Awake()
    {
        base.Awake();
        
        assign(); 
        //Initialize();
    }
    protected virtual void Start()
    {
        initialize();
    }
    public void SendMessageQuick(Message msg)
    {
        MasterManager.instance.HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void SendMessageQuick(ushort title, int target, System.Object data)
    {
        var msg = MessagePack(title,target,data);
        MasterManager.instance.HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void RegisterRequest(int managerNumber)
    {
        _currentManagerNumber = managerNumber;
        var msg = MessagePack(MessageTitles.system_registerRequest,_currentManagerNumber,null);
        MasterManager.instance.ReceiveMessage(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void DeregisterRequest()
    {
        _currentManagerNumber = -1;
        var msg = MessagePack(MessageTitles.system_deregisterRequest,_currentManagerNumber,GetUniqueID());
        MasterManager.instance.SendMessageDirectInMasterQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    //할당 등 생성 요소들
    public virtual void assign(){}
    //매니저 가입 요청, 값 초기화
    public virtual void initialize(){}
    //최초 업데이트
    public virtual void firstUpdate(){}
    //Update
    public virtual void progress(float deltaTime){}
    //LateUpdate
    public virtual void afterProgress(float deltaTime){}
    //FixedUpdate
    public virtual void fixedProgress(float deltaTime){}
    
    public virtual void release(bool disposeFromMaster)
    {
        if(disposeFromMaster == false)
            DeregisterRequest();
#if UNITY_EDITOR
        Debug_ClearQueue();
#endif
    }
    public override void dispose(bool disposeFromMaster)
    {
        base.dispose(disposeFromMaster);
        release(disposeFromMaster);
    }
    protected virtual void OnTriggerEnter2D(Collider2D coll)
    {
        int instanceID = coll.gameObject.GetInstanceID();
        SendMessageEx(MessageTitles.system_onTriggerEnter,instanceID,this);
    }
    protected virtual void OnTriggerExit2D(Collider2D coll)
    {
        int instanceID = coll.gameObject.GetInstanceID();
        SendMessageEx(MessageTitles.system_onTriggerExit,instanceID,this);
    }
    protected virtual void OnDestroy()
    {
        dispose(false);
    }
    protected virtual void OnDisable()
    {
        whenDeactive?.Invoke();
    }
}
