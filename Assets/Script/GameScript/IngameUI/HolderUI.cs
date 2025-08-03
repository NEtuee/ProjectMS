using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

//프리팹으로 런타임에서 생성되는 ProjectorUI를 모아서 관리하는 용도
//DP, 적 랭크뱃지, 적 인디케이터 등
public abstract class HolderUI : MonoBehaviour, IUIDataReceivable
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
    //HolderUI
    protected ObjectPool<ProjectorUI> _objectPool;
    protected List<ProjectorUI> _uiHolder;
    protected abstract Vector2 _poolSize { get; }
    protected RectTransform _holdingComponent;
    protected abstract ProjectorUI _projectorUIPrefab { get; }


    //공통 메서드
    public virtual void Initialize()
    {
        _objectPool = new ObjectPool<ProjectorUI>(
            createFunc: CreateProjectorUI,
            actionOnGet: TakeProjectorUI,
            actionOnRelease: ReturnProjectorUI,
            actionOnDestroy: DestroyProjectorUI,
            false, (int)_poolSize.x, (int)_poolSize.y);

        _holdingComponent = GetComponent<RectTransform>();
        _uiHolder = new List<ProjectorUI>();

        SetInitialConstructor();
    }
    protected abstract void SetInitialConstructor();
    public virtual void ReceiveUpdatedData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == DataType)
            _receivedData = data;

        ShareUpdatedData();
    }
    protected abstract void ShareUpdatedData(); //데이터를 가공해 하위 ProjectorUI에 전달
    public virtual void ReceiveUpdatedSubData(IPackedUIData data)
    {
        if (data is SubUIData subData)
            _receivedSubData = subData;

        ShareUpdatedSubData(ReceivedSubData);
    }
    public virtual void ShareUpdatedSubData(SubUIData subData) //추가로 하위 ProjectorUI에 SubUIData 전달
    {
        foreach (ProjectorUI projectorUI in _uiHolder)
            projectorUI.ReceiveUpdatedSubData(ReceivedSubData);
    }
    protected abstract void UpdateHolder(); //데이터를 사용해 Holder를 업데이트


    //오브젝트 풀 메서드
    protected virtual ProjectorUI CreateProjectorUI() //프리팹 생성
    {
        ProjectorUI projectorUI = Instantiate(_projectorUIPrefab, _holdingComponent);
        projectorUI.Initialize();

        return projectorUI;
    }
    protected virtual void TakeProjectorUI(ProjectorUI projectorUI) //홀더 등록
    {
        projectorUI.Activate();
    }
    protected virtual void ReturnProjectorUI(ProjectorUI projectorUI) //홀더 해제
    {
        projectorUI.Deactivate();
    }
    protected virtual void DestroyProjectorUI(ProjectorUI projectorUI) //프리팹 삭제
    {
        Destroy(projectorUI);
    }
    public virtual void Activate()
    {
        foreach (ProjectorUI projectorUI in _uiHolder)
        {
            if (projectorUI != null)
                projectorUI.Activate();
        }
    }
    public virtual void Deactivate()
    {
        foreach (ProjectorUI projectorUI in _uiHolder)
        {
            if (projectorUI != null)
                projectorUI.Deactivate();
        }
    }
    public void UpdateUI(float deltaTime)
    {
        UpdateHolder();

        foreach (ProjectorUI projectorUI in _uiHolder)
            projectorUI.UpdateUI(deltaTime);
    }
}