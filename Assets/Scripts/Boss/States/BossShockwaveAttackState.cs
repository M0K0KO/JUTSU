using UnityEngine;
using UnityEngine.AI;

public class BossShockwaveAttackState : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;
    
    private int _attackHash = Animator.StringToHash("shockwaveAttack");
    
    public BossShockwaveAttackState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
    }

    public override void OnEnter()
    {
        StateMachine.CanShockwaveAttack = false;
        _animator.SetBool("shouldMove", false);
        _animator.SetTrigger(_attackHash);
        _agent.ResetPath();
        
    }
}
