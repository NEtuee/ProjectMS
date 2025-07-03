using System.Collections;
using UnityEngine;

public abstract class UIState
{
    public abstract UIState ChangeState(); // 특별한 조건 없으면 스스로 반환
    public virtual IEnumerator OnEnter() { yield return null; }
    public virtual void OnUpdate() { }
    public virtual IEnumerator OnExit() { yield return null; }
}