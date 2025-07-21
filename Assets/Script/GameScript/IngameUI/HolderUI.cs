using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

//프리팹으로 런타임에서 생성되는 ProjectorUI를 모아서 관리하는 용도
//DP, 적 랭크뱃지, 적 인디케이터 등
public abstract class HolderUI : MonoBehaviour
{
    protected abstract UIDataType _dataType { get; }
    protected IPackedUIData _receivedData;
    protected IPackedUIData _receivedSubData;
    public IPackedUIData ReceivedData => _receivedData;
    public SubUIData ReceivedSubData => (SubUIData)_receivedSubData;
    protected ObjectPool<ProjectorUI> _objectPool;
    protected List<ProjectorUI> _uiHolder;
    protected abstract ProjectorUI _projectorUIPrefab { get; }


    //공통 메서드
    public virtual void PrepareInitialize()
    {
        _objectPool = new ObjectPool<ProjectorUI>(CreateProjectorUI, TakeProjectorUI, ReturnProjectorUI, DestroyProjectorUI, true, 5, 50);
    }
    public virtual void ReceiveData(IPackedUIData data)
    {
        if (data != null && data.UIDataType == _dataType)
            _receivedData = data;
    }
    public virtual void ReceiveSubData(IPackedUIData data)
    {
        if (data is SubUIData subData)
            _receivedSubData = subData;

        ShareSubData(ReceivedSubData);
    }
    public virtual void ShareSubData(SubUIData subData) //추가로 하위 ProjectorUI에 SubUIData 전달
    {
        foreach (ProjectorUI projectorUI in _uiHolder)
            projectorUI.ReceiveSubData(ReceivedSubData);
    }
    protected virtual ProjectorUI CreateProjectorUI() //프리팹 생성
    {
        ProjectorUI projectorUI = Instantiate(_projectorUIPrefab);
        return projectorUI;
    }
    protected virtual void TakeProjectorUI(ProjectorUI projectorUI)
    {

    }
    protected virtual void ReturnProjectorUI(ProjectorUI projectorUI)
    {

    }
    protected virtual void DestroyProjectorUI(ProjectorUI projectorUI) //프리팹 삭제
    {

    }
    public abstract void Initialize();
    public abstract void Activate();
    public abstract void Deactivate();
}