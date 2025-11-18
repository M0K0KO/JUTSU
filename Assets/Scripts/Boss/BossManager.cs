using System;
using Unity.Cinemachine;
using UnityEngine;

public class BossManager : MonoBehaviour, IDamgeable
{
    [HideInInspector] public BossStateMachine StateMachine { get; private set; }
    [HideInInspector] public BossSoundEffect SoundEffect { get; private set; }
    [SerializeField] private Collider leftHandCollider;
    [SerializeField] private Collider rightHandCollider;
    
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        StateMachine = GetComponent<BossStateMachine>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        leftHandCollider.enabled = false;
        rightHandCollider.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (StateMachine.CurrentState != StateMachine.ChargeAttackState &&
                StateMachine.CurrentState != StateMachine.ShockwaveAttackState)
            {
                StateMachine.ChangeState(StateMachine.NormalHitState);
            }
        }
    }

    public void TakeDamage(bool shouldPlayHitReaction = false, GestureType gestureType = GestureType.None)
    {
        switch (gestureType)
        {
            case GestureType.None:
            {
                if (shouldPlayHitReaction)
                {
                    StateMachine.ChangeState(StateMachine.NormalHitState);
                }
                break;
            }
            
        }
    }

    public void PlayImpulse()
    {
        _impulseSource.GenerateImpulse();
    }

    
    
}
