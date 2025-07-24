using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ProjectorUI : MonoBehaviour
{
    protected abstract UIDataType _dataType { get; }
    protected IPackedUIData _receivedData;
    protected IPackedUIData _projectingData;
    protected abstract IReadOnlyCollection<UIEventKey> _validEventKeys { get; }
    protected IPackedUIData _receivedSubData;
    protected IPackedUIData _projectingSubData;
    public UIDataType DataType => _dataType;
    public IReadOnlyCollection<UIEventKey> ValidEventKeys => _validEventKeys;
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
    protected abstract IDictionary<TUIStateType, UIState> _stateMap<TUIStateType>() where TUIStateType : Enum;
    protected UIStateMachine _stateMachine;
    protected UIEffectManager _uiEffectManager;
    protected Coroutine _memorySavingCoroutine;


    //공통 메서드
    public virtual void Initialize()
    {
        _stateMachine = new UIStateMachine(this);
        _uiEffectManager = gameObject.AddComponent<UIEffectManager>();

        SetInitialConstructor();
        SetStateMap();
        SetUIVisualModule();

        Deactivate();
    }
    protected abstract void SetInitialConstructor();
    protected abstract void SetStateMap();
    protected abstract void SetUIVisualModule();
    public virtual void ReceiveData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == DataType)
            _receivedData = data;
    }
    public virtual void ReceiveSubData(IPackedUIData data)
    {
        if (data is SubUIData subData && ValidEventKeys.Contains(subData.UIEventKey))
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
    protected IEnumerator MemorySavingCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        UpdateProjection();
        UpdateUIState();
    }
    public abstract void Activate();
    public abstract void Deactivate();
}