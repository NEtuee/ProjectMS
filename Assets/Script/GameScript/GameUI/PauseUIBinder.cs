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
}
