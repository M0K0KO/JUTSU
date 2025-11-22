using UnityEngine;
using UnityEngine.AI;

public class BossChargeAttackState : BossBaseState
{
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _bossTransform;
    private GameObject _player;
    private bool _shouldRotate = true;
    private float _rotationTimer;
    private float _rotationDuration = 0.85f;

    private int _attackTypeId = Animator.StringToHash("AttackType");
    private int _attackTriggerId = Animator.StringToHash("Attack");
    private BossAttack[] _chargeAttacks = { BossAttack.ChargeAttack1, BossAttack.ChargeAttack2 };
    private int _chargeAttackIndex = 0;

    public BossChargeAttackState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _bossTransform = stateMachine.BossTransform;
        _player = stateMachine.PlayerGameObject;
    }

    public override void OnEnter()
    {
        _agent.ResetPath();
        _rotationTimer =_rotationDuration;
        _shouldRotate = true;
        
        _animator.SetInteger(_attackTypeId, (int)_chargeAttacks[_chargeAttackIndex]);
        _chargeAttackIndex = (_chargeAttackIndex + 1) % _chargeAttacks.Length;
        _animator.SetTrigger(_attackTriggerId);
    }

    public override void OnUpdate()
    {
        if (_shouldRotate)
        {
            if (_rotationTimer > 0.001f)
            {
                Quaternion currentRotation = _bossTransform.rotation;
                Quaternion targetRotation = _bossTransform.rotation;
                Vector3 toPlayer = _player.transform.position - _bossTransform.position;
                toPlayer.y = 0f;
                if (toPlayer != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(toPlayer);
                }

                float scaledInterpSpeed = 4f * _animator.speed;
                _bossTransform.rotation = UnrealInterp.QInterpTo(currentRotation, targetRotation, Time.deltaTime, scaledInterpSpeed);
                
                _rotationTimer -= Time.deltaTime;
            }
            else
            {
                _shouldRotate = false;
            }
        }
    }
    
    
    
}
