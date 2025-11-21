using UnityEngine;
using UnityEngine.AI;

public class BossChargeAttackState : BossBaseState
{
    private int _attackIndex = 1;
    private int _attackCount = 2;
    private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _bossTransform;
    private GameObject _player;
    private bool _shouldRotate = true;
    private float _rotationTimer;
    private float _rotationDuration = 0.85f;

    public BossChargeAttackState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _bossTransform = stateMachine.BossTransform;
        _player = stateMachine.PlayerGameObject;
    }

    public override void OnEnter()
    {
        _animator.SetBool("shouldMove", false);
        _agent.ResetPath();
        _rotationTimer =_rotationDuration;
        _shouldRotate = true;
        int triggerHash = GetAttackTriggerHash(_attackIndex);
        _animator.SetTrigger(triggerHash);
        _attackIndex = (_attackIndex + 1) % _attackCount;
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

    private int GetAttackTriggerHash(int index)
    {
        switch (index)
        {
            case 0:
                return Animator.StringToHash("chargeAttack1");
            case 1:
                return Animator.StringToHash("chargeAttack2");
            default:
                // fallback hash
                return Animator.StringToHash("chargeAttack1");
        }
    }
}
