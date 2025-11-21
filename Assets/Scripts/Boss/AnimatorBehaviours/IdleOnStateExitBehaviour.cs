using UnityEngine;

public class IdleOnStateExitBehaviour : StateMachineBehaviour
{
    public enum BossStateType
    {
        AkaHit,
        NormalHit,
        ChargeAttack,
        ShockwaveAttack
    }
    
    [SerializeField] private BossStateType bossStateType;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BossStateMachine stateMachine = animator.GetComponent<BossStateMachine>();
        switch (bossStateType)
        {
            case BossStateType.AkaHit:
            {
                if (stateMachine.CurrentState == stateMachine.AkaHitState)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }
                break; 
            }
            case BossStateType.ChargeAttack:
            {
                if (stateMachine.CurrentState == stateMachine.ChargeAttackState)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }
                break;
            }
            case BossStateType.NormalHit:
            {
                if (stateMachine.CurrentState == stateMachine.NormalHitState)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }
                break;
            }
            case BossStateType.ShockwaveAttack:
            {
                if (stateMachine.CurrentState == stateMachine.ShockwaveAttackState)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }

                break;
            }
                
        }
        
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
