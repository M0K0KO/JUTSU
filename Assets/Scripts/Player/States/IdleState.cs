using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.player.mover.DisableMove();
        stateMachine.player.mover.EnableRotation();
        stateMachine.player.mover.ChangeSpeedToZero();
    }
    
    public override void OnUpdateState()
    {
        if (stateMachine.isStrafing)
            stateMachine.player.mover.StrafeRotate();
        
        if (stateMachine.player.playerInput.MoveInput != Vector2.zero)
        {
            if (stateMachine.player.playerInput.RunInput)
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