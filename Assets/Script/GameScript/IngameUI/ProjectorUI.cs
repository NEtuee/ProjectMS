//모든 인게임 UI요소가 상속받는 추상 클래스
//실제 데이터와 눈에 보이는 데이터를 따로 분리(애니메이션 같은 것)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public abstract class ProjectorUI<TState> : MonoBehaviour where TState : System.Enum
{
    //투영 컴포넌트(이미지, 텍스트박스 등), 하이어라키에 존재하는 UI일 경우 인스펙터에서 연결, 인스턴스 생성해야한다면 Initialize에서 
    //ex.   [SerializeField] private Image HPProgressImage;
    //ProjectorUI의 데이터팩
    //ex.   public class AkaneHPManagedData : IManagedData
    //      {
    //      public float hpPercentage;
    //      public AkaneHPManagedData(float hpPercentage) { this.hpPercentage = hpPercentage; }
    //      }
    //실제 데이터와 투영할 데이터
    protected IManagedData _receivedData;
    protected IManagedData _projectingData;
    //상태머신, 상태 enum인 TState StateType은 각 ProjectorUI에서 따로
    protected StateMachine<TState> _stateMachine;
    //상태
    //ex. public enum StateType { NONE, Opening, Idle, Closing }


    //IngameUI 및 HolderUI에서 수동으로 초기화
    //StateMachine에 StateMap 전달, State의 Initialize 실행
    public abstract void Initialize();
    //연결된 투영 컴포넌트들([SerializeField] 이미지, 텍스트 박스 등)이 인스펙터 상에서 잘 연결됐는지 확인, 위의 Initialize에 바로 연결?
    public abstract bool CheckLinkedComponent();
    //IngameUI에서 호출, 해당 ProjectorUI의 활성화, OpeningState로 강제전환 (다음 스테이지로 넘어갔을 때 등)
    public abstract void Activate();
    //IngameUI에서 호출, 해당 ProjectorUI의 비활성화, ClosingState로 강제전환 (사망 화면 및 다음 스테이지로 넘어가기 전 등)
    public abstract void Deactivate();
    //IngameUI에서 데이터를 각 ProjectorUI로 전달할 때 사용(LateUpdate)
    //전달받은 IManagedData가 현재 ProjectorUI에서 사용하는 것과 맞는지 후 StateMachine의 현재 State의 메소드를 실행
    public abstract void UpdateProjection(IManagedData data);
}