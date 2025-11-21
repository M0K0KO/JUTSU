using System.Collections;
using UnityEngine;

public class RollState : BaseState
{
    private Coroutine rollCoroutine;
    
    public RollState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnterState()
    {
        if (!stateMachine.isStrafing || stateMachine.player.playerInput.RunInput)
        {
            stateMachine.player.mover.EnableRotation();
            PivotTowardsMoveDirection();
            stateMachine.player.mover.DisableRotation();
        }

        stateMachine.player.mover.EnableMove();
        
        Vector3 rollDir = Vector2.zero;
        if (stateMachine.player.playerInput.MoveInput != Vector2.zero)
            rollDir = stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput);
        else
            rollDir = new Vector3(stateMachine.transform.forward.x, 0f, stateMachine.transform.forward.z).normalized;
        rollCoroutine = stateMachine.StartCoroutine(stateMachine.player.mover.Roll(rollDir));
        
        stateMachine.player.animController.UpdateAnimParam("horizontalMoveAmount", stateMachine.player.playerInput.MoveInput.x);
        stateMachine.player.animController.UpdateAnimParam("verticalMoveAmount", stateMachine.player.playerInput.MoveInput.y);
        stateMachine.player.animController.TriggerAnimParam("roll");
    }
    
    public override void OnUpdateState()
    {
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.player.animController.ResetTriggerAnimParam("roll");
        if (rollCoroutine != null) stateMachine.StopCoroutine(rollCoroutine);
    }
    
    private void PivotTowardsMoveDirection()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = stateMachine.player.mover.GetCameraRelativeMoveDirection(stateMachine.player.playerInput.MoveInput);
        
        targetDirection.y = 0;
        targetDirection.Normalize();
        stateMachine.player.mover.Rotate(targetDirection, false);
    }
}