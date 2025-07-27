using System.Collections.Generic;

public class UIDataPacker
{
    private GameEntityBase _targetAkane;
    private List<GameEntityBase> _targetEntityList;

    public void RefreshTargetAkane(GameEntityBase akane) { _targetAkane = akane; }
    public void RefreshTargetEntity(List<GameEntityBase> entityList) { _targetEntityList = entityList; }
    public IPackedUIData PackNewData(IPackedUIData outdatedData) //매 프레임 업데이트 되는 메인 데이터
    {
        switch (outdatedData.UIDataType)
        {
            case UIDataType.AkaneHP:
                return UpdateAkaneHPData((AkaneHP.AkaneHPData)outdatedData);
            case UIDataType.AkaneBP:
                return UpdateAkaneBPData((AkaneBP.AkaneBPData)outdatedData);
            case UIDataType.AkaneDP:
                return UpdateAkaneDPData((AkaneDPHolder.AkaneDPData)outdatedData);
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
    private AkaneBP.AkaneBPData UpdateAkaneBPData(AkaneBP.AkaneBPData outdatedData)
    {
        float newBPPercentage = _targetAkane.getStatusPercentage("Blood");
        float newChangeAmount = newBPPercentage - outdatedData.BPPercentage;

        AkaneBP.AkaneBPData updatedData = new AkaneBP.AkaneBPData(newBPPercentage, newChangeAmount);
        return updatedData;
    }
    private AkaneDPHolder.AkaneDPData UpdateAkaneDPData(AkaneDPHolder.AkaneDPData outdatedData)
    {
        int newCurrentDP = (int)_targetAkane.getStatus("DashPoint");
        int newMaxDP = (int)_targetAkane.getStatusInfo().getMaxStatus("DashPoint");
        float newDelayedRegenTime = 0.0f;
        if (newCurrentDP < newMaxDP)
        {
            newDelayedRegenTime = (float)_targetAkane.getStatusInfo().getBuffDurationPercentage(35); //35번 버프 DashPointRegenDelayed
        }

        AkaneDPHolder.AkaneDPData updatedData = new AkaneDPHolder.AkaneDPData(newCurrentDP, newMaxDP, newDelayedRegenTime);
        return updatedData;
    }
    public SubUIData PackNewSubData(UIEventKey key) //임의 업데이트 되는 서브 데이터
    {
        return new SubUIData(key);
    }
    


    //enabledCharacter (인디케이터 등에 사용) 업데이트 및 데이터 전달
}