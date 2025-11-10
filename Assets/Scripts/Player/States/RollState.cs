using System.Collections;
using UnityEngine;

public class RollState : BaseState
{
    private Coroutine rollCoroutine;
    
    public RollState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnterState()
    {
        if (!stateMachine.isStrafing) PivotTowardsMoveDirection();
        stateMachine.mover.DisableRotation();
        stateMachine.mover.EnableMove();
        
        Vector3 rollDir = Vector2.zero;
        if (stateMachine.playerInput.MoveInput != Vector2.zero)
            rollDir = stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput);
        else
            rollDir = new Vector3(stateMachine.transform.forward.x, 0f, stateMachine.transform.forward.z).normalized;
        rollCoroutine = stateMachine.StartCoroutine(stateMachine.mover.Roll(rollDir));
        
        stateMachine.animationController.UpdateAnimParam("horizontalMoveAmount", stateMachine.playerInput.MoveInput.x);
        stateMachine.animationController.UpdateAnimParam("verticalMoveAmount", stateMachine.playerInput.MoveInput.y);
        stateMachine.animationController.TriggerAnimParam("roll");
    }
    
    public override void OnUpdateState()
    {
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.animationController.ResetTriggerAnimParam("roll");
    }
    
    private void PivotTowardsMoveDirection()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = stateMachine.mover.GetCameraRelativeMoveDirection(stateMachine.playerInput.MoveInput);
        
        targetDirection.y = 0;
        targetDirection.Normalize();
        stateMachine.mover.Rotate(targetDirection, false);
    }
}