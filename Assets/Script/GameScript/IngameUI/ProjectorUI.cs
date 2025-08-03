using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ProjectorUI : MonoBehaviour, IUIDataReceivable
{
    protected abstract IPackedUIData _dataStruct { get; }
    protected IPackedUIData _receivedData;
    protected abstract IReadOnlyCollection<UIEventKey> _validEventKeys { get; }
    protected IPackedUIData _receivedSubData;
    //IUIDataReceivable
    public UIDataType DataType => _dataStruct.UIDataType;
    public IReadOnlyCollection<UIEventKey> ValidEventKeys => _validEventKeys;
    public IPackedUIData ReceivedData => _receivedData;
    public SubUIData ReceivedSubData => (SubUIData)_receivedSubData;
    //ProjectorUI
    protected IPackedUIData _projectingData;
    protected IPackedUIData _projectingSubData;
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
    public abstract IReadOnlyCollection<UIVisualModule> UIVisualModules { get; }
    protected abstract IDictionary<Enum, UIState> _stateMap { get; }
    protected UIStateMachine _stateMachine;
    public List<Coroutine> _projectingCoroutineList;


    //공통 메서드
    public virtual void Initialize()
    {
        SetDataConstructor();
        SetStateMap();
        SetUIVisualModule();

        Deactivate();
    }
    protected abstract void SetDataConstructor();
    protected abstract void SetStateMap();
    protected abstract void SetUIVisualModule();
    public virtual void ReceiveUpdatedData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == DataType)
            _receivedData = data;
    }
    public virtual void ReceiveUpdatedSubData(IPackedUIData data)
    {
        if (data is SubUIData subData && ValidEventKeys.Contains(subData.UIEventKey))
            _receivedSubData = subData;
    }
    public void UpdateUI(float deltaTime)
    {
        UpdateProjection(deltaTime);
        UpdateUIState();
    }
    protected virtual void UpdateProjection(float deltaTime)
    {
        _stateMachine.ProjectingCurrentState();

        foreach (UIVisualModule uiVisualModule in UIVisualModules)
            uiVisualModule.UpdateSprite(deltaTime);
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
            _stateMachine.RequestStateByUpdate();
    }
    public void StopAllProjectionCoroutine()
    {
        foreach (Coroutine projectingCoroutine in _projectingCoroutineList)
        {
            if (projectingCoroutine != null)
                StopCoroutine(projectingCoroutine);
        }

        _projectingCoroutineList.Clear();

        foreach (UIVisualModule uiVisualModule in UIVisualModules)
            uiVisualModule.StopAllEffects();
    }
    public abstract void Activate();
    public abstract void Deactivate();
}