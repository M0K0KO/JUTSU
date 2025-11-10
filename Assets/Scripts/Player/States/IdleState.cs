using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.mover.DisableMove();
        
        stateMachine.mover.ChangeSpeedToZero();
    }
    
    public override void OnUpdateState()
    {
        if (stateMachine.isStrafing)
            stateMachine.mover.StrafeRotate();
        
        if (stateMachine.playerInput.MoveInput != Vector2.zero)
        {
            if (stateMachine.playerInput.RunInput)
                stateMachine.ChangeState(stateMachine.runState);
            else
                stateMachine.ChangeState(stateMachine.walkState);
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        
    }
}