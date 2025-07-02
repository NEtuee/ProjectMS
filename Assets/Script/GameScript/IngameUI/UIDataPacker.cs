using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDataPacker
{
    private GameEntityBase _targetAkane;
    private List<GameEntityBase> _targetEntityList;

    public void UpdateTargetAkane(GameEntityBase akane) { _targetAkane = akane;}
    public void UpdateTargetEntity(List<GameEntityBase> entityList) { _targetEntityList = entityList;}
    public IPackedUIData PackNewData(IPackedUIData outdatedData) //매 프레임 업데이트 되는 메인 데이터
    {
        switch (outdatedData.UIDataType)
        {
            case UIDataType.AkaneHP:
                return UpdateAkaneHPData((AkaneHP.AkaneHPData)outdatedData);
            default:
                return outdatedData;
        }
    }
    private AkaneHP.AkaneHPData UpdateAkaneHPData(AkaneHP.AkaneHPData outdatedData)
    {
        float newHPPercentage = _targetAkane.getStatusPercentage("HP");
        float newChangeAmount = newHPPercentage - outdatedData.HPPercentage;

        AkaneHP.AkaneHPData updatedData = new AkaneHP.AkaneHPData(newHPPercentage, newChangeAmount);
        return updatedData;
    }
    public SubUIData PackNewSubData(UIEventKey key) //임의 업데이트 되는 서브 데이터
    {
        return new SubUIData(key);
    }
}