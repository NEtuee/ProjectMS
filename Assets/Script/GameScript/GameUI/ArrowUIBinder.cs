using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowUIBinder : UIObjectBinder
{
    public GameObject Arrow;
    
    public override bool CheckValidLink(out string reason)
    {
        reason = string.Empty;
        return true;
    }
}
