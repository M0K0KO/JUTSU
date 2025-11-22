using UnityEngine;
using UnityEngine.AI;

public class BossNormalHitState : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private GameObject _player;
    private Transform _bossTransform;
    private Quaternion _targetRotation;
    private float _enterTime;
    private float _rotationDuration = 0.2f;

    private int _attackTriggerId = Animator.StringToHash("Attack");
    private int _hitTypeId = Animator.StringToHash("HitType");
    private int _hitTriggerId = Animator.StringToHash("Hit");
    
    public BossNormalHitState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _bossTransform = stateMachine.BossTransform;
        _player = stateMachine.PlayerGameObject;
        
    }

    public override void OnEnter()
    {
        _enterTime = Time.time;

        FindTargetRotation();
        
        _agent.ResetPath();
        _animator.ResetTrigger(_attackTriggerId);
        _animator.SetInteger(_hitTypeId, (int)BossHit.NormalHit);
        _animator.SetTrigger(_hitTriggerId);
        
        
    }

    public override void OnUpdate()
    {
        if (Time.time - _enterTime < _rotationDuration)
        {
            _bossTransform.rotation = UnrealInterp.QInterpTo(_bossTransform.rotation, _targetRotation, Time.deltaTime, 10f);
        }
    }

    private void FindTargetRotation()
    {
        if (_player)
        {
            Vector3 toPlayer = _player.transform.position - _bossTransform.position;
            if (toPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(toPlayer);
                _targetRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
                return;
            }
        }

        _targetRotation = _bossTransform.rotation;
    }
    
}
