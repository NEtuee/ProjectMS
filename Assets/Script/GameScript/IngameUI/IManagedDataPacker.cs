using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManagedData { public UIDataType uiDataType { get; } }
public enum UIDataType { AkaneHP, AkaneBP, AkaneDP }

public class IManagedDataPacker
{
    private GameEntityBase _targetAkane;
    private GameEntityBase[] _targetEntityList;

    public void UpdateTargetAkane(GameEntityBase akane)
    {
        _targetAkane = akane;
    }
    public void UpdateTargetEntity(GameEntityBase[] entityList)
    {
        _targetEntityList = entityList;
    }
    public IManagedData PackData(IManagedData outdatedData)
    {
        switch (outdatedData.uiDataType)
        {
            case UIDataType.AkaneHP:
                PackAkaneHPData((AkaneHP.AkaneHPManagedData)outdatedData);
                break;
            case UIDataType.AkaneBP:
                PackAkaneBPData((AkaneBP.AkaneBPManagedData)outdatedData);
                break;
            default:
                break;
        }

        return outdatedData;
    }
    private AkaneHP.AkaneHPManagedData PackAkaneHPData(AkaneHP.AkaneHPManagedData outdatedData)
    {
        float newHPPercentage = _targetAkane.getStatusPercentage("HP");
        float newChangeAmount = newHPPercentage - outdatedData.hpPercentage;

        AkaneHP.AkaneHPManagedData updatedData = new AkaneHP.AkaneHPManagedData(newHPPercentage, newChangeAmount);
        return updatedData;
    }

    private AkaneBP.AkaneBPManagedData PackAkaneBPData(AkaneBP.AkaneBPManagedData outdatedData)
    {
        float newBPPercentage = _targetAkane.getStatus("Blood") / 100.0f;
        float newChangeAmount = newBPPercentage - outdatedData.bpPercentage;

        AkaneBP.AkaneBPManagedData updatedData = new AkaneBP.AkaneBPManagedData(newBPPercentage, newChangeAmount);
        return updatedData;
    }
}