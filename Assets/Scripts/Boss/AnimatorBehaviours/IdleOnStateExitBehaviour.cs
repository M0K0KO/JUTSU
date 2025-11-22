using System;
using UnityEngine;

public class IdleOnStateExitBehaviour : StateMachineBehaviour
{
    public enum BossStateType
    {
        NormalHit,
        AkaHit,
        KonHit,
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

        BossBaseState stateToCheck = null;
        switch (bossStateType)
        {
            case BossStateType.NormalHit:
                stateToCheck = stateMachine.NormalHitState;
                break;
            case BossStateType.AkaHit:
                stateToCheck = stateMachine.AkaHitState;
                break;
            case BossStateType.KonHit:
                stateToCheck = stateMachine.KonHitState;
                break;
            case BossStateType.ChargeAttack:
                stateToCheck = stateMachine.ChargeAttackState;
                break;
            case BossStateType.ShockwaveAttack:
                stateToCheck = stateMachine.ShockwaveAttackState;
                break;
        }

        if (stateToCheck != null && stateToCheck == stateMachine.CurrentState)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
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
