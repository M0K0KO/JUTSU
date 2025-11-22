using UnityEngine;
using UnityEngine.AI;

public class BossKonHit : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _bossTransform;
    private GameObject _player;
    private Rigidbody _rigidbody;
    
    private int _attackTriggerId = Animator.StringToHash("Attack");
    private int _hitTypeId = Animator.StringToHash("HitType");
    private int _hitTriggerId = Animator.StringToHash("Hit");
    
    public BossKonHit(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _bossTransform = stateMachine.BossTransform;
        _player = stateMachine.PlayerGameObject;
        _rigidbody = stateMachine.BossRigidbody;
    }

    public override void OnEnter()
    {
        
        _agent.ResetPath();

        _animator.ResetTrigger(_attackTriggerId);
        _animator.SetInteger(_hitTypeId, (int)BossHit.KonHit);
        _animator.SetTrigger(_hitTriggerId);
        
        StateMachine.Manager.konHitImpulseSource.GenerateImpulse();
    }
}