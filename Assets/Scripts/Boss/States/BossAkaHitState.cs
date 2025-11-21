using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class BossAkaHitState : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _bossTransform;
    private GameObject _player;
    private Rigidbody _rigidbody;
    
    private Vector3 _initialDirection;
    private float _duration;

    private Quaternion _targetRotation;
    private float _enterTime;
    private float _rotationDuration = 0.2f;

    private bool _playingEndAnimation = false;
    
    public BossAkaHitState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _bossTransform = stateMachine.BossTransform;
        _player = stateMachine.PlayerGameObject;
        _rigidbody = stateMachine.BossRigidbody;
    }

    public override void OnEnter()
    {
        _playingEndAnimation = false;
        _enterTime = Time.time;
        _agent.ResetPath();
        _animator.SetBool("shouldMove", false);
        _animator.CrossFadeInFixedTime("AkaHitLoop", 0.15f);
        
        FindTargetRotation();
        
        _initialDirection = StateMachine.AkaInitialDirection;
        _initialDirection.y = 0f;
        _initialDirection.Normalize();
        
        _duration = StateMachine.AkaDuration;
        _rigidbody.linearVelocity = 20f * _initialDirection;
    }

    public override void OnUpdate()
    {
        if (Time.time - _enterTime < _rotationDuration)
        {
            _bossTransform.rotation =
                UnrealInterp.QInterpTo(_bossTransform.rotation, _targetRotation, Time.deltaTime, 10f);
        }

        if (Time.time - _enterTime > _duration)
        {
            if (!_playingEndAnimation)
            {
                _playingEndAnimation = true;
                _rigidbody.linearVelocity = Vector3.zero;
                _animator.CrossFadeInFixedTime("AkaHitNormalEnd", 0.15f);
            }
        }
    }

    private void FindTargetRotation()
    {
        if (_player)
        {
            Vector3 toPlayer = _player.transform.position - _bossTransform.position;
            if (toPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(_player.transform.position - _bossTransform.position);
                _targetRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
                return;
            }
        }

        _targetRotation = _bossTransform.rotation;
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !_playingEndAnimation)
        {
            Vector3 normal2D = collision.contacts[0].normal;
            normal2D.y = 0f;
            normal2D.Normalize();
            _bossTransform.DOMove(_bossTransform.position + normal2D * 1.5f, 0.15f).SetEase(Ease.InOutSine);
            _playingEndAnimation = true;
            _rigidbody.linearVelocity = Vector3.zero;
            _animator.CrossFadeInFixedTime("AkaHitWallStart", 0.2f);
        }
    }
}
