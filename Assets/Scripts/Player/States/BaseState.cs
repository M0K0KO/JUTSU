using UnityEngine;

public class BaseState 
{
    protected PlayerStateMachine stateMachine;
    
    public BaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public virtual void OnEnterState() {}
    public virtual void OnExitState() {}
    public virtual void OnUpdateState() {}
    public virtual void OnFixedUpdateState() {}
}
