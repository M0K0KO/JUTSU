using UnityEngine;

public class AttackState : BaseState
{
    public AttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private bool nextCombo = false;
    
    public override void OnEnterState()
    {
        stateMachine.player.mover.EnableMove();
        
        stateMachine.player.mover.EnableRotation();
        PivotTowardsTargetDirection();
        stateMachine.player.mover.DisableRotation();
        
        stateMachine.player.animController.TriggerAnimParam("attack");
    }
    
    public override void OnUpdateState()
    {
        if (nextCombo)
        {
            stateMachine.player.animController.TriggerAnimParam("attack");
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.player.animController.ResetTriggerAnimParam("attack");
        nextCombo = false;
    }

    private void PivotTowardsTargetDirection()
    {
        Vector3 targetDirection = Vector3.zero;
        if (stateMachine.isStrafing)
            // snap the rotation towards enemy
            targetDirection = stateMachine.currentTargetEnemy.transform.position - stateMachine.transform.position;
        else
            // snap the rotation towards camera forward
            targetDirection = stateMachine.player.playerCam.transform.forward;
        
        targetDirection.y = 0;
        targetDirection.Normalize();
        stateMachine.player.mover.Rotate(targetDirection, false);
    }

    public void BufferAttackInput()
    {
        AnimatorStateInfo stateInfo = stateMachine.player.animator.GetCurrentAnimatorStateInfo(1);
        if (!stateInfo.IsName("Empty") && stateInfo.normalizedTime >= 0.35f)
        {
            nextCombo = true;
        }
    }
}
