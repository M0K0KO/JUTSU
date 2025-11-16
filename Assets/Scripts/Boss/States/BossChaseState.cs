using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossChaseState :BossBaseState
{
    private float _attackDistance = 4f;
    
    public BossChaseState(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        StateMachine.BossAnimator.SetBool("shouldMove", true);
    }

    public override void OnUpdate()
    {
        NavMeshAgent agent = StateMachine.BossAgent;
        Quaternion lookRotation =
            Quaternion.LookRotation(StateMachine.PlayerGameObject.transform.position -
                                    StateMachine.BossTransform.position);
        Quaternion targetRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        StateMachine.transform.rotation =
            UnrealInterp.QInterpTo(StateMachine.transform.rotation, targetRotation, Time.deltaTime, 4f);
        
    }

    public override void OnFixedUpdate()
    {
        NavMeshAgent agent = StateMachine.BossAgent;
        GameObject player = StateMachine.PlayerGameObject;
        if (!player) return;

        Vector3 playerPosition = player.transform.position;

        float currentAnimLocomotionValue = StateMachine.BossAnimator.GetFloat("locomotion");
        float newAnimLocomotionValue = UnrealInterp.FInterpTo(currentAnimLocomotionValue, 1f, Time.deltaTime, 2f);
        StateMachine.BossAnimator.SetFloat("locomotion", newAnimLocomotionValue);

        agent.SetDestination(playerPosition);

        float distToPlayer = Vector3.Distance(playerPosition, agent.transform.position);
        if (distToPlayer <= _attackDistance)
        {
            StateMachine.ChangeState(StateMachine.IdleState);
            return;
        }
    }

    public override void OnExit(){}

    public override void OnAnimatorMove()
    {
        
    }
}
