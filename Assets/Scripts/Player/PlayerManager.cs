using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerInput playerInput { get; private set; }
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerMover mover { get; private set; }
    public PlayerAnimationController animController { get; private set; }
    public PlayerJutsuManager jutsu { get; private set; }
    public PlayerVFXManager vfxManager { get; private set; }
    public BaseAudioSourceHolder audioSourceHolder { get; private set; }
    public PlayerCameraImpulseManager impulseManager { get; private set; }

    public Animator animator { get; private set; }
    public CharacterController cc { get; private set; }
    public Camera playerCam { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        stateMachine = GetComponent<PlayerStateMachine>();
        mover = GetComponent<PlayerMover>();
        animController = GetComponent<PlayerAnimationController>();
        jutsu = GetComponent<PlayerJutsuManager>();
        vfxManager = GetComponent<PlayerVFXManager>();
        audioSourceHolder = GetComponentInChildren<BaseAudioSourceHolder>();
        impulseManager = GetComponent<PlayerCameraImpulseManager>();
        
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        playerCam = Camera.main;
    }
}
