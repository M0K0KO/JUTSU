using UnityEngine;
using UnityEngine.AI;

public class BossShockwaveAttackState : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;

    private int _attackTypeId = Animator.StringToHash("AttackType");
    private int _attackTriggerId = Animator.StringToHash("Attack");
    
    public BossShockwaveAttackState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
    }

    public override void OnEnter()
    {
        StateMachine.CanShockwaveAttack = false;
        _animator.SetInteger(_attackTypeId, (int)BossAttack.ShockwaveAttack);
        _animator.SetTrigger(_attackTriggerId);
        _agent.ResetPath();
        
    }
}
