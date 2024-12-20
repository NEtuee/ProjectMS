using UnityEngine;
using System.Collections.Generic;

public struct AttachChildDescription
{
    public ObjectBase   _childObject;
    public Vector3      _pivot;
}

public abstract class ObjectBase : MessageReceiver, IProgress
{
    public System.Action            whenDeactive = ()=>{};

    public AttackProcessorManager   _attackProcessorManager = new AttackProcessorManager();

    protected int                   _currentManagerNumber = -1;

    protected AttackState           _attackState = AttackState.Default;
    protected DefenceState          _defenceState = DefenceState.Default;

    protected GameObject            _spriteObject;
    protected SpriteRenderer        _spriteRenderer;
    protected Animator              _animator;
    protected Vector3               _direction = Vector3.right;
    protected Vector3               _localSpritePosition = Vector3.zero;
    protected ObjectBase            _parentObject = null;
    protected ObjectBase            _childObject = null;
    private Vector3                 _childPivot = Vector3.zero;

    protected AllyInfoData          _allyInfo = null;

    protected ObjectBase            _summonObject = null;

    public void setAttackState(AttackState state) {_attackState = state;}
    public void setDefenceState(DefenceState state) {_defenceState = state;}

    protected void initializeObject()
    {
        _attackState = AttackState.Default;
        _defenceState = DefenceState.Default;
    }

    protected override void Awake()
    {
        base.Awake();
        assign(); 
        //Initialize();
    }
    protected virtual void Start()
    {
        if(gameObject.activeInHierarchy)
            initialize();
    }

    public virtual void deactive()
    {
        detachChildObject();
        detachFromParent();
        
        setSummonObject(null);

        while(gameObject.transform.childCount == 0)
        {
            gameObject.transform.GetChild(0).SetParent(null);
        }

        gameObject.SetActive(false);
    }

    public void updatePosition(Vector3 position)
    {
        if(hasParentObject())
            return;

        transform.position = position;

        if(CameraControlEx.Instance().isCameraTargetObject(this))
            _spriteObject.transform.localPosition = Vector3.zero;
        else
            _spriteObject.transform.position = MathEx.floorNoSign(transform.position,2);

        _spriteObject.transform.localPosition += _localSpritePosition;
        updateChildTransform();
    }

    public void createSpriteRenderObject()
    {
        _spriteObject = new GameObject("SpriteObject");
        _spriteObject.transform.SetParent(this.transform);
        _spriteObject.transform.localPosition = Vector3.zero;

        _animator = _spriteObject.AddComponent<Animator>();
        _spriteRenderer = _spriteObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.material = Material.Instantiate(getBaseMaterial());
    }

    public void updateChildTransform()
    {
        if(hasChildObject() == false)
            return;

        float angle = _spriteRenderer.transform.rotation.eulerAngles.z;
        if(_spriteRenderer.flipX)
        {
            angle -= 180f;
            angle *= -1f;
        }

        _childObject.transform.position = transform.position + (Quaternion.Euler(0f,0f,angle) * _childPivot);
    }

    public bool attachChildObject(AttachChildDescription childDescription)
    {
        if(hasParentObject())
            return false;

        detachChildObject();

        if(childDescription._childObject != null)
            childDescription._childObject.detachFromParent();

        _childPivot = childDescription._pivot;
        _childObject = childDescription._childObject;
        _childObject.setParentObject(this);
        return true;
    }

    public void detachChildObject()
    {
        if(hasChildObject() == false)
            return;

        _childObject.setParentObject(null);
        _childObject = null;
    }

    public void detachFromParent()
    {
        if(hasParentObject() == false)
            return;
        
        _parentObject.detachChildObject();
    }

    public void setAllyInfo(AllyInfoData allyInfo) {_allyInfo = allyInfo;}

    public void setSummonObject(ObjectBase summonObject)
    {
        _summonObject = summonObject;
    }

    public ObjectBase getSummonObject()
    {
        return _summonObject;
    }

    public void setParentObject(ObjectBase parentObject)
    {
        _parentObject = parentObject;
    }

    public ObjectBase getChildObject()
    {
        return _childObject;
    }

    public ObjectBase getParentObject()
    {
        return _parentObject;
    }

    public bool hasParentObject()
    {
        return _parentObject != null;
    }

    public bool hasChildObject()
    {
        return _childObject != null;
    }

    public Material getBaseMaterial()
    {
        return ResourceContainerEx.Instance().GetMaterial("Material/Material_CharacterMaster");
    }

    public Animator getAnimator() 
    {
        return _animator;
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
        var msg = MessagePack(MessageTitles.system_deregisterRequest,_currentManagerNumber,GetUniqueID());
        _currentManagerNumber = -1;

        MasterManager.instance.SendMessageDirectInMaster(msg);
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
    public virtual void progress(float deltaTime)
    {
        WeightRandomManager.Instance().updateRandom();
    }
    //LateUpdate
    public virtual void afterProgress(float deltaTime){}
    //FixedUpdate
    public virtual void fixedProgress(float deltaTime){}
    
    public virtual void release(bool disposeFromMaster)
    {
        if(disposeFromMaster == false && _currentManagerNumber != -1)
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

    public float getDistance(ObjectBase obj)
    {
        if(obj == null)
            return -1f;
        
        return Vector3.Distance(transform.position,obj.transform.position);
    }
    public float getDistanceSq(ObjectBase obj) {return (transform.position - obj.transform.position).sqrMagnitude;}

    public Material getCurrentMaterial()
    {
        return _spriteRenderer?.material;
    }

    public void setSortingOrder(int sortingOrder)
    {
        if(_spriteRenderer == null)
            return;

        _spriteRenderer.sortingOrder = sortingOrder;
    }

    public Transform getSpriteRendererTransform()
    {
        if(_spriteRenderer == null)
            return null;

        return _spriteRenderer.transform;
    }

    public int getAllyInfoKey()
    {
        return _allyInfo == null ? -1 : _allyInfo._index;
    }

    public AllyInfoData getAllyInfo()
    {
        return _allyInfo;
    }

    public Vector3 getDirection() {return _direction;}
    public void setDirection(Vector3 direction) {_direction = direction;}

}
