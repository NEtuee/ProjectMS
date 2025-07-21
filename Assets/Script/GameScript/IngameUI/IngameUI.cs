//데이터를 가공해서 하위 ProjectorUI에 전달
//하위 ProjectorUI가 요구하는 IPackedUIData로 포장


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private AkaneDPHolder _akaneDPHolder;

    private List<HolderUI> _holderUIList = new List<HolderUI>();
    //QTE
    //보스HP
    //보스 페이즈
    //적 랭크뱃지
    //적 화면밖 인디케이터
    //적 특수공격 인디케이터
    //말풍선
    //대화창
    //크로스헤어



    private void Awake()
    {
        Instance = this;
        InitializeProjectorUI();
    }
    private void InitializeProjectorUI()
    {
        //UIList 요소 추가
        _projectorUIList.Add(_akaneHP);
        _projectorUIList.Add(_akaneBP);

        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Initialize();
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
    }
    private void UpdateUIData(ProjectorUI projectorUI)
    {
        var outdatedData = projectorUI.ReceivedData;
        var updatedData = _uiDataPacker.PackNewData(outdatedData);
        DeliverData(projectorUI, updatedData);
    }
    private void DeliverData(ProjectorUI projectorUI, IPackedUIData updatedData)
    {
        projectorUI.ReceiveData(updatedData);
    }
    //서브 UI데이터(이벤트 키), 임의 호출
    public void NotifyUIEventHappened(string stringKey)
    {
        UIEventKey enumKey;
        if (!Enum.TryParse(stringKey, out enumKey))
            enumKey = UIEventKey.NONE;
        
        SubUIData updatedSubData = _uiDataPacker.PackNewSubData(enumKey);

        foreach (ProjectorUI projectorUI in _projectorUIList)
            DeliverSubData(projectorUI, updatedSubData);
    }
    private void DeliverSubData(ProjectorUI projectorUI, IPackedUIData updatedData)
    {
        projectorUI.ReceiveSubData(updatedData);
    }
    public void DeactivateAllUI()
    {
        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Deactivate();
    }
}