using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectorUI : MonoBehaviour
{
    protected abstract UIDataType _dataType { get; }
    protected IPackedUIData _receivedData;
    protected IPackedUIData _projectingData;
    protected abstract IReadOnlyCollection<UIEventKey> _validEventKeys { get; }
    protected IPackedUIData _receivedSubData;
    protected IPackedUIData _projectingSubData;
    public IPackedUIData ReceivedData => _receivedData;
    public SubUIData ReceivedSubData => (SubUIData)_receivedSubData;
    public IPackedUIData ProjectingData
    {
        get => _projectingData;
        set => _projectingData = value;
    }
    public SubUIData ProjectingSubData
    {
        get => (SubUIData)_projectingSubData;
        set => _projectingSubData = value;
    }
    protected UIStateMachine _stateMachine;
    protected AnimationPlayer _animationPlayer;
    protected UICoroutineManager _uiCoroutineManager;


    //공통 메서드
    public virtual void PrepareInitialize()
    {
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();

        _stateMachine = new UIStateMachine(this);
        _animationPlayer = new AnimationPlayer();
        _uiCoroutineManager = gameObject.AddComponent<UICoroutineManager>();
    }
    public virtual void ReceiveData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == _dataType)
            _receivedData = data;
    }
    public virtual void ReceiveSubData(IPackedUIData data)
    {
        if (data is SubUIData subData && _validEventKeys.Contains(subData.UIEventKey))
            _receivedSubData = subData;
    }
    protected virtual void UpdateProjection()
    {
        _stateMachine.ProjectingCurrentState();
    }
    protected virtual void UpdateUIState()
    {
        if (ReceivedSubData != null && ReceivedSubData.UIEventKey != UIEventKey.NONE)
        {
            _stateMachine.RequestStateBySubData(ReceivedSubData);

            _projectingSubData = _receivedSubData;
            _receivedSubData = new SubUIData();
        }
        else
        {
            _stateMachine.RequestStateByUpdate();
        }
    }
    private void LateUpdate()
    {
        UpdateProjection();
        UpdateUIState();
    }
    public abstract void Initialize();
    public abstract void Activate();
    public abstract void Deactivate();
}