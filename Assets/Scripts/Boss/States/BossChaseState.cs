using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossChaseState :BossBaseState
{
    private float _attackDistance = 4.5f;
    private NavMeshAgent _agent;
    private Animator _animator;

    private int _locomotionId = Animator.StringToHash("Locomotion");
    
    public BossChaseState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _agent = stateMachine.BossAgent;
        _animator = stateMachine.BossAnimator;
    }

    public override void OnEnter()
    {

    }

    public override void OnUpdate()
    {
        NavMeshAgent agent = StateMachine.BossAgent;
        Quaternion lookRotation =
            Quaternion.LookRotation(StateMachine.PlayerGameObject.transform.position -
                                    StateMachine.BossTransform.position);
        Quaternion targetRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        StateMachine.transform.rotation =
            UnrealInterp.QInterpTo(StateMachine.transform.rotation, targetRotation, Time.deltaTime, 4f * _animator.speed);

        GameObject player = StateMachine.PlayerGameObject;
        if (!player) return;

        Vector3 playerPosition = player.transform.position;
        float currentAnimLocomotionValue = StateMachine.BossAnimator.GetFloat(_locomotionId);
        float newAnimLocomotionValue = UnrealInterp.FInterpTo(currentAnimLocomotionValue, 1f, Time.deltaTime, 2f * _animator.speed);
        StateMachine.BossAnimator.SetFloat(_locomotionId, newAnimLocomotionValue);

        _agent.SetDestination(playerPosition);

        float distToPlayer = Vector3.Distance(playerPosition, _agent.transform.position);
        if (distToPlayer <= _attackDistance)
        {
            StateMachine.ChangeState(StateMachine.ChargeAttackState);
            return;
        }

        if (StateMachine.CanShockwaveAttack)
        {
            StateMachine.ChangeState(StateMachine.ShockwaveAttackState);
            return;
        }
        
    }

    public override void OnExit()
    {
        StateMachine.ResetLocomotionValue(0.2f);
    }
}
