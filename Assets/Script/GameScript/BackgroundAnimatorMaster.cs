using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAnimatorMaster : MonoBehaviour
{
    public AnimatorRelay[] _animatorRelay = null;

    public void setTrigger(string key, string trigger)
    {
        Debug.Log(key);
        foreach(var item in _animatorRelay)
        {
            if(item._key != key)
                continue;

            item.setTrigger(trigger);
        }
    }
}
