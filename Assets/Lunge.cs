using System.Collections;
using UnityEngine;

public class Lunge : StateMachineBehaviour
{
    private PlayerManager player;

    [SerializeField] private AnimationCurve lungeCurve;
    [SerializeField] private float moveAmount;
    [SerializeField] private bool isFront = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player) player = animator.GetComponent<PlayerManager>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float multiplier = isFront ? 1f : -1f;
        float currentSpeed = lungeCurve.Evaluate(stateInfo.normalizedTime) * moveAmount;
        Vector3 moveVector = player.transform.forward * currentSpeed * Time.deltaTime * multiplier;
        
        player.cc.Move(moveVector);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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