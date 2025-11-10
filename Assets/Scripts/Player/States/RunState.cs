using UnityEngine;

public class RunState : BaseState
{
    public RunState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.mover.EnableMove();
        stateMachine.mover.ChangeSpeedToRunSpeed();
    }
    
    public override void OnUpdateState()
    {        
        stateMachine.mover.EnableRotation();
        
        stateMachine.mover.Move(stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput));
        stateMachine.mover.Rotate(stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput));
        
        if (stateMachine.playerInput.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
        else if (!stateMachine.playerInput.RunInput)
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