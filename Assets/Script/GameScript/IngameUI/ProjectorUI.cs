using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ProjectorUI : MonoBehaviour
{
    protected abstract UIDataType DataType { get; }
    protected IPackedUIData _receivedData;
    protected IPackedUIData _projectingData;
    protected abstract IReadOnlyCollection<UIEventKey> ValidEventKeys { get; }
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



    public virtual void Initialize()
    {
        _stateMachine = new UIStateMachine(this);
        _receivedSubData = new SubUIData();
        _projectingSubData = new SubUIData();
    }
    public abstract void Activate();
    public abstract void Deactivate();
    public virtual void UpdateData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == DataType)
            _receivedData = data;
    }
    public virtual void UpdateSubData(IPackedUIData data)
    {
        if (data is SubUIData subData && ValidEventKeys.Contains(subData.UIEventKey))
            _receivedSubData = subData;
    }
    public void ConfirmSubData()
    {
        _projectingSubData = _receivedSubData;
        _receivedSubData = null;
    }
    protected virtual void UpdateProjection()
    {
        _stateMachine.UpdateState();
    }
    protected void LateUpdate()
    {
        UpdateProjection();
    }
}