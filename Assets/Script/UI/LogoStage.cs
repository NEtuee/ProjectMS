using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoStage : MonoBehaviour
{
    public void addSignal()
    {
        MasterManager.instance._stageProcessor.addSequencerSignal("NextStage");
    }
}
