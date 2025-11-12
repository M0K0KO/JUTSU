using UnityEngine;

public class RunState : BaseState
{
    public RunState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.player.mover.EnableMove();
        stateMachine.player.mover.EnableRotation();
        stateMachine.player.mover.ChangeSpeedToRunSpeed();
    }
    
    public override void OnUpdateState()
    {        
        stateMachine.player.mover.EnableRotation();
        
        stateMachine.player.mover.Move(stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput));
        stateMachine.player.mover.Rotate(stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput));
        
        if (stateMachine.player.playerInput.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
        else if (!stateMachine.player.playerInput.RunInput)
        {
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