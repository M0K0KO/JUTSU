using UnityEngine;


public abstract class BossBaseState
{
    public BossStateMachine StateMachine { get; private set; }
    
    protected BossBaseState(BossStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void OnEnter() {}
    public virtual void OnUpdate() {}
    public virtual void OnFixedUpdate() {}
    public virtual void OnExit() {}
    public virtual void OnCollisionEnter(Collision collision) {}
    public virtual void OnTriggerEnter(Collider other) {}
}
