using System.Collections;
using UnityEngine;

public abstract class UIState
{
    public virtual void OnUpdate() { }
    public abstract UIState ChangeState(SubUIData subData); // SubUIData(UIEventKey) 받아올때마다 체크, 특별한 조건 없으면 null 반환, 자기 자신 반환시 그 상태 재시작
    public abstract UIState UpdateState(); //매 프레임 검사하는 eventKey 사용하지 않는 ChangeState?
    public virtual IEnumerator OnEnter() { yield break; } //ForceChange 체크할 때, MoveNext()에서 false를 반환 
    public virtual IEnumerator OnExit() { yield break; }
}