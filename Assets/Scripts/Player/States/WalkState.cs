using UnityEngine;

public class WalkState : BaseState
{
    public WalkState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.mover.EnableMove();
        stateMachine.mover.ChangeSpeedToWalkSpeed();
    }
    
    public override void OnUpdateState()
    {
        stateMachine.mover.EnableRotation();
        
        stateMachine.mover.Move(stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput));
        
        if (!stateMachine.isStrafing)
            stateMachine.mover.Rotate(stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput));
        else
            stateMachine.mover.StrafeRotate();
        
        if (stateMachine.playerInput.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
        else if (stateMachine.playerInput.RunInput)
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