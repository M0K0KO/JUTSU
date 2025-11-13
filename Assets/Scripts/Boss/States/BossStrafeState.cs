using UnityEngine;
using UnityEngine.AI;

public class BossStrafeState : BossBaseState
{
    private const float CloseRangeStrafeRadius = 7f;
    private const float FarRangeStrafeRadius = 12f;
    private const float MaxStrafeDestinationSampleAngle = 40f;
    private const float MinStrafeDestinationSampleAngle = 20f;

    private const float StrafeRotationSpeed = 200f;
    private const float StrafeAgentSpeed = 2f;


    public BossStrafeState(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        StateMachine.BossAnimator.CrossFadeInFixedTime("Walk", 0.7f);
        StateMachine.BossAgent.speed = StrafeAgentSpeed;
        

        NavMeshAgent bossAgent = StateMachine.BossAgent;
        GameObject playerGameObject = StateMachine.PlayerGameObject;

        if (playerGameObject)
        {
            Transform bossTransform = StateMachine.BossTransform;

            float sampleAngle = Random.Range(MinStrafeDestinationSampleAngle, MaxStrafeDestinationSampleAngle);
            int sign = Random.Range(0, 2) == 0 ? -1 : 1;

            Vector3 playerToBoss = bossTransform.position - playerGameObject.transform.position;
            playerToBoss.y = 0;
            playerToBoss.Normalize();

            Vector3 offsetDir = Quaternion.Euler(0f, sampleAngle * sign, 0f) * playerToBoss;

            Vector3 rawSamplePosition = playerGameObject.transform.position + offsetDir * CloseRangeStrafeRadius;
            if (NavMesh.SamplePosition(rawSamplePosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                bossAgent.SetDestination(hit.position);
            }
            else
            {
                StateMachine.ChangeState(StateMachine.IdleState);
            }
        }
    }

    public override void OnUpdate()
    {
        GameObject playerGameObject = StateMachine.PlayerGameObject;

        if (playerGameObject)
        {
            Transform bossTransform = StateMachine.BossTransform;
            Vector3 lookDirection = playerGameObject.transform.position - bossTransform.position;
            lookDirection.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            bossTransform.rotation = Quaternion.RotateTowards(bossTransform.rotation, lookRotation, StrafeRotationSpeed * Time.deltaTime);

            NavMeshAgent bossAgent = StateMachine.BossAgent;

            if (bossAgent.remainingDistance <= bossAgent.stoppingDistance && !bossAgent.pathPending)
            {
                StateMachine.ChangeState(StateMachine.IdleState);
                return;
            }


            Vector3 bossVelocity = StateMachine.BossAgent.velocity;
            Vector3 localDirection = StateMachine.BossTransform.InverseTransformDirection(bossVelocity);
            Vector3 normalizedDirection2D = new Vector3(localDirection.x, 0, localDirection.z).normalized;

            StateMachine.BossAnimator.SetFloat("ForwardAxis", localDirection.z);
            StateMachine.BossAnimator.SetFloat("RightAxis", localDirection.x);
        }
    }
}
