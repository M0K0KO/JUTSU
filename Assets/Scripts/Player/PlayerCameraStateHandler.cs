using Unity.Cinemachine;
using UnityEngine;

public enum PlayerCameraState
{
    Base,
    Strafe,
    Jutsu
}
//[TODO]
// which camera state should we return after using skill?
// what if we cannot find a target for skillCamera?
public class PlayerCameraStateHandler : MonoBehaviour
{
    public static PlayerCameraStateHandler instance;
    
    private Animator animator;
    
    [SerializeField] private CinemachineCamera baseCamera;
    [SerializeField] private CinemachineCamera strafeCamera;
    [SerializeField] private CinemachineCamera skillCamera;

    public PlayerCameraState currentState { get; private set; } = PlayerCameraState.Base;
    
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EventManager.OnCameraStateChange += UpdateCameraState;
    }

    private void OnDisable()
    {
        EventManager.OnCameraStateChange -= UpdateCameraState;
    }

    public void UpdateCameraState(PlayerCameraState state, Transform target)
    {
        currentState = state;
        switch (state)
        {
            case PlayerCameraState.Base:
                animator.Play("Base");
                break;
            case PlayerCameraState.Strafe:
                animator.Play("Strafe");
                strafeCamera.LookAt = target;
                break;
            case PlayerCameraState.Jutsu:
                animator.Play("Skill");
                skillCamera.LookAt = target;
                break;
        }
    }
}
