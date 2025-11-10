using UnityEngine;

public class AttackState : BaseState
{
    public AttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private bool nextCombo = false;
    
    public override void OnEnterState()
    {
        stateMachine.mover.EnableMove();
        PivotTowardsTargetDirection();
        stateMachine.mover.DisableRotation();
        stateMachine.animationController.TriggerAnimParam("attack");
    }
    
    public override void OnUpdateState()
    {
        if (nextCombo)
        {
            stateMachine.animationController.TriggerAnimParam("attack");
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.animationController.ResetTriggerAnimParam("attack");
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
            targetDirection = stateMachine.playerCam.transform.forward;
        
        targetDirection.y = 0;
        targetDirection.Normalize();
        stateMachine.mover.Rotate(targetDirection, false);
    }

    public void BufferAttackInput()
    {
        AnimatorStateInfo stateInfo = stateMachine.animationController.animator.GetCurrentAnimatorStateInfo(1);
        if (!stateInfo.IsName("Empty") && stateInfo.normalizedTime >= 0.35f)
        {
            nextCombo = true;
        }
    }
}
