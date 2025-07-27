using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

public class IngameUI : MonoBehaviour
{
    public static IngameUI Instance;
    private GameEntityBase _mainUIEntity;
    private List<GameEntityBase> _subUIEntityList;
    private UIDataPacker _uiDataPacker = new UIDataPacker();

    //인스펙터 상에서 연결할 UI
    private List<ProjectorUI> _projectorUIList = new List<ProjectorUI>();
    [SerializeField] private AkaneHP _akaneHP;
    [SerializeField] private AkaneBP _akaneBP;
    //QTE
    //보스HP (체력 감소에만 반응?)
    //보스 페이즈
    //크로스헤어

    private List<HolderUI> _holderUIList = new List<HolderUI>();
    [SerializeField] private AkaneDPHolder _akaneDPHolder;


    //적 랭크뱃지
    //적 화면밖 인디케이터
    //적 특수공격 인디케이터
    //말풍선
    //대화창




    private void Awake()
    {
        Instance = this;
        InitializeProjectorUI();
        InitializeHolderUI();
    }
    private void InitializeProjectorUI()
    {
        //UIList 요소 추가
        _projectorUIList.Add(_akaneHP);
        _projectorUIList.Add(_akaneBP);

        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Initialize();
    }
    private void InitializeHolderUI()
    {
        _holderUIList.Add(_akaneDPHolder);

        foreach (HolderUI holderUI in _holderUIList)
            holderUI.Initialize();
    }
    //Awake 후 엔티티를 참조하는 UI를 설정
    public void SetMainUIEntity(GameEntityBase mainUIEntity)
    {
        //아카네 체크?
        _mainUIEntity = mainUIEntity;
        _uiDataPacker.RefreshTargetAkane(_mainUIEntity);
    }
    public void SetSubUIEntityList(List<GameEntityBase> subUIEntityList)
    {
        _subUIEntityList = subUIEntityList;
        _uiDataPacker.RefreshTargetEntity(_subUIEntityList);
    }
    public void ActivateAllUI()
    {
        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Activate();
    }
    //메인 UI데이터, 매 프레임마다 호출
    void LateUpdate()
    {
        if (_mainUIEntity == null)
            return;

        foreach (ProjectorUI projectorUI in _projectorUIList)
            UpdateUIData(projectorUI);
        foreach (HolderUI holderUI in _holderUIList)
            UpdateUIData(holderUI);

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.UpdateUI(deltaTime);
        foreach (HolderUI holderUI in _holderUIList)
            holderUI.UpdateUI(deltaTime);
    }
    private void UpdateUIData(IUIDataReceivable ingameUI)
    {
        var outdatedData = ingameUI.ReceivedData;
        var updatedData = _uiDataPacker.PackNewData(outdatedData);
        DeliverUpdatedUIData(ingameUI, updatedData);
    }
    private void DeliverUpdatedUIData(IUIDataReceivable ingameUI, IPackedUIData updatedData)
    {
        ingameUI.ReceiveUpdatedData(updatedData);
    }
    //서브 UI데이터(이벤트 키), 임의 호출
    public void NotifyUIEventHappened(string stringKey)
    {
        UIEventKey enumKey;
        if (!Enum.TryParse(stringKey, out enumKey))
            enumKey = UIEventKey.NONE;

        SubUIData updatedSubData = _uiDataPacker.PackNewSubData(enumKey);

        foreach (ProjectorUI projectorUI in _projectorUIList)
            DeliverUpdatedSubData(projectorUI, updatedSubData);
        foreach (HolderUI holderUI in _holderUIList)
            DeliverUpdatedSubData(holderUI, updatedSubData);
    }
    private void DeliverUpdatedSubData(IUIDataReceivable ingameUI, IPackedUIData updatedData)
    {
        ingameUI.ReceiveUpdatedSubData(updatedData);
    }
    public void DeactivateAllUI()
    {
        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Deactivate();
    }
}