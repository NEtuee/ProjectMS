using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUIBinder : UIObjectBinder
{
    public override bool CheckValidLink(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void resume()
    {
        MasterManager.instance.activePauseUI();
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }

    public void quit()
    {
        Application.Quit();
        FMODAudioManager.Instance().Play(5005 ,Vector3.zero);
    }
}
