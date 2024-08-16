using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleCommand 
{
    public void Test()
    {
        Debug.Log("Console Command Test");
        Console.ConsoleLog("Test");
    }

    public void clearall()
    {
        MasterManager.instance._stageProcessor.stopStage(true);
        HPSphereUIManager.Instance().setActive(true);
        GameUI.Instance.SetActiveCrossHair(true);
        GameUI.Instance.SetEntity(null);
        GameUI.Instance.activeOptionUI(false);
        GameUI.Instance.ActivePauseUI(false);
        ScreenDirector._instance.setActiveMainHud(true);
        ScreenDirector._instance._screenFader.clear();
        LetterBox._instance.clear();
        FMODAudioManager.Instance().clearAll();
        EffectManager._instance.killSwitchAll();

        MasterManager.instance.ActiveTitleMenu();
    }

    public void killall()
    {
        MasterManager.instance._stageProcessor.killAllCharacterWithoutKeepAliveCharacter();
    }

}
