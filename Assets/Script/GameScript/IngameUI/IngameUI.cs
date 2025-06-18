//데이터를 가공해서 하위 ProjectorUI에 전달
//하위 ProjectorUI가 요구하는 IManagedData로 포장


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    public static IngameUI Instance;
    private GameEntityBase _mainUIEntity;
    private GameEntityBase[] _subUIEntityList;
    private IManagedDataPacker _iManagedDataPacker = new IManagedDataPacker();

    //인스펙터 상에서 연결할 UI
    private List<ProjectorUI> _projectorUIList = new List<ProjectorUI>();
    private Dictionary<string, List<Action>> _actionSubscriberUI;
    [SerializeField] private AkaneHP _akaneHP;
    [SerializeField] private AkaneBP _akaneBP;
    [SerializeField] private AkaneDPHolder _akaneDPHolder;

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
        BindActionSubscriberUI();
    }
    private void InitializeProjectorUI()
    {
        _projectorUIList.Add(_akaneHP);

        foreach (ProjectorUI projectorUI in _projectorUIList)
            projectorUI.Initialize();
    }
    private void BindActionSubscriberUI()
    {
        //_notifySubscriber.Add(string _actionName, new List<Action>());
        //_notifySubscriber[string _actionName].Add(_projectorUI.method);
    }



    //Awake 후 엔티티를 참조하는 UI를 설정
    public void SetMainUIEntity(GameEntityBase mainUIEntity)
    {
        //아카네 체크?
        _mainUIEntity = mainUIEntity;
        _iManagedDataPacker.UpdateTargetAkane(_mainUIEntity);
    }
    public void SetSubUIEntityList(GameEntityBase[] subUIEntityList)
    {
        _subUIEntityList = subUIEntityList;
        _iManagedDataPacker.UpdateTargetEntity(_subUIEntityList);
    }


    public void ActivateUI()
    {
        _akaneHP.Activate();
    }
    //값 업데이트, IManagedData 세부 struct로 업데이트 및 전달
    private void LateUpdate()
    {
        if (_mainUIEntity == null)
            return;

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();
        _akaneHP.UpdateProjection(_iManagedDataPacker.PackData(_akaneHP.ReceivedData));
        //_akaneBP.UpdateProjection(_iManagedDataPacker.PackData(_akaneBP.ReceivedData));
    }
    //특정 액션 발생하면 UI 호출
    public void NotifyToUI(string key)
    {
        if (_actionSubscriberUI.TryGetValue(key, out var subscriberList) == false)
            return;
        foreach (var action in subscriberList)
            action?.Invoke();
    }
}