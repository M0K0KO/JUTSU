using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerAnimationController : MonoBehaviour
{
    private PlayerManager player;

    [SerializeField] private float animaParamSmoothTime = 10f;

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void LateUpdate()
    {
        LerpUpdateAnimParam("moveAmount", player.mover.GetMoveAmount());
        if (player.mover.canRotate)
        {
            LerpUpdateAnimParam("horizontalMoveAmount", player.playerInput.MoveInput.x);
            LerpUpdateAnimParam("verticalMoveAmount", player.playerInput.MoveInput.y);
        }
        UpdateAnimParam("isMoving", player.playerInput.MoveInput != Vector2.zero);
        UpdateAnimParam("isRunning", player.playerInput.RunInput != false);
        UpdateAnimParam("isStrafing", player.stateMachine.isStrafing);
    }

    public void UpdateAnimParam(string targetParam, float value)
    {
        player.animator.SetFloat(targetParam, value);
    }

    public void LerpUpdateAnimParam(string targetParam, float value)
    {
        float lerpValue = Mathf.Lerp(player.animator.GetFloat(targetParam), value, animaParamSmoothTime * Time.deltaTime);
        player.animator.SetFloat(targetParam, lerpValue);
    }

    public void UpdateAnimParam(string targetParam, bool value)
    {
        player.animator.SetBool(targetParam, value);
    }

    public void TriggerAnimParam(string targetParam)
    {
        player.animator.SetTrigger(targetParam);
    }

    public void ResetTriggerAnimParam(string targetParam)
    {
        player.animator.ResetTrigger(targetParam);
    }
}
