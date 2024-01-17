using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObjectBinder : MonoBehaviour
{
    public abstract bool CheckValidLink(out string reason);
}

public interface IUIElement
{
    public bool CheckValidBinderLink(out string reason);
    
    public void SetBinder<T>(T binder) where T : UIObjectBinder;

    public void Initialize();
}