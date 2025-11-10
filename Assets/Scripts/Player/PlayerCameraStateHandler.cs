using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerCameraState
{
    Base,
    Strafe,
    Skill
}
//[TODO]
// which camera state should we return after using skill?
// what if we cannot find a target for skillCamera?
public class PlayerCameraStateHandler : MonoBehaviour
{
    private Animator animator;
    
    [SerializeField] private CinemachineCamera baseCamera;
    [SerializeField] private CinemachineCamera strafeCamera;
    [SerializeField] private CinemachineCamera skillCamera;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EventManager.OnCameraStateChange += UpdateCameraTarget;
    }

    private void OnDisable()
    {
        EventManager.OnCameraStateChange -= UpdateCameraTarget;
    }

    private void UpdateCameraTarget(PlayerCameraState state, Transform target)
    {
        switch (state)
        {
            case PlayerCameraState.Base:
                animator.Play("Base");
                break;
            case PlayerCameraState.Strafe:
                animator.Play("Strafe");
                strafeCamera.LookAt = target;
                break;
            case PlayerCameraState.Skill:
                animator.Play("Skill");
                skillCamera.LookAt = target;
                break;
        }
    }
    
}
