using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorRelay : MonoBehaviour
{
    public string _key;
    
    private Animator _animator;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void setTrigger(string triggerName)
    {
        _animator?.SetTrigger(triggerName);
    }
}
