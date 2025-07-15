using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//프리팹으로 동적으로 생성되는 UI를 모아서 관리하는 용도? UI의 생성도 여기서 해야하는지,, 아니면 IngameUI에서?
//데이터에 맞춰 유동적으로,, 오브젝트풀링?
//적 랭크뱃지, 적 인디케이터
public abstract class HolderUI : ProjectorUI
{
    protected List<ProjectorUI> _holdingProjectorUI { get; private set; } = new List<ProjectorUI>();



    public override void ReceiveSubData(IPackedUIData data) //덮어씀, 추가로 하위 ProjectorUI에 SubUIData 전달
    {
        base.ReceiveSubData(data);

        if (data is SubUIData subData)
            foreach (ProjectorUI projectorUI in _holdingProjectorUI)
                projectorUI.ReceiveSubData(subData);
    }

    protected virtual void CreateProjectorUI()
    {

    }
    protected virtual void DestroyProjectorUI() //파괴할 특정 ProjectorUI에 접근?? 그런 방식이 아니라 데이터 홀더를 삭제,, 데이터 전달은 남은 거에
    {

    }
}