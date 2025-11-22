using UnityEngine;

public class BossIdleState : BossBaseState
{
    private float _stateEnterTime;

    private const float IdleWaitDuration = 0.85f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public BossIdleState(BossStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    public override void OnEnter()
    {
        StateMachine.BossAgent.ResetPath();
        _stateEnterTime = Time.time;
    }

    public override void OnUpdate()
    {
        if (Time.time - _stateEnterTime >= IdleWaitDuration)
        {
            StateMachine.ChangeState(StateMachine.ChaseState);
        }
        
    }
}
