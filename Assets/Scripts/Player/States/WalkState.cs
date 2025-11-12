using UnityEngine;

public class WalkState : BaseState
{
    public WalkState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.player.mover.EnableMove();
        stateMachine.player.mover.EnableRotation();
        stateMachine.player.mover.ChangeSpeedToWalkSpeed();
    }
    
    public override void OnUpdateState()
    {
        stateMachine.player.mover.EnableRotation();
        
        stateMachine.player.mover.Move(stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput));
        
        if (!stateMachine.isStrafing)
            stateMachine.player.mover.Rotate(stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput));
        else
            stateMachine.player.mover.StrafeRotate();
        
        if (stateMachine.player.playerInput.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
        else if (stateMachine.player.playerInput.RunInput)
        {
            stateMachine.ChangeState(stateMachine.runState);
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        
    }
}