//데이터를 가공해서 하위 ProjectorUI에 전달
//하위 ProjectorUI가 요구하는 IManagedData로 포장


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    public static IngameUI Instance;
    private GameEntityBase _targetAkane;
    private GameEntityBase _targetEntity;

    //인스펙터 상에서 연결할 UI
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
    }
    private void InitializeProjectorUI()
    {
        
    }
    private void CheckIManagedData()
    {

    }
    public void SetAkane(GameEntityBase targetEntity)
    {
        _targetAkane = targetEntity;
        if (targetEntity == null) return;
    }
    //UI 업데이트
    private void LateUpdate()
    {

    }
}