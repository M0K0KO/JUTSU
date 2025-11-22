using UnityEngine;

public class HitState : BaseState
{
    public HitState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void OnEnterState()
    {
        stateMachine.player.mover.DisableMove();
        stateMachine.player.mover.DisableRotation();
        stateMachine.player.mover.ChangeSpeedToZero();

        stateMachine.player.animator.Play("Hit");
    }
    
    public override void OnUpdateState()
    {
        if (stateMachine.hitAnimationQueue.TryDequeue(out bool _))
        {
            stateMachine.player.animator.Play("Hit");
        }

        AnimatorStateInfo stateInfo = stateMachine.player.animator.GetCurrentAnimatorStateInfo(2);
        if (stateInfo.IsName("Empty"))
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.hitAnimationQueue.Clear();
    }
}