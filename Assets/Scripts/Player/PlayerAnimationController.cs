using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerAnimationController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerInput playerInput;
    private PlayerMover mover;
    public Animator animator { get; private set; }

    [SerializeField] private float animaParamSmoothTime = 10f;

    private void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        mover = GetComponent<PlayerMover>();
    }

    private void LateUpdate()
    {
        LerpUpdateAnimParam("moveAmount", mover.GetMoveAmount());
        if (stateMachine.mover.canRotate)
        {
            LerpUpdateAnimParam("horizontalMoveAmount", playerInput.MoveInput.x);
            LerpUpdateAnimParam("verticalMoveAmount", playerInput.MoveInput.y);
        }
        UpdateAnimParam("isMoving", playerInput.MoveInput != Vector2.zero);
        UpdateAnimParam("isRunning", playerInput.RunInput != false);
        UpdateAnimParam("isStrafing", stateMachine.isStrafing);
    }

    public void UpdateAnimParam(string targetParam, float value)
    {
        animator.SetFloat(targetParam, value);
    }

    public void LerpUpdateAnimParam(string targetParam, float value)
    {
        float lerpValue = Mathf.Lerp(animator.GetFloat(targetParam), value, animaParamSmoothTime * Time.deltaTime);
        animator.SetFloat(targetParam, lerpValue);
    }

    public void UpdateAnimParam(string targetParam, bool value)
    {
        animator.SetBool(targetParam, value);
    }

    public void TriggerAnimParam(string targetParam)
    {
        animator.SetTrigger(targetParam);
    }

    public void ResetTriggerAnimParam(string targetParam)
    {
        animator.ResetTrigger(targetParam);
    }
}
